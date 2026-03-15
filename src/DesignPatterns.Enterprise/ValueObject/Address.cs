namespace DesignPatterns.Enterprise.ValueObject;

/// <summary>
/// Immutable address value object. Two addresses are equal when every component matches.
/// </summary>
public sealed class Address : ValueObject
{
    public string Street { get; }
    public string City { get; }
    public string State { get; }
    public string PostalCode { get; }
    public string Country { get; }

    public Address(string street, string city, string state, string postalCode, string country)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(street);
        ArgumentException.ThrowIfNullOrWhiteSpace(city);
        ArgumentException.ThrowIfNullOrWhiteSpace(country);

        Street = street.Trim();
        City = city.Trim();
        State = (state ?? string.Empty).Trim();
        PostalCode = (postalCode ?? string.Empty).Trim();
        Country = country.Trim().ToUpperInvariant();
    }

    /// <summary>Returns a new Address with the specified street, keeping other fields.</summary>
    public Address WithStreet(string street) => new(street, City, State, PostalCode, Country);

    /// <summary>Returns a new Address with the specified city.</summary>
    public Address WithCity(string city) => new(Street, city, State, PostalCode, Country);

    public string SingleLine => $"{Street}, {City}, {State} {PostalCode}, {Country}";

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Street;
        yield return City;
        yield return State;
        yield return PostalCode;
        yield return Country;
    }

    public override string ToString() => SingleLine;
}
