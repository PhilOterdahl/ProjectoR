namespace ProjectoR.Examples.Common.Domain.User;

public record UserWasEnrolled(
    string Id,
    string FirstName,
    string LastName,
    string Email,
    string Mobile,
    string CountryCode,
    string City,
    string PostalCode,
    string Street,
    DateTimeOffset EnrolledDate
);