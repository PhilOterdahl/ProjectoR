namespace ProjectoR.Examples.Common.Processes.RelocateStudent;

public record RelocateStudentRequest(
    string CountryCode,
    string City,
    string PostalCode,
    string Street
);