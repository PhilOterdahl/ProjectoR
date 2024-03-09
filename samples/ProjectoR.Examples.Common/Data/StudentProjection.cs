namespace ProjectoR.Examples.Common.Data;

public class StudentProjection
{
    public string Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public ContactInformation ContactInformation { get; set; }
    public Address Address { get; set; }
}

public class ContactInformation
{
    public string Email { get; set; }
    public string Mobile { get; set; }
}

public class Address
{
    public string CountryCode { get; set; }
    public string PostalCode { get; set; }
    public string City { get; set; }
    public string Street { get; set; }
}