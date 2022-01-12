# Introduction 
This is an Identity Provider using OIDC

# Adding and securing a new Blazor client

## 1. Update `Config.Clients` 


### Add a new `Client` and populate it with necessary credentials.

Property | Comment
--:                     | ---
ClientId                | Any string representative of the client (alphanumeric only, no spaces).
ClientName              | Any string representative of the client.
AllowedGrantTypes       | default to `GranTypes.Code`.
RequireClientSecret     | default to `false`.
RequirePkce             | default to `true`.
RedirectUris            | Array of acceptable uris that handles the authentication login callback.  `{client uri}/authentication/login-callback` is a typical ASP.NET callback.
PostLogoutRedirectUris  | Array of acceptable uris that handles the authentication logout callback.  `{client uri}/authentication/logout-callback` is a typical ASP.NET callback.
AllowedScopes           | Scopes available from this identity provider.
AllowedCorsOrigins      | Array of acceptable uris that handles the authentication logout callback.  `{client uri}`.
RequireConsent          | default to `false` unless the client should request user permission to view items within the allowed scopes.


## 2. Add OIDC authentication to `Program.cs`
```csharp
builder.Services.AddOidcAuthentication(options =>
    builder.Configuration.Bind("OidcConfiguration", options.ProviderOptions));
```

## 3. Add OidcConfiguration to `appsettings.json`
```json
"OidcConfiguration": {
    "Authority": "{IdentityProvider URI}",
    "ClientId": "{ClientId defined above}",
    "DefaultScopes": [ // openid and profile are included automatically
      "email"
    ],
    "RedirectUri": "{a RedirectUri defined above that associates with this client's uri}",
    "PostLogoutRedirectUri": "{a PostLogoutRedirectUris defined above that associates with this client's uri}",
    "ResponseType": "code" // should match the AllowedGrantTypes
  }
```

## 4. Add `Authentication.razor` component in the `Pages` folder
```csharp
@page "/authentication/{action}"
@using Microsoft.AspNetCore.Components.WebAssembly.Authentication
<RemoteAuthenticatorView Action="@Action">
    <LoggingIn> //customize as needed
        <div class="nmsi-center-screen">            
            <h1>Logging in...</h1> 
        </div>
    </LoggingIn>
</RemoteAuthenticatorView>

@code{
    [Parameter] public string? Action { get; set; }
}
```

## 5. Update App.Razor
### CascadingAuthenticationState
This component wraps the application and cascades the authentication state into all subcomponents making authentication state easily handled.

### AuthorizeRouteView
This component handles routing and provides a means for globally defining a 'not authorized' action.

#### Example `App.Razor`
```csharp
<CascadingAuthenticationState>
    <BlazyToast CascadeValue="true">
        <Router AppAssembly="@typeof(App).Assembly">
            <Found Context="routeData">
                <AuthorizeRouteView RouteData="@routeData" DefaultLayout="@typeof(MainLayout)">
              
                    <NotAuthorized>
                        @if (context.User.Identity?.IsAuthenticated != true)
                        {
                            <RedirectToLogin />
                        }
                        else
                        {
                            <p role="alert">You are not authorized to access this resource.</p>
                        }
                    </NotAuthorized>
                    <Authorizing>
                        <div class="d-grid justify-content-center my-5">
                            <Loading Text="Authorizing..." />
                        </div>
                    </Authorizing>
                </AuthorizeRouteView>
                <FocusOnNavigate RouteData="@routeData" Selector="h1" />
            </Found>
            <NotFound>
                <PageTitle>Not found</PageTitle>
                <LayoutView Layout="@typeof(MainLayout)">
                    <p role="alert">Sorry, there's nothing at this address.</p>
                </LayoutView>
            </NotFound>
        </Router>
    </BlazyToast>
</CascadingAuthenticationState>
```

# Adding and securing a new API

## 1. Update `Config.Clients`

### Add the API to ApiResources, ApiScopes, and to the list of AllowedScopes for any client that will need access to the API.

```csharp
public static IEnumerable<ApiScope> ApiScopes =>
    new ApiScope[]
    {
        new ApiScope("{apiScopeId}", "{API Name}")
    };

public static IEnumerable<ApiResource> ApiResources =>
    new ApiResource[]
    {
        new ApiResource("{apiScopeId}") // this doesn't necessarily need to be the same
        {
            Scopes = { "{apiScopeId}" }
        }
    };

public static IEnumerable<Client> Clients =>
    new Client[]
    {
        new Client
        {
            ...
            AllowedScopes = { "openid", "profile", "email", "{apiScopeId}" },       
            ...
        }
    };
```

## 2. Update client app to enable it to talk to a protected API

The app needs to add the token to the headers of API requests.  One way to do this is as follows:

1. Add the API's URI to `appsettings.json`
```json
"{apiName}": "{apiUri}"
```

2. In the client, create a new `MessageHandler` class that inherits from `AuthorizationMessageHandler`
```csharp
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;

public class ProtectedApiAuthMessageHandler : AuthorizationMessageHandler
{
    public ProtectedApiAuthMessageHandler(
        IAccessTokenProvider provider,
        NavigationManager navigation,
        IConfiguration config)
        : base(provider, navigation)
    {
        ConfigureHandler(
            authorizedUrls: new[] {
                config.GetValue<string>("{apiName}")
            });
    }
}
```

3. Add the `MessageHandler` to `Program.cs`

<small>Note that `AddHttpClient` comes from `Microsoft.Extensions.Http`</small>

```csharp
builder.Services.AddTransient<ProtectedApiAuthMessageHandler>();

builder.Services.AddHttpClient<{API-Calling Service}>(
    client => client.BaseAddress = new Uri(builder.Configuration.GetValue<string>("{apiName}")))
    .AddHttpMessageHandler<ProtectedApiAuthMessageHandler>();
```

## 3. Update API to look for OIDC authentication

1. Update `Program.cs`
```csharp
var builder = WebApplication.CreateBuilder(args);

// build a require authenticated user policy
var requireAuthenticatedUserPolicy = new AuthorizationPolicyBuilder()
    .RequireAuthenticatedUser()
    .Build();

// add criteria to identify the identity server
builder.Services.AddAuthentication(
    IdentityServerAuthenticationDefaults.AuthenticationScheme)
    .AddIdentityServerAuthentication(options =>
    {
        options.Authority = "{IdentityProvider URI}";
        options.ApiName = "{apiScopeId}";
    });

...

// add the authenticated user policy to all controllers
builder.Services.AddControllers(configure =>
    configure.Filters.Add(new AuthorizeFilter(requireAuthenticatedUserPolicy)));

...

// verify that auth/auth is included
app.UseAuthentication();
app.UseAuthorization();

...

app.Run();
```
