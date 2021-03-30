// Project: aguacongas/DymamicAuthProviders
// Copyright (c) 2021 @Olivier Lefebvre
using Aguacongas.AspNetCore.Authentication.Persistence;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Logging;
using Moq;
using StackExchange.Redis;
using System;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Aguacongas.AspNetCore.Authentication.Redis.Test
{
    [Collection("Redis")]
    public class DynamicProviderStoreTest
    {
        private readonly ITestOutputHelper _output;
        private readonly TestFixture _fixture;

        public DynamicProviderStoreTest(ITestOutputHelper output, TestFixture fixture)
        {
            _output = output;
            _fixture = fixture;
        }

        [Fact]
        public async Task Assertions()
        {
            Assert.Throws<ArgumentNullException>(() => new DynamicProviderStore<SchemeDefinition>(null, null, null, null));
            var databaseMock = new Mock<IDatabase>().Object;
            Assert.Throws<ArgumentNullException>(() => new DynamicProviderStore<SchemeDefinition>(databaseMock, null, null, null));
            Assert.Throws<ArgumentNullException>(() => new DynamicProviderStore<SchemeDefinition>(databaseMock, new SchemeDefinitionSerializer<SchemeDefinition>(), null, null));
            var eventHandlerMock = new Mock<IDynamicProviderUpdatedEventHandler>().Object;
            Assert.Throws<ArgumentNullException>(() => new DynamicProviderStore<SchemeDefinition>(databaseMock, new SchemeDefinitionSerializer<SchemeDefinition>(), eventHandlerMock, null));
            var loggerMock = new Mock<ILogger<DynamicProviderStore<SchemeDefinition>>>().Object;
            var store = new DynamicProviderStore<SchemeDefinition>(databaseMock, new SchemeDefinitionSerializer<SchemeDefinition>(), eventHandlerMock, loggerMock);
            await Assert.ThrowsAsync<ArgumentNullException>(() => store.AddAsync(null));
            await Assert.ThrowsAsync<ArgumentNullException>(() => store.UpdateAsync(null));
            await Assert.ThrowsAsync<ArgumentNullException>(() => store.RemoveAsync(null));
            await Assert.ThrowsAsync<ArgumentException>(() => store.FindBySchemeAsync(null));
            await Assert.ThrowsAsync<ArgumentException>(() => store.FindBySchemeAsync(""));
            await Assert.ThrowsAsync<ArgumentException>(() => store.FindBySchemeAsync("  "));
        }

        [Fact]
        public async Task Concurrent_updates_should_fail()
        {
            var db = CreateDabase();
            var store = CreateStore(db);

            var scheme = Guid.NewGuid().ToString();
            await store.AddAsync(new SchemeDefinition
            {
                Scheme = scheme,
                DisplayName = "test",
                HandlerType = typeof(CookieAuthenticationHandler),
                Options = new CookieAuthenticationOptions()
            });

            var definition = await store.FindBySchemeAsync(scheme);
            var definition2 = await store.FindBySchemeAsync(scheme);

            await store.UpdateAsync(definition);

            await Assert.ThrowsAsync<InvalidOperationException>(() => store.UpdateAsync(definition2));
        }

        public DynamicProviderStore CreateStore(IDatabase database)
        {
            return new DynamicProviderStore(database, new SchemeDefinitionSerializer<SchemeDefinition>(), new Mock<IDynamicProviderUpdatedEventHandler>().Object, new Mock<ILogger<DynamicProviderStore>>().Object);
        }
        public IDatabase CreateDabase()
        {
            return _fixture.Database;
        }
    }
}
