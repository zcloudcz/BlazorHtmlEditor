namespace BlazorHtmlEditor.Models;

/// <summary>
/// Contains information about a data model property.
/// This class is used to describe properties of a model for UI display and template generation.
/// </summary>
public class ModelPropertyInfo
{
    /// <summary>
    /// Gets or sets the actual property name as defined in the code.
    /// Example: "FirstName"
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the user-friendly display name for the property.
    /// This typically comes from the [Display(Name = "...")] attribute.
    /// Example: "First Name"
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the type name of the property in a friendly format.
    /// Example: "string", "int", "DateTime", "List&lt;string&gt;"
    /// </summary>
    public string TypeName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the description of the property.
    /// This typically comes from the [Display(Description = "...")] attribute.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the Razor expression to use this property in a template.
    /// Example: "@Model.FirstName"
    /// </summary>
    public string RazorExpression { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether this property is a collection type
    /// (e.g., List, Array, IEnumerable).
    /// </summary>
    public bool IsCollection { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this property is a complex type
    /// (i.e., a class, not a primitive type like string or int).
    /// </summary>
    public bool IsComplex { get; set; }
}
