namespace TextGame.Domain.GameText
{
    public static class ValidatorsText
    {
        public const string LoginIsRequired = "Логин обязателен.";
        public const string LoginMinLength = "Логин должен быть не менее {min} символов.";
        public const string LoginMaxLength = "Логин должен быть не более {max} символов.";

        public const string LoginAlreadyExist = "Пользователь с таким логином уже существует.";

        public const string PasswordShouldContainUpperCase = "Пароль должен содержать хотя бы одну заглавную латинскую букву.";
        public const string PasswordShouldContainDigit = "Пароль должен содержать хотя бы одну цифру.";
    }
}