namespace TextGame.Domain.Entities
{
    public class User
    {
        public Guid Id { get; private set; } = Guid.NewGuid();
        // init + null! в объявлении вместо required: атрибут [SetsRequiredMembers] не гасит
        // CS8618 на приватном EF-конструкторе, а без него required даёт CS9035 на местах
        // создания. Контракт «значение обязательно» уже несёт сам конструктор.
        public string Login { get; init; } = null!;
        public string HashedPass { get; init; } = null!;
        public User(string login, string hashedPass)
        {
            Login = login;
            HashedPass = hashedPass;
        }
        // EF Core материализует сущность через этот конструктор и заполняет свойства из БД рефлексией.
        private User() { }
    }
}
