namespace Aguacongas.AspNetCore.Authentication.Persistence
{
    public class DynamicProviderUpdatedEvent
    {
        public DynamicProviderUpdatedEvent()
        {
        }

        public DynamicProviderUpdatedEvent(DynamicProviderUpdateType updateType, ISchemeDefinition schemeDefinition)
        {
            UpdateType = updateType;
            SchemeDefinition = schemeDefinition;
        }

        public DynamicProviderUpdateType UpdateType { get; }
        public ISchemeDefinition SchemeDefinition { get; }
    }
}
