using System.Collections;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using BlazorHtmlEditor.Models;

namespace BlazorHtmlEditor.Services;

/// <summary>
/// Implementation of IModelMetadataProvider using reflection.
/// This class uses .NET reflection to inspect types and extract property information
/// including names, types, attributes, and relationships between properties.
/// </summary>
public class ModelMetadataProvider : IModelMetadataProvider
{
    /// <summary>
    /// Gets a flat list of properties from a model type.
    /// This method iterates through all public instance properties and extracts their metadata.
    /// </summary>
    /// <param name="modelType">The type to inspect</param>
    /// <returns>Enumerable of property information objects</returns>
    public IEnumerable<ModelPropertyInfo> GetProperties(Type modelType)
    {
        // Return empty if type is null
        if (modelType == null)
            yield break;

        // Get all public instance properties using reflection
        var properties = modelType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var prop in properties)
        {
            // Try to get display name from attributes, fall back to property name
            // First check [DisplayName], then [Display(Name=...)], finally use actual name
            var displayName = prop.GetCustomAttribute<DisplayNameAttribute>()?.DisplayName
                           ?? prop.GetCustomAttribute<DisplayAttribute>()?.Name
                           ?? prop.Name;

            // Try to get description from attributes
            // Check [Description] and [Display(Description=...)]
            var description = prop.GetCustomAttribute<DescriptionAttribute>()?.Description
                           ?? prop.GetCustomAttribute<DisplayAttribute>()?.Description
                           ?? string.Empty;

            // Determine if this property is a collection type
            // Collections implement IEnumerable, but string is excluded
            var isCollection = typeof(IEnumerable).IsAssignableFrom(prop.PropertyType)
                            && prop.PropertyType != typeof(string);

            // Determine if this is a complex type (class object, not primitive or value type)
            // Primitives: int, bool, etc. Value types: struct, DateTime, decimal
            var isComplex = !prop.PropertyType.IsPrimitive
                         && !prop.PropertyType.IsValueType
                         && prop.PropertyType != typeof(string)
                         && !isCollection;

            yield return new ModelPropertyInfo
            {
                Name = prop.Name,
                DisplayName = displayName,
                TypeName = GetFriendlyTypeName(prop.PropertyType),
                Description = description,
                RazorExpression = GetRazorExpression(prop.Name),
                IsCollection = isCollection,
                IsComplex = isComplex
            };
        }
    }

    /// <summary>
    /// Gets complete metadata for a model type including nested properties.
    /// This method creates a hierarchical structure of all properties.
    /// </summary>
    /// <param name="modelType">The type to analyze</param>
    /// <param name="maxDepth">Maximum depth for nested properties (default: 3)</param>
    /// <returns>Complete metadata with property hierarchy</returns>
    public TemplateModelMeta GetModelMetadata(Type modelType, int maxDepth = 3)
    {
        if (modelType == null)
            throw new ArgumentNullException(nameof(modelType));

        // Recursively get all properties starting from the root
        var properties = GetModelProps(modelType, "", 0, maxDepth, new HashSet<Type>());

        return new TemplateModelMeta(
            TypeName: modelType.FullName ?? modelType.Name,
            Properties: properties
        );
    }

    /// <summary>
    /// Generates a Razor expression for a property name.
    /// Simply prepends "@Model." to create valid Razor syntax.
    /// </summary>
    /// <param name="propertyName">The property name</param>
    /// <returns>Razor expression string (e.g., "@Model.FirstName")</returns>
    public string GetRazorExpression(string propertyName)
    {
        return $"@Model.{propertyName}";
    }

    /// <summary>
    /// Recursively retrieves properties including nested properties for complex types.
    /// This method builds a hierarchical structure of properties, following object relationships.
    /// Includes protection against infinite recursion through depth limiting and type tracking.
    /// </summary>
    /// <param name="type">Type to inspect</param>
    /// <param name="pathPrefix">Current path prefix (e.g., "Customer.Address")</param>
    /// <param name="currentDepth">Current recursion depth</param>
    /// <param name="maxDepth">Maximum allowed recursion depth</param>
    /// <param name="visitedTypes">Set of types already visited (prevents circular references)</param>
    /// <returns>List of properties with nested hierarchy</returns>
    private List<ModelProp> GetModelProps(Type type, string pathPrefix, int currentDepth, int maxDepth, HashSet<Type> visitedTypes)
    {
        var result = new List<ModelProp>();

        // Prevent infinite recursion
        // Stop if we've reached max depth or already visited this type
        if (currentDepth >= maxDepth || visitedTypes.Contains(type))
            return result;

        // Mark this type as visited
        visitedTypes.Add(type);

        // Get all public instance properties
        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var prop in properties)
        {
            // Get display name from attributes or property name
            var displayName = prop.GetCustomAttribute<DisplayNameAttribute>()?.DisplayName
                           ?? prop.GetCustomAttribute<DisplayAttribute>()?.Name
                           ?? prop.Name;

            // Check if this is a collection type (IEnumerable, List, Array, etc.)
            var isCollection = typeof(IEnumerable).IsAssignableFrom(prop.PropertyType)
                            && prop.PropertyType != typeof(string);

            // Check if this is a complex type (class object)
            var isComplex = !prop.PropertyType.IsPrimitive
                         && !prop.PropertyType.IsValueType
                         && prop.PropertyType != typeof(string)
                         && !isCollection;

            // Build the full path to this property
            // Example: if pathPrefix is "Customer" and prop.Name is "Address", path becomes "Customer.Address"
            var path = string.IsNullOrEmpty(pathPrefix)
                ? prop.Name
                : $"{pathPrefix}.{prop.Name}";

            // For complex types, recursively get their nested properties
            List<ModelProp>? children = null;
            if (isComplex && currentDepth + 1 < maxDepth)
            {
                // Create a copy of visited types for this branch of recursion
                var childVisited = new HashSet<Type>(visitedTypes);
                children = GetModelProps(prop.PropertyType, path, currentDepth + 1, maxDepth, childVisited);

                // If it has children, confirm it's a complex type
                if (children.Count > 0)
                {
                    isComplex = true;
                }
            }

            // Add this property to the result
            result.Add(new ModelProp(
                Name: prop.Name,
                ClrType: GetFriendlyTypeName(prop.PropertyType),
                DisplayName: displayName,
                Path: path,
                IsCollection: isCollection,
                IsComplex: isComplex,
                Children: children?.AsReadOnly()
            ));
        }

        return result;
    }

    /// <summary>
    /// Converts a .NET type to a friendly, readable type name.
    /// Handles common types and generic types (like List&lt;T&gt;).
    /// </summary>
    /// <param name="type">The type to convert</param>
    /// <returns>User-friendly type name string</returns>
    private static string GetFriendlyTypeName(Type type)
    {
        // Map common types to C# keywords
        if (type == typeof(string)) return "string";
        if (type == typeof(int)) return "int";
        if (type == typeof(long)) return "long";
        if (type == typeof(bool)) return "bool";
        if (type == typeof(DateTime)) return "DateTime";
        if (type == typeof(decimal)) return "decimal";
        if (type == typeof(double)) return "double";
        if (type == typeof(float)) return "float";

        // Handle generic types (e.g., List<string> -> "List<string>")
        if (type.IsGenericType)
        {
            // Remove the "`1" or "`2" suffix from generic type names
            var genericTypeName = type.Name.Split('`')[0];
            // Recursively get friendly names for generic type arguments
            var genericArgs = string.Join(", ", type.GetGenericArguments().Select(GetFriendlyTypeName));
            return $"{genericTypeName}<{genericArgs}>";
        }

        // Default: return the type's name
        return type.Name;
    }
}
