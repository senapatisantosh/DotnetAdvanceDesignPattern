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
        Price = 2499.99m,
        Color = "Silver"
    };

    private static readonly Product Shirt = new()
    {
        Name = "Classic T-Shirt",
        Category = "Clothing",
        Brand = "Nike",
        Price = 29.99m,
        Color = "Red"
    };

    private static readonly Product Book = new()
    {
        Name = "Design Patterns",
        Category = "Books",
        Brand = "Addison-Wesley",
        Price = 49.99m,
        Color = "White"
    };

    [Fact]
    public void FieldMatch_MatchesCorrectly()
    {
        var expr = new FieldMatchExpression("category", "Electronics");
        expr.Interpret(Laptop).Should().BeTrue();
        expr.Interpret(Shirt).Should().BeFalse();
    }

    [Fact]
    public void AndExpression_BothMustMatch()
    {
        var expr = new AndExpression(
            new FieldMatchExpression("category", "Electronics"),
            new FieldMatchExpression("brand", "Apple"));

        expr.Interpret(Laptop).Should().BeTrue();
        expr.Interpret(Shirt).Should().BeFalse();
    }

    [Fact]
    public void OrExpression_EitherCanMatch()
    {
        var expr = new OrExpression(
            new FieldMatchExpression("category", "Electronics"),
            new FieldMatchExpression("category", "Books"));

        expr.Interpret(Laptop).Should().BeTrue();
        expr.Interpret(Book).Should().BeTrue();
        expr.Interpret(Shirt).Should().BeFalse();
    }

    [Fact]
    public void NotExpression_NegatesResult()
    {
        var expr = new NotExpression(new FieldMatchExpression("color", "Red"));
        expr.Interpret(Laptop).Should().BeTrue();
        expr.Interpret(Shirt).Should().BeFalse();
    }

    [Fact]
    public void Parser_SimpleFieldMatch()
    {
        var parser = new SearchQueryParser();
        var expr = parser.Parse("category:Electronics");
        expr.Interpret(Laptop).Should().BeTrue();
    }

    [Fact]
    public void Parser_AndExpression()
    {
        var parser = new SearchQueryParser();
        var expr = parser.Parse("category:Electronics AND brand:Apple");

        expr.Interpret(Laptop).Should().BeTrue();
        expr.Interpret(Shirt).Should().BeFalse();
    }

    [Fact]
    public void Parser_OrExpression()
    {
        var parser = new SearchQueryParser();
        var expr = parser.Parse("category:Electronics OR category:Books");

        expr.Interpret(Laptop).Should().BeTrue();
        expr.Interpret(Book).Should().BeTrue();
        expr.Interpret(Shirt).Should().BeFalse();
    }

    [Fact]
    public void Parser_NotExpression()
    {
        var parser = new SearchQueryParser();
        var expr = parser.Parse("NOT color:Red");

        expr.Interpret(Laptop).Should().BeTrue();
        expr.Interpret(Shirt).Should().BeFalse();
    }

    [Fact]
    public void Parser_ComplexExpressionWithParentheses()
    {
        var parser = new SearchQueryParser();
        var expr = parser.Parse("(category:Electronics OR category:Books) AND NOT brand:Apple");

        expr.Interpret(Laptop).Should().BeFalse(); // Electronics but Apple
        expr.Interpret(Book).Should().BeTrue();    // Books and not Apple
        expr.Interpret(Shirt).Should().BeFalse();  // Not Electronics or Books
    }

    [Fact]
    public void Parser_FilterProducts()
    {
        var products = new[] { Laptop, Shirt, Book };
        var parser = new SearchQueryParser();
        var expr = parser.Parse("category:Electronics OR category:Books");

        var results = products.Where(p => expr.Interpret(p)).ToList();

        results.Should().HaveCount(2);
        results.Should().Contain(Laptop);
        results.Should().Contain(Book);
    }

    [Fact]
    public void Parser_InvalidQuery_Throws()
    {
        var parser = new SearchQueryParser();
        var act = () => parser.Parse("invalid_no_colon");
        act.Should().Throw<FormatException>();
    }
}
