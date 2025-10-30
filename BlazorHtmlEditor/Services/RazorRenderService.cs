using RazorLight;

namespace BlazorHtmlEditor.Services;

/// <summary>
/// Implementation of IRazorRenderService using RazorLight library.
/// RazorLight provides a complete Razor engine for runtime template compilation.
/// </summary>
public class RazorRenderService : IRazorRenderService
{
    private readonly RazorLightEngine _engine;

    public RazorRenderService()
    {
        // Create RazorLight engine
        _engine = new RazorLightEngineBuilder()
            .UseMemoryCachingProvider() // Cache compiled templates in memory
            .Build();
    }

    /// <summary>
    /// Renders Razor template with provided data using RazorLight.
    /// </summary>
    public async Task<string> RenderAsync<TModel>(string razorTemplate, TModel model)
    {
        if (string.IsNullOrWhiteSpace(razorTemplate))
        {
            return string.Empty;
        }

        try
        {
            // RazorLight.CompileRenderStringAsync: Compiles and renders the template
            // - Key: Unique identifier for caching (we use template hash)
            // - Template: Razor template string
            // - Model: Data for binding
            var key = $"template_{razorTemplate.GetHashCode()}";
            var result = await _engine.CompileRenderStringAsync(key, razorTemplate, model);

            return result;
        }
        catch (Exception ex)
        {
            // Return error message as HTML for display to user
            return $@"
<div style='padding: 20px; background: #fff5f5; border: 2px solid #fc8181; border-radius: 8px; font-family: monospace;'>
    <h3 style='color: #c53030; margin-top: 0;'>❌ Razor Compilation Error</h3>
    <pre style='white-space: pre-wrap; color: #333;'>{System.Net.WebUtility.HtmlEncode(ex.Message)}</pre>
</div>";
        }
    }
}
