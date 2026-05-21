using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Newtonsoft.Json;
using TextGame.Domain.Entities;

namespace TextGame.Infrastructure.JSON
{
    public class GameSessionStateConverter : ValueConverter<GameSessionState, string>
    {
        private static string SerializeWithLog(GameSessionState state)
        {
            var json = JsonConvert.SerializeObject(state, JSON.Options.GameObjectsSerializeSettings);
            Console.WriteLine($"[DB SAVE] JSON: {json}");
            return json;
        }

        private static GameSessionState DeserializeWithLog(string json)
        {
            Console.WriteLine($"[DB LOAD] JSON: {json}");
            return JsonConvert.DeserializeObject<GameSessionState>(
                       json, JSON.Options.GameObjectsSerializeSettings)
                   ?? new GameSessionState();
        }

        public GameSessionStateConverter()
            : base(
                v => SerializeWithLog(v),          // теперь это Expression
                v => DeserializeWithLog(v)         // теперь это Expression
            )
        { }
    }
}