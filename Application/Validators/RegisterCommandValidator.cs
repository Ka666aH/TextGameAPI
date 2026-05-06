using FluentValidation;
using TextGame.Application.DTO;
using TextGame.Application.Interfaces.Repositories;

namespace TextGame.Application.Validators
{
    public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
    {
        public RegisterCommandValidator(IUserRepository userRepository)
        {
            RuleFor(x => x.Login)
                .NotEmpty().WithMessage("Логин обязателен.")
                //.MinimumLength(3).WithMessage("Логин должен быть не менее 3 символов.")
                .MaximumLength(100)
                .MustAsync(async (login, ct) =>
                    await userRepository.GetAsync(login, ct) == null)
                .WithMessage("Пользователь с таким логином уже существует.");

            RuleFor(x => x.Password)
                .NotEmpty()
                //.MinimumLength(6).WithMessage("Пароль должен быть не менее 6 символов")
                .Matches(@"[A-Z]").WithMessage("Пароль должен содержать хотя бы одну заглавную букву")
                .Matches(@"[0-9]").WithMessage("Пароль должен содержать хотя бы одну цифру");

            RuleFor(x => x.DeviceName)
                .NotEmpty()
                .MaximumLength(64);
        }
    }
}
