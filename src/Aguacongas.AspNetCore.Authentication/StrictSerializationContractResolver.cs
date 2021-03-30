// Project: aguacongas/DymamicAuthProviders
// Copyright (c) 2021 @Olivier Lefebvre
using Microsoft.AspNetCore.Authentication;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace Aguacongas.AspNetCore.Authentication
{
    public class StrictSerializationContractResolver : DefaultContractResolver
    {
        private static readonly Dictionary<Type, AuthenticationSchemeOptions> defaultOptionsObjects = new Dictionary<Type, AuthenticationSchemeOptions>();

        public static bool IsSupportedType(Type type)
        {
            if (type == null)
            {
                return false;
            }

            var typeInfo = type.GetTypeInfo();
            if (typeInfo.IsGenericType &&
                typeInfo.GetGenericArguments().Length == 1 && //exclude dictionary
                (typeInfo.GetGenericTypeDefinition() == typeof(Nullable<>) || typeof(IEnumerable).IsAssignableFrom(type)))
            {
                // nullable type, check if the nested type is simple.
                return IsSupportedType(typeInfo.GetGenericArguments()[0]);
            }

            if (typeInfo.GetElementType() != null)
            {
                return IsSupportedType(typeInfo.GetElementType());
            }

            return typeInfo.IsValueType
              || typeInfo.IsEnum
              || type.Equals(typeof(TimeSpan))
              || type.Equals(typeof(DateTime))
              || type.Equals(typeof(DateTimeOffset))
              || type.Equals(typeof(string))
              || type.Equals(typeof(decimal));
        }

        /// <summary>
        /// Creates a <see cref="T:Newtonsoft.Json.Serialization.JsonProperty" /> for the given <see
        /// cref="T:System.Reflection.MemberInfo" />.
        /// </summary>
        /// <param name="member">
        /// The member to create a <see cref="T:Newtonsoft.Json.Serialization.JsonProperty" /> for.
        /// </param>
        /// <param name="memberSerialization">
        /// The member's parent <see cref="T:Newtonsoft.Json.MemberSerialization" />.
        /// </param>
        /// <returns>
        /// A created <see cref="T:Newtonsoft.Json.Serialization.JsonProperty" /> for the given <see
        /// cref="T:System.Reflection.MemberInfo" />.
        /// </returns>
        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            var property = base.CreateProperty(member, memberSerialization);
            var propertyInfo = member as PropertyInfo;
            var propertyType = propertyInfo?.PropertyType;
            if (propertyType != null && IsSupportedType(propertyType) && typeof(AuthenticationSchemeOptions).IsAssignableFrom(member.DeclaringType) &&
                property.DefaultValue == null)
            {
                if (!defaultOptionsObjects.TryGetValue(member.DeclaringType, out var defaultOptions))
                {
                    try
                    {
                        defaultOptions = Activator.CreateInstance(member.DeclaringType) as AuthenticationSchemeOptions;
                        defaultOptionsObjects[member.DeclaringType] = defaultOptions;
                    }
                    catch
                    {
                        //ok, won't work for this type, continue as you were
                    }
                }

                if (defaultOptions != null)
                {
                    property.DefaultValue = propertyInfo.GetValue(defaultOptions);
                }
            }

            property.ShouldSerialize = instance => propertyType != null && IsSupportedType(propertyType);

            return property;
        }
    }
}
