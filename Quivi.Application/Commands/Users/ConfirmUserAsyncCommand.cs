using Microsoft.AspNetCore.Identity;
using Quivi.Domain.Repositories.EntityFramework.Identity;
using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Abstractions.Events;
using Quivi.Infrastructure.Abstractions.Events.Data.Users;
using Quivi.Infrastructure.Roles;

namespace Quivi.Application.Commands.Users
{
    public class ConfirmUserAsyncCommand : ICommand<Task>
    {
        public required string Email { get; init; }
        public required string Code { get; init; }
        public required UserAppType UserType { get; init; }
        public required Action OnCodeExpired { get; init; }
    }

    public class ConfirmUserAsyncCommandHandler : ICommandHandler<ConfirmUserAsyncCommand, Task>
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IEventService eventService;

        public ConfirmUserAsyncCommandHandler(UserManager<ApplicationUser> userManager,
                                                IEventService eventService)
        {
            this.userManager = userManager;
            this.eventService = eventService;
        }

        public async Task Handle(ConfirmUserAsyncCommand command)
        {
            var user = await userManager.FindByEmailAsync(command.Email);
            if (user == null)
                return;

            if (user.EmailConfirmed)
                return;

            var result = await userManager.ConfirmEmailAsync(user, command.Code);
            if (result.Succeeded)
            {
                if (user.Id == 1)
                    await AddAdminRoles(user);
                return;
            }

            await GenerateNewToken(command, user);
        }

        private async Task GenerateNewToken(ConfirmUserAsyncCommand command, ApplicationUser user)
        {
            var code = await userManager.GenerateEmailConfirmationTokenAsync(user);
            await eventService.Publish(new OnUserEmailTokenGeneratedEvent
            {
                Id = user.Id,
                Code = code,
                Type = command.UserType,
            });
            command.OnCodeExpired();
        }

        private async Task AddAdminRoles(ApplicationUser user)
        {
            await userManager.AddToRoleAsync(user, QuiviRoles.SuperAdmin);
            await userManager.AddToRoleAsync(user, QuiviRoles.Admin);
        }
    }
}
