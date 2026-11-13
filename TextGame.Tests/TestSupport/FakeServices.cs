using TextGame.Application.DTO;
using TextGame.Application.Interfaces.Services;
using TextGame.Domain.Entities.GameObjects.Items;

namespace TextGame.Tests.TestSupport;

/// <summary>
/// Game info is only carried inside win/defeat exceptions; the orchestration tests
/// assert on the exception type, never on the payload.
/// </summary>
internal sealed class FakeGameInfoService : IGameInfoService
{
    public GameInfoDTO GetGameInfo() => null!;
}

/// <summary>
/// Item checking (uncarryable / steal rules) is irrelevant for the orchestration
/// scenarios under test here.
/// </summary>
internal sealed class FakeCheckItemService : ICheckItemService
{
    public List<Item> CheckedItems { get; } = [];

    public void CheckItem(Item item) => CheckedItems.Add(item);
}
