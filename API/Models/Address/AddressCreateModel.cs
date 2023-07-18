using System.Diagnostics.CodeAnalysis;

namespace API.Models.Address;

[ExcludeFromCodeCoverage]
public class AddressCreateModel
{
    public AddressCreateModel(string id, string streetNumber, string street, string city, string province,
        string country, string postalCode, string? apartment = null, string? additionalInfo = null,
        bool? isDefault = null)
    {
        Id = id;
        StreetNumber = streetNumber;
        Street = street;
        Apartment = apartment;
        AdditionalInfo = additionalInfo;
        City = city;
        Province = province;
        Country = country;
        PostalCode = postalCode;
        IsDefault = isDefault;
    }

    public string StreetNumber { get; set; }
    public string Street { get; set; }
    public string? Apartment { get; set; }
    public string? AdditionalInfo { get; set; }
    public string City { get; set; }
    public string Province { get; set; }
    public string PostalCode { get; set; }
    public string Country { get; set; }
    public bool? IsDefault { get; set; }
    public string Id { get; set; }
}