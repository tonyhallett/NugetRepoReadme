using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Build.Framework;

namespace NugetRepoReadme.MSBuildHelpers
{
    internal class MSBuildMetadataProvider : IMSBuildMetadataProvider
    {
        internal class RequiredPropertyInfo
        {
            private readonly PropertyInfo _propertyInfo;

            public RequiredPropertyInfo(PropertyInfo propertyInfo, bool isRequired = true)
            {
                _propertyInfo = propertyInfo;
                IsRequired = isRequired;
            }

            public string Name => _propertyInfo.Name;

            public bool IsRequired { get; }

            public void SetValue(object obj, object value) => _propertyInfo.SetValue(obj, value);
        }

        private static readonly Dictionary<Type, List<RequiredPropertyInfo>> s_properties = new Dictionary<Type, List<RequiredPropertyInfo>>();

        private static List<RequiredPropertyInfo> GetRequiredProperties(Type type)
        {
            if (!s_properties.TryGetValue(type, out List<RequiredPropertyInfo>? requiredProps))
            {
                IEnumerable<PropertyInfo> properties = type.GetProperties().Where(p => p.GetCustomAttribute<IgnoreMetadataAttribute>() == null && p.CanWrite);
                requiredProps = properties.Select(p =>
                    {
                        bool isRequired = p.GetCustomAttribute<RequiredMetadataAttribute>() != null;
                        return new RequiredPropertyInfo(p, isRequired);
                    }).ToList();

            }

            return requiredProps;
        }

        public T GetCustomMetadata<T>(ITaskItem item)
            where T : new()
        {
            T metadata = new T();

            foreach (RequiredPropertyInfo requiredPropertyInfo in GetRequiredProperties(typeof(T)))
            {
                string metadataValue = item.GetMetadata(requiredPropertyInfo.Name);

                requiredPropertyInfo.SetValue(metadata, metadataValue);

                if (requiredPropertyInfo.IsRequired && string.IsNullOrEmpty(metadataValue) && metadata is IRequiredMetadata requiredMetadata)
                {
                    requiredMetadata.AddMissingMetadataName(requiredPropertyInfo.Name);
                }
            }

            return metadata;
        }
    }
}
