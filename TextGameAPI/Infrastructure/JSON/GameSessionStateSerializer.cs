using Newtonsoft.Json;
using TextGame.Domain.Entities;

namespace TextGame.Infrastructure.JSON
{
    public static class GameSessionStateSerializer
    {
        public static string Serialize(GameSessionState state) =>
            JsonConvert.SerializeObject(state, Options.GameObjectsSerializeSettings);

        public static GameSessionState Deserialize(string json) =>
            JsonConvert.DeserializeObject<GameSessionState>(json, Options.GameObjectsSerializeSettings) ??
            new GameSessionState();
    }
}