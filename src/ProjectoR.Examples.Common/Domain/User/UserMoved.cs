namespace ProjectoR.Examples.Common.Domain.User;

public record UserMoved(
    string Id,
    string CountryCode,
    string City,
    string PostalCode,
    string Street
);