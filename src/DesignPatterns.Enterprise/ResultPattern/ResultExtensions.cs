namespace DesignPatterns.Enterprise.ResultPattern;

/// <summary>
/// Functional-style extensions for Result — Map, Bind, Match — so that
/// callers can chain operations without nested if-else blocks.
/// </summary>
public static class ResultExtensions
{
    /// <summary>
    /// Transforms the success value while preserving the result wrapper.
    /// If the result is a failure, the error propagates unchanged.
    /// </summary>
    public static Result<TOut> Map<TIn, TOut>(this Result<TIn> result, Func<TIn, TOut> mapper) =>
        result.IsSuccess
            ? Result<TOut>.Success(mapper(result.Value))
            : Result<TOut>.Failure(result.Error);

    /// <summary>
    /// Chains two result-producing operations. If the first fails, the second is skipped.
    /// This is the monadic bind / flatMap.
    /// </summary>
    public static Result<TOut> Bind<TIn, TOut>(this Result<TIn> result, Func<TIn, Result<TOut>> binder) =>
        result.IsSuccess
            ? binder(result.Value)
            : Result<TOut>.Failure(result.Error);

    /// <summary>
    /// Pattern matches on success/failure to produce a single output.
    /// </summary>
    public static TOut Match<TIn, TOut>(
        this Result<TIn> result,
        Func<TIn, TOut> onSuccess,
        Func<Error, TOut> onFailure) =>
        result.IsSuccess ? onSuccess(result.Value) : onFailure(result.Error);

    /// <summary>
    /// Executes a side effect on success without changing the result.
    /// </summary>
    public static Result<T> Tap<T>(this Result<T> result, Action<T> action)
    {
        if (result.IsSuccess) action(result.Value);
        return result;
    }

    /// <summary>
    /// Converts a nullable value to a Result, using the provided error when null.
    /// </summary>
    public static Result<T> ToResult<T>(this T? value, Error error) where T : class =>
        value is not null ? Result<T>.Success(value) : Result<T>.Failure(error);

    /// <summary>
    /// Combines multiple results; returns first failure or all values.
    /// </summary>
    public static Result<IReadOnlyList<T>> Combine<T>(params Result<T>[] results)
    {
        var firstFailure = results.FirstOrDefault(r => r.IsFailure);
        if (firstFailure is not null)
            return Result<IReadOnlyList<T>>.Failure(firstFailure.Error);

        IReadOnlyList<T> values = results.Select(r => r.Value).ToList().AsReadOnly();
        return Result<IReadOnlyList<T>>.Success(values);
    }

    /// <summary>Async Map.</summary>
    public static async Task<Result<TOut>> MapAsync<TIn, TOut>(
        this Task<Result<TIn>> resultTask, Func<TIn, TOut> mapper)
    {
        var result = await resultTask;
        return result.Map(mapper);
    }

    /// <summary>Async Bind.</summary>
    public static async Task<Result<TOut>> BindAsync<TIn, TOut>(
        this Task<Result<TIn>> resultTask, Func<TIn, Result<TOut>> binder)
    {
        var result = await resultTask;
        return result.Bind(binder);
    }
}
