using MSiccDev.ServerlessBlog.AdminClient.View;
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
        public const string DebugLocallyStorageName = "DebugLocallyStorageName";


        public static string LoginPageRoute => nameof(LoginPage);
        public static string BlogPageRoute => nameof(BlogPage);
        public static string AuthorPageRoute => nameof(AuthorPage);
        public static string MediaPageRoute => nameof(MediaPage);
        public static string PostsPageRoute => nameof(PostsPage);
        public static string SettingsPageRoute => nameof(SettingsPage);
        public static string TagsPageRoute => nameof(TagsPage);

    }
}