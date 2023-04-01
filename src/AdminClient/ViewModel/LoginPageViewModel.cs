using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using MSiccDev.ServerlessBlog.AdminClient.Common;
using MSiccDev.ServerlessBlog.AdminClient.Services;
using MSiccDev.ServerlessBlog.AdminClient.View;
using MSiccDev.ServerlessBlog.ClientSdk;
using MSiccDev.ServerlessBlog.DtoModel;
using Newtonsoft.Json;
namespace MSiccDev.ServerlessBlog.AdminClient.ViewModel
{
    // ReSharper disable ClassNeverInstantiated.Global
    public class LoginPagePageViewModel : BasePageViewModel, IQueryAttributable
        // ReSharper restore ClassNeverInstantiated.Global
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<LoginPagePageViewModel> _logger;
		private readonly IBlogClient _blogClient;
        private readonly ICacheService _cacheService;
        private readonly INavigationService _navigationService;
        private readonly IActionSheetService _actionSheetService;

		private string? _azureFunctionBaseUrl;
        private string? _azureAdClientId;
        private string? _azureAdCallbackUrl;
        private string? _azureTenantId;
        private string? _azureAdScopeToAdd;
        private string? _azureAdScopeToRemove;


        private AsyncRelayCommand? _authorizationCommand;
        private RelayCommand? _addAzureAdScopeCommand;
        private string? _returnRoute = nameof(BlogPage);

        public LoginPagePageViewModel(IHttpClientFactory httpClientFactory,
            ILogger<LoginPagePageViewModel> logger,
            IBlogClient blogClient,
            ICacheService cacheService,
            IActionSheetService actionSheetService,
            INavigationService navigationService)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
			_blogClient = blogClient;
            _cacheService = cacheService;
            _actionSheetService = actionSheetService;
            _navigationService = navigationService;
        }

        private bool CanExecuteLoginAsync() =>
            Connectivity.Current.NetworkAccess == NetworkAccess.Internet &&
            !string.IsNullOrWhiteSpace(this.AzureFunctionBaseUrl) &&
            !string.IsNullOrWhiteSpace(this.AzureAdClientId) &&
            !string.IsNullOrWhiteSpace(this.AzureTenantId) &&
            !string.IsNullOrWhiteSpace(this.AzureAdCallbackUrl) &&
            this.AzureAdScopes.Any();

        private bool CanExecuteAddAzureAdScope() =>
            !string.IsNullOrWhiteSpace(this.AzureAdScopeToAdd);

        protected override async Task ExecuteViewAppearingAsync()
        {
            this.AzureFunctionBaseUrl = await SecureStorage.Default.GetAsync(Constants.AzureFunctionBaseUrlStorageName);
            this.AzureTenantId = await SecureStorage.Default.GetAsync(Constants.AzureAdTenantIdStorageName);
            this.AzureAdClientId = await SecureStorage.Default.GetAsync(Constants.AzureAdClientIdStorageName);
            this.AzureAdCallbackUrl = await SecureStorage.Default.GetAsync(Constants.AzureAdCallbackUrlStorageName);

            string savedScopesString = await SecureStorage.Default.GetAsync(Constants.AzureAdScopesStorageName);
            if (!string.IsNullOrWhiteSpace(savedScopesString))
            {
                foreach (string scope in savedScopesString.ConvertToList())
                    this.AzureAdScopes.Add(scope);
            }
            
            this.AuthorizationCommand.NotifyCanExecuteChanged();
        }
        
        
        private void AddAzureAdScope()
        {
            if (!string.IsNullOrWhiteSpace(this.AzureAdScopeToAdd))
            {
                this.AzureAdScopes.Add(this.AzureAdScopeToAdd);
                this.AuthorizationCommand.NotifyCanExecuteChanged();

                this.AzureAdScopeToAdd = null;
            }
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            _returnRoute = query["returnRoute"].ToString();
        }
        
        private async Task AuthorizeAsync()
        {
            await SaveUserLoginDataAsync();

            if (Preferences.Get(Constants.DebugLocallyStorageName, false))
            {
                this.AzureTenantId = Guid.Empty.ToString();
                this.AzureAdClientId = Guid.Empty.ToString();
                this.AzureAdCallbackUrl = null;

                await LoadBlogsInitiallyAndSelectOneAsync();
            }
            else
            {

                string stateValue = GetLoginUrlWithStateValue(out string loginUrl);

                try
                {
                    WebAuthenticatorResult authResult = await WebAuthenticator.Default.AuthenticateAsync(
                    new WebAuthenticatorOptions
                    {
                        Url = new Uri(loginUrl),
                        CallbackUrl = new Uri(this.AzureAdCallbackUrl!),
                        PrefersEphemeralWebBrowserSession = true
                    });

                    if (authResult.Properties.ContainsKey("code"))
                    {
                        HttpResponseMessage response = await TryGetAccessTokenResponseAsync(authResult, stateValue);

                        response.EnsureSuccessStatusCode();

                        string responseContent = await response.Content.ReadAsStringAsync();

                        if (!string.IsNullOrWhiteSpace(responseContent))
                        {
                            AzureAdAccessTokenResponse? azureAdAccessToken = JsonConvert.DeserializeObject<AzureAdAccessTokenResponse>(responseContent);

                            if (Preferences.Default.Get(Constants.HasObtainedValidAccessTokenStorageName, false))
                            {
                                if (azureAdAccessToken != null)
                                {
                                    await SecureStorage.Default.SetAsync(Constants.AzureAdAccessTokenStorageName, responseContent);

                                    await _navigationService.NavigateToRouteAsync(_returnRoute!, false, ShellNavigationSearchDirection.Up);
                                }
                            }
                            else
                            {
                                if (azureAdAccessToken != null)
                                {
                                    await SecureStorage.Default.SetAsync(Constants.AzureAdAccessTokenStorageName, responseContent);
                                    await LoadBlogsInitiallyAndSelectOneAsync();
                                }
                            }
                        }
                    }
                    //TODO: Show error message
                }
                catch (Exception ex)
                {
                    if (ex is TaskCanceledException canceledException)
                    {
                        //TODO
                    }

                    _logger.LogError(ex, "Error during authentication process:");

                }
            }
        }





        private async Task SaveUserLoginDataAsync()
        {

            await SecureStorage.SetAsync(Constants.AzureFunctionBaseUrlStorageName, this.AzureFunctionBaseUrl ?? throw new InvalidOperationException());
            await SecureStorage.SetAsync(Constants.AzureAdClientIdStorageName, this.AzureAdClientId ?? throw new InvalidOperationException());
            await SecureStorage.SetAsync(Constants.AzureAdCallbackUrlStorageName, this.AzureAdCallbackUrl ?? throw new InvalidOperationException());
            await SecureStorage.SetAsync(Constants.AzureAdTenantIdStorageName, this.AzureTenantId ?? throw new InvalidOperationException());
            await SecureStorage.SetAsync(Constants.AzureAdScopesStorageName, this.AzureAdScopes.ToArray().ToSeparatedValuesString(" "));
        }

        private string GetLoginUrlWithStateValue(out string loginUrl)
        {
            string stateValue = Guid.NewGuid().ToString();

            loginUrl = $"https://login.microsoftonline.com/{this.AzureTenantId}/oauth2/v2.0/authorize".
                       AddParameterToUri("client_id", this.AzureAdClientId!).
                       AddParameterToUri("response_type", "code").
                       AddParameterToUri("redirect_uri", this.AzureAdCallbackUrl!).
                       AddParameterToUri("response_mode", "query").
                       AddParameterToUri("scope", this.AzureAdScopes.ToArray().ToSeparatedValuesString(" ")).
                       AddParameterToUri("state", stateValue);

            return stateValue;
        }

        private async Task<HttpResponseMessage> TryGetAccessTokenResponseAsync(WebAuthenticatorResult authResult, string stateValue)
        {

            if (authResult.Properties.ContainsKey("state"))
                if (authResult.Properties["state"] != stateValue)
                    throw new NotSupportedException("state value must be the same between request and response");

            Dictionary<string, string> tokenRequestBodyParameters = new Dictionary<string, string>
            {
                { "grant_type", "authorization_code" },
                { "client_id", this.AzureAdClientId! },
                { "code", authResult.Properties["code"] },
                { "redirect_uri", this.AzureAdCallbackUrl! }
            };

            if (tokenRequestBodyParameters == null)
                throw new ArgumentNullException(nameof(tokenRequestBodyParameters));

            string? userImpersonationScope = this.AzureAdScopes.SingleOrDefault(value => value.Contains("user_impersonation"));
            if (!string.IsNullOrWhiteSpace(userImpersonationScope))
                tokenRequestBodyParameters.Add("scope", userImpersonationScope);
            else
                throw new NotSupportedException("user_impersonation scope is always needed");


            using HttpClient loginClient = _httpClientFactory.CreateClient();


            HttpRequestMessage request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri($"https://login.microsoftonline.com/{this.AzureTenantId}/oauth2/v2.0/token"),
                Content = new FormUrlEncodedContent(tokenRequestBodyParameters)
            };

            return await loginClient.SendAsync(request, HttpCompletionOption.ResponseContentRead);
        }

        private async Task GetSelectedBlogAsync(List<BlogOverview> blogs)
        {

            Blog? selectedBlog = null;

            if (blogs.Count == 1)
            {
                selectedBlog = blogs.First();
            }
            else
            {
                string? selectedBlogPopupResult = await _actionSheetService.ShowActionSheetAsync("Select a blog:", "Cancel", blogs.Select(blog => blog.Name).ToArray());

                if (!string.IsNullOrWhiteSpace(selectedBlogPopupResult))
                    selectedBlog = blogs.SingleOrDefault(blog => blog.Name == selectedBlogPopupResult);
            }

            if (selectedBlog != null)
            {
                Preferences.Default.Set(Constants.CurrentSelectedBlogIdStorageName, selectedBlog.BlogId.ToString());

                await _cacheService.RefreshAsync(selectedBlog.BlogId!.Value);
            }
        }

        private async Task LoadBlogsInitiallyAndSelectOneAsync()
        {
            Preferences.Default.Set(Constants.HasObtainedValidAccessTokenStorageName, true);

            _blogClient.Init(this.AzureFunctionBaseUrl!);

            List<BlogOverview>? blogs = await _cacheService.GetBlogsAsync(30, true);

            if (blogs?.Any() ?? false)
            {
                await GetSelectedBlogAsync(blogs);

                await _navigationService.NavigateToRouteAsync(_returnRoute, false, ShellNavigationSearchDirection.Up);
            }
            else
            {
                _logger.LogError("No blogs found at {ApiBaseUrl}", this.AzureFunctionBaseUrl);

                //TODO: ASK TO ADD NEW BLOG?
            }
        }
        
        
        public string? AzureFunctionBaseUrl
        {
            get => _azureFunctionBaseUrl;
            set
            {
                SetProperty(ref _azureFunctionBaseUrl, value);
                this.AuthorizationCommand.NotifyCanExecuteChanged();
            }
        }

        public string? AzureTenantId
        {
            get => _azureTenantId;
            set
            {
                SetProperty(ref _azureTenantId, value);
                this.AuthorizationCommand.NotifyCanExecuteChanged();
            }
        }


        public string? AzureAdClientId
        {
            get => _azureAdClientId;
            set
            {
                SetProperty(ref _azureAdClientId, value);
                this.AuthorizationCommand.NotifyCanExecuteChanged();
            }
        }

        public string? AzureAdCallbackUrl
        {
            get => _azureAdCallbackUrl;
            set
            {
                SetProperty(ref _azureAdCallbackUrl, value);
                this.AuthorizationCommand.NotifyCanExecuteChanged();
            }
        }

        public string? AzureAdScopeToAdd
        {
            get => _azureAdScopeToAdd;
            set
            {
                SetProperty(ref _azureAdScopeToAdd, value);
                this.AddAzureAdScopeCommand.NotifyCanExecuteChanged();
            }
        }

        public string? AzureAdScopeToRemove
        {
            get => _azureAdScopeToRemove;
            set
            {
                SetProperty(ref _azureAdScopeToRemove, value);

                if (!string.IsNullOrWhiteSpace(_azureAdScopeToRemove))
                    this.AzureAdScopes.Remove(_azureAdScopeToRemove);

                this.AuthorizationCommand.NotifyCanExecuteChanged();
            }
        }

        public ObservableCollection<string> AzureAdScopes { get; set; } = new ObservableCollection<string>();


        public AsyncRelayCommand AuthorizationCommand => _authorizationCommand ??= new AsyncRelayCommand(AuthorizeAsync, CanExecuteLoginAsync);
        public RelayCommand AddAzureAdScopeCommand => _addAzureAdScopeCommand ??= new RelayCommand(AddAzureAdScope, CanExecuteAddAzureAdScope);



    }
}
