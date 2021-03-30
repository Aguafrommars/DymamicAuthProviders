// Project: aguacongas/DymamicAuthProviders
// Copyright (c) 2021 @Olivier Lefebvre
using System;
using System.Collections.Generic;

namespace Aguacongas.AspNetCore.Authentication
{
    public interface IDynamicProviderHandlerTypeProvider
    {
        IEnumerable<Type> GetManagedHandlerTypes();
    }
}