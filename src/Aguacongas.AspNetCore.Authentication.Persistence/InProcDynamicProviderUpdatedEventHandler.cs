using System;
using System.Threading;
using System.Threading.Tasks;

namespace Aguacongas.AspNetCore.Authentication.Persistence
{
    public class InProcDynamicProviderUpdatedEventHandler : IDynamicProviderUpdatedEventHandler
    {
        private readonly AuthenticationSchemeProviderWrapper _schemeProviderWrapper;

        public InProcDynamicProviderUpdatedEventHandler(AuthenticationSchemeProviderWrapper schemeProviderWrapper)
        {
            _schemeProviderWrapper = schemeProviderWrapper;
        }

        public Task HandleAsync(DynamicProviderUpdatedEvent @event, CancellationToken cancellationToken = default)
        {
            return @event?.UpdateType switch
            {
                DynamicProviderUpdateType.Updated => _schemeProviderWrapper.UpdateAsync(@event?.SchemeDefinition, cancellationToken),
                DynamicProviderUpdateType.Added => _schemeProviderWrapper.AddAsync(@event?.SchemeDefinition, cancellationToken),
                DynamicProviderUpdateType.Removed => _schemeProviderWrapper.RemoveAsync(@event?.SchemeDefinition?.Scheme, cancellationToken),
                _ => throw new ArgumentNullException(nameof(@event))
            };
        }
    }
}
