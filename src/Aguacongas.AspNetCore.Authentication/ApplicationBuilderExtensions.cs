// Project: aguacongas/DymamicAuthProviders
// Copyright (c) 2021 @Olivier Lefebvre
using Aguacongas.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.AspNetCore.Builder
{
    /// <summary>
    /// IApplicationBuilder extensions to load configuration
    /// </summary>
    public static class ApplicationBuilderExtensions
    {
        /// <summary>
        /// Loads the dynamic authentication configuration.
        /// </summary>
        /// <param name="provider">The provider.</param>
        /// <returns></returns>
        public static IServiceProvider LoadDynamicAuthenticationConfiguration(this IServiceProvider provider)
        {
            using (var scope = provider.CreateScope())
            {
                var manager = scope.ServiceProvider.GetRequiredService<AuthenticationSchemeProviderWrapper>();
                var store = scope.ServiceProvider.GetService<IDynamicProviderStore>();
                var schemes = store?.GetSchemeDefinitionsAsync().ToListAsync().ConfigureAwait(false).GetAwaiter().GetResult();
                manager.InitializeAsync(schemes).ConfigureAwait(false).GetAwaiter().GetResult();
            }
            return provider;
        }

        /// <summary>
        /// Loads the dynamic authentication configuration.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <returns></returns>
        public static IApplicationBuilder UseDynamicAuthenticationConfiguration(this IApplicationBuilder builder)
        {
            builder.ApplicationServices.LoadDynamicAuthenticationConfiguration();
            return builder;
        }
    }
}
