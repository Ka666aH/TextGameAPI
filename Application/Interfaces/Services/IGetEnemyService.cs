using TextGame.Domain.GameObjects.Enemies;

namespace TextGame.Application.Interfaces.Services
{
    public interface IGetEnemyService
    {
        Enemy GetEnemy();
        //List<Enemy> GetEnemies();
    }
}