using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace BlazorHtmlEditor.Components;

/// <summary>
/// Code editor component using Monaco Editor with Razor syntax highlighting.
/// This component wraps the Monaco Editor (the editor used in VS Code) to provide
/// a rich code editing experience with IntelliSense-like features, syntax highlighting,
/// and various formatting options.
/// </summary>
public partial class RazorCodeEditor : IAsyncDisposable
{
    /// <summary>
    /// Gets or sets the code content displayed in the editor.
    /// This is a two-way binding parameter.
    /// </summary>
    [Parameter]
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the callback invoked when the code content changes.
    /// This is triggered on every edit in the editor.
    /// </summary>
    [Parameter]
    public EventCallback<string> OnCodeChanged { get; set; }

    /// <summary>
    /// Gets or sets the unique DOM element ID for this editor instance.
    /// This allows multiple editors on the same page.
    /// Defaults to a unique GUID-based ID.
    /// </summary>
    [Parameter]
    public string EditorId { get; set; } = $"monaco-editor-{Guid.NewGuid():N}";

    /// <summary>
    /// Reference to this component for JavaScript interop callbacks.
    /// Used to receive events from JavaScript (e.g., content changes).
    /// </summary>
    private DotNetObjectReference<RazorCodeEditor>? dotNetRef;

    /// <summary>
    /// Flag indicating whether the Monaco editor has been initialized.
    /// </summary>
    private bool isInitialized = false;

    /// <summary>
    /// Flag to prevent infinite loops when updating editor content.
    /// Set to true when updating from parameter to avoid triggering OnCodeChanged.
    /// </summary>
    private bool isUpdatingFromParameter = false;

    /// <summary>
    /// Lifecycle method called after the component has rendered.
    /// On first render, initializes the Monaco Editor with configuration options.
    /// </summary>
    /// <param name="firstRender">True if this is the first time the component is rendered</param>
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            try
            {
                // Create a reference to this component for JavaScript callbacks
                dotNetRef = DotNetObjectReference.Create(this);

                // Initialize Monaco editor with configuration
                // Monaco is the editor engine that powers VS Code
                await JSRuntime.InvokeVoidAsync("MonacoEditorInterop.createEditor", EditorId, new
                {
                    value = Code,                       // Initial code content
                    language = "html",                  // Language mode (HTML for Razor templates)
                    theme = "vs",                       // Visual Studio light theme
                    fontSize = 14,                      // Font size in pixels
                    lineNumbers = "on",                 // Show line numbers
                    renderWhitespace = "selection",     // Show whitespace when text is selected
                    scrollBeyondLastLine = false,       // Prevent scrolling past the last line
                    wordWrap = "on",                    // Enable word wrapping
                    autoIndent = "full",                // Automatic indentation
                    formatOnPaste = true,               // Auto-format when pasting
                    formatOnType = true,                // Auto-format while typing
                    tabSize = 4,                        // Tab size in spaces
                    insertSpaces = true,                // Use spaces instead of tabs
                    minimap = new { enabled = true },   // Show minimap (code overview)
                    automaticLayout = true              // Automatically adjust layout on resize
                }, dotNetRef);

                isInitialized = true;
                Console.WriteLine("Monaco Editor initialized successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error initializing Monaco editor: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Lifecycle method called when component parameters are set or changed.
    /// Updates the editor content if the Code parameter changes externally.
    /// </summary>
    protected override async Task OnParametersSetAsync()
    {
        // Only update if editor is initialized and we're not already updating
        if (isInitialized && !isUpdatingFromParameter)
        {
            try
            {
                // Get current editor value from JavaScript
                var currentValue = await JSRuntime.InvokeAsync<string>("MonacoEditorInterop.getValue", EditorId);

                // Only update if the new value is different
                if (currentValue != Code)
                {
                    // Set flag to prevent triggering OnCodeChanged callback
                    isUpdatingFromParameter = true;
                    await JSRuntime.InvokeVoidAsync("MonacoEditorInterop.setValue", EditorId, Code);
                    isUpdatingFromParameter = false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating editor value: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Callback method invoked from JavaScript when editor content changes.
    /// This method is called by the Monaco editor's onChange event handler.
    /// The [JSInvokable] attribute allows JavaScript code to call this .NET method.
    /// </summary>
    /// <param name="newValue">The new content from the editor</param>
    [JSInvokable]
    public async Task OnEditorContentChanged(string newValue)
    {
        // Only trigger callback if not updating from parameter
        // This prevents infinite loops when updating editor programmatically
        if (!isUpdatingFromParameter)
        {
            Code = newValue;
            await OnCodeChanged.InvokeAsync(newValue);
        }
    }

    /// <summary>
    /// Inserts text at the current cursor position in the editor.
    /// Used when user clicks on a model property to insert it into the template.
    /// </summary>
    /// <param name="text">The text to insert (typically a Razor expression like "@Model.PropertyName")</param>
    public async Task InsertTextAtCursor(string text)
    {
        if (isInitialized)
        {
            try
            {
                await JSRuntime.InvokeVoidAsync("MonacoEditorInterop.insertText", EditorId, text);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error inserting text: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Sets keyboard focus to the editor.
    /// Useful after inserting text to allow immediate continued editing.
    /// </summary>
    public async Task FocusEditor()
    {
        if (isInitialized)
        {
            try
            {
                await JSRuntime.InvokeVoidAsync("MonacoEditorInterop.focus", EditorId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error focusing editor: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Gets the current content from the editor.
    /// This method retrieves the latest content directly from the Monaco editor instance.
    /// </summary>
    /// <returns>The current editor content, or the Code parameter if editor is not initialized</returns>
    public async Task<string> GetValue()
    {
        if (isInitialized)
        {
            try
            {
                var value = await JSRuntime.InvokeAsync<string>("MonacoEditorInterop.getValue", EditorId);
                return value ?? Code; // Fallback to parameter if JS returns null
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting value: {ex.Message}");
                return Code; // Return parameter if JS call fails
            }
        }
        // If not initialized, return current parameter value
        return Code;
    }

    /// <summary>
    /// Sets the editor content programmatically.
    /// This method updates the Monaco editor with new content.
    /// </summary>
    /// <param name="value">The new content to set in the editor</param>
    public async Task SetValue(string value)
    {
        if (isInitialized)
        {
            try
            {
                // Set flag to prevent triggering OnCodeChanged callback
                isUpdatingFromParameter = true;
                await JSRuntime.InvokeVoidAsync("MonacoEditorInterop.setValue", EditorId, value);
                isUpdatingFromParameter = false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error setting value: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Disposes the Monaco editor and releases resources.
    /// This method is called automatically when the component is removed from the UI.
    /// Implements IAsyncDisposable pattern for proper cleanup.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        // Dispose Monaco editor instance
        if (isInitialized)
        {
            try
            {
                await JSRuntime.InvokeVoidAsync("MonacoEditorInterop.dispose", EditorId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error disposing editor: {ex.Message}");
            }
        }

        // Dispose .NET object reference used for JavaScript callbacks
        dotNetRef?.Dispose();
    }
}
