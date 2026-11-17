using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Reflection;

namespace TextGame.Infrastructure.JSON
{
    public class GameObjectStateContractResolver : DefaultContractResolver
    {
        // Доступ к защищённым сеттерам
        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            var property = base.CreateProperty(member, memberSerialization);
            if (!property.Writable)
            {
                if (member is PropertyInfo propInfo)
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
    }
}
