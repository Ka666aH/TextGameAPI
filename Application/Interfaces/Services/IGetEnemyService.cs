using TextGame.Domain.GameObjects.Enemies;

namespace TextGame.Application.Interfaces.Services
{
    public interface IGetEnemyService
    {
        Enemy GetEnemyById();
        //List<Enemy> GetEnemies();
    }
}