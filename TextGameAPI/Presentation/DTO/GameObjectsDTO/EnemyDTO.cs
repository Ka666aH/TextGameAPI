namespace TextGame.Presentation.DTO.GameObjectsDTO
{
    public record EnemyDTO(int Id, string Name, string Description, int Health, int Damage, int DamageBlock)
        : GameObjectDTO(Name, Description);
}