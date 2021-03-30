// Project: aguacongas/DymamicAuthProviders
// Copyright (c) 2021 @Olivier Lefebvre
using Aguacongas.AspNetCore.Authentication;
using Aguacongas.AspNetCore.Authentication.Persistence;
using Aguacongas.AspNetCore.Authentication.RavenDb;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;
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
        /// <param name="builder">The builder.</param>
        /// <param name="getDocumentStore">(Optional) The get document store function. When null the document store is retrived from the DI.</param>
        /// <param name="dataBase">(Optional) The data base When null the default document store data base is used.</param>
        /// <returns>
        /// The <see cref="DynamicAuthenticationBuilder" />
        /// </returns>
        public static DynamicAuthenticationBuilder AddRavenDbStore(this DynamicAuthenticationBuilder builder, Func<IServiceProvider, IDocumentStore> getDocumentStore = null, string dataBase = null)
        {
            return builder.AddRavenDbStore<SchemeDefinition>();
        }

        /// <summary>
        /// Adds the entity framework store.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="getDocumentStore">(Optional) The get document store function. When null the document store is retrived from the DI.</param>
        /// <param name="dataBase">(Optional) The data base When null the default document store data base is used.</param>
        /// <returns>
        /// The <see cref="DynamicAuthenticationBuilder" />
        /// </returns>
        public static DynamicAuthenticationBuilder AddRavenDbStore<TDefinitionType>(this DynamicAuthenticationBuilder builder, Func<IServiceProvider, IDocumentStore> getDocumentStore = null, string dataBase = null)
            where TDefinitionType : SchemeDefinition
        {
            if (getDocumentStore == null)
            {
                getDocumentStore = p => p.GetRequiredService<IDocumentStore>();
            }

            AddStore(builder.Services, typeof(TDefinitionType), getDocumentStore, dataBase);
            return builder;
        }

        private static void AddStore(IServiceCollection service, Type definitionType, Func<IServiceProvider, IDocumentStore> getDocumentStore, string dataBase)
        {
            var storeType = typeof(DynamicProviderStore<>).MakeGenericType(definitionType);
            var loggerType = typeof(ILogger<>).MakeGenericType(storeType);

            service.TryAddTransient(storeType, p =>
            {
                var session = getDocumentStore(p).OpenAsyncSession(new SessionOptions
                {
                    Database = dataBase
                });
                session.Advanced.UseOptimisticConcurrency = true;
                var constructor = storeType.GetConstructor(new[] { typeof(IAsyncDocumentSession), typeof(IAuthenticationSchemeOptionsSerializer), loggerType });
                return constructor.Invoke(new[] { session, p.GetRequiredService<IAuthenticationSchemeOptionsSerializer>(), p.GetRequiredService(loggerType) });
            });
            service.TryAddTransient(typeof(IDynamicProviderMutationStore<>).MakeGenericType(definitionType), p => p.GetRequiredService(storeType));
            service.TryAddTransient(typeof(IDynamicProviderStore), p => p.GetRequiredService(storeType));
            service.TryAddTransient<IDynamicProviderUpdatedEventHandler, InProcDynamicProviderUpdatedEventHandler>();
            service.TryAddTransient<IAuthenticationSchemeOptionsSerializer, AuthenticationSchemeOptionsSerializer>();
        }
    }
}
