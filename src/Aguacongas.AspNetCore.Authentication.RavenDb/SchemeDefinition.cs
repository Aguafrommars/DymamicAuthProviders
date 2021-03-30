// Project: aguacongas/DymamicAuthProviders
// Copyright (c) 2021 @Olivier Lefebvre
using Microsoft.AspNetCore.Authentication;
using System;

namespace Aguacongas.AspNetCore.Authentication.RavenDb
{
    /// <summary>
    /// Scheme definition for entity framework store
    /// </summary>
    /// <seealso cref="ISchemeDefinition" />
    public class SchemeDefinition : ISchemeDefinition, ICloneable
    {
        /// <summary>
        /// Gets or sets the display name.
        /// </summary>
        /// <value>
        /// The display name.
        /// </value>
        public string DisplayName { get; set; }

        /// <summary>
        /// Gets or sets the type of the handler.
        /// </summary>
        /// <value>
        /// The type of the handler.
        /// </value>
        public Type HandlerType { get; set; }

        /// <summary>
        /// Gets or sets the options.
        /// </summary>
        /// <value>
        /// The options.
        /// </value>
        public AuthenticationSchemeOptions Options { get; set; }

        /// <summary>
        /// Gets or sets the scheme.
        /// </summary>
        /// <value>
        /// The scheme.
        /// </value>
        public string Scheme { get; set; }

        /// <summary>
        /// Gets or sets the serialized handler type.
        /// </summary>
        /// <value>
        /// The name of the serialized handler type.
        /// </value>
        public string SerializedHandlerType { get; set; }

        /// <summary>
        /// Gets or sets the serialized options.
        /// </summary>
        /// <value>
        /// The serialized options.
        /// </value>
        public string SerializedOptions { get; set; }

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>
        /// A new object that is a copy of this instance.
        /// </returns>
        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}