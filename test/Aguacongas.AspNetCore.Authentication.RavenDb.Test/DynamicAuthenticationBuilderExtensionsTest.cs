using Aguacongas.AspNetCore.Authentication.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Raven.Client.Documents;
using Xunit;

namespace Aguacongas.AspNetCore.Authentication.RavenDb.Test
{
    public class DynamicAuthenticationBuilderExtensionsTest
    {
        [Fact]
        public void AddRavenDbStore_should_use_di_document_store()
        {
            var builder = new ServiceCollection().AddAuthentication().AddDynamicAuthentication().AddRavenDbStore();
            Assert.Contains(builder.Services, description => description.ServiceType == typeof(IDynamicProviderMutationStore<SchemeDefinition>));
        }

        [Fact]
        public void AddRavenDbStore_should_use_get_document_store_function()
        {
            var builder = new ServiceCollection().AddAuthentication().AddDynamicAuthentication().AddRavenDbStore(p => new DocumentStore());
            Assert.Contains(builder.Services, description => description.ServiceType == typeof(IDynamicProviderMutationStore<SchemeDefinition>));
        }
    }
}
