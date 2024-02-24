namespace ProjectoR.Examples.Common.Domain.Student;

public record StudentRelocated(
    string Id,
    string CountryCode,
    string City,
    string PostalCode,
    string Street
);