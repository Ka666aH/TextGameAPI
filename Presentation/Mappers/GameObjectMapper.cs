using TextGame.Presentation.DTO;
using TextGame.Domain.GameText;
using TextGame.Domain.Entities.GameObjects;
using TextGame.Domain.Entities.GameObjects.Enemies;
using TextGame.Domain.Entities.GameObjects.Items;
using TextGame.Domain.Entities.GameObjects.Rooms;
using TextGame.Domain.Entities.GameObjects.Items.Equipments;
using TextGame.Domain.Entities.GameObjects.Items.Heals;
using TextGame.Domain.Entities.GameObjects.Items.Other;
using TextGame.Domain.Entities.GameObjects.Items.Equipments.Armors;
using TextGame.Domain.Entities.GameObjects.Items.Equipments.Weapons;

namespace TextGame.Presentation.Mappers
{
    public static class GameObjectMapper
    {
        public static GameObjectDTO ToDTO(GameObject gameObject)
        {
            return gameObject switch
            {
                Room room =>
                room switch
                {
                    StartRoom or EndRoom or Shop => new RoomWithoutEnemiesDTO(room.Id, room.Name!, room.Description!),
                    _ => new RoomWithEnemiesDTO(room.Id, room.Name!, room.Description!, ToDTO(room.Enemies).Cast<EnemyDTO>()),
                },
                Enemy enemy => new EnemyDTO(enemy.Id, enemy.Name!, enemy.Description!, enemy.Health, enemy.Damage, enemy.DamageBlock),
                Item item =>
                item switch
                {
                    Chest chest => new ChestDTO(chest.Id, chest.Name!, chest.Description!, chest.IsClosed),
                    Heal heal => new HealDTO(heal.Id, heal.Name!, heal.Description!, heal.Cost, heal.MaxHealthBoost, heal.CurrentHealthBoost),
                    Equipment equipment =>
                    equipment switch
                    {
                        Weapon weapon => new WeaponDTO(weapon.Id, weapon.Name!, weapon.Description!, weapon.Cost, weapon.Durability, weapon.Damage),
                        Armor armor => new ArmorDTO(armor.Id, armor.Name!, armor.Description!, armor.Cost, armor.Durability, armor.DamageBlock),

                        _ => new EquipmentDTO(equipment.Id, equipment.Name!, equipment.Description!, equipment.Cost, equipment.Durability),
                    },

                    _ => new ItemDTO(item.Id, item.Name!, item.Description!, item.Cost),
                },
                _ => new GameObjectDTO(gameObject.Name ?? GeneralLabeles.GameObjectDefaultName, gameObject.Description ?? GeneralLabeles.GameObjectDefaultDescription)
            };
        }
        public static List<GameObjectDTO> ToDTO<T>(IEnumerable<T> gameObjects) where T : GameObject 
            => [.. gameObjects.Select(ToDTO)];
        //{
        //    //List<GameObjectDTO> gameObjectsDTO = [];
        //    //foreach (T gameObject in gameObjects)
        //    //{
        //    //    gameObjectsDTO.Add(ToDTO(gameObject));
        //    //}
        //    //return gameObjectsDTO;
        //    return [.. gameObjects.Select(ToDTO)];
        //}
    }
}