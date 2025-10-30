using Microsoft.AspNetCore.Components;
using BlazorHtmlEditor.Models;

namespace BlazorHtmlEditor.Components;

/// <summary>
/// Simplified Razor template editor with Code/Preview tabs.
/// No Design mode - just Monaco editor and live preview using RazorLight.
/// </summary>
public partial class TemplateEditor<TModel> where TModel : class, new()
{
    #region Parameters

    [Parameter]
    public string Title { get; set; } = "Razor Template Editor";

    [Parameter]
    public string InitialContent { get; set; } = string.Empty;

    [Parameter]
    public EventCallback<string> OnContentChanged { get; set; }

    [Parameter]
    public EventCallback<string> OnSave { get; set; }

    [Parameter]
    public bool ShowSaveButton { get; set; } = true;

    [Parameter]
    public bool ShowPropertiesPanel { get; set; } = true;

    [Parameter]
    public EditorTab DefaultTab { get; set; } = EditorTab.Code;

    #endregion

    #region Fields

    /// <summary>
    /// Reference to the Monaco code editor component.
    /// Used to interact with the editor (insert text, get/set values, etc.).
    /// </summary>
    private RazorCodeEditor? codeEditor;

    /// <summary>
    /// Currently active tab (Code or Preview).
    /// </summary>
    private EditorTab currentTab;

    /// <summary>
    /// Current template code content.
    /// Synchronized with the editor content.
    /// </summary>
    private string currentCode = string.Empty;

    /// <summary>
    /// List of model properties available for insertion into the template.
    /// Populated using IModelMetadataProvider during initialization.
    /// </summary>
    private IEnumerable<ModelPropertyInfo> modelProperties = Enumerable.Empty<ModelPropertyInfo>();

    /// <summary>
    /// Demo data instance used for preview rendering.
    /// Created automatically with sample values for all properties.
    /// </summary>
    private TModel? demoData;

    #endregion

    #region Lifecycle

    /// <summary>
    /// Lifecycle method called when the component is initialized.
    /// Sets up initial state, loads model metadata, and creates demo data.
    /// </summary>
    protected override void OnInitialized()
    {
        // Set the initial active tab from parameter
        currentTab = DefaultTab;

        // Load initial template content
        currentCode = InitialContent;

        // Get model properties using reflection for the Properties panel
        // This provides users with a list of available properties to insert
        modelProperties = MetadataProvider.GetProperties(typeof(TModel));

        // Create demo data for preview rendering
        // This generates sample values for all properties
        demoData = CreateDemoData();
    }

    #endregion

    #region Tab Management

    /// <summary>
    /// Switches to another tab (Code or Preview).
    /// Saves the current editor content before switching to ensure no data loss.
    /// </summary>
    /// <param name="newTab">The tab to switch to</param>
    private async Task SwitchTab(EditorTab newTab)
    {
        // Do nothing if already on the requested tab
        if (currentTab == newTab)
            return;

        // Save current content from Monaco editor before switching tabs
        // This ensures we don't lose any unsaved changes
        if (currentTab == EditorTab.Code && codeEditor != null)
        {
            currentCode = await codeEditor.GetValue();
        }

        // Update current tab and refresh UI
        currentTab = newTab;
        StateHasChanged();
    }

    #endregion

    #region Event Handlers

    /// <summary>
    /// Callback invoked when code changes in the Monaco editor.
    /// This is triggered on every keystroke in the editor.
    /// Updates the internal state and notifies parent component.
    /// </summary>
    /// <param name="newCode">The new code content from the editor</param>
    private async Task OnCodeContentChanged(string newCode)
    {
        currentCode = newCode;
        await OnContentChanged.InvokeAsync(currentCode);
    }

    /// <summary>
    /// Callback invoked when a property is selected from the Model Properties panel.
    /// Inserts the property's Razor expression (e.g., "@Model.FirstName") at the cursor position.
    /// </summary>
    /// <param name="property">The property that was selected</param>
    private async Task OnPropertySelected(ModelPropertyInfo property)
    {
        // Only insert if we're on the Code tab and editor is available
        if (currentTab == EditorTab.Code && codeEditor != null)
        {
            await codeEditor.InsertTextAtCursor(property.RazorExpression);
        }
    }

    /// <summary>
    /// Callback invoked when the Save button is clicked.
    /// Retrieves the current content and notifies the parent component.
    /// </summary>
    private async Task OnSaveClicked()
    {
        // Get latest content from editor if we're on Code tab
        // This ensures we save the most recent changes
        if (currentTab == EditorTab.Code && codeEditor != null)
        {
            currentCode = await codeEditor.GetValue();
        }

        // Notify parent component to save the template
        await OnSave.InvokeAsync(currentCode);
    }

    #endregion

    #region Demo Data

    /// <summary>
    /// Creates demo data for preview rendering.
    /// Uses reflection to create an instance with reasonable default values for each property type.
    /// This allows the preview to work even before the user provides actual data.
    /// </summary>
    /// <returns>A new instance of TModel with demo values populated</returns>
    private TModel CreateDemoData()
    {
        // Create a new instance of the model
        var instance = new TModel();
        var type = typeof(TModel);

        // Iterate through all properties and set demo values based on type
        foreach (var prop in type.GetProperties())
        {
            // Skip read-only properties
            if (!prop.CanWrite)
                continue;

            try
            {
                // Set appropriate demo value based on property type
                if (prop.PropertyType == typeof(string))
                {
                    // For strings, use a descriptive demo value
                    prop.SetValue(instance, $"Demo {prop.Name}");
                }
                else if (prop.PropertyType == typeof(int))
                {
                    // For integers, use 42 as a common placeholder
                    prop.SetValue(instance, 42);
                }
                else if (prop.PropertyType == typeof(decimal))
                {
                    // For decimals, use a typical currency amount
                    prop.SetValue(instance, 99.99m);
                }
                else if (prop.PropertyType == typeof(DateTime))
                {
                    // For dates, use current date/time
                    prop.SetValue(instance, DateTime.Now);
                }
                else if (prop.PropertyType == typeof(bool))
                {
                    // For booleans, default to true
                    prop.SetValue(instance, true);
                }
                // Other types are left with their default values
            }
            catch
            {
                // Silently ignore any errors when setting demo data
                // This handles cases like complex types or properties with validation
            }
        }

        return instance;
    }

    #endregion

    #region Public API

    /// <summary>
    /// Gets the current editor content
    /// </summary>
    public async Task<string> GetContent()
    {
        if (currentTab == EditorTab.Code && codeEditor != null)
        {
            return await codeEditor.GetValue();
        }

        return currentCode;
    }

    /// <summary>
    /// Sets the editor content
    /// </summary>
    public async Task SetContent(string content)
    {
        currentCode = content;

        if (currentTab == EditorTab.Code && codeEditor != null)
        {
            await codeEditor.SetValue(content);
        }

        StateHasChanged();
    }

    #endregion
}

/// <summary>
/// Editor tabs
/// </summary>
public enum EditorTab
{
    Code,
    Preview
}
