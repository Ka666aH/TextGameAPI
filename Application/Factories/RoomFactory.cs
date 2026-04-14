using TextGame.Application.Interfaces.Factories;
using TextGame.Domain.GameObjects.Rooms;

namespace TextGame.Application.Factories
{
    public class RoomFactory : IRoomFactory
    {
        public StartRoom CreateStartRoom() => new StartRoom("СТАРТОВАЯ КОМАНТА", "В потолке дыра, через которую Вы сюда провалились.", 0);
        public EndRoom CreateEndRoom(int roomNumber) => new EndRoom("ВЫХОД", "Выход наружу. Свобода.", roomNumber);
        public EmptyRoom CreateEmptyRoom(int roomNumber) => new EmptyRoom("ПУСТАЯ КОМНАТА", "Ничего интересного.", roomNumber);
        public SmallRoom CreateSmallRoom(int roomNumber) => new SmallRoom("МАЛЕНЬКАЯ КОМНАТА", "Тесная комната. Внутри может быть предмет.", roomNumber);
        public BigRoom CreateBigRoom(int roomNumber) => new BigRoom("БОЛЬШАЯ КОМНАТА", "Просторная комната. Внутри может быть до трёх предметов.", roomNumber);
        public Shop CreateShopRoom(int roomNumber) => new Shop("МАГАЗИН", "Здесь мутный торгаш продаёт своё добро.", roomNumber);
    }
}