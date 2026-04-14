using TextGame.Domain.GameObjects.Enemies;
using TextGame.Domain.GameObjects.Items;
using TextGame.Domain.GameObjects.Items.Equipments;
using TextGame.Domain.GameObjects.Items.Equipments.Weapons;
using TextGame.Domain.GameObjects.Items.Equipments.Armors;
using TextGame.Domain.GameObjects.Items.Heal;
using TextGame.Domain.GameObjects.Rooms;
using TextGame.Presentation.DTO;
using TextGame.Domain.GameObjects;

namespace TextGame.Presentation.Mappers
{
    public static class GameObjectMapper
    {
        public static object ToDTO(GameObject gameObject)
        {
            return gameObject switch
            {
                Room room =>
                room switch
                {
                    StartRoom or EndRoom or Shop => new RoomWithoutEnemiesDTO(room.Number, room.Name!, room.Description!),
                    _ => new RoomDTO(room.Number, room.Name!, room.Description!, room.Enemies),
                },
                Enemy enemy => new EnemyDTO(enemy.Id, enemy.Name!, enemy.Description!, enemy.Health, enemy.Damage, enemy.DamageBlock),
                Item item =>
                item switch
                {
                    Chest chest => new ChestDTO(chest.Id, chest.Name!, chest.Description!),
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
                _ => new GameObjectDTO(gameObject.Name ?? "НЕИЗВЕСТНО", gameObject.Description ?? "НЕИЗВЕСТНО")
            };
        }
        public static List<object> ToDTO<T>(IEnumerable<T> gameObjects) where T : GameObject
        {
            List<object> gameObjectsDTO = new List<object>();
            foreach (T gameObject in gameObjects)
            {
                gameObjectsDTO.Add(ToDTO(gameObject));
            }
            return gameObjectsDTO;
        }
    }
}