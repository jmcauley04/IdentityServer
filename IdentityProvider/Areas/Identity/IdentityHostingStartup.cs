﻿using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NMSI.IdentityProvider.Areas.Identity.Data;
using NMSI.IdentityProvider.Contexts;

[assembly: HostingStartup(typeof(NMSI.IdentityProvider.Areas.Identity.IdentityHostingStartup))]
namespace NMSI.IdentityProvider.Areas.Identity
{
    public class IdentityHostingStartup : IHostingStartup
    {
        public void Configure(IWebHostBuilder builder)
        {
            builder.ConfigureServices((context, services) => {
                services.AddDbContext<UserDbContext>(options =>
                    options.UseSqlServer(
                        context.Configuration.GetConnectionString("UserDbContextConnection")));

                services.AddIdentity<ApplicationUser, IdentityRole>(
                    options => options.SignIn.RequireConfirmedAccount = true)
                    .AddEntityFrameworkStores<UserDbContext>()
                    .AddDefaultTokenProviders();
            });
        }
    }
}