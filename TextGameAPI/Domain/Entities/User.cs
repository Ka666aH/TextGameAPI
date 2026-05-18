namespace TextGame.Domain.Entities
{
    public class User
    {
        public Guid Id { get; private set; } = Guid.NewGuid();
        public string Login { get; private set; }
        public string HashedPass { get; private set; }
        private User() { }
        public User(string login, string hashedPass)
        {
            Login = login;
            HashedPass = hashedPass;
        }
    }
}