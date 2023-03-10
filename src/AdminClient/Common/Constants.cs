namespace MSiccDev.ServerlessBlog.AdminClient.Common
{
    public static class Constants
    {

        public const string AzureLogoutPath = "/.auth/logout";

        public const string AzureAdAccessTokenStorageName = "AzAdAccessToken";

        public const string AzureFunctionBaseUrlStorageName = "AzureFunctionBaseUrl";
        public const string AzureAdClientIdStorageName = "AzureAdClientId";
        public const string AzureAdCallbackUrlStorageName = "AzureAdCallbackUrl";
        public const string AzureAdTenantIdStorageName = "AzureAdTenantId";
        public const string AzureAdScopesStorageName = "AzureAdScopes";

        public const string HasObtainedValidAccessTokenStorageName = "HasObtainValidAccessToken";
        public const string CurrentSelectedBlogIdStorageName = "CurrentSelectedBlogId";
    }
}