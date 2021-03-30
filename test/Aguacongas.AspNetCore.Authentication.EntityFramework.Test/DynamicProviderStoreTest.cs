// Project: aguacongas/DymamicAuthProviders
// Copyright (c) 2021 @Olivier Lefebvre
using Aguacongas.AspNetCore.Authentication.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Aguacongas.AspNetCore.Authentication.EntityFramework.Test
{
    public class DynamicProviderStoreTest
    {
        [Fact]
        public async Task Assertions()
        {
            Assert.Throws<ArgumentNullException>(() => new DynamicProviderStore<SchemeDefinition>(null, null, null, null));
            var contextMock = new SchemeDbContext<SchemeDefinition>(new FakeDbContextOptions());
            Assert.Throws<ArgumentNullException>(() => new DynamicProviderStore<SchemeDefinition>(contextMock, null, null, null));
            Assert.Throws<ArgumentNullException>(() => new DynamicProviderStore<SchemeDefinition>(contextMock, new AuthenticationSchemeOptionsSerializer(), null, null));
            var eventHandlerMock = new Mock<IDynamicProviderUpdatedEventHandler>().Object;
            Assert.Throws<ArgumentNullException>(() => new DynamicProviderStore<SchemeDefinition>(contextMock, new AuthenticationSchemeOptionsSerializer(), eventHandlerMock, null));
            var loggerMock = new Mock<ILogger<DynamicProviderStore<SchemeDefinition>>>().Object;
            var store = new DynamicProviderStore<SchemeDefinition>(contextMock, new AuthenticationSchemeOptionsSerializer(), eventHandlerMock, loggerMock);
            await Assert.ThrowsAsync<ArgumentNullException>(() => store.AddAsync(null));
            await Assert.ThrowsAsync<ArgumentNullException>(() => store.UpdateAsync(null));
            await Assert.ThrowsAsync<ArgumentNullException>(() => store.RemoveAsync(null));
            await Assert.ThrowsAsync<ArgumentException>(() => store.FindBySchemeAsync(null));
            await Assert.ThrowsAsync<ArgumentException>(() => store.FindBySchemeAsync(""));
            await Assert.ThrowsAsync<ArgumentException>(() => store.FindBySchemeAsync("  "));
        }
    }

    class FakeDbContextOptions : DbContextOptions
    {
        public FakeDbContextOptions() : base(new Mock<IReadOnlyDictionary<Type, IDbContextOptionsExtension>>().Object)
        {
        }

        public override Type ContextType => typeof(SchemeDbContext<SchemeDefinition>);

        public override DbContextOptions WithExtension<TExtension>(TExtension extension)
        {
            throw new NotImplementedException();
        }
    }
}
