using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Reflection;

public class GameObjectStateContractResolver : DefaultContractResolver
{
    // Доступ к защищённым сеттерам (из PrivateSetterContractResolver)
    protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
    {
        var property = base.CreateProperty(member, memberSerialization);
        if (!property.Writable)
        {
            var propInfo = member as PropertyInfo;
            if (propInfo != null)
            {
                var setter = propInfo.GetSetMethod(true);
                if (setter != null)
                {
                    property.Writable = true;
                    property.ValueProvider = new ReflectionValueProvider(propInfo);
                }
            }
        }
        return property;
    }

    // Сортировка: "$type" первым
    protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
    {
        var properties = base.CreateProperties(type, memberSerialization);
        return properties
            .OrderBy(p => p.PropertyName != "$type")
            .ThenBy(p => p.Order ?? 0)
            .ToList();
    }
}