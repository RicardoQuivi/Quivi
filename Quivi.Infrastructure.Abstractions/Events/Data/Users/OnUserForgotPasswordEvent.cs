namespace Quivi.Infrastructure.Abstractions.Events.Data.Users
{
    public record OnUserForgotPasswordEvent : IEvent
    {
        public int Id { get; init; }
        public required string Code { get; init; }
        public required UserAppType UserType { get; init; }
    }
}