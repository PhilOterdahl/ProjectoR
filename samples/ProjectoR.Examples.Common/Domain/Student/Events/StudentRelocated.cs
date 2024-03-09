namespace ProjectoR.Examples.Common.Domain.Student.Events;

public record StudentRelocated(
    string Id,
    AddressState OldAddress,
    AddressState NewAddress
);

public record AddressState(
    string CountryCode,
    string City,
    string PostalCode,
    string Street
);