using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TextGame.Domain.Entities.GameObjects.Enemies;
using TextGame.Domain.Entities.GameObjects.Items;
using TextGame.Domain.Entities.GameObjects.Rooms;

public class RoomConverter : JsonConverter<Room>
{
    public override Room? ReadJson(JsonReader reader, Type objectType, Room? existingValue,
        bool hasExistingValue, JsonSerializer serializer)
    {
        var jsonObject = JObject.Load(reader);
        var typeName = jsonObject["$type"]?.Value<string>();
        if (string.IsNullOrEmpty(typeName))
            throw new JsonSerializationException("$type not found");

        var type = Type.GetType(typeName);
        if (type == null || !typeof(Room).IsAssignableFrom(type))
            throw new JsonSerializationException($"Invalid room type: {typeName}");

        var room = (Room?)Activator.CreateInstance(type, true);
        if (room == null)
            throw new JsonSerializationException($"Cannot create instance of {type}");

        serializer.Populate(jsonObject.CreateReader(), room);
        return room;
    }

    public override void WriteJson(JsonWriter writer, Room? value, JsonSerializer serializer)
    {
        // Используем стандартную сериализацию (с $type)
        //serializer.Serialize(writer, value);
    }
}
public class ItemConverter : JsonConverter<Item>
{
    public override Item? ReadJson(JsonReader reader, Type objectType, Item? existingValue,
        bool hasExistingValue, JsonSerializer serializer)
    {
        var jsonObject = JObject.Load(reader);
        var typeName = jsonObject["$type"]?.Value<string>();
        if (string.IsNullOrEmpty(typeName))
            throw new JsonSerializationException("$type not found for Item");

        var type = Type.GetType(typeName);
        if (type == null || !typeof(Item).IsAssignableFrom(type))
            throw new JsonSerializationException($"Invalid item type: {typeName}");

        var item = (Item?)Activator.CreateInstance(type, true);
        if (item == null)
            throw new JsonSerializationException($"Cannot create instance of {type}");

        serializer.Populate(jsonObject.CreateReader(), item);
        return item;
    }

    public override void WriteJson(JsonWriter writer, Item? value, JsonSerializer serializer)
    {
        //serializer.Serialize(writer, value);
    }
}
public class EnemyConverter : JsonConverter<Enemy>
{
    public override Enemy? ReadJson(JsonReader reader, Type objectType, Enemy? existingValue,
        bool hasExistingValue, JsonSerializer serializer)
    {
        var jsonObject = JObject.Load(reader);
        var typeName = jsonObject["$type"]?.Value<string>();
        if (string.IsNullOrEmpty(typeName))
            throw new JsonSerializationException("$type not found for Enemy");

        var type = Type.GetType(typeName);
        if (type == null || !typeof(Enemy).IsAssignableFrom(type))
            throw new JsonSerializationException($"Invalid enemy type: {typeName}");

        var enemy = (Enemy?)Activator.CreateInstance(type, true);
        if (enemy == null)
            throw new JsonSerializationException($"Cannot create instance of {type}");

        serializer.Populate(jsonObject.CreateReader(), enemy);
        return enemy;
    }

    public override void WriteJson(JsonWriter writer, Enemy? value, JsonSerializer serializer)
    {
        //serializer.Serialize(writer, value);
    }
}