namespace TextGame.Domain.GameExceptions
{
    public class ImpossibleStealException : GameException
    {
        public ImpossibleStealException() : base("CAN_NOT_STEAL", "Невозможно украсть. За Вами следят.") { }
    }
}