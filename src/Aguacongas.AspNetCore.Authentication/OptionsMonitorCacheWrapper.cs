// Project: aguacongas/DymamicAuthProviders
// Copyright (c) 2021 @Olivier Lefebvre
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;

namespace Aguacongas.AspNetCore.Authentication
{
    /// <summary>
    /// Wrapper for <see cref="IOptionsMonitorCache{TOptions}" />.
    /// </summary>
    /// <remarks>For internal use, you should not use this class</remarks>
    /// <typeparam name="TOptions">The type of the options.</typeparam>
    /// <seealso cref="IOptionsMonitorCache{AuthenticationOptions}" />
    public class OptionsMonitorCacheWrapper<TOptions> : IOptionsMonitorCache<AuthenticationSchemeOptions>
        where TOptions : AuthenticationSchemeOptions
    {
        private readonly Action<string, TOptions> _onAdded;
        private readonly IOptionsMonitorCache<TOptions> _parent;
        private readonly IEnumerable<IPostConfigureOptions<TOptions>> _postConfigures;

        /// <summary>
        /// Initializes a new instance of the <see cref="OptionsMonitorCacheWrapper{TOptions}" /> class.
        /// </summary>
        /// <param name="parent">The parent.</param>
        /// <param name="postConfigures">The post configures actions list.</param>
        /// <param name="onAdded">The on added action.</param>
        /// <exception cref="ArgumentNullException">parent or postConfigures or onAdded</exception>
        /// <remarks>For internal user, you should not use this class</remarks>
        public OptionsMonitorCacheWrapper(IOptionsMonitorCache<TOptions> parent, IEnumerable<IPostConfigureOptions<TOptions>> postConfigures, Action<string, TOptions> onAdded)
        {
            _parent = parent ?? throw new ArgumentNullException(nameof(parent));
            _postConfigures = postConfigures ?? throw new ArgumentNullException(nameof(postConfigures));
            _onAdded = onAdded ?? throw new ArgumentNullException(nameof(onAdded));
        }

        /// <summary>
        /// Clears all options instances from the cache.
        /// </summary>
        public void Clear()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets a named options instance, or adds a new instance created with createOptions.
        /// </summary>
        /// <param name="name">The name of the options instance.</param>
        /// <param name="createOptions">The func used to create the new instance.</param>
        /// <returns>The options instance.</returns>
        /// <exception cref="NotImplementedException"></exception>
        /// <remarks>This method is not implemented.</remarks>
        public AuthenticationSchemeOptions GetOrAdd(string name, Func<AuthenticationSchemeOptions> createOptions)
        {
            return _parent.GetOrAdd(name, () => createOptions?.Invoke() as TOptions);
        }

        /// <summary>
        /// Tries to adds a new option to the cache, will return false if the name already exists.
        /// </summary>
        /// <param name="name">The name of the options instance.</param>
        /// <param name="options">The options instance.</param>
        /// <returns>Whether anything was added.</returns>
        public bool TryAdd(string name, AuthenticationSchemeOptions options)
        {
            var result = _parent.TryAdd(name, (TOptions)options);
            _onAdded.Invoke(name, options as TOptions);
            foreach (var postConfigure in _postConfigures)
            {
                postConfigure.PostConfigure(name, options as TOptions);
            }
            return result;
        }

        /// <summary>
        /// Try to remove an options instance.
        /// </summary>
        /// <param name="name">The name of the options instance.</param>
        /// <returns>Whether anything was removed.</returns>
        public bool TryRemove(string name)
        {
            return _parent.TryRemove(name);
        }
    }

    /// <summary>
    /// Factory to create wrapper for <see cref="IOptionsMonitorCache{TOptions}" />
    /// </summary>
    /// <remarks>For internal user, you should not use this class</remarks>
    public class OptionsMonitorCacheWrapperFactory
    {
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="OptionsMonitorCacheWrapperFactory" /> class.
        /// </summary>
        /// <param name="serviceProvider">The service provider.</param>
        /// <remarks>For internal user, you should not use this class</remarks>
        public OptionsMonitorCacheWrapperFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        /// <summary>
        /// Gets the <see cref="IOptionsMonitorCache{AuthenticationSchemeOptions}" /> wrapper for
        /// the option type
        /// </summary>
        /// <param name="optionsType">Type of the options.</param>
        /// <returns></returns>
        /// <remarks>For internal user, you should not use this class</remarks>
        public IOptionsMonitorCache<AuthenticationSchemeOptions> Get(Type optionsType)
        {
            var type = typeof(OptionsMonitorCacheWrapper<>).MakeGenericType(optionsType);
            var wrapper = _serviceProvider.GetRequiredService(type);
            return (IOptionsMonitorCache<AuthenticationSchemeOptions>)wrapper;
        }
    }
}
