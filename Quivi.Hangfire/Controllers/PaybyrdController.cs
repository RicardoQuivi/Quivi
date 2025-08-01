using Microsoft.AspNetCore.Mvc;
using Paybyrd.Api.Models;
using Quivi.Infrastructure.Abstractions.Converters;
using Quivi.Infrastructure.Abstractions.Services.Charges;

namespace Quivi.Hangfire.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaybyrdController : ControllerBase
    {
        private readonly IChargeProcessor chargeProcessor;
        private readonly IIdConverter idConverter;

        public PaybyrdController(IIdConverter idConverter, IChargeProcessor chargeProcessor)
        {
            this.idConverter = idConverter;
            this.chargeProcessor = chargeProcessor;
        }

        [HttpPost]
        public async Task<IActionResult> Post(PaybyrdEvent request)
        {
            var allowedEvents = new[]
            {
                EventType.Transaction.Payment.Success,
                EventType.Transaction.Payment.Canceled,
                EventType.Transaction.Payment.Pending,
                EventType.Transaction.Payment.Canceled,
                EventType.Transaction.Payment.Error,
            };
            if (allowedEvents.Contains(request.Event) && request.Content is Payment p)
            {
                var chargeId = idConverter.FromPublicId(p.OrderRef);
                await chargeProcessor.CheckAndUpdateState(chargeId);
            }

            return Ok();
        }
    }
}
