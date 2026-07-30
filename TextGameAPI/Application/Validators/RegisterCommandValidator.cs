using FluentValidation;
using TextGame.Application.DTO;
using TextGame.Application.Interfaces.Repositories;
using TextGame.Domain.GameText;

namespace TextGame.Application.Validators
{
    public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
    {
        private const int _loginMinLength = 3;
        private const int _loginMaxLength = 100;
        public RegisterCommandValidator()
        {
            RuleFor(x => x.Login)
                .NotEmpty().WithMessage(ValidatorsText.LoginIsRequired)
                .MinimumLength(_loginMinLength).WithMessage(string.Format(ValidatorsText.LoginMinLength, _loginMinLength))
                .MaximumLength(_loginMaxLength).WithMessage(string.Format(ValidatorsText.LoginMaxLength, _loginMaxLength));

            RuleFor(x => x.Password)
                .NotEmpty()
                //.MinimumLength(6).WithMessage("Пароль должен быть не менее 6 символов")
                .Matches(@"[A-Z]").WithMessage("Пароль должен содержать хотя бы одну заглавную букву.")
                .Matches(@"[0-9]").WithMessage("Пароль должен содержать хотя бы одну цифру.");
        }
    }
}
