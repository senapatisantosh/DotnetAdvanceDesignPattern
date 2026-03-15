namespace DesignPatterns.Behavioral.ChainOfResponsibility.Pipeline;

/// <summary>
/// Pipeline variant of Chain of Responsibility.
/// Each step processes the request AND passes it forward (every step runs).
/// Unlike the classic chain where processing stops at the first handler that can handle it.
/// </summary>
public interface IPipelineStep<T>
{
    Task<T> ProcessAsync(T input, Func<T, Task<T>> next);
}

/// <summary>
/// Builds and executes a pipeline of processing steps.
/// </summary>
public sealed class Pipeline<T>
{
    private readonly List<IPipelineStep<T>> _steps = [];

    public Pipeline<T> AddStep(IPipelineStep<T> step)
    {
        _steps.Add(step);
        return this;
    }

    public async Task<T> ExecuteAsync(T input)
    {
        // Build the pipeline from the inside out (last step wraps the identity function)
        Func<T, Task<T>> pipeline = x => Task.FromResult(x);

        for (int i = _steps.Count - 1; i >= 0; i--)
        {
            var step = _steps[i];
            var next = pipeline;
            pipeline = x => step.ProcessAsync(x, next);
        }

        return await pipeline(input);
    }
}
