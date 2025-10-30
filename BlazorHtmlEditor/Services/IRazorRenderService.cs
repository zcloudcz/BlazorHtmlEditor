namespace BlazorHtmlEditor.Services;

/// <summary>
/// Service for rendering Razor templates using RazorLight library.
/// This service provides runtime compilation and rendering of Razor templates,
/// allowing dynamic HTML generation from templates and data models.
/// </summary>
public interface IRazorRenderService
{
    /// <summary>
    /// Renders a Razor template to HTML with the provided data.
    /// The template is compiled at runtime and executed with the model data.
    /// Supports full Razor syntax including @model directive, conditionals, loops, etc.
    /// </summary>
    /// <typeparam name="TModel">The type of the data model</typeparam>
    /// <param name="razorTemplate">Razor template string (can include @model directive)</param>
    /// <param name="model">Data model to bind to the template</param>
    /// <returns>Rendered HTML string, or an error message if compilation fails</returns>
    Task<string> RenderAsync<TModel>(string razorTemplate, TModel model);
}
