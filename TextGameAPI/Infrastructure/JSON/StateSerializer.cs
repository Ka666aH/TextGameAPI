using Newtonsoft.Json;
using TextGame.Domain.Entities;
using TextGame.Domain.GameExceptions;

namespace TextGame.Infrastructure.JSON
{
    public static class StateSerializer
    {
        public static string Serialize(State state) =>
            JsonConvert.SerializeObject(state, Options.GameObjectsSerializeSettings);

        public static State Deserialize(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                throw new CorruptStateException();

            try
            {
                return JsonConvert.DeserializeObject<State>(json, Options.GameObjectsSerializeSettings)
                    ?? throw new CorruptStateException();
            }
            catch (JsonException exception)
            {
                throw new CorruptStateException(exception);
            }
        }
    }
}
