// Project: aguacongas/DymamicAuthProviders
// Copyright (c) 2021 @Olivier Lefebvre
using Microsoft.AspNetCore.Authentication;
using System;

namespace Aguacongas.AspNetCore.Authentication
{
    /// <summary>
    /// <see cref="AuthenticationSchemeOptions" /> serializer interface
    /// </summary>
    public interface IAuthenticationSchemeOptionsSerializer
    {
        /// <summary>
        /// Deserializes the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="optionsType">Type of the options.</param>
        /// <returns>An AuthenticationSchemeOptions instance.</returns>
        AuthenticationSchemeOptions DeserializeOptions(string value, Type optionsType);

        /// <summary>
        /// Deserializes the type.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        Type DeserializeType(string value);

        /// <summary>
        /// Serializes the specified options.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <param name="optionsType">Type of the options.</param>
        /// <returns>The serialized result.</returns>
        string SerializeOptions(AuthenticationSchemeOptions options, Type optionsType);

        /// <summary>
        /// Serializes the type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        string SerializeType(Type type);
    }
}
