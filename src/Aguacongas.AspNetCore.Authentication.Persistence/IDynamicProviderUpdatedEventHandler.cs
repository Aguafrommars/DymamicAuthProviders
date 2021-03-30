using System.Threading;
using System.Threading.Tasks;

namespace Aguacongas.AspNetCore.Authentication.Persistence
{
    public interface IDynamicProviderUpdatedEventHandler
    {
        Task HandleAsync(DynamicProviderUpdatedEvent @event, CancellationToken cancellationToken = default);
    }
}
