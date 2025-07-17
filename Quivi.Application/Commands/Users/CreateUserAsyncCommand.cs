using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Quivi.Domain.Entities.Financing;
using Quivi.Domain.Repositories.EntityFramework.Identity;
using Quivi.Infrastructure.Abstractions;
using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Abstractions.Events;
using Quivi.Infrastructure.Abstractions.Events.Data.Users;

namespace Quivi.Application.Commands.Users
{
    public class CreateUserAsyncCommand : ICommand<Task<ApplicationUser?>>
    {
        public class CreatePersonData
        {
            public string? PhoneNumber { get; init; }
            public string? VatNumber { get; init; }
        }

        public required string Email { get; init; }
        public string? Password { get; init; }
        public CreatePersonData? PersonData { get; init; }


        public required Action OnInvadidEmail { get; init; }
        public required Action OnEmailAlreadyExists { get; init; }
        public required Action<PasswordOptions> OnInvalidPassword { get; init; }
    }

    public class CreateUserAsyncCommandHandler : ICommandHandler<CreateUserAsyncCommand, Task<ApplicationUser?>>
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IOptions<IdentityOptions> identityOptions;
        private readonly IDateTimeProvider dateTimeProvider;
        private readonly IEventService eventService;

        public CreateUserAsyncCommandHandler(UserManager<ApplicationUser> userManager,
                                                IOptions<IdentityOptions> identityOptions,
                                                IDateTimeProvider dateTimeProvider,
                                                IEventService eventService)
        {
            this.userManager = userManager;
            this.identityOptions = identityOptions;
            this.dateTimeProvider = dateTimeProvider;
            this.eventService = eventService;
        }

        public async Task<ApplicationUser?> Handle(CreateUserAsyncCommand command)
        {
            if (string.IsNullOrWhiteSpace(command.Email))
            {
                command.OnInvadidEmail();
                return null;
            }

            var applicationUser = await CreateUser(command);
            if (applicationUser == null)
                return null;

            if (applicationUser.EmailConfirmed)
                return applicationUser;

            var code = await userManager.GenerateEmailConfirmationTokenAsync(applicationUser);
            await eventService.Publish(new OnUserEmailTokenGeneratedEvent
            {
                Id = applicationUser.Id,
                Code = code,
            });
            return applicationUser;
        }

        private async Task<ApplicationUser?> CreateUser(CreateUserAsyncCommand command)
        {
            string emailAddress = command.Email.Trim();

            ApplicationUser? applicationUser = await userManager.FindByEmailAsync(emailAddress);
            if (applicationUser != null)
            {
                if (!applicationUser.EmailConfirmed && string.IsNullOrWhiteSpace(command.Password) == false)
                {
                    var resetToken = await userManager.GeneratePasswordResetTokenAsync(applicationUser);
                    var passwordChangeResult = await userManager.ResetPasswordAsync(applicationUser, resetToken, command.Password);

                    if (!passwordChangeResult.Succeeded)
                    {
                        command.OnInvalidPassword(identityOptions.Value.Password);
                        return null;
                    }
                }

                return applicationUser;
            }

            if (command.Password == null)
            {
                command.OnInvalidPassword(identityOptions.Value.Password);
                return null;
            }

            var now = dateTimeProvider.GetUtcNow();
            applicationUser = new ApplicationUser
            {
                UserName = emailAddress,
                Email = emailAddress,

                CreatedDate = now,
                ModifiedDate = now,

                Person = command.PersonData == null ? null : new Person
                {
                    PhoneNumber = command.PersonData.PhoneNumber,
                    Vat = command.PersonData.VatNumber,
                },
            };

            var createResult = await userManager.CreateAsync(applicationUser, command.Password);
            if (!createResult.Succeeded)
            {
                command.OnInvalidPassword(identityOptions.Value.Password);
                return null;
            }

            return applicationUser;
        }
    }
}