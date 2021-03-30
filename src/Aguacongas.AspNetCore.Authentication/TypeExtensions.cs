// Project: aguacongas/DymamicAuthProviders
// Copyright (c) 2021 @Olivier Lefebvre
using Microsoft.AspNetCore.Authentication;

namespace System
{
    /// <summary>
    /// Type extensions
    /// </summary>
    public static class TypeExtensions
    {
        /// <summary>
        /// Gets the type of the authentication scheme options.
        /// </summary>
        /// <param name="handlerType">Type of the handler.</param>
        /// <returns>The type of the authentication scheme options.</returns>
        /// <exception cref="ArgumentException">
        /// Parameter handlerType should be a <see
        /// cref="AuthenticationHandler{AuthenticationSchemeOptions}" />}.
        /// </exception>
        public static Type GetAuthenticationSchemeOptionsType(this Type handlerType)
        {
            if (handlerType.GetInterface(nameof(IAuthenticationHandler)) == null)
            {
                throw new ArgumentException($"Parameter {nameof(handlerType)} should be a {nameof(AuthenticationHandler<AuthenticationSchemeOptions>)}");
            }

            var genericTypeArguments = GetGenericTypeArguments(handlerType);
            var optionsType = genericTypeArguments[0];
            return optionsType;
        }

        private static Type[] GetGenericTypeArguments(Type handlerType)
        {
            if (handlerType.GenericTypeArguments.Length == 1)
            {
                return handlerType.GenericTypeArguments;
            }

            if (handlerType.BaseType == null || handlerType.BaseType == typeof(object))
            {
                throw new ArgumentException($"Parameter {nameof(handlerType)} should be a {nameof(AuthenticationHandler<AuthenticationSchemeOptions>)}");
            }

            return GetGenericTypeArguments(handlerType.BaseType);
        }
    }
}
