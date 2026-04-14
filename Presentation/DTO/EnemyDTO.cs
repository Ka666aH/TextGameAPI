namespace TextGame.Presentation.DTO
{
    public record EnemyDTO(int? Id, string Name, string Description, int Health, int Damage, int DamageBlock);
}