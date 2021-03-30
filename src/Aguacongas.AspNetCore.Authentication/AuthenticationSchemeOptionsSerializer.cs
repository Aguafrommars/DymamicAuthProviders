// Project: aguacongas/DymamicAuthProviders
// Copyright (c) 2021 @Olivier Lefebvre
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyModel;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Reflection;

namespace Aguacongas.AspNetCore.Authentication
{
    /// <summary>
    /// Manage <see cref="AuthenticationSchemeOptions" /> serialization.
    /// </summary>
    /// <seealso cref="IAuthenticationSchemeOptionsSerializer" />
    public class AuthenticationSchemeOptionsSerializer : IAuthenticationSchemeOptionsSerializer
    {
        /// <summary>
        /// Gets or sets the json serializer settings.
        /// </summary>
        /// <value>The json serializer settings.</value>
        public static JsonSerializerSettings JsonSerializerSettings { get; } = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            Formatting = Formatting.None,
            DefaultValueHandling = DefaultValueHandling.Ignore,
            ContractResolver = new StrictSerializationContractResolver()
        };

        /// <summary>
        /// Deserializes the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="optionsType">Type of the options.</param>
        /// <returns>An AuthenticationSchemeOptions instance.</returns>
        public virtual AuthenticationSchemeOptions DeserializeOptions(string value, Type optionsType)
        {
            return Deserialize(value, optionsType) as AuthenticationSchemeOptions;
        }

        /// <summary>
        /// Deserializes the type.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public virtual Type DeserializeType(string value)
        {
            return GetType(Deserialize(value, typeof(TypeDefinition)) as TypeDefinition);
        }

        /// <summary>
        /// Serializes the specified options.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <param name="optionsType">Type of the options.</param>
        /// <returns>The serialized result.</returns>
        public virtual string SerializeOptions(AuthenticationSchemeOptions options, Type optionsType)
        {
            return Serialize(options, optionsType);
        }

        /// <summary>
        /// Serializes the type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        public virtual string SerializeType(Type type)
        {
            return Serialize(CreateTypeDefinition(type), typeof(Type));
        }

        /// <summary>
        /// Deserializes the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        protected virtual object Deserialize(string value, Type type)
        {
            return JsonConvert.DeserializeObject(value, type, JsonSerializerSettings);
        }

        /// <summary>
        /// Serializes the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        protected virtual string Serialize(object value, Type type)
        {
            return JsonConvert.SerializeObject(value, type, JsonSerializerSettings);
        }

        private TypeDefinition CreateTypeDefinition(Type type)
        {
            if (type.IsGenericType)
            {
                var genericType = type.GetGenericTypeDefinition();
                return new TypeDefinition
                {
                    Name = genericType.FullName,
                    ArgsTypeDefinition = type
                       .GetGenericArguments().Select(CreateTypeDefinition).ToArray()
                };
            }

            return new TypeDefinition
            {
                Name = type.FullName
            };
        }

        private Type GetType(string typeName)
        {
            var platform = Environment.OSVersion.Platform.ToString();
            var runtimeAssemblyNames = DependencyContext.Default.GetRuntimeAssemblyNames(platform);

            return runtimeAssemblyNames
                    .Select(Assembly.Load)
                    .Select(a => a.GetType(typeName))
                    .First(t => t != null);
        }

        private Type GetType(TypeDefinition typeDefinition)
        {
            if (typeDefinition.ArgsTypeDefinition != null)
            {
                var type = GetType(typeDefinition.Name);
                var argsTypes = typeDefinition.ArgsTypeDefinition.Select(GetType).ToArray();

                return type.MakeGenericType(argsTypes);
            }
            return GetType(typeDefinition.Name);
        }

        private class TypeDefinition
        {
            public TypeDefinition[] ArgsTypeDefinition { get; set; }
            public string Name { get; set; }
        }
    }
}
