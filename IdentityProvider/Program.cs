// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityModel;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NMSI.IdentityProvider.Areas.Identity.Data;
using NMSI.IdentityProvider.Contexts;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;
using System;
using System.Linq;
using System.Security.Claims;

namespace NMSI.IdentityProvider
{
    public class Program
    {
        public static int Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
                .MinimumLevel.Override("System", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.AspNetCore.Authentication", LogEventLevel.Information)
                .Enrich.FromLogContext()
                // uncomment to write to Azure diagnostics stream
                //.WriteTo.File(
                //    @"D:\home\LogFiles\Application\identityserver.txt",
                //    fileSizeLimitBytes: 1_000_000,
                //    rollOnFileSizeLimit: true,
                //    shared: true,
                //    flushToDiskInterval: TimeSpan.FromSeconds(1))
                .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level}] {SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}{NewLine}", theme: AnsiConsoleTheme.Code)
                .CreateLogger();

            try
            {


                Log.Information("Starting host...");
                var host = CreateHostBuilder(args).Build();

                using(var scope = host.Services.CreateScope())
                {
                    try
                    {
                        var context = scope.ServiceProvider.GetService<UserDbContext>();

                        // ensure the db is migrated before seeding
                        context.Database.Migrate();

                        // use the user manager to create test users
                        var userManager = scope.ServiceProvider
                            .GetRequiredService<UserManager<ApplicationUser>>();

                        Seed(userManager, "Jack", "Torrance", "jack.torrance@email.com", "P@ssword1");
                        Seed(userManager, "Wendy", "Torrance", "wendy.torrance@email.com", "P@ssword1");
                    }
                    catch(Exception ex)
                    {
                        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
                        logger.LogError(ex, "An error occured while seeding the database.");
                    }
                }


                host.Run();
                return 0;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Host terminated unexpectedly.");
                return 1;
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseSerilog()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });

        static void Seed(UserManager<ApplicationUser> userManager, string givenName, string familyName, string email, string password)
        {
            var user = userManager.FindByNameAsync(givenName).Result;
            if (user is null)
            {
                user = new ApplicationUser()
                {
                    UserName = givenName,
                    EmailConfirmed = true
                };

                var result = userManager.CreateAsync(user, password).Result;

                if (!result.Succeeded)
                    throw new Exception(result.Errors.First().Description);

                result = userManager.AddClaimsAsync(user, new Claim[]
                {
                                new Claim(JwtClaimTypes.Name, $"{givenName} {familyName}"),
                                new Claim(JwtClaimTypes.GivenName, givenName),
                                new Claim(JwtClaimTypes.FamilyName, familyName),
                                new Claim(JwtClaimTypes.Email, email)
                }).Result;

                if (!result.Succeeded)
                    throw new Exception(result.Errors.First().Description);
            }
        }
    }
}