using System.Diagnostics;

namespace DesignPatterns.Behavioral.TemplateMethod;

/// <summary>
/// Abstract base class defining the template method for document export.
/// The algorithm skeleton: LoadData → Validate → Transform → Write → Finalize
/// Subclasses override specific steps while keeping the overall flow intact.
/// </summary>
public abstract class DocumentExporter
{
    /// <summary>
    /// Template method — defines the export algorithm skeleton.
    /// Sealed so subclasses cannot change the overall flow.
    /// </summary>
    public ExportResult Export(IReadOnlyList<Dictionary<string, string>> data)
    {
        var steps = new List<string>();
        var sw = Stopwatch.StartNew();

        try
        {
            // Step 1: Load / prepare data
            steps.Add("LoadData");
            var loadedData = LoadData(data);

            // Step 2: Validate
            steps.Add("Validate");
            var validationErrors = Validate(loadedData);
            if (validationErrors.Count > 0)
            {
                return new ExportResult
                {
                    Success = false,
                    Format = FormatName,
                    Content = string.Empty,
                    RecordCount = 0,
                    Steps = steps,
                    ErrorMessage = string.Join("; ", validationErrors),
                    Duration = sw.Elapsed
                };
            }

            // Step 3: Transform
            steps.Add("Transform");
            var transformed = Transform(loadedData);

            // Step 4: Write
            steps.Add("Write");
            var output = Write(transformed);

            // Step 5: Finalize (hook — optional override)
            steps.Add("Finalize");
            var finalOutput = Finalize(output);

            sw.Stop();

            return new ExportResult
            {
                Success = true,
                Format = FormatName,
                Content = finalOutput,
                RecordCount = loadedData.Count,
                Steps = steps,
                Duration = sw.Elapsed
            };
        }
        catch (Exception ex)
        {
            sw.Stop();
            return new ExportResult
            {
                Success = false,
                Format = FormatName,
                Content = string.Empty,
                RecordCount = 0,
                Steps = steps,
                ErrorMessage = ex.Message,
                Duration = sw.Elapsed
            };
        }
    }

    /// <summary>Format name (e.g. "PDF", "CSV", "Excel")</summary>
    protected abstract string FormatName { get; }

    /// <summary>Load and prepare data for export.</summary>
    protected virtual IReadOnlyList<Dictionary<string, string>> LoadData(
        IReadOnlyList<Dictionary<string, string>> rawData) => rawData;

    /// <summary>Validate data before transformation. Return list of errors (empty = valid).</summary>
    protected virtual List<string> Validate(IReadOnlyList<Dictionary<string, string>> data)
    {
        var errors = new List<string>();
        if (data.Count == 0)
            errors.Add("No data to export.");
        return errors;
    }

    /// <summary>Transform data into format-specific representation.</summary>
    protected abstract IReadOnlyList<string> Transform(IReadOnlyList<Dictionary<string, string>> data);

    /// <summary>Write the transformed data into the final output string.</summary>
    protected abstract string Write(IReadOnlyList<string> transformedData);

    /// <summary>Hook method — optional post-processing. Default is identity.</summary>
    protected virtual string Finalize(string output) => output;
}
