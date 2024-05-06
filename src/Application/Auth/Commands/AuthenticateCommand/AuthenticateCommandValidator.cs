using FluentValidation.Results;
using Microsoft.AspNetCore.Identity;
using MyAuthorizationDemo.Domain.Entities;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace RiskManagement.Application.Auth.Commands.AuthenticateCommand;

public class AuthenticateCommandValidator : AbstractValidator<MyAuthorizationDemo.Application.Auth.Commands.AuthenticateCommand.AuthenticateCommand>
{
    public AuthenticateCommandValidator(SignInManager<ApplicationUser> signInManager)
    {
        RuleFor(c => c.Username)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("ยังไม่ได้ระบุรหัสผู้ใช้");

        RuleFor(c => c)
            .CustomAsync(async (command, validatorContext, token) =>
            {
                if (command.Username != null && command.Password != null)
                {
                    SignInResult? result =
                        await signInManager.PasswordSignInAsync(command.Username, command.Password, false, false);

                    if (!result.Succeeded)
                    {
                        validatorContext.AddFailure(new ValidationFailure("Password", "รหัสผ่านไม่ถูกต้อง"));
                    }
                }
            });
    }
}
