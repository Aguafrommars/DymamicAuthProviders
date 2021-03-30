// Project: aguacongas/DymamicAuthProviders
// Copyright (c) 2021 @Olivier Lefebvre
using Microsoft.AspNetCore.Authentication;
using System;

namespace Aguacongas.AspNetCore.Authentication
{
    /// <summary>
    /// Base class for scheme definition.
    /// </summary>
    public interface ISchemeDefinition
    {
        /// <summary>
        /// Gets or sets the display name.
        /// </summary>
        /// <value>The display name.</value>
        string DisplayName { get; set; }

        /// <summary>
        /// Gets or sets the type of the handler.
        /// </summary>
        /// <value>The type of the handler.</value>
        Type HandlerType { get; set; }

        /// <summary>
        /// Gets or sets the options.
        /// </summary>
        /// <value>The options.</value>
        AuthenticationSchemeOptions Options { get; set; }

        /// <summary>
        /// Gets or sets the scheme.
        /// </summary>
        /// <value>The scheme.</value>
        string Scheme { get; set; }
    }
}
