namespace ProjectoR.Examples.Common.Domain.User;

public record UserState : AggregateState
{
    public string Id { get; init; }
    public string FirstName { get; init; }
    public string LastName  { get; init; }
    public string Email { get; init; }
    public string Mobile { get; init; }
    public string CountryCode { get; init; }
    public string City { get; init; }
    public string PostalCode { get; init; }
    public string Street { get; init; }
    
    public DateTimeOffset EnrolledDate { get; init; }
    public DateTimeOffset? QuitDate { get; init; }
}