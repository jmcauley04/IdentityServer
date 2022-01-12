using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;

namespace BlazorOIDC.Client.MessageHandlers
{
    public class OnDemandApiAuthMessageHandler : AuthorizationMessageHandler
    {
        public OnDemandApiAuthMessageHandler(
            IAccessTokenProvider provider, 
            NavigationManager navigation,
            IConfiguration config) 
            : base(provider, navigation)
        {
            ConfigureHandler(
                authorizedUrls: new[] {
                    config.GetValue<string>("OnDemandCoursesApiUri")
                });
        }
    }
}
