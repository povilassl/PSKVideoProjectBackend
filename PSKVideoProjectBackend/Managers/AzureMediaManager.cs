using Azure;
using Azure.Identity;
using Azure.ResourceManager;
using Azure.ResourceManager.Media;
using Azure.ResourceManager.Media.Models;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using log4net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Management.Media;
using Microsoft.Azure.Management.Media.Models;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Azure;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.Rest.Azure.Authentication;
using NReco.VideoInfo;
using PSKVideoProjectBackend.Models;
using PSKVideoProjectBackend.Models.Enums;
using PSKVideoProjectBackend.Properties;
using PSKVideoProjectBackend.Repositories;
using PSKVideoProjectBackend.Wrappers;
using System;
using System.Diagnostics;

namespace PSKVideoProjectBackend.Managers
{
    public sealed class AzureMediaManager
    {
        public static AzConfigWrapper? _mediaConfig = null;
        public static IAzureMediaServicesClient? _mediaClient = null;
        public static MediaServicesAccountResource _mediaServicesAccount = null;
        public static MediaTransformResource? _videoTransform = null;

        static BlobServiceClient _thumbnailBlobClient;
        static BlobContainerClient _thumbnailContainerClient;

        static string _thumbnailBlobUriPrefix = "https://mediastorageaccount1312.blob.core.windows.net/thumbnail-images/";
        static string _streamingEndpointName = "default";

        private readonly ILogger _logger;
        private readonly SignalRManager _signalRManager;
        private readonly IServiceScopeFactory _serviceScopeFactory;


        public AzureMediaManager(SignalRManager signalRManager, ILogger<AzureMediaManager> logger, IServiceScopeFactory serviceScopeFactory)
        {
            _signalRManager = signalRManager;
            _logger = logger;
            _serviceScopeFactory = serviceScopeFactory;
        }

        /// <summary>
        /// Used to initialize all the necessary connections (setup)
        /// </summary>
        /// <returns></returns>
        public async Task InitManager()
        {
            try
            {
                //setting up config for thumbnail blob client and container
                var builder = new ConfigurationBuilder()
                        .SetBasePath(Directory.GetCurrentDirectory())
                        .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

                var configuration = builder.Build();
                string connectionString = configuration.GetConnectionString("ThumbnailBlobStorageConnectionString")!;

                _thumbnailBlobClient = new BlobServiceClient(connectionString);
                _thumbnailContainerClient = _thumbnailBlobClient.GetBlobContainerClient("thumbnail-images");

                //setting up config for media resources
                //_logger.Info("Dir:" + Directory.GetCurrentDirectory());
                _logger.LogInformation("Dir:" + Directory.GetCurrentDirectory());

                AzConfigWrapper config = new AzConfigWrapper(new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("mediaconfigappsettings.json", optional: true, reloadOnChange: true)
                    .AddEnvironmentVariables()
                    .Build());


                IAzureMediaServicesClient client = await CreateMediaServicesClientAsync(config);

                _mediaClient = client;
                _mediaConfig = config;
            }
            catch (Exception exception)
            {
                if (exception.Source is not null && exception.Source.Contains("ActiveDirectory"))
                {
                    _logger.LogInformation("TIP: Make sure that you have filled out the appsettings.json file before running this sample.");
                }

                if (exception.GetBaseException() is ErrorResponseException apiException)
                {
                    _logger.LogError($"API call failed with error code '{apiException.Body.Error.Code}' and message '{apiException.Body.Error.Message}'.");
                }

                throw;
            }

            try
            {
                //Create media services account
                var mediaServicesResourceId = MediaServicesAccountResource.CreateResourceIdentifier(
                    _mediaConfig.SubscriptionId,
                    _mediaConfig.ResourceGroup,
                    _mediaConfig.AccountName);

                var credential = new DefaultAzureCredential(includeInteractiveCredentials: true);
                var armClient = new ArmClient(credential);
                _mediaServicesAccount = armClient.GetMediaServicesAccountResource(mediaServicesResourceId);

                // Ensure that you have customized encoding Transform. This is a one-time setup operation.
                _videoTransform = await CreateTransformAsync(_mediaServicesAccount, "ContentAwareEncoding");
            }
            catch (Exception e)
            {
                _logger.LogError("Could not create Media Services Account or Video Transform");
                _logger.LogError(e.Message);

                throw;
            }
        }

        private static async Task<IAzureMediaServicesClient> CreateMediaServicesClientAsync(AzConfigWrapper config)
        {
            // Use ApplicationTokenProvider.LoginSilentAsync to get a token using a service principal with symmetric key
            ClientCredential clientCredential = new ClientCredential(config.AadClientId, config.AadSecret);
            var credentials = await ApplicationTokenProvider.LoginSilentAsync(config.AadTenantId, clientCredential, ActiveDirectoryServiceSettings.Azure);

            return new AzureMediaServicesClient(config.ArmEndpoint, credentials) {
                SubscriptionId = config.SubscriptionId,
            };
        }

        // If the specified transform exists, return that transform. If the it does not
        // exist, creates a new transform with the specified output. In this case, the
        // output is set to encode a video using a custom preset.
        static async Task<MediaTransformResource> CreateTransformAsync(MediaServicesAccountResource mediaServicesAccount, string transformName)
        {
            // Create the custom Transform with the outputs defined above
            // Does a Transform already exist with the desired name? This method will just overwrite (Update) the Transform if it exists already. 
            // In production code, you may want to be cautious about that. It really depends on your scenario.
            var transform = await mediaServicesAccount.GetMediaTransforms().CreateOrUpdateAsync(
                WaitUntil.Completed,
                transformName,
                new MediaTransformData {
                    Outputs = { new MediaTransformOutput(
                        preset: new Azure.ResourceManager.Media.Models.BuiltInStandardEncoderPreset(Azure.ResourceManager.Media.Models.EncoderNamedPreset.ContentAwareEncoding)) }
                });

            return transform.Value;
        }

        /// <summary>
        /// Used to upload new video to our azure media services and DB
        /// </summary>
        /// <param name="videoToUpload"></param>
        /// <param name="user"></param>
        /// <param name="sendEmail"></param>
        /// <returns></returns>
        public async Task<bool> UploadVideo(VideoToUpload videoToUpload, RegisteredUser user, bool sendEmail)
        {
            try
            {
                string uniqueness = Guid.NewGuid().ToString()[..13];
                string jobName = $"job-{uniqueness}";
                string locatorName = $"locator-{uniqueness}";
                string inputAssetName = $"input-{uniqueness}";
                string outputAssetName = $"output-{videoToUpload.VideoFile.FileName}-{uniqueness}";

                //We need to upload files before returning because othwerwise the IFormFile objects will be disposed of
                (var inputAsset, var duration, var thumbnailUrl) = await UploadFiles(videoToUpload, inputAssetName);

                if (inputAsset is null || duration == 0 || String.IsNullOrEmpty(thumbnailUrl))
                {
                    return false;
                }

                //Fire and forget - notifications and logging are in place
                ContinueRunningVideoUpload(user, sendEmail, inputAsset, duration, thumbnailUrl, videoToUpload, outputAssetName, jobName, locatorName);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return false;
            }
        }

        private async Task ContinueRunningVideoUpload(RegisteredUser user,
            bool sendEmail, MediaAssetResource inputAsset, uint duration, string thumbnailUrl,
            VideoToUpload videoToUpload, string outputAssetName, string jobName, string locatorName)
        {
            // Output from the Job must be written to an Asset, so let's create one.
            var outputAsset = await CreateOutputAssetAsync(_mediaServicesAccount, outputAssetName);

            var job = await SubmitJobAsync(_videoTransform!, jobName, inputAsset, outputAsset);

            var streamUrl = await ContinueRunningJobInBackground(job, inputAsset, outputAsset, locatorName);

            if (string.IsNullOrEmpty(streamUrl))
            {
                _logger.LogError("There was en error encoding the video asset");
                await CleanUpAsync(_videoTransform!, job, inputAsset, outputAsset);

                await PushNotificationForUser(false, sendEmail, user.Id.ToString());
            }

            var uploaded = new UploadedVideo(videoToUpload) {
                Username = user.Username,
                UserId = user.Id,
                ThumbnailURL = thumbnailUrl,
                VideoURL = streamUrl,
                VideoDurationInSeconds = duration
            };

            //Since we end up here after a fire-and-forget action, our dbContext scope would have ended by now
            //So we create a new scope just for this action
            //Other way is to keep the scope for the whole singleton duration, but that would be bad practise
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var apiDbContext = scope.ServiceProvider.GetService<ApiDbContext>()!;

                await apiDbContext.UploadedVideos.AddAsync(uploaded);
                await apiDbContext.SaveChangesAsync();
            }

            await PushNotificationForUser(true, sendEmail, user.Id.ToString());
        }

        private async Task<(MediaAssetResource, uint, string)> UploadFiles(VideoToUpload videoToUpload, string inputAssetName)
        {
            (var asset, var duration) = await CreateInputAssetAsync(_mediaServicesAccount, inputAssetName, videoToUpload.VideoFile);
            var thumbnailUrl = await UploadThumbnailImage(videoToUpload.ThumbnailImage);

            return (asset, duration, thumbnailUrl);
        }

        private async Task<string> ContinueRunningJobInBackground(
            MediaJobResource job, MediaAssetResource inputAsset,
            MediaAssetResource outputAsset, string locatorName)
        {
            try
            {
                job = await WaitForJobToFinishAsync(job);

                if (job.Data.State == MediaJobState.Error)
                {
                    _logger.LogError($"Job finished with error message: {job.Data.Outputs[0].Error.Message}");
                    _logger.LogError($"                  error details: {job.Data.Outputs[0].Error.Details[0].Message}");
                    await CleanUpAsync(_videoTransform!, job, inputAsset, outputAsset);
                    return null!;
                }

                var streamingLocator = await CreateStreamingLocatorAsync(_mediaServicesAccount, outputAsset.Data.Name, locatorName);
                var streamingEndpoint = (await _mediaServicesAccount.GetStreamingEndpoints().GetAsync(_streamingEndpointName)).Value;

                _logger.LogInformation("Getting the streaming manifest URLs for HLS and DASH");
                var streamingUrl = await GetStreamingUrlAsync(streamingLocator, streamingEndpoint);

                return streamingUrl;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return null!;
            }
        }

        private async Task<string> GetStreamingUrlAsync(StreamingLocatorResource locator, StreamingEndpointResource streamingEndpoint)
        {
            try
            {
                var paths = await locator.GetStreamingPathsAsync();

                var retList = new List<Tuple<string, string>>();

                //Right now it returns only the first path (out of 3 available in total)
                string streamingFormatPath = paths.Value.StreamingPaths[0].Paths[0];

                var uriBuilder = new UriBuilder() {
                    Scheme = "https",
                    Host = streamingEndpoint.Data.HostName,
                    Path = streamingFormatPath
                };

                return uriBuilder.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return null!;
            }
        }

        async Task<(MediaAssetResource, uint)> CreateInputAssetAsync(
           MediaServicesAccountResource mediaServicesAccount, string assetName, IFormFile videoFile)
        {
            // We are assuming that the Asset name is unique.
            MediaAssetResource asset;
            uint duration;

            string fileName = DateTime.Now.ToString("yyyyMMddHHmmssfff") + "_" + videoFile.FileName;
            string tempFolder = Path.GetTempPath();
            string tempFilePath = Path.Combine(tempFolder, fileName);

            try
            {
                asset = await mediaServicesAccount.GetMediaAssets().GetAsync(assetName);

                // The Asset already exists and we are going to overwrite it
                _logger.LogWarning($"The Asset named {assetName} already exists. It will be overwritten.");
            }
            catch (RequestFailedException)
            {
                // Call Media Services API to create an Asset.
                // This method creates a container in storage for the Asset.
                // The files (blobs) associated with the Asset will be stored in this container.

                _logger.LogInformation("Creating an input Asset...");
                asset = (await mediaServicesAccount.GetMediaAssets().CreateOrUpdateAsync(WaitUntil.Completed, assetName, new MediaAssetData())).Value;
            }

            try
            {
                // Use Media Services API to get back a response that contains
                // SAS URL for the Asset container into which to upload blobs.
                // That is where you would specify read-write permissions
                // and the expiration time for the SAS URL.
                var sasUriCollection = asset.GetStorageContainerUrisAsync(
                    new MediaAssetStorageContainerSasContent {
                        Permissions = MediaAssetContainerPermission.ReadWrite,
                        ExpireOn = DateTime.UtcNow.AddHours(1)
                    });

                var sasUri = await sasUriCollection.FirstOrDefaultAsync();

                // Use Storage API to get a reference to the Asset container that was created by calling Asset's CreateOrUpdate method.
                var container = new BlobContainerClient(sasUri);
                BlobClient blobClient = container.GetBlobClient(videoFile.FileName);

                await blobClient.UploadAsync(videoFile.OpenReadStream(), new BlobHttpHeaders { ContentType = videoFile.ContentType });

                //Save temp file to get duration of mp4
                using (var stream = new FileStream(tempFilePath, FileMode.Create))
                {
                    videoFile.CopyTo(stream);
                }

                var ffProbe = new FFProbe();
                var videoInfo = ffProbe.GetMediaInfo(tempFilePath);
                duration = (uint)videoInfo.Duration.TotalSeconds;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return (null!, 0);
            }
            finally
            {
                if (File.Exists(tempFilePath)) File.Delete(tempFilePath);
            }

            return (asset, duration);
        }


        // Creates an output Asset. The output from the encoding Job must be written to an Asset.
        async Task<MediaAssetResource> CreateOutputAssetAsync(MediaServicesAccountResource mediaServicesAccount, string assetName)
        {
            _logger.LogInformation($"Creating an output Asset. Name: {assetName}, Account Id: {mediaServicesAccount.Id}");
            var asset = await mediaServicesAccount.GetMediaAssets().CreateOrUpdateAsync(
                WaitUntil.Completed,
                assetName,
                new MediaAssetData());

            return asset.Value;
        }


        // Submits a request to Media Services to apply the specified Transform to a given input video.
        async Task<MediaJobResource> SubmitJobAsync(MediaTransformResource transform, string jobName, MediaAssetResource inputAsset, MediaAssetResource outputAsset)
        {
            try
            {
                // We are assuming that the Job name is unique.
                var job = await transform.GetMediaJobs().CreateOrUpdateAsync(
                    WaitUntil.Completed,
                    jobName,
                    new MediaJobData {
                        Input = new MediaJobInputAsset(assetName: inputAsset.Data.Name),
                        Outputs = { new MediaJobOutputAsset(outputAsset.Data.Name) }
                    });

                return job.Value;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return null!;
            }

        }

        // Polls Media Services for the status of the Job.
        async Task<MediaJobResource> WaitForJobToFinishAsync(MediaJobResource job)
        {
            try
            {
                var sleepInterval = TimeSpan.FromSeconds(30);
                MediaJobState? state;

                do
                {
                    job = await job.GetAsync();
                    state = job.Data.State.GetValueOrDefault();

                    if (state != MediaJobState.Finished && state != MediaJobState.Error && state != MediaJobState.Canceled)
                    {
                        await Task.Delay(sleepInterval);
                    }
                }
                while (state != MediaJobState.Finished && state != MediaJobState.Error && state != MediaJobState.Canceled);

                return job;
            }
            catch (Exception ex)
            {
                //_logger.Error(ex.Message);
                _logger.LogError(ex.Message);
                return null;
            }

        }

        // Delete the resources that were created.
        async Task CleanUpAsync(MediaTransformResource transform, MediaJobResource job, MediaAssetResource? inputAsset, MediaAssetResource outputAsset)
        {
            try
            {
                if (job != null) await job.DeleteAsync(WaitUntil.Completed);

                if (transform != null) await transform.DeleteAsync(WaitUntil.Completed);

                if (inputAsset != null) await inputAsset.DeleteAsync(WaitUntil.Completed);

                if (outputAsset != null) await outputAsset.DeleteAsync(WaitUntil.Completed);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }

        }

        /// Creates a StreamingLocator for the specified Asset and with the specified streaming policy name.
        /// Once the StreamingLocator is created the output Asset is available to clients for playback.
        async Task<StreamingLocatorResource> CreateStreamingLocatorAsync(
            MediaServicesAccountResource mediaServicesAccount,
            string assetName,
            string locatorName)
        {
            try
            {
                var locator = await mediaServicesAccount.GetStreamingLocators().CreateOrUpdateAsync(
                    WaitUntil.Completed,
                    locatorName,
                    new StreamingLocatorData {
                        AssetName = assetName,
                        StreamingPolicyName = "Predefined_ClearStreamingOnly"
                    });

                return locator.Value;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return null;
            }
        }

        async Task<string> UploadThumbnailImage(IFormFile image)
        {
            Response<BlobContentInfo> response;

            var originalFileName = Path.GetFileNameWithoutExtension(image.FileName);
            var extension = Path.GetExtension(image.FileName);
            var newFileName = $"{originalFileName}_{Guid.NewGuid()}{extension}";

            using (var memoryStream = new MemoryStream())
            {
                try
                {
                    image.CopyTo(memoryStream);
                    memoryStream.Position = 0;

                    BlobClient blobClient = _thumbnailContainerClient.GetBlobClient(newFileName);

                    response = await blobClient.UploadAsync(memoryStream, new BlobHttpHeaders { ContentType = image.ContentType });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.Message);
                    return null!;
                }

            }

            var rawResponse = response.GetRawResponse();

            //returning Blob URI
            return rawResponse.ReasonPhrase == "Created" ? _thumbnailBlobUriPrefix + newFileName : "";
        }

        private async Task PushNotificationForUser(bool isSuccess, bool sendEmail, string userId)
        {
            string message = isSuccess ? Resources.VideoUploadNotificationSuccess : Resources.VideoUploadNotificationFail;

            await _signalRManager.SendMessageToUser(userId, message);
        }
    }
}
