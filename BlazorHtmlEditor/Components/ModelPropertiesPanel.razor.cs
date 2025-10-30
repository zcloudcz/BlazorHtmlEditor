using Microsoft.AspNetCore.Components;
using BlazorHtmlEditor.Models;

namespace BlazorHtmlEditor.Components;

/// <summary>
/// Panel component that displays data model properties.
/// Allows users to search and select properties to insert into the template editor.
/// </summary>
public partial class ModelPropertiesPanel
{
    /// <summary>
    /// Gets or sets the collection of model properties to display.
    /// These properties are typically obtained from IModelMetadataProvider.
    /// </summary>
    [Parameter]
    public IEnumerable<ModelPropertyInfo>? Properties { get; set; }

    /// <summary>
    /// Gets or sets the callback invoked when a property is selected.
    /// Used to insert the property's Razor expression into the editor.
    /// </summary>
    [Parameter]
    public EventCallback<ModelPropertyInfo> OnPropertySelected { get; set; }

    /// <summary>
    /// Stores the current search term entered by the user.
    /// Used to filter the properties list in real-time.
    /// </summary>
    private string searchTerm = string.Empty;

    /// <summary>
    /// Gets the filtered list of properties based on the search term.
    /// Filters by DisplayName, Name, and TypeName using case-insensitive comparison.
    /// </summary>
    private IEnumerable<ModelPropertyInfo> FilteredProperties
    {
        get
        {
            // Return empty collection if no properties are available
            if (Properties == null)
                return Enumerable.Empty<ModelPropertyInfo>();

            // If no search term, return all properties
            if (string.IsNullOrWhiteSpace(searchTerm))
                return Properties;

            // Filter properties by search term
            // Searches in DisplayName, Name, and TypeName fields
            return Properties.Where(p =>
                p.DisplayName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                p.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                p.TypeName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase));
        }
    }

    /// <summary>
    /// Handles the property click event.
    /// Invokes the OnPropertySelected callback to notify parent component.
    /// </summary>
    /// <param name="property">The property that was clicked</param>
    private async Task OnPropertyClick(ModelPropertyInfo property)
    {
        await OnPropertySelected.InvokeAsync(property);
    }
}
