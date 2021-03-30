// Project: aguacongas/DymamicAuthProviders
// Copyright (c) 2021 @Olivier Lefebvre
using Aguacongas.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// AuthenticationBuilder extensions
    /// </summary>
    public static class AuthenticationBuilderExtensions
    {
        /// <summary>
        /// Configures the DI for dynamic scheme management.
        /// </summary>
        /// <typeparam name="TSchemeDefinition">The type of the definition.</typeparam>
        /// <param name="builder">The builder.</param>
        /// <returns></returns>
        public static DynamicAuthenticationBuilder AddDynamicAuthentication(this AuthenticationBuilder builder)
        {
            var dynamicBuilder = new DynamicAuthenticationBuilder(builder.Services);
            builder.Services.TryAddSingleton<OptionsMonitorCacheWrapperFactory>();
            builder.Services.TryAddTransient(provider => new AuthenticationSchemeProviderWrapper
                (
                    provider.GetRequiredService<IAuthenticationSchemeProvider>(),
                    provider.GetRequiredService<OptionsMonitorCacheWrapperFactory>(),
                    dynamicBuilder.HandlerTypes
                ));
            builder.Services.TryAddTransient<IDynamicProviderHandlerTypeProvider>(sp => sp.GetRequiredService<AuthenticationSchemeProviderWrapper>());
            return dynamicBuilder;
        }
    }
}
