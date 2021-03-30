// Project: aguacongas/DymamicAuthProviders
// Copyright (c) 2021 @Olivier Lefebvre
using Aguacongas.AspNetCore.Authentication;
using Aguacongas.AspNetCore.Authentication.Persistence;
using Aguacongas.AspNetCore.Authentication.Redis;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// <see cref="DynamicAuthenticationBuilder"/> extensions.
    /// </summary>
    public static class DynamicAuthenticationBuilderExtensions
    {
        /// <summary>
        /// Adds an Redis implementation of identity stores.
        /// </summary>
        /// <param name="builder">The <see cref="DynamicAuthenticationBuilder" /> instance this method extends.</param>
        /// <param name="configure">Action to configure <see cref="ConfigurationOptions" /></param>
        /// <param name="database">(Optional) The redis database to use</param>
        /// <returns>
        /// The <see cref="DynamicAuthenticationBuilder" /> instance this method extends.
        /// </returns>
        public static DynamicAuthenticationBuilder AddRedisStore(this DynamicAuthenticationBuilder builder, Action<ConfigurationOptions> configure, int? database = null)
        {
            return builder.AddRedisStore<SchemeDefinition>(configure, database);
        }

        /// <summary>
        /// Adds an Redis implementation of identity stores.
        /// </summary>
        /// <typeparam name="TSchemeDefinition">The type of scheme definition.</typeparam>
        /// <param name="builder">The <see cref="DynamicAuthenticationBuilder" /> instance this method extends.</param>
        /// <param name="configure">Action to configure <see cref="ConfigurationOptions" /></param>
        /// <param name="database">(Optional) The redis database to use</param>
        /// <returns>
        /// The <see cref="DynamicAuthenticationBuilder" /> instance this method extends.
        /// </returns>
        public static DynamicAuthenticationBuilder AddRedisStore<TSchemeDefinition>(this DynamicAuthenticationBuilder builder, Action<ConfigurationOptions> configure, int? database = null)
            where TSchemeDefinition : SchemeDefinition, new()
        {
            var services = builder.Services;

            services.Configure(configure)
                .AddSingleton<IConnectionMultiplexer>(provider =>
                {
                    var options = provider.GetRequiredService<IOptions<ConfigurationOptions>>().Value;
                    var redisLogger = CreateLogger(provider);
                    return ConnectionMultiplexer.Connect(options, redisLogger);
                });

            return builder.AddRedisStore<TSchemeDefinition>(provider =>
            {
                var options = provider.GetRequiredService<IOptions<ConfigurationOptions>>().Value;
                var multiplexer = provider.GetRequiredService<IConnectionMultiplexer>();
                return multiplexer.GetDatabase(database ?? (options.DefaultDatabase ?? -1));
            });
        }

        /// <summary>
        /// Adds an Redis implementation of identity stores.
        /// </summary>
        /// <param name="builder">The <see cref="DynamicAuthenticationBuilder" /> instance this method extends.</param>
        /// <param name="configuration">The redis configuration string</param>
        /// <param name="database">(Optional) The redis database to use</param>
        /// <returns>
        /// The <see cref="DynamicAuthenticationBuilder" /> instance this method extends.
        /// </returns>
        public static DynamicAuthenticationBuilder AddRedisStore(this DynamicAuthenticationBuilder builder, string configuration, int? database = null)
        {
            return builder.AddRedisStore<SchemeDefinition>(configuration, database);
        }

        /// <summary>
        /// Adds an Redis implementation of identity stores.
        /// </summary>
        /// <typeparam name="TSchemeDefinition">The type of scheme definition.</typeparam>
        /// <param name="builder">The <see cref="DynamicAuthenticationBuilder" /> instance this method extends.</param>
        /// <param name="configuration">The redis configuration string</param>
        /// <param name="database">(Optional) The redis database to use</param>
        /// <returns>
        /// The <see cref="DynamicAuthenticationBuilder" /> instance this method extends.
        /// </returns>
        public static DynamicAuthenticationBuilder AddRedisStore<TSchemeDefinition>(this DynamicAuthenticationBuilder builder, string configuration, int? database = null)
            where TSchemeDefinition : SchemeDefinition, new()
        {
            var services = builder.Services;

            services.AddSingleton<IConnectionMultiplexer>(provider =>
            {
                var redisLogger = CreateLogger(provider);

                return ConnectionMultiplexer.Connect(configuration, redisLogger);
            });

            return builder
                .AddRedisStore<TSchemeDefinition>(provider =>
                {
                    var multiplexer = provider.GetRequiredService<IConnectionMultiplexer>();
                    return multiplexer.GetDatabase(database ?? -1);
                });
        }

        /// <summary>
        /// Adds the entity framework store.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="getDatabase">A function returning a <see cref="IDatabase"/></param>
        /// <returns>The <see cref="DynamicAuthenticationBuilder"/></returns>
        public static DynamicAuthenticationBuilder AddRedisStore(this DynamicAuthenticationBuilder builder, Func<IServiceProvider, IDatabase> getDatabase)
        {
            return builder.AddRedisStore<SchemeDefinition>(getDatabase);
        }

        /// <summary>
        /// Adds the redis store.
        /// </summary>
        /// <typeparam name="TSchemeDefinition">The type of scheme definition.</typeparam>
        /// <param name="builder">The builder.</param>
        /// <param name="getDatabase">A function returning a <see cref="IDatabase"/></param>
        /// <returns>The <see cref="DynamicAuthenticationBuilder"/></returns>
        public static DynamicAuthenticationBuilder AddRedisStore<TSchemeDefinition>(this DynamicAuthenticationBuilder builder, Func<IServiceProvider, IDatabase> getDatabase)
            where TSchemeDefinition : SchemeDefinition, new()
        {
            var services = builder.Services;

            services.TryAddTransient<ISchemeDefinitionSerializer<TSchemeDefinition>, SchemeDefinitionSerializer<TSchemeDefinition>>();
            services.TryAddTransient<DynamicProviderStore<TSchemeDefinition>>(provider =>
            {
                var db = getDatabase(provider);
                var serializer = provider.GetRequiredService<ISchemeDefinitionSerializer<TSchemeDefinition>>();
                var logger = provider.GetRequiredService<ILogger<DynamicProviderStore<TSchemeDefinition>>>();
                var eventHandler = provider.GetRequiredService<IDynamicProviderUpdatedEventHandler>();

                return new DynamicProviderStore<TSchemeDefinition>(db, serializer, eventHandler, logger);
            });
            services.TryAddTransient<IDynamicProviderStore>(sp => sp.GetRequiredService<DynamicProviderStore<TSchemeDefinition>>());
            services.TryAddTransient<IDynamicProviderMutationStore<TSchemeDefinition>>(sp => sp.GetRequiredService<DynamicProviderStore<TSchemeDefinition>>());
            services.TryAddTransient<IDynamicProviderUpdatedEventHandler, InProcDynamicProviderUpdatedEventHandler>();
            return builder;
        }
        private static RedisLogger CreateLogger(IServiceProvider provider)
        {
            var logger = provider.GetService<ILogger<RedisLogger>>();
            var redisLogger = logger != null ? new RedisLogger(logger) : null;
            return redisLogger;
        }
    }
}
