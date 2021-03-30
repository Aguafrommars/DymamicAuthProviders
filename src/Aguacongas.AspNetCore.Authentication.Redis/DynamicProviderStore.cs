// Project: aguacongas/DymamicAuthProviders
// Copyright (c) 2021 @Olivier Lefebvre
using Aguacongas.AspNetCore.Authentication.Persistence;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Aguacongas.AspNetCore.Authentication.Redis
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
        /// <param name="db">The Redis db.</param>
        /// <param name="authenticationSchemeOptionsSerializer">The authentication scheme options serializer.</param>
        /// <param name="providerUpdatedEventHandler">The event handler</param>
        /// <param name="logger">The logger.</param>
        public DynamicProviderStore(IDatabase db, 
            ISchemeDefinitionSerializer<SchemeDefinition> authenticationSchemeOptionsSerializer,
            IDynamicProviderUpdatedEventHandler providerUpdatedEventHandler,
            ILogger<DynamicProviderStore> logger) : 
            base(db, authenticationSchemeOptionsSerializer, providerUpdatedEventHandler, logger)
        {
        }
    }

    /// <summary>
    /// Implement a store for <see cref="IDynamicProviderMutationStore{TSchemeDefinition}"/> with EntityFramework.
    /// </summary>
    /// <typeparam name="TSchemeDefinition">The type of the definition.</typeparam>
    /// <seealso cref="IDynamicProviderStore" />
    public class DynamicProviderStore<TSchemeDefinition> : IDynamicProviderStore, IDynamicProviderMutationStore<TSchemeDefinition>
        where TSchemeDefinition : SchemeDefinition, new()
    {
        /// <summary>
        /// The store key
        /// </summary>
        public const string StoreKey = "{schemes}";
        /// <summary>
        /// The concurency key
        /// </summary>
        public const string ConcurencyKey = "{schemes}-concurency";

        private readonly IDatabase _db;
        private readonly ISchemeDefinitionSerializer<TSchemeDefinition> _authenticationSchemeOptionsSerializer;
        private readonly IDynamicProviderUpdatedEventHandler _providerUpdatedEventHandler;
        private readonly ILogger<DynamicProviderStore<TSchemeDefinition>> _logger;


        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicProviderStore{TSchemeDefinition}" /> class.
        /// </summary>
        /// <param name="db">The Redis db.</param>
        /// <param name="authenticationSchemeOptionsSerializer">The authentication scheme options serializer.</param>
        /// <param name="providerUpdatedEventHandler">The event handler</param>
        /// <param name="logger">The logger.</param>
        /// <exception cref="ArgumentNullException">
        /// db
        /// or
        /// authenticationSchemeOptionsSerializer
        /// or
        /// logger
        /// </exception>
        /// <exception cref="System.ArgumentNullException">db
        /// or
        /// authenticationSchemeOptionsSerializer
        /// or
        /// logger</exception>
        public DynamicProviderStore(IDatabase db,
            ISchemeDefinitionSerializer<TSchemeDefinition> authenticationSchemeOptionsSerializer,
            IDynamicProviderUpdatedEventHandler providerUpdatedEventHandler,
            ILogger<DynamicProviderStore<TSchemeDefinition>> logger)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
            _authenticationSchemeOptionsSerializer = authenticationSchemeOptionsSerializer ?? throw new ArgumentNullException(nameof(authenticationSchemeOptionsSerializer));
            _providerUpdatedEventHandler = providerUpdatedEventHandler ?? throw new ArgumentNullException(nameof(providerUpdatedEventHandler));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Adds a defnition asynchronously.
        /// </summary>
        /// <param name="definition">The definition.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">definition</exception>
        public virtual async Task AddAsync(TSchemeDefinition definition, CancellationToken cancellationToken = default)
        {
            definition = definition ?? throw new ArgumentNullException(nameof(definition));

            var tran = _db.CreateTransaction();
            _ = tran.AddCondition(Condition.HashNotExists(StoreKey, definition.Scheme));
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            tran.HashSetAsync(StoreKey,
                definition.Scheme,
                _authenticationSchemeOptionsSerializer.Serialize(definition));
            tran.HashSetAsync(ConcurencyKey, definition.Scheme, 0);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

            var result = await tran.ExecuteAsync().ConfigureAwait(false);
            if (!result)
            {
                throw new InvalidOperationException($"The scheme {definition.Scheme} already exists");
            }

            await _providerUpdatedEventHandler.HandleAsync(new DynamicProviderUpdatedEvent(DynamicProviderUpdateType.Added, definition)).ConfigureAwait(false);
            _logger.LogInformation("Scheme {scheme} added for {handlerType} with options: {options}", definition.Scheme, definition.HandlerType, definition.SerializedOptions);
        }

        /// <summary>
        /// Gets the scheme definitions list.
        /// </summary>
        /// <value>
        /// The scheme definitions list.
        /// </value>
        public async IAsyncEnumerable<ISchemeDefinition> GetSchemeDefinitionsAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var items = await _db.HashGetAllAsync(StoreKey).ConfigureAwait(false);
            foreach (var item in items)
            {
                yield return _authenticationSchemeOptionsSerializer.Deserialize(item.Value);
            }
        }


        /// <summary>
        /// Finds scheme definition by scheme asynchronous.
        /// </summary>
        /// <param name="scheme">The scheme.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// An instance of TSchemeDefinition or null.
        /// </returns>
        /// <exception cref="System.ArgumentException">Parameter {nameof(scheme)}</exception>
        public virtual async Task<TSchemeDefinition> FindBySchemeAsync(string scheme, CancellationToken cancellationToken = default)
        {
            CheckScheme(scheme);

            var value = await _db.HashGetAsync(StoreKey, scheme).ConfigureAwait(false);
            if (value.HasValue)
            {
                var definition = _authenticationSchemeOptionsSerializer.Deserialize(value);
                definition.ConcurrencyStamp = (long)await _db.HashGetAsync(ConcurencyKey, scheme).ConfigureAwait(false);
                return definition;
            }

            return default;
        }


        /// <summary>
        /// Removes a scheme definition asynchronous.
        /// </summary>
        /// <param name="definition">The definition.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">definition</exception>
        public virtual async Task RemoveAsync(TSchemeDefinition definition, CancellationToken cancellationToken = default)
        {
            definition = definition ?? throw new ArgumentNullException(nameof(definition));

            var tran = _db.CreateTransaction();
            _ = tran.AddCondition(Condition.HashEqual(ConcurencyKey, definition.Scheme, definition.ConcurrencyStamp));
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            tran.HashDeleteAsync(StoreKey, definition.Scheme);
            tran.HashDeleteAsync(ConcurencyKey, definition.Scheme);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

            var result = await tran.ExecuteAsync().ConfigureAwait(false);
            if (!result)
            {
                throw new InvalidOperationException($"ConcurrencyStamp not match for scheme {definition.Scheme}");
            }

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

            definition.ConcurrencyStamp = 0;

            var tran = _db.CreateTransaction();
            _ = tran.AddCondition(Condition.HashEqual(ConcurencyKey, definition.Scheme, definition.ConcurrencyStamp));
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            tran.HashSetAsync(StoreKey, definition.Scheme, _authenticationSchemeOptionsSerializer.Serialize(definition));
            var concurency = tran.HashIncrementAsync(ConcurencyKey, definition.Scheme);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

            var result = await tran.ExecuteAsync().ConfigureAwait(false);
            if (!result)
            {
                throw new InvalidOperationException($"ConcurrencyStamp not match for scheme {definition.Scheme}");
            }

            definition.ConcurrencyStamp = concurency.Result;

            await _providerUpdatedEventHandler.HandleAsync(new DynamicProviderUpdatedEvent(DynamicProviderUpdateType.Updated, definition)).ConfigureAwait(false);
            _logger.LogInformation("Scheme {scheme} updated for {handlerType} with options: {options}", definition.Scheme, definition.HandlerType, definition.SerializedOptions);
        }

        private static void CheckScheme(string scheme)
        {
            if (string.IsNullOrWhiteSpace(scheme))
            {
                throw new ArgumentException($"Parameter {nameof(scheme)} cannor be null or empty");
            }
        }
    }
}
