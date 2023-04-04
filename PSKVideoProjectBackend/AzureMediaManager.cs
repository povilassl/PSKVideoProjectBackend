using DemoVideoApplication;
using Azure.ResourceManager.Media;
using Azure.ResourceManager.Media.Models;

namespace PSKVideoProjectBackend
{
    public static class AzureMediaManager
    {
        public static IAzureMediaServicesClient _client = null;
        public static AzConfigWrapper? _config = null;

        public static async Task InitManager()
        {
            Console.WriteLine("Dir:" + Directory.GetCurrentDirectory());

            AzConfigWrapper config = new AzConfigWrapper(new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("configappsettings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build());

            try
            {
                IAzureMediaServicesClient client = await CreateMediaServicesClientAsync(config);

                Console.WriteLine("connected");

                _client = client;
                _config = config;
            }
            catch (Exception exception)
            {
                if (exception.Source.Contains("ActiveDirectory"))
                {
                    Console.Error.WriteLine("TIP: Make sure that you have filled out the appsettings.json file before running this sample.");
                }

                Console.Error.WriteLine($"{exception.Message}");


                if (exception.GetBaseException() is ErrorResponseException apiException)
                {
                    Console.Error.WriteLine(
                        $"ERROR: API call failed with error code '{apiException.Body.Error.Code}' and message '{apiException.Body.Error.Message}'.");
                }
            }
        }
    }
}
