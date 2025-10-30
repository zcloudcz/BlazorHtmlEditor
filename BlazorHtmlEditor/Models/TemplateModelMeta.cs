namespace BlazorHtmlEditor.Models;

/// <summary>
/// Metadata about a data model for template generation.
/// This record contains the full type information and a hierarchical list of properties
/// that can be used in Razor templates.
/// </summary>
/// <param name="TypeName">Full type name (e.g., "MyApp.Models.Invoice")</param>
/// <param name="Properties">List of model properties with their metadata</param>
public record TemplateModelMeta(
    string TypeName,
    IReadOnlyList<ModelProp> Properties
);

/// <summary>
/// Represents a single property of a data model for template building.
/// Supports nested properties for complex types and collections.
/// This record provides all necessary information to generate Razor expressions
/// and display properties in the UI.
/// </summary>
/// <param name="Name">Property name as defined in code</param>
/// <param name="ClrType">CLR type of the property (string, int, DateTime, etc.)</param>
/// <param name="DisplayName">Display name (from [Display] attribute or Name)</param>
/// <param name="Path">Full path to the property (e.g., "Customer.Address.City")</param>
/// <param name="IsCollection">Whether this is a collection type (List, Array, IEnumerable)</param>
/// <param name="IsComplex">Whether this is a complex type (class, not primitive)</param>
/// <param name="Children">Nested properties (for complex types)</param>
public record ModelProp(
    string Name,
    string ClrType,
    string? DisplayName = null,
    string? Path = null,
    bool IsCollection = false,
    bool IsComplex = false,
    IReadOnlyList<ModelProp>? Children = null
)
{
    /// <summary>
    /// Gets the Razor expression for accessing this property in a template.
    /// Uses the full Path if available, otherwise just the Name.
    /// Example: "@Model.Customer.Address.City"
    /// </summary>
    public string RazorExpression => $"@Model.{Path ?? Name}";

    /// <summary>
    /// Gets the display name with fallback to property name.
    /// Used for showing user-friendly labels in the UI.
    /// </summary>
    public string Display => DisplayName ?? Name;
}
