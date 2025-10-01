using Microsoft.AspNetCore.Mvc;
using Quivi.Application.Commands.Users;
using Quivi.Application.Extensions;
using Quivi.Guests.Api.Dtos.Requests.Users;
using Quivi.Guests.Api.Dtos.Responses.Users;
using Quivi.Guests.Api.Validations;
using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Abstractions.Events.Data.Users;
using Quivi.Infrastructure.Validations;

namespace Quivi.Guests.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly ICommandProcessor commandProcessor;

        public UsersController(ICommandProcessor commandProcessor)
        {
            this.commandProcessor = commandProcessor;
        }

        [HttpPost]
        public async Task<CreateUserResponse> Create([FromBody] CreateUserRequest request)
        {
            using var validator = new ModelStateValidator<CreateUserRequest, ValidationError>(request);
            await commandProcessor.Execute(new CreateUserAsyncCommand
            {
                Name = request.Name,
                Email = request.Email,
                Password = request.Password,
                UserType = UserAppType.Guests,
                PersonData = new CreateUserAsyncCommand.CreatePersonData
                {
                    PhoneNumber = request.PhoneNumber,
                    VatNumber = request.VatNumber,
                },

                OnEmailAlreadyExists = () => validator.AddError(m => m.Email, ValidationError.InvalidEmail),
                OnInvadidEmail = () => validator.AddError(m => m.Email, ValidationError.InvalidEmail),
                OnInvalidPassword = (passwordOptions, errors) =>
                {
                    var context = errors.ToErrorContext(passwordOptions);
                    validator.AddError(m => m.Password, ValidationError.InvalidPassword, context);
                },
            });

            return new CreateUserResponse
            {

            };
        }

        [HttpPost("confirm")]
        public async Task<ConfirmUserEmailResponse> ConfirmEmail([FromBody] ConfirmUserEmailRequest request)
        {
            using var validator = new ModelStateValidator<ConfirmUserEmailRequest, ValidationError>(request);
            await commandProcessor.Execute(new ConfirmUserAsyncCommand
            {
                Email = request.Email,
                Code = request.Code,
                UserType = UserAppType.Guests,

                OnCodeExpired = () => validator.AddError(p => p.Code, ValidationError.Expired),
            });

            return new ConfirmUserEmailResponse
            {

            };
        }

        [HttpPost("password/forgot")]
        public async Task<ForgotUserPasswordResponse> ForgotPassword([FromBody] ForgotUserPasswordRequest request)
        {
            using var validator = new ModelStateValidator<ForgotUserPasswordRequest, ValidationError>(request);
            await commandProcessor.Execute(new ForgotUserPasswordAsyncCommand
            {
                Email = request.Email,
                UserType = UserAppType.Guests,
            });

            return new ForgotUserPasswordResponse
            {

            };
        }

        [HttpPost("password/reset")]
        public async Task<ResetUserPasswordResponse> ResetPassword([FromBody] ResetUserPasswordRequest request)
        {
            using var validator = new ModelStateValidator<ResetUserPasswordRequest, ValidationError>(request);
            await commandProcessor.Execute(new ResetUserPasswordAsyncCommand
            {
                Email = request.Email,
                Code = request.Code,
                Password = request.Password,

                OnCodeExpired = () => validator.AddError(p => p.Code, ValidationError.Expired),
                OnInvalidPassword = (passwordOptions) => validator.AddError(m => m.Password, ValidationError.InvalidPassword, passwordOptions),
            });

            return new ResetUserPasswordResponse
            {

            };
        }
    }
}