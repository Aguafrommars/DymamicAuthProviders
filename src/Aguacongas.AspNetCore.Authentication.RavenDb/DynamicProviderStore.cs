// Project: aguacongas/DymamicAuthProviders
// Copyright (c) 2021 @Olivier Lefebvre
using Aguacongas.AspNetCore.Authentication.Persistence;
using Microsoft.Extensions.Logging;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Aguacongas.AspNetCore.Authentication.RavenDb
{
    /// <summary>
    /// Implement a store for <see cref="IDynamicProviderMutationStore{TSchemeDefinition}"/> with EntityFramework.
    /// </summary>
    /// <seealso cref="IDynamicProviderStore" />
    public class DynamicProviderStore : DynamicProviderStore<SchemeDefinition>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicProviderStore"/> class.
        /// </summary>
        /// <param name="session">The document session.</param>
        /// <param name="authenticationSchemeOptionsSerializer">The authentication scheme options serializer.</param>
        /// <param name="providerUpdatedEventHandler">The event handler</param>
        /// <param name="logger">The logger.</param>
        public DynamicProviderStore(IAsyncDocumentSession session,
            IAuthenticationSchemeOptionsSerializer authenticationSchemeOptionsSerializer,
            IDynamicProviderUpdatedEventHandler providerUpdatedEventHandler,
            ILogger<DynamicProviderStore> logger) : base(session, authenticationSchemeOptionsSerializer, providerUpdatedEventHandler, logger)
        {
        }
    }

    /// <summary>
    /// Implement a store for <see cref="IDynamicProviderMutationStore{TSchemeDefinition}"/> with EntityFramework.
    /// </summary>
    /// <typeparam name="TSchemeDefinition">The type of the definition.</typeparam>
    public class DynamicProviderStore<TSchemeDefinition> : IDynamicProviderStore, IDynamicProviderMutationStore<TSchemeDefinition>
        where TSchemeDefinition : SchemeDefinition, new()
    {
        private readonly IAsyncDocumentSession _session;
        private readonly IAuthenticationSchemeOptionsSerializer _authenticationSchemeOptionsSerializer;
        private readonly IDynamicProviderUpdatedEventHandler _providerUpdatedEventHandler;
        private readonly ILogger<DynamicProviderStore<TSchemeDefinition>> _logger;
        private readonly string _entitybasePath;

        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicProviderStore{TSchemeDefinition}"/> class.
        /// </summary>
        /// <param name="session">The document session.</param>
        /// <param name="authenticationSchemeOptionsSerializer">The authentication scheme options serializer.</param>
        /// <param name="providerUpdatedEventHandler">The event handler</param>
        /// <param name="logger">The logger.</param>
        /// <exception cref="ArgumentNullException">
        /// context
        /// or
        /// authenticationSchemeOptionsSerializer
        /// or
        /// logger
        /// </exception>
        public DynamicProviderStore(IAsyncDocumentSession session,
            IAuthenticationSchemeOptionsSerializer authenticationSchemeOptionsSerializer,
            IDynamicProviderUpdatedEventHandler providerUpdatedEventHandler,
            ILogger<DynamicProviderStore<TSchemeDefinition>> logger)
        {
            _session = session ?? throw new ArgumentNullException(nameof(session));
            _authenticationSchemeOptionsSerializer = authenticationSchemeOptionsSerializer ?? throw new ArgumentNullException(nameof(authenticationSchemeOptionsSerializer));
            _providerUpdatedEventHandler = providerUpdatedEventHandler ?? throw new ArgumentNullException(nameof(providerUpdatedEventHandler));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _entitybasePath = typeof(TSchemeDefinition).Name.ToLower() + "/";
        }

        /// <summary>
        /// Adds a defnition asynchronously.
        /// </summary>
        /// <param name="definition">The definition.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">definition</exception>
        public virtual async Task AddAsync(TSchemeDefinition definition, CancellationToken cancellationToken = default)
        {
            definition = definition ?? throw new ArgumentNullException(nameof(definition));

            cancellationToken.ThrowIfCancellationRequested();

            var data = Serialize(definition);
            await _session.StoreAsync(data, $"{_entitybasePath}{definition.Scheme}", cancellationToken).ConfigureAwait(false);
            await _session.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            await _providerUpdatedEventHandler.HandleAsync(new DynamicProviderUpdatedEvent(DynamicProviderUpdateType.Added, definition)).ConfigureAwait(false);

            _logger.LogInformation("Scheme {scheme} added for {handlerType} with options: {options}", definition.Scheme, definition.HandlerType, data.SerializedOptions);
        }

        /// <summary>
        /// Gets the scheme definitions list.
        /// </summary>
        /// <returns>
        /// The scheme definitions list.
        /// </returns>
        public async IAsyncEnumerable<ISchemeDefinition> GetSchemeDefinitionsAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var items = await _session.Query<TSchemeDefinition>().ToListAsync(cancellationToken);
            foreach (var item in items)
            {
                yield return Deserialize(item);
            }
        }

        /// <summary>
        /// Removes a scheme definition asynchronous.
        /// </summary>
        /// <param name="definition">The definition.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">definition</exception>
        public virtual async Task RemoveAsync(TSchemeDefinition definition, CancellationToken cancellationToken = default)
        {
            definition = definition ?? throw new ArgumentNullException(nameof(definition));

            cancellationToken.ThrowIfCancellationRequested();

            var data = await _session.LoadAsync<TSchemeDefinition>($"{_entitybasePath}{definition.Scheme}", cancellationToken).ConfigureAwait(false);
            _session.Delete(data);
            await _session.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            await _providerUpdatedEventHandler.HandleAsync(new DynamicProviderUpdatedEvent(DynamicProviderUpdateType.Removed, definition)).ConfigureAwait(false);

            _logger.LogInformation("Scheme {scheme} removed", definition.Scheme);
        }

        /// <summary>
        /// Updates a scheme definition asynchronous.
        /// </summary>
        /// <param name="definition">The definition.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">definition</exception>
        public virtual async Task UpdateAsync(TSchemeDefinition definition, CancellationToken cancellationToken = default)
        {
            definition = definition ?? throw new ArgumentNullException(nameof(definition));

            cancellationToken.ThrowIfCancellationRequested();

            var serialized = Serialize(definition);

            var data = await _session.LoadAsync<TSchemeDefinition>($"{_entitybasePath}{definition.Scheme}", cancellationToken).ConfigureAwait(false);

            data.SerializedOptions = serialized.SerializedOptions;
            data.SerializedHandlerType = serialized.SerializedHandlerType;
            var handlerType = definition.HandlerType;
            var options = definition.Options;

            data.HandlerType = null;
            data.Options = null;

            await _session.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            await _providerUpdatedEventHandler.HandleAsync(new DynamicProviderUpdatedEvent(DynamicProviderUpdateType.Updated, definition)).ConfigureAwait(false);

            definition.HandlerType = handlerType;
            definition.Options = options;

            _logger.LogInformation("Scheme {scheme} updated for {handlerType} with options: {options}", definition.Scheme, definition.HandlerType, data.SerializedOptions);
        }

        /// <summary>
        /// Finds scheme definition by scheme asynchronous.
        /// </summary>
        /// <param name="scheme">The scheme.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// An instance of TSchemeDefinition or null.
        /// </returns>
        /// <exception cref="ArgumentException">Parameter {nameof(scheme)}</exception>
        public virtual async Task<TSchemeDefinition> FindBySchemeAsync(string scheme, CancellationToken cancellationToken = default)
        {
            CheckScheme(scheme);

            cancellationToken.ThrowIfCancellationRequested();
            var data = await _session.LoadAsync<TSchemeDefinition>($"{_entitybasePath}{scheme}", cancellationToken).ConfigureAwait(false);

            if (data != null)
            {
                return Deserialize(data);
            }

            return null;
        }

        private static void CheckScheme(string scheme)
        {
            if (string.IsNullOrWhiteSpace(scheme))
            {
                throw new ArgumentException($"Parameter {nameof(scheme)} cannor be null or empty");
            }
        }

        private TSchemeDefinition Serialize(TSchemeDefinition definition)
        {
            var data = (TSchemeDefinition)definition.Clone();
            data.SerializedHandlerType = _authenticationSchemeOptionsSerializer.SerializeType(definition.HandlerType);
            data.SerializedOptions = _authenticationSchemeOptionsSerializer.SerializeOptions(definition.Options, definition.HandlerType.GetAuthenticationSchemeOptionsType());
            data.HandlerType = null;
            data.Options = null;
            return data;
        }

        private TSchemeDefinition Deserialize(TSchemeDefinition data)
        {
            var handlerType = _authenticationSchemeOptionsSerializer.DeserializeType(data.SerializedHandlerType);
            data.HandlerType = handlerType;
            data.Options = _authenticationSchemeOptionsSerializer.DeserializeOptions(data.SerializedOptions, handlerType.GetAuthenticationSchemeOptionsType());
            return data;
        }
    }
}
