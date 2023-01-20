using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using MSiccDev.ServerlessBlog.AdminClient.Common;
using MSiccDev.ServerlessBlog.ClientSdk;
using Newtonsoft.Json;
namespace MSiccDev.ServerlessBlog.AdminClient.ViewModel
{
    public class LoginPageViewModel : ObservableObject
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<LoginPageViewModel> _logger;
        private string? _azureFunctionBaseUrl;
        private string? _azureAdClientId;
        private string? _azureAdCallbackUrl;
        private string? _azureTenantId;
        private string? _azureAdScopeToAdd;
        private string? _azureAdScopeToRemove;


        private AsyncRelayCommand? _authorizationCommand;
        private RelayCommand? _addAzureAdScopeCommand;

        public LoginPageViewModel(IHttpClientFactory httpClientFactory, ILogger<LoginPageViewModel> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
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

        private void AddAzureAdScope()
        {
            if (!string.IsNullOrWhiteSpace(this.AzureAdScopeToAdd))
            {
                this.AzureAdScopes.Add(this.AzureAdScopeToAdd);
                this.AuthorizationCommand.NotifyCanExecuteChanged();

                this.AzureAdScopeToAdd = null;
            }
        }

        private async Task AuthorizeAsync()
        {
            await SecureStorage.SetAsync(Constants.AzureFunctionBaseUrlStorageName, this.AzureFunctionBaseUrl ?? throw new InvalidOperationException());
            await SecureStorage.SetAsync(Constants.AzureAdClientIdStorageName, this.AzureAdClientId ?? throw new InvalidOperationException());
            await SecureStorage.SetAsync(Constants.AzureAdCallbackUrlStorageName, this.AzureAdCallbackUrl ?? throw new InvalidOperationException());
            await SecureStorage.SetAsync(Constants.AzureAdTenantIdStorageName, this.AzureTenantId ?? throw new InvalidOperationException());
            await SecureStorage.SetAsync(Constants.AzureAdScopesStorageName, this.AzureAdScopes.ToArray().ToSeparatedValuesString(" "));

            string stateValue = Guid.NewGuid().ToString();

            string loginUrl =
                $"https://login.microsoftonline.com/{this.AzureTenantId}/oauth2/v2.0/authorize".
                    AddParameterToUri("client_id", this.AzureAdClientId).
                    AddParameterToUri("response_type", "code").
                    AddParameterToUri("redirect_uri", this.AzureAdCallbackUrl).
                    AddParameterToUri("response_mode", "query").
                    AddParameterToUri("scope", this.AzureAdScopes.ToArray().ToSeparatedValuesString(" ")).
                    AddParameterToUri("state", stateValue);


            try
            {
                WebAuthenticatorResult authResult = await WebAuthenticator.Default.AuthenticateAsync(
                new WebAuthenticatorOptions
                {
                    Url = new Uri(loginUrl),
                    CallbackUrl = new Uri(this.AzureAdCallbackUrl),
                    PrefersEphemeralWebBrowserSession = true
                });

                if (authResult.Properties.ContainsKey("code"))
                {
                    if (authResult.Properties.ContainsKey("state"))
                        if (authResult.Properties["state"] != stateValue)
                            throw new NotSupportedException("state value must be the same between request and response");

                    Dictionary<string, string> tokenRequestBodyParameters = new Dictionary<string, string>
                    {
                        { "grant_type", "authorization_code" },
                        { "client_id", this.AzureAdClientId },
                        { "code", authResult.Properties["code"] },
                        { "redirect_uri", this.AzureAdCallbackUrl }
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

                    HttpResponseMessage? response = await loginClient.SendAsync(request, HttpCompletionOption.ResponseContentRead);
                    string responseContent = await response.Content.ReadAsStringAsync();

                    response.EnsureSuccessStatusCode();

                    if (!string.IsNullOrWhiteSpace(responseContent))
                    {
                        AzureAdAccessTokenResponse? azureAdAccessToken = JsonConvert.DeserializeObject<AzureAdAccessTokenResponse>(responseContent);

                        if (azureAdAccessToken != null)
                        {
                            await SecureStorage.SetAsync(Constants.AzureAdAccessTokenStorageName, responseContent);
                            Preferences.Set(Constants.HasObtainedValidAccessTokenStorageName, true);

                            Ioc.Default.GetRequiredService<IBlogClient>().Init(this.AzureFunctionBaseUrl);

                            //TODO: Navigate to Home here with Shell
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
                //TODO
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


        public AsyncRelayCommand AuthorizationCommand =>
            _authorizationCommand ??= new AsyncRelayCommand(AuthorizeAsync, CanExecuteLoginAsync);


        public RelayCommand AddAzureAdScopeCommand =>
            _addAzureAdScopeCommand ??= new RelayCommand(AddAzureAdScope, CanExecuteAddAzureAdScope);

    }
}
