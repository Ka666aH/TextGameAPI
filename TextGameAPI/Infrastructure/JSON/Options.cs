using Newtonsoft.Json;
namespace TextGame.Infrastructure.JSON
{
    public static class Options
    {
        public static readonly JsonSerializerSettings GameObjectsSerializeSettings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.All,           
            TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple,
            MetadataPropertyHandling = MetadataPropertyHandling.ReadAhead,
            ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor,
            NullValueHandling = NullValueHandling.Ignore,
            ContractResolver = new GameObjectStateContractResolver(),   
        };
    }
}
