namespace ProjectoR.Examples.Common.Processes.EnrollStudent;

public record EnrollStudentRequest(
    string FirstName,
    string LastName,
    string Email,
    string Mobile,
    string CountryCode,
    string City,
    string PostalCode,
    string Street
);