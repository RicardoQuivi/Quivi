using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Quivi.Domain.Repositories.EntityFramework.Identity;
using Quivi.Infrastructure.Abstractions.Cqrs;

namespace Quivi.Application.Commands.Users
{
    public class ResetUserPasswordAsyncCommand : ICommand<Task>
    {
        public required string Email { get; init; }
        public required string Password { get; init; }
        public required string Code { get; init; }

        public required Action<PasswordOptions> OnInvalidPassword { get; init; }
        public required Action OnCodeExpired { get; init; }
    }

    public class ResetUserPasswordAsyncCommandHandler : ICommandHandler<ResetUserPasswordAsyncCommand, Task>
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IOptions<IdentityOptions> identityOptions;

        public ResetUserPasswordAsyncCommandHandler(UserManager<ApplicationUser> userManager,
                                                    IOptions<IdentityOptions> identityOptions)
        {
            this.userManager = userManager;
            this.identityOptions = identityOptions;
        }

        public async Task Handle(ResetUserPasswordAsyncCommand command)
        {
            var user = await userManager.FindByEmailAsync(command.Email);
            if (user == null)
                return;

            var changeResult = await userManager.ResetPasswordAsync(user, command.Code, command.Password);
            if(changeResult.Succeeded == false)
            {
                if (changeResult.Errors.Any(e => e.Code == "InvalidToken"))
                {
                    command.OnCodeExpired();
                }
                else
                {
                    command.OnInvalidPassword(identityOptions.Value.Password);
                }
                return;
            }
        }
    }
}
