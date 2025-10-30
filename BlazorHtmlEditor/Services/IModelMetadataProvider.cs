using BlazorHtmlEditor.Models;

namespace BlazorHtmlEditor.Services;

/// <summary>
/// Provides metadata about data model properties using reflection.
/// This service inspects types at runtime to extract property information,
/// display names, descriptions, and type details for use in the template editor.
/// </summary>
public interface IModelMetadataProvider
{
    /// <summary>
    /// Gets a flat list of model properties (legacy method for backward compatibility).
    /// This method returns properties without nested hierarchy.
    /// </summary>
    /// <param name="modelType">The type of the model to inspect</param>
    /// <returns>Collection of property information objects</returns>
    IEnumerable<ModelPropertyInfo> GetProperties(Type modelType);

    /// <summary>
    /// Gets complete model metadata including nested properties.
    /// This method recursively explores complex types and builds a property hierarchy.
    /// Use this for advanced scenarios where you need to access nested object properties.
    /// </summary>
    /// <param name="modelType">The type of the model to inspect</param>
    /// <param name="maxDepth">Maximum nesting depth to explore (default: 3). Prevents infinite recursion.</param>
    /// <returns>Complete metadata with hierarchical property structure</returns>
    TemplateModelMeta GetModelMetadata(Type modelType, int maxDepth = 3);

    /// <summary>
    /// Generates a Razor expression for accessing a property in a template.
    /// Converts a property name into the Razor syntax format.
    /// </summary>
    /// <param name="propertyName">Name of the property</param>
    /// <returns>Razor expression in the format "@Model.PropertyName"</returns>
    string GetRazorExpression(string propertyName);
}
