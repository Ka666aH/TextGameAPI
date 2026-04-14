namespace TextGame.Domain.GameExceptions
{
    public class InBattleException : GameException
    {
        public InBattleException() : base("IN_BATTLE", "В бою!") { }
    }
}