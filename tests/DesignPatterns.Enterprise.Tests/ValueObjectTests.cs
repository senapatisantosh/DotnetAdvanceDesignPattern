using DesignPatterns.Enterprise.ValueObject;
using FluentAssertions;

namespace DesignPatterns.Enterprise.Tests;

public class ValueObjectTests
{
    // ── Money ──────────────────────────────────────────────────────────

    [Fact]
    public void Money_equality_by_amount_and_currency()
    {
        var a = Money.USD(10m);
        var b = Money.USD(10m);

        a.Should().Be(b);
        (a == b).Should().BeTrue();
    }

    [Fact]
    public void Money_inequality_different_amount()
    {
        Money.USD(10m).Should().NotBe(Money.USD(20m));
    }

    [Fact]
    public void Money_inequality_different_currency()
    {
        Money.USD(10m).Should().NotBe(Money.EUR(10m));
    }

    [Fact]
    public void Money_add_same_currency()
    {
        var result = Money.USD(10m) + Money.USD(20m);

        result.Should().Be(Money.USD(30m));
    }

    [Fact]
    public void Money_subtract_same_currency()
    {
        var result = Money.USD(30m) - Money.USD(10m);

        result.Should().Be(Money.USD(20m));
    }

    [Fact]
    public void Money_multiply()
    {
        var result = Money.USD(10m) * 3m;

        result.Should().Be(Money.USD(30m));
    }

    [Fact]
    public void Money_cross_currency_throws()
    {
        var act = () => Money.USD(10m) + Money.EUR(5m);

        act.Should().Throw<InvalidOperationException>().WithMessage("*different currencies*");
    }

    [Fact]
    public void Money_comparison_operators()
    {
        (Money.USD(10m) > Money.USD(5m)).Should().BeTrue();
        (Money.USD(5m) < Money.USD(10m)).Should().BeTrue();
        (Money.USD(10m) >= Money.USD(10m)).Should().BeTrue();
        (Money.USD(10m) <= Money.USD(10m)).Should().BeTrue();
    }

    [Fact]
    public void Money_state_properties()
    {
        Money.USD(10m).IsPositive.Should().BeTrue();
        Money.USD(0m).IsZero.Should().BeTrue();
        Money.USD(-5m).IsNegative.Should().BeTrue();
    }

    [Fact]
    public void Money_rounds_to_two_decimals()
    {
        var money = new Money(10.999m, "USD");

        money.Amount.Should().Be(11.00m);
    }

    [Fact]
    public void Money_hash_codes_match_for_equal_values()
    {
        var a = Money.USD(42.50m);
        var b = Money.USD(42.50m);

        a.GetHashCode().Should().Be(b.GetHashCode());
    }

    // ── Address ────────────────────────────────────────────────────────

    [Fact]
    public void Address_equality_by_all_components()
    {
        var a = new Address("123 Main St", "Springfield", "IL", "62701", "US");
        var b = new Address("123 Main St", "Springfield", "IL", "62701", "US");

        a.Should().Be(b);
    }

    [Fact]
    public void Address_inequality_on_any_different_component()
    {
        var a = new Address("123 Main St", "Springfield", "IL", "62701", "US");
        var b = new Address("456 Oak Ave", "Springfield", "IL", "62701", "US");

        a.Should().NotBe(b);
    }

    [Fact]
    public void Address_with_street_returns_new_instance()
    {
        var original = new Address("123 Main St", "Springfield", "IL", "62701", "US");
        var updated = original.WithStreet("456 Oak Ave");

        updated.Street.Should().Be("456 Oak Ave");
        updated.City.Should().Be("Springfield");
        original.Street.Should().Be("123 Main St"); // immutable
    }

    [Fact]
    public void Address_country_is_uppercased()
    {
        var addr = new Address("123 Main", "City", "ST", "12345", "us");

        addr.Country.Should().Be("US");
    }

    [Fact]
    public void Address_single_line_format()
    {
        var addr = new Address("123 Main", "City", "ST", "12345", "US");

        addr.SingleLine.Should().Be("123 Main, City, ST 12345, US");
    }

    // ── DateRange ──────────────────────────────────────────────────────

    [Fact]
    public void DateRange_calculates_inclusive_days()
    {
        var range = new DateRange(new DateTime(2025, 1, 1), new DateTime(2025, 1, 10));

        range.Days.Should().Be(10);
    }

    [Fact]
    public void DateRange_same_day_is_one_day()
    {
        var range = new DateRange(new DateTime(2025, 6, 15), new DateTime(2025, 6, 15));

        range.Days.Should().Be(1);
    }

    [Fact]
    public void DateRange_end_before_start_throws()
    {
        var act = () => new DateRange(new DateTime(2025, 12, 31), new DateTime(2025, 1, 1));

        act.Should().Throw<ArgumentException>().WithMessage("*End*before*Start*");
    }

    [Fact]
    public void DateRange_contains_date()
    {
        var range = new DateRange(new DateTime(2025, 1, 1), new DateTime(2025, 1, 31));

        range.Contains(new DateTime(2025, 1, 15)).Should().BeTrue();
        range.Contains(new DateTime(2025, 2, 1)).Should().BeFalse();
        range.Contains(new DateTime(2024, 12, 31)).Should().BeFalse();
    }

    [Fact]
    public void DateRange_overlaps_detection()
    {
        var jan = new DateRange(new DateTime(2025, 1, 1), new DateTime(2025, 1, 31));
        var midJanFeb = new DateRange(new DateTime(2025, 1, 15), new DateTime(2025, 2, 15));
        var march = new DateRange(new DateTime(2025, 3, 1), new DateTime(2025, 3, 31));

        jan.Overlaps(midJanFeb).Should().BeTrue();
        jan.Overlaps(march).Should().BeFalse();
    }

    [Fact]
    public void DateRange_intersection()
    {
        var jan = new DateRange(new DateTime(2025, 1, 1), new DateTime(2025, 1, 31));
        var midJanFeb = new DateRange(new DateTime(2025, 1, 15), new DateTime(2025, 2, 15));

        var intersection = jan.Intersect(midJanFeb);

        intersection.Should().NotBeNull();
        intersection!.Start.Should().Be(new DateTime(2025, 1, 15));
        intersection.End.Should().Be(new DateTime(2025, 1, 31));
    }

    [Fact]
    public void DateRange_no_intersection_returns_null()
    {
        var jan = new DateRange(new DateTime(2025, 1, 1), new DateTime(2025, 1, 31));
        var march = new DateRange(new DateTime(2025, 3, 1), new DateTime(2025, 3, 31));

        jan.Intersect(march).Should().BeNull();
    }

    [Fact]
    public void DateRange_equality()
    {
        var a = new DateRange(new DateTime(2025, 1, 1), new DateTime(2025, 1, 31));
        var b = new DateRange(new DateTime(2025, 1, 1), new DateTime(2025, 1, 31));

        a.Should().Be(b);
        a.GetHashCode().Should().Be(b.GetHashCode());
    }
}
