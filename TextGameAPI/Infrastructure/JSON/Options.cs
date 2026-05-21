using Newtonsoft.Json;
namespace TextGame.Infrastructure.JSON
{
    public static class Options
    {
        public static readonly JsonSerializerSettings GameObjectsSerializeSettings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.All,           
            TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Full,
            MetadataPropertyHandling = MetadataPropertyHandling.ReadAhead,
            SerializationBinder = new GameTypeBinder(),
            ContractResolver = new GameObjectStateContractResolver(),   
            ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor,
            NullValueHandling = NullValueHandling.Ignore,
            Formatting = Formatting.None,
            //Converters = 
            //[
            //    new RoomConverter(),
            //    new ItemConverter(),
            //    new EnemyConverter()
            //]
        };
    }
}
