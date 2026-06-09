using TextGame.Application.Interfaces.Services;
using TextGame.Domain.Entities.GameObjects.Rooms;
using TextGame.Domain.GameExceptions;

namespace TextGame.Application.GuardAttributes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public abstract class GuardAttribute : Attribute
    {
        public abstract void Validate(IStateService state);
    }

    public class RequireGameStartedAttribute : GuardAttribute
    {
        public override void Validate(IStateService state)
        {
            if (!state.IsGameStarted) throw new UnstartedGameException();
        }
    }

    public class RequireNotInBattleAttribute : GuardAttribute
    {
        public override void Validate(IStateService state)
        {
            if (state.IsInBattle) throw new InBattleException();
        }
    }

    public class RequireCurrentRoomIsSearchedAttribute : GuardAttribute
    {
        public override void Validate(IStateService state)
        {
            if (!state.CurrentRoom!.IsSearched) throw new UnsearchedRoomException();
        }
    }

    public class RequireNotShopAttribute : GuardAttribute
    {
        public override void Validate(IStateService state)
        {
            if (state.CurrentRoom is Shop) throw new ImpossibleStealException();
        }
    }

    public class RequireShopAttribute : GuardAttribute
    {
        public override void Validate(IStateService state)
        {
            if (state.CurrentRoom is not Shop) throw new NotShopException();
        }
    }
}
