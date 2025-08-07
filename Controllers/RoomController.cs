using Microsoft.AspNetCore.Mvc;
using System.Drawing;
namespace TextGame.Controllers
{

    [ApiController]
    [Route("room")]
    public class RoomController
    {
        private readonly IGameRepository gameRepository;

        public RoomController(IGameRepository gameRepository)
        {
            this.gameRepository = gameRepository;
        }
        [HttpPost("next")]
        public IResult GoNextRoom()
        {
            gameRepository.GoNextRoom();
            var room = gameRepository.GetCurrentRoom();
            return Results.Ok(new SuccessfulResponse(room));
        }
        [HttpPost("{roomId}")]
        public IResult GoRoom(int roomId)
        {
            gameRepository.GetRoomById(roomId);
            var room = gameRepository.GetCurrentRoom();
            return Results.Ok(new SuccessfulResponse(room));
        }
        [HttpPost("{roomId}/items")]
        public IResult Search(int roomId)
        {
            var items = gameRepository.Search(roomId);
            return Results.Ok(new SuccessfulResponse(items));
        }
        [HttpPost("{roomId}/items/take/{itemId}")]
        public IResult TakeItem(int roomId, int itemId)
        {
            gameRepository.TakeItem(roomId, itemId);
            return Results.Ok(new SuccessfulResponse(gameRepository.GetInventoryItem(itemId)));
        }
        [HttpPost("{roomId}/items/takeall")]
        public IResult TakeAllItems(int roomId)
        {
            var itemsIds = gameRepository.Search(roomId).Where(i => i.IsCarryable == true).Select(i => i.Id).ToList();
            gameRepository.TakeAllItems(roomId);
            return Results.Ok(new SuccessfulResponse(gameRepository.GetInventoryItems(itemsIds)));
        }
        #region CHEST
        [HttpPost("{roomId}/items/{chestId}/chest/open")]
        public IResult OpenChest(int roomId, int chestId)
        {
            gameRepository.OpenChest(roomId, chestId);
            return Results.Ok(new SuccessfulResponse(gameRepository.SearchChest(roomId, chestId)));
        }
        [HttpPost("{roomId}/items/{chestId}/chest/unlock")]
        public IResult UnlockChest(int roomId, int chestId)
        {
            gameRepository.UnlockChest(roomId, chestId);
            return Results.Ok(new SuccessfulResponse(gameRepository.ReturnChestDTO(roomId, chestId)));
        }
        [HttpPost("{roomId}/items/{chestId}/chest/search")]
        public IResult SearchChest(int roomId, int chestId)
        {
            return Results.Ok(new SuccessfulResponse(gameRepository.SearchChest(roomId, chestId)));
        }
        [HttpPost("{roomId}/items/{chestId}/chest/take/{itemId}")]
        public IResult TakeItemFromChest(int roomId, int chestId, int itemId)
        {
            gameRepository.TakeItemFromChest(roomId, chestId, itemId);
            return Results.Ok(new SuccessfulResponse(gameRepository.GetInventoryItem(itemId)));
        }
        [HttpPost("{roomId}/items/{chestId}/chest/takeall")]
        public IResult TakeAllItemsFromChest(int roomId, int chestId)
        {
            var itemsIds = gameRepository.SearchChest(roomId, chestId).Where(i => i.IsCarryable == true).Select(i => i.Id).ToList();
            gameRepository.TakeAllItemsFromChest(roomId, chestId);
            return Results.Ok(new SuccessfulResponse(gameRepository.GetInventoryItems(itemsIds)));
        }
        #endregion
    }
}