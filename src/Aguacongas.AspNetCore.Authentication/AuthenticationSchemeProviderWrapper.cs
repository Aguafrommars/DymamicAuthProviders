// Project: aguacongas/DymamicAuthProviders
// Copyright (c) 2021 @Olivier Lefebvre
using Microsoft.AspNetCore.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Aguacongas.AspNetCore.Authentication
{
    /// <summary>
    /// Dynamic scheme manager which not persist the changes.
    /// </summary>
    /// <typeparam name="ISchemeDefinition">The type of the scheme definition.</typeparam>
    public class AuthenticationSchemeProviderWrapper : IDynamicProviderHandlerTypeProvider
    {
        private readonly IAuthenticationSchemeProvider _schemeProvider;
        private readonly OptionsMonitorCacheWrapperFactory _wrapperFactory;

        /// <summary>
        /// Initializes a new instance of the <see
        /// cref="AuthenticationSchemeProviderWrapper{TSchemeDefinition}" /> class.
        /// </summary>
        /// <param name="schemeProvider">The scheme provider.</param>
        /// <param name="wrapperFactory">The wrapper factory.</param>
        /// <param name="managedTypes">The list of managed handlers types.</param>
        /// <exception cref="ArgumentNullException">schemeProvider or wrapperFactory or store</exception>
        public AuthenticationSchemeProviderWrapper(IAuthenticationSchemeProvider schemeProvider,
            OptionsMonitorCacheWrapperFactory wrapperFactory,
            IEnumerable<Type> managedTypes)
        {
            _schemeProvider = schemeProvider ?? throw new ArgumentNullException(nameof(schemeProvider));
            _wrapperFactory = wrapperFactory ?? throw new ArgumentNullException(nameof(wrapperFactory));
            managedHandlerTypes = managedTypes ?? throw new ArgumentNullException(nameof(managedTypes));
        }

        private readonly IEnumerable<Type> managedHandlerTypes;

        /// <summary>
        /// Gets the type of the managed handler.
        /// </summary>
        /// <returns>The type of the managed handler.</returns>
        public virtual IEnumerable<Type> GetManagedHandlerTypes()
        {
            return managedHandlerTypes;
        }

        /// <summary>
        /// Adds a scheme asynchronously.
        /// </summary>
        /// <param name="definition">The definition.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">definition</exception>
        public virtual async Task AddAsync(ISchemeDefinition definition, CancellationToken cancellationToken = default)
        {
            definition = definition ?? throw new ArgumentNullException(nameof(definition));

            var handlerType = definition.HandlerType;
            var optionsType = GetOptionsType(handlerType);
            var optionsMonitorCache = _wrapperFactory.Get(optionsType);

            var scheme = definition.Scheme;
            if (await _schemeProvider.GetSchemeAsync(scheme).ConfigureAwait(false) != null)
            {
                _schemeProvider.RemoveScheme(scheme);
                optionsMonitorCache.TryRemove(scheme);
            }

            _schemeProvider.AddScheme(new AuthenticationScheme(scheme, definition.DisplayName, handlerType));
            optionsMonitorCache.TryAdd(scheme, definition.Options);
        }

        public virtual async Task InitializeAsync(IEnumerable<ISchemeDefinition> schemeDefinitions, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            IList<AuthenticationScheme> toRemove = (await _schemeProvider.GetAllSchemesAsync()).Where(s => GetManagedHandlerTypes().Contains(s.HandlerType)).ToList();
            if (schemeDefinitions != null)
            {
                foreach (var definition in schemeDefinitions)
                {
                    if (GetManagedHandlerTypes().Contains(definition.HandlerType))
                    {
                        await AddAsync(definition, cancellationToken).ConfigureAwait(false);
                        toRemove = toRemove.Where(s => s.HandlerType != definition.HandlerType).ToList();
                    }
                }
            }

            if (toRemove.Count > 0)
            {
                foreach (var scheme in toRemove)
                {
                    Remove(scheme);
                }
            }
        }

        /// <summary>
        /// Removes the scheme asynchronous.
        /// </summary>
        /// <param name="name">The scheme.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">scheme cannot be null or white space.</exception>
        public virtual async Task RemoveAsync(string name, CancellationToken cancellationToken = default)
        {
            CheckName(name);

            Remove(await _schemeProvider.GetSchemeAsync(name).ConfigureAwait(false));
        }

        /// <summary>
        /// Updates the scheme asynchronous.
        /// </summary>
        /// <param name="definition">The definition.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">definition</exception>
        /// <exception cref="InvalidOperationException">The scheme does not exist.</exception>
        public virtual async Task UpdateAsync(ISchemeDefinition definition, CancellationToken cancellationToken = default)
        {
            definition = definition ?? throw new ArgumentNullException(nameof(definition));

            var handlerType = definition.HandlerType;
            var optionsType = GetOptionsType(handlerType);
            var scheme = definition.Scheme;

            if (await _schemeProvider.GetSchemeAsync(scheme).ConfigureAwait(false) == null)
            {
                throw new InvalidOperationException($"The scheme {scheme} does not exist.");
            }

            var optionsMonitorCache = _wrapperFactory.Get(optionsType);

            _schemeProvider.RemoveScheme(scheme);
            optionsMonitorCache.TryRemove(scheme);

            _schemeProvider.AddScheme(new AuthenticationScheme(scheme, definition.DisplayName, handlerType));
            optionsMonitorCache.TryAdd(scheme, definition.Options);
        }

        private static void CheckName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException($"{nameof(name)} cannot be null or white space.");
            }
        }

        private Type GetOptionsType(Type handlerType)
        {
            return handlerType.GetAuthenticationSchemeOptionsType();
        }

        private void Remove(AuthenticationScheme scheme)
        {
            if (scheme != null)
            {
                var optionsType = GetOptionsType(scheme.HandlerType);
                var optionsMonitorCache = _wrapperFactory.Get(optionsType);

                _schemeProvider.RemoveScheme(scheme.Name);
                optionsMonitorCache.TryRemove(scheme.Name);
            }
        }
    }
}
