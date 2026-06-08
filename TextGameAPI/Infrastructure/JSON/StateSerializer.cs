using Newtonsoft.Json;
using TextGame.Domain.Entities;

namespace TextGame.Infrastructure.JSON
{
    public static class StateSerializer
    {
        public static string Serialize(State state) =>
            JsonConvert.SerializeObject(state, Options.GameObjectsSerializeSettings);

        public static State Deserialize(string json) =>
            JsonConvert.DeserializeObject<State>(json, Options.GameObjectsSerializeSettings) ??
            new State();
    }
}