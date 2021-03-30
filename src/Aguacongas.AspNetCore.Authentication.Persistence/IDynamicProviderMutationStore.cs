// Project: aguacongas/DymamicAuthProviders
// Copyright (c) 2021 @Olivier Lefebvre
using System.Threading;
using System.Threading.Tasks;

namespace Aguacongas.AspNetCore.Authentication.Persistence
{
    public interface IDynamicProviderMutationStore<TSchemeDefinition> where TSchemeDefinition : ISchemeDefinition, new()
    {
        Task AddAsync(TSchemeDefinition definition, CancellationToken cancellationToken = default);
        Task<TSchemeDefinition> FindBySchemeAsync(string scheme, CancellationToken cancellationToken = default);
        Task RemoveAsync(TSchemeDefinition definition, CancellationToken cancellationToken = default);
        Task UpdateAsync(TSchemeDefinition definition, CancellationToken cancellationToken = default);
    }
}