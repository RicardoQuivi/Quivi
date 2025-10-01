using Microsoft.AspNetCore.Identity;
using Quivi.Domain.Repositories.EntityFramework.Identity;
using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Abstractions.Events;
using Quivi.Infrastructure.Abstractions.Events.Data.Users;

namespace Quivi.Application.Commands.Users
{
    public class ForgotUserPasswordAsyncCommand : ICommand<Task>
    {
        public required string Email { get; init; }
        public required UserAppType UserType { get; init; }
    }

    public class ForgotUserPasswordAsyncCommandHandler : ICommandHandler<ForgotUserPasswordAsyncCommand, Task>
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IEventService eventService;

        public ForgotUserPasswordAsyncCommandHandler(UserManager<ApplicationUser> userManager,
                                                IEventService eventService)
        {
            this.userManager = userManager;
            this.eventService = eventService;
        }

        public async Task Handle(ForgotUserPasswordAsyncCommand command)
        {
            var user = await userManager.FindByEmailAsync(command.Email);
            if (user == null)
                return;

            var code = await userManager.GeneratePasswordResetTokenAsync(user);
            await eventService.Publish(new OnUserForgotPasswordEvent
            {
                Id = user.Id,
                Code = code,
                UserType = command.UserType,
            });
        }
    }
}