using DesignPatterns.Enterprise.ResultPattern;
using DesignPatterns.Enterprise.ResultPattern.Examples;
using FluentAssertions;

namespace DesignPatterns.Enterprise.Tests;

public class ResultPatternTests
{
    [Fact]
    public void Success_result_has_value()
    {
        var result = Result<int>.Success(42);

        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.Value.Should().Be(42);
    }

    [Fact]
    public void Failure_result_has_error()
    {
        var result = Result<int>.Failure(Error.Validation("Code", "Bad input"));

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Code");
        result.Error.Type.Should().Be(ErrorType.Validation);
    }

    [Fact]
    public void Accessing_value_on_failure_throws()
    {
        var result = Result<int>.Failure(Error.Failure("E", "fail"));

        var act = () => result.Value;

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Implicit_conversion_from_value()
    {
        Result<string> result = "hello";

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("hello");
    }

    [Fact]
    public void Implicit_conversion_from_error()
    {
        Result<string> result = Error.NotFound("NF", "not found");

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public void Map_transforms_value_on_success()
    {
        var result = Result<int>.Success(5)
            .Map(x => x * 2);

        result.Value.Should().Be(10);
    }

    [Fact]
    public void Map_propagates_error_on_failure()
    {
        var error = Error.Failure("E", "fail");
        var result = Result<int>.Failure(error)
            .Map(x => x * 2);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(error);
    }

    [Fact]
    public void Bind_chains_operations()
    {
        var result = Result<int>.Success(10)
            .Bind(x => x > 0 ? Result<string>.Success($"Value: {x}") : Error.Validation("V", "must be positive"));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("Value: 10");
    }

    [Fact]
    public void Bind_short_circuits_on_failure()
    {
        var result = Result<int>.Failure(Error.Failure("E", "fail"))
            .Bind(x => Result<string>.Success($"Value: {x}"));

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Match_handles_both_paths()
    {
        var success = Result<int>.Success(42).Match(
            onSuccess: v => $"Got {v}",
            onFailure: e => $"Error: {e.Code}");

        var failure = Result<int>.Failure(Error.Failure("E1", "fail")).Match(
            onSuccess: v => $"Got {v}",
            onFailure: e => $"Error: {e.Code}");

        success.Should().Be("Got 42");
        failure.Should().Be("Error: E1");
    }

    [Fact]
    public void Tap_executes_side_effect_without_changing_result()
    {
        var sideEffectValue = 0;
        var result = Result<int>.Success(42)
            .Tap(v => sideEffectValue = v);

        result.Value.Should().Be(42);
        sideEffectValue.Should().Be(42);
    }

    [Fact]
    public void Combine_succeeds_when_all_succeed()
    {
        var combined = ResultExtensions.Combine(
            Result<int>.Success(1),
            Result<int>.Success(2),
            Result<int>.Success(3));

        combined.IsSuccess.Should().BeTrue();
        combined.Value.Should().HaveCount(3);
    }

    [Fact]
    public void Combine_fails_on_first_failure()
    {
        var combined = ResultExtensions.Combine(
            Result<int>.Success(1),
            Result<int>.Failure(Error.Failure("E", "fail")),
            Result<int>.Success(3));

        combined.IsFailure.Should().BeTrue();
        combined.Error.Code.Should().Be("E");
    }

    [Fact]
    public void ToResult_converts_nullable()
    {
        string? value = "hello";
        string? nullValue = null;

        value.ToResult(Error.NotFound("NF", "missing")).IsSuccess.Should().BeTrue();
        nullValue.ToResult(Error.NotFound("NF", "missing")).IsFailure.Should().BeTrue();
    }

    // ── UserRegistrationService integration ────────────────────────────

    [Fact]
    public void Registration_succeeds_with_valid_input()
    {
        var service = new UserRegistrationService();

        var result = service.Register("user@example.com", "Password1", "John Doe");

        result.IsSuccess.Should().BeTrue();
        result.Value.Email.Should().Be("user@example.com");
    }

    [Fact]
    public void Registration_fails_with_invalid_email()
    {
        var service = new UserRegistrationService();

        var result = service.Register("bad-email", "Password1", "John");

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Email.Invalid");
    }

    [Fact]
    public void Registration_fails_with_short_password()
    {
        var service = new UserRegistrationService();

        var result = service.Register("user@example.com", "short", "John");

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Password.TooShort");
    }

    [Fact]
    public void Registration_fails_with_duplicate_email()
    {
        var service = new UserRegistrationService();
        service.Register("user@example.com", "Password1", "John");

        var result = service.Register("user@example.com", "Password2", "Jane");

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Email.Duplicate");
    }
}
