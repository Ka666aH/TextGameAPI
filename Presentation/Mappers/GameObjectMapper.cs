using TextGame.Domain.GameObjects.Enemies;
using TextGame.Domain.GameObjects.Items;
using TextGame.Domain.GameObjects.Items.Equipments;
using TextGame.Domain.GameObjects.Items.Equipments.Weapons;
using TextGame.Domain.GameObjects.Items.Equipments.Armors;
using TextGame.Domain.GameObjects.Items.Heal;
using TextGame.Domain.GameObjects.Rooms;
using TextGame.Presentation.DTO;
using TextGame.Domain.GameObjects;
using TextGame.Domain.GameObjects.Items.Other;
using TextGame.Domain.GameText;

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
                    StartRoom or EndRoom or Shop => new RoomWithoutEnemiesDTO(room.Number, room.Name!, room.Description!),
                    _ => new RoomWithEnemiesDTO(room.Number, room.Name!, room.Description!, ToDTO(room.Enemies).Cast<EnemyDTO>()),
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