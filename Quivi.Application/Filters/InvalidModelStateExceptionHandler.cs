using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Quivi.Infrastructure.Exceptions;
using System.Net;

namespace Quivi.Application.Filters
{
    public class InvalidModelStateExceptionHandler : IAsyncExceptionFilter
    {
        public Task OnExceptionAsync(ExceptionContext context)
        {
            if (context.Exception is InvalidModelStateExceptionBase modelException)
            {
                context.Result = new ObjectResult(modelException.ValidationResults.SelectMany(s => s.MemberNames.Select(m => new
                {
                    Property = m,
                    Validation = s,
                })).Select(s => new
                {
                    Property = s.Property,
                    ErrorMessage = s.Validation.ErrorMessage,
                    ErrorCode = s.Validation.ErrorCode,
                    Context = s.Validation.Context,
                }))
                {
                    StatusCode = (int)HttpStatusCode.BadRequest
                };

                context.ExceptionHandled = true;
            }

            return Task.CompletedTask;
        }
    }
}
