// Project: aguacongas/DymamicAuthProviders
// Copyright (c) 2021 @Olivier Lefebvre
using Aguacongas.AspNetCore.Authentication;
using Aguacongas.AspNetCore.Authentication.EntityFramework;
using Aguacongas.AspNetCore.Authentication.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// <see cref="DynamicAuthenticationBuilder"/> extensions.
    /// </summary>
    public static class DynamicAuthenticationBuilderExtensions
    {
        /// <summary>
        /// Adds the entity framework store.
        /// </summary>
        /// <typeparam name="TContext">The type of the context.</typeparam>
        /// <param name="builder">The builder.</param>
        /// <returns>The <see cref="DynamicAuthenticationBuilder"/></returns>
        public static DynamicAuthenticationBuilder AddEntityFrameworkStore(this DynamicAuthenticationBuilder builder)
        {
            return builder.AddEntityFrameworkStore<SchemeDbContext, SchemeDefinition>();
        }
        /// <summary>
        /// Adds the entity framework store.
        /// </summary>
        /// <typeparam name="TContext">The type of the context.</typeparam>
        /// <param name="builder">The builder.</param>
        /// <returns>The <see cref="DynamicAuthenticationBuilder"/></returns>
        public static DynamicAuthenticationBuilder AddEntityFrameworkStore<TContext>(this DynamicAuthenticationBuilder builder)
            where TContext : DbContext
        {
            return builder.AddEntityFrameworkStore<TContext, SchemeDefinition>();
        }
        /// <summary>
        /// Adds the entity framework store.
        /// </summary>
        /// <typeparam name="TContext">The type of the context.</typeparam>
        /// <param name="builder">The builder.</param>
        /// <returns>The <see cref="DynamicAuthenticationBuilder"/></returns>
        public static DynamicAuthenticationBuilder AddEntityFrameworkStore<TContext, TDefinitionType>(this DynamicAuthenticationBuilder builder)
            where TContext : DbContext
            where TDefinitionType : SchemeDefinition
        {
            AddStore(builder.Services, typeof(TDefinitionType), typeof(TContext));
            return builder;
        }

        private static void AddStore(IServiceCollection service, Type definitionType, Type contextType)
        {

            var storeType = typeof(DynamicProviderStore<,>).MakeGenericType(definitionType, contextType);

            service.TryAddTransient(typeof(IDynamicProviderStore), storeType);
            service.TryAddTransient(typeof(IDynamicProviderMutationStore<>).MakeGenericType(definitionType), storeType);
            service.TryAddTransient<IAuthenticationSchemeOptionsSerializer, AuthenticationSchemeOptionsSerializer>();
            service.TryAddTransient<IDynamicProviderUpdatedEventHandler, InProcDynamicProviderUpdatedEventHandler>();

        }
    }
}
