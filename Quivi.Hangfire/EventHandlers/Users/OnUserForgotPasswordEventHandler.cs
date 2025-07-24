using Microsoft.AspNetCore.Identity;
using Quivi.Domain.Repositories.EntityFramework.Identity;
using Quivi.Infrastructure.Abstractions.Configurations;
using Quivi.Infrastructure.Abstractions.Events.Data.Users;
using Quivi.Infrastructure.Abstractions.Jobs;
using Quivi.Infrastructure.Abstractions.Services.Mailing;
using System.Net;

namespace Quivi.Hangfire.EventHandlers
{
    public class OnUserForgotPasswordEventHandler : BackgroundEventHandler<OnUserForgotPasswordEvent>
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IAppHostsSettings hostsSettings;
        private readonly IEmailEngine emailEngine;
        private readonly IEmailService emailService;

        public OnUserForgotPasswordEventHandler(UserManager<ApplicationUser> userManager,
                                                        IAppHostsSettings hostsSettings,
                                                        IEmailService emailService,
                                                        IBackgroundJobHandler backgroundJobHandler,
                                                        IEmailEngine emailEngine) : base(backgroundJobHandler)
        {
            this.userManager = userManager;
            this.hostsSettings = hostsSettings;
            this.emailService = emailService;
            this.emailEngine = emailEngine;
        }

        public override async Task Run(OnUserForgotPasswordEvent message)
        {
            var applicationUser = await userManager.FindByIdAsync(message.Id.ToString());
            if (applicationUser == null)
                throw new Exception("This should never happen. If it did, then an event for a non existent user was sent.");

            await emailService.SendAsync(new MailMessage
            {
                ToAddress = applicationUser.Email!,
                Subject = "Recuperar Password",
                Body = emailEngine.ForgotPassword(new ForgotPasswordParameters
                {
                    Email = applicationUser.Email!,
                    ResetPasswordUrl = $"{hostsSettings.Backoffice.TrimEnd('/')}/forgotPassword/reset?email={WebUtility.UrlEncode(applicationUser.Email)}&code={WebUtility.UrlEncode(message.Code)}",
                }),
            });

            //TODO: Implement email razor
            //_backgroundJobHandler.Enqueue(() => _emailService.SendForgot(user.Email,
            //                                    _webConfigHelpers.GetMerchantEmailPasswordResetUrl(user.InternaId, code),
            //                                    _webConfigHelpers.WebDashboardServerUrl));
        }
    }
}