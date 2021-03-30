// Project: aguacongas/DymamicAuthProviders
// Copyright (c) 2021 @Olivier Lefebvre
using Microsoft.AspNetCore.Authentication;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Aguacongas.AspNetCore.Authentication.Test
{
    public class AuthenticationSchemeProviderWrapperTest
    {
        [Fact]
        public async Task Assertions()
        {
            Assert.Throws<ArgumentNullException>(() => new AuthenticationSchemeProviderWrapper(null, null, null));
            var schemeProviderMock = new Mock<IAuthenticationSchemeProvider>().Object;
            Assert.Throws<ArgumentNullException>(() => new AuthenticationSchemeProviderWrapper(schemeProviderMock, null, null));
            var serviceProviderMock = new Mock<IServiceProvider>().Object;
            var factory = new OptionsMonitorCacheWrapperFactory(serviceProviderMock);
            Assert.Throws<ArgumentNullException>(() => new AuthenticationSchemeProviderWrapper(schemeProviderMock, factory, null));
            var storeMock = new Mock<IDynamicProviderStore>().Object;
            var manager = new AuthenticationSchemeProviderWrapper(schemeProviderMock, factory, new List<Type>());
            await Assert.ThrowsAsync<ArgumentNullException>(() => manager.AddAsync(null));
            await Assert.ThrowsAsync<ArgumentNullException>(() => manager.UpdateAsync(null));
            await Assert.ThrowsAsync<ArgumentException>(() => manager.RemoveAsync(null));
            await Assert.ThrowsAsync<ArgumentException>(() => manager.RemoveAsync(""));
            await Assert.ThrowsAsync<ArgumentException>(() => manager.RemoveAsync("  "));
        }
    }
    public class FakeSchemeDefinition : ISchemeDefinition
    {
        public string DisplayName { get; set; }
        public Type HandlerType { get; set; }
        public AuthenticationSchemeOptions Options { get; set; }
        public string Scheme { get; set; }
    }

}
