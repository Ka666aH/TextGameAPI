using Microsoft.AspNetCore.Mvc;
using System.Drawing;
namespace TextGame.Controllers
{

    [ApiController]
    [Route("room")]
    public class RoomController
    {
        private readonly IRoomControllerRepository RoomControllerRepository;

        public RoomController(IRoomControllerRepository roomControllerRepository)
        {
            RoomControllerRepository = roomControllerRepository;
        }
        [HttpPost("next")]
        public IResult GoNextRoom()
        {
            RoomControllerRepository.GoNextRoom();
            var room = RoomControllerRepository.GetCurrentRoom();
            return Results.Ok(new SuccessfulResponse(GameObjectMapper.ToDTO(room)));
        }
        [HttpPost("{roomId}")]
        public IResult GoRoom(int roomId)
        {
            RoomControllerRepository.GetRoomById(roomId);
            var room = RoomControllerRepository.GetCurrentRoom();
            return Results.Ok(new SuccessfulResponse(GameObjectMapper.ToDTO(room)));
        }
        [HttpPost("{roomId}/items")]
        public IResult Search(int roomId)
        {
            var items = RoomControllerRepository.Search(roomId);
            var itemsDTOs = items.Select(item => GameObjectMapper.ToDTO(item)).ToList();
            return Results.Ok(new SuccessfulResponse(itemsDTOs));
        }
        [HttpPost("{roomId}/items/{itemId}/take")]
        public IResult TakeItem(int roomId, int itemId)
        {
            RoomControllerRepository.TakeItem(roomId, itemId);
            return Results.Ok(new SuccessfulResponse(RoomControllerRepository.GetGameStats()));
        }
        [HttpPost("{roomId}/items/takeall")]
        public IResult TakeAllItems(int roomId)
        {
            RoomControllerRepository.TakeAllItems(roomId);
            return Results.Ok(new SuccessfulResponse(RoomControllerRepository.GetGameStats()));
        }
        #region CHEST
        [HttpPost("{roomId}/items/{chestId}/chest/open")]
        public IResult OpenChest(int roomId, int chestId)
        {
            RoomControllerRepository.OpenChest(roomId, chestId);
            var items = RoomControllerRepository.SearchChest(roomId, chestId);
            var itemsDTOs = items.Select(item => GameObjectMapper.ToDTO(item)).ToList();
            return Results.Ok(new SuccessfulResponse(itemsDTOs));
        }
        [HttpPost("{roomId}/items/{chestId}/chest/unlock")]
        public IResult UnlockChest(int roomId, int chestId)
        {
            RoomControllerRepository.UnlockChest(roomId, chestId);
            return Results.Ok(new SuccessfulResponse(RoomControllerRepository.ReturnChestDTO(roomId, chestId)));
        }
        [HttpPost("{roomId}/items/{chestId}/chest/items")]
        public IResult SearchChest(int roomId, int chestId)
        {
            var items = RoomControllerRepository.SearchChest(roomId, chestId);
            var itemsDTOs = items.Select(item => GameObjectMapper.ToDTO(item)).ToList();
            return Results.Ok(new SuccessfulResponse(itemsDTOs));
        }
        [HttpPost("{roomId}/items/{chestId}/chest/items/{itemId}/take")]
        public IResult TakeItemFromChest(int roomId, int chestId, int itemId)
        {
            RoomControllerRepository.TakeItemFromChest(roomId, chestId, itemId);
            return Results.Ok(new SuccessfulResponse(RoomControllerRepository.GetGameStats()));
        }
        [HttpPost("{roomId}/items/{chestId}/chest/items/takeall")]
        public IResult TakeAllItemsFromChest(int roomId, int chestId)
        {
            RoomControllerRepository.TakeAllItemsFromChest(roomId, chestId);
            return Results.Ok(new SuccessfulResponse(RoomControllerRepository.GetGameStats()));
        }
        #endregion
    }
}