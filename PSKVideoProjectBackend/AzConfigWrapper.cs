namespace DemoVideoApplication
{
    public class AzConfigWrapper
    {
        private readonly IConfiguration _config;

        public AzConfigWrapper(IConfiguration config)
        {
            _config = config;
        }

        public string SubscriptionId
        {
            get { return _config["AZURE_SUBSCRIPTION_ID"]!; }
        }

        public string ResourceGroup
        {
            get { return _config["AZURE_RESOURCE_GROUP"]!; }
        }

        public string AccountName
        {
            get { return _config["AZURE_MEDIA_SERVICES_ACCOUNT_NAME"]!; }
        }

        public string AadTenantId
        {
            get { return _config["AZURE_TENANT_ID"]!; }
        }

        public string AadClientId
        {
            get { return _config["AZURE_CLIENT_ID"]!; }
        }

        public string AadSecret
        {
            get { return _config["AZURE_CLIENT_SECRET"]!; }
        }

        public Uri ArmAadAudience
        {
            get { return new Uri(_config["AZURE_ARM_TOKEN_AUDIENCE"]!); }
        }

        public Uri AadEndpoint
        {
            get { return new Uri(_config["AadEndpoint"]!); }
        }

        public Uri ArmEndpoint
        {
            get { return new Uri(_config["AZURE_ARM_ENDPOINT"]!); }
        }
    }
}
