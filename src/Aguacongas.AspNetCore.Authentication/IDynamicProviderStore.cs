// Project: aguacongas/DymamicAuthProviders
// Copyright (c) 2021 @Olivier Lefebvre
using System.Collections.Generic;
using System.Threading;

namespace Aguacongas.AspNetCore.Authentication
{
    /// <summary>
    /// Interface for store use by <see
    /// cref="AuthenticationSchemeProviderWrapper{TSchemeDefinition}" />.
    /// </summary>
    /// <typeparam name="TSchemeDefinition">The type of the scheme definition.</typeparam>
    public interface IDynamicProviderStore
    {
        /// <summary>
        /// Gets the scheme definitions list.
        /// </summary>
        /// <returns>The scheme definitions list.</returns>
        IAsyncEnumerable<ISchemeDefinition> GetSchemeDefinitionsAsync(CancellationToken cancellationToken = default);
    }
}
