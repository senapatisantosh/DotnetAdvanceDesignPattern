using DesignPatterns.Behavioral.Interpreter;
using DesignPatterns.Behavioral.Interpreter.Expressions;
using FluentAssertions;

namespace DesignPatterns.Behavioral.Tests;

public class InterpreterTests
{
    private static readonly Product Laptop = new()
    {
        Name = "MacBook Pro",
        Category = "Electronics",
        Brand = "Apple",
        Price = 2499,
        Color = "Silver"
    };

    private static readonly Product Sneakers = new()
    {
        Name = "Air Max 90",
        Category = "Footwear",
        Brand = "Nike",
        Price = 120,
        Color = "Red"
    };

    private static readonly Product Book = new()
    {
        Name = "Design Patterns",
        Category = "Books",
        Brand = "O'Reilly",
        Price = 45,
        Color = "Blue"
    };

    private static readonly List<Product> AllProducts = [Laptop, Sneakers, Book];

    [Fact]
    public void FieldMatch_MatchesCorrectProduct()
    {
        var expr = new FieldMatchExpression("category", "Electronics");

        expr.Interpret(Laptop).Should().BeTrue();
        expr.Interpret(Sneakers).Should().BeFalse();
    }

    [Fact]
    public void AndExpression_RequiresBothConditions()
    {
        var expr = new AndExpression(
            new FieldMatchExpression("category", "Electronics"),
            new FieldMatchExpression("brand", "Apple"));

        expr.Interpret(Laptop).Should().BeTrue();
        expr.Interpret(Sneakers).Should().BeFalse();
    }

    [Fact]
    public void OrExpression_MatchesEitherCondition()
    {
        var expr = new OrExpression(
            new FieldMatchExpression("category", "Electronics"),
            new FieldMatchExpression("category", "Books"));

        expr.Interpret(Laptop).Should().BeTrue();
        expr.Interpret(Book).Should().BeTrue();
        expr.Interpret(Sneakers).Should().BeFalse();
    }

    [Fact]
    public void NotExpression_NegatesResult()
    {
        var expr = new NotExpression(new FieldMatchExpression("color", "Red"));

        expr.Interpret(Laptop).Should().BeTrue();
        expr.Interpret(Sneakers).Should().BeFalse();
    }

    [Fact]
    public void Parser_ParsesSimpleFieldMatch()
    {
        var parser = new SearchQueryParser();
        var expr = parser.Parse("category:Electronics");

        var matches = AllProducts.Where(p => expr.Interpret(p)).ToList();
        matches.Should().ContainSingle().Which.Should().Be(Laptop);
    }

    [Fact]
    public void Parser_ParsesAndExpression()
    {
        var parser = new SearchQueryParser();
        var expr = parser.Parse("category:Electronics AND brand:Apple");

        expr.Interpret(Laptop).Should().BeTrue();
        expr.Interpret(Sneakers).Should().BeFalse();
    }

    [Fact]
    public void Parser_ParsesOrExpression()
    {
        var parser = new SearchQueryParser();
        var expr = parser.Parse("category:Electronics OR category:Books");

        var matches = AllProducts.Where(p => expr.Interpret(p)).ToList();
        matches.Should().HaveCount(2);
        matches.Should().Contain(Laptop);
        matches.Should().Contain(Book);
    }

    [Fact]
    public void Parser_ParsesNotExpression()
    {
        var parser = new SearchQueryParser();
        var expr = parser.Parse("NOT color:Red");

        var matches = AllProducts.Where(p => expr.Interpret(p)).ToList();
        matches.Should().HaveCount(2);
        matches.Should().NotContain(Sneakers);
    }

    [Fact]
    public void Parser_ParsesParenthesizedExpression()
    {
        var parser = new SearchQueryParser();
        var expr = parser.Parse("(category:Electronics OR category:Books) AND NOT brand:Apple");

        var matches = AllProducts.Where(p => expr.Interpret(p)).ToList();
        matches.Should().ContainSingle().Which.Should().Be(Book);
    }

    [Fact]
    public void Parser_ThrowsOnInvalidQuery()
    {
        var parser = new SearchQueryParser();
        var act = () => parser.Parse("invalidtoken");
        act.Should().Throw<FormatException>();
    }
}
