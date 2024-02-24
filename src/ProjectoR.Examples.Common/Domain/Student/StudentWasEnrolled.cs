namespace ProjectoR.Examples.Common.Domain.Student;

public record StudentWasEnrolled
(
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