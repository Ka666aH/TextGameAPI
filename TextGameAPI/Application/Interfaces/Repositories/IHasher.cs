namespace TextGame.Application.Interfaces.Repositories
{
    public interface IHasher
    {
        string Hash(string password);
        bool Verify(string password, string hashedPassword);
    }
}