// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityServer4.Models;
using System.Collections.Generic;

namespace NMSI.IdentityProvider
{
    public static class Config
    {
        public static IEnumerable<IdentityResource> Ids =>
            new IdentityResource[]
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile(),
                new IdentityResources.Email()
            };

        public static IEnumerable<ApiScope> ApiScopes =>
            new ApiScope[]
            {
                new ApiScope("blazorOIDCApi", "Blazor OIDC API")
            };

        public static IEnumerable<ApiResource> ApiResources =>
            new ApiResource[]
            {
                new ApiResource("blazorOIDCApi")
                {
                    Scopes = { "blazorOIDCApi" }
                }
            };

        public static IEnumerable<Client> Clients =>
            new Client[]
            {
                new Client
                {
                    ClientId = "blazorOIDC",
                    ClientName = "Blazor OIDC",
                    AllowedGrantTypes = GrantTypes.Code,
                    RequireClientSecret = false,
                    RequirePkce = true,
                    RedirectUris = { "https://localhost:5000/authentication/login-callback" },
                    PostLogoutRedirectUris = { "https://localhost:5000/authentication/logout-callback" },
                    AllowedScopes = { "openid", "profile", "email", "blazorOIDCApi" },                    
                    AllowedCorsOrigins = { "https://localhost:5000" },
                    RequireConsent = false
                }
            };
    }
}
