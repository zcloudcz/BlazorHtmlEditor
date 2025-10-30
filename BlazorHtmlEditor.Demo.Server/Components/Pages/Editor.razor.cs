using Microsoft.AspNetCore.Components;

namespace BlazorHtmlEditor.Demo.Server.Components.Pages;

/// <summary>
/// Demo page with Razor template editor.
/// This page demonstrates the TemplateEditor component functionality
/// with a sample customer model and handles template changes and saves.
/// </summary>
public partial class Editor
{
    /// <summary>
    /// Flag indicating whether to show the save notification toast.
    /// </summary>
    private bool showSaveNotification = false;

    /// <summary>
    /// The initial Razor template content shown when the editor loads.
    /// This template demonstrates how to access model properties and includes
    /// basic HTML structure with styling.
    /// </summary>
    private string initialTemplate = @"@model BlazorHtmlEditor.Demo.Server.Models.SampleModel

<!DOCTYPE html>
<html>
<head>
    <title>Customer Details</title>
    <style>
        body { font-family: Arial, sans-serif; margin: 20px; }
        .header { background-color: #007bff; color: white; padding: 20px; }
        .content { padding: 20px; }
        .field { margin: 10px 0; }
        .label { font-weight: bold; }
    </style>
</head>
<body>
    <div class=""header"">
        <h1>Customer Information</h1>
    </div>

    <div class=""content"">
        <div class=""field"">
            <span class=""label"">Name:</span> @Model.FirstName @Model.LastName
        </div>
        <div class=""field"">
            <span class=""label"">Email:</span> @Model.Email
        </div>
        <div class=""field"">
            <span class=""label"">Phone:</span> @Model.PhoneNumber
        </div>
        <div class=""field"">
            <span class=""label"">Address:</span> @Model.Address, @Model.City, @Model.PostalCode
        </div>
        <div class=""field"">
            <span class=""label"">Country:</span> @Model.Country
        </div>
        <div class=""field"">
            <span class=""label"">Birth Date:</span> @Model.BirthDate
        </div>
        <div class=""field"">
            <span class=""label"">Account Balance:</span> @Model.AccountBalance
        </div>
        <div class=""field"">
            <span class=""label"">Status:</span> @(Model.IsActive ? ""Active"" : ""Inactive"")
        </div>

        @if (!string.IsNullOrEmpty(Model.Notes))
        {
            <div class=""field"">
                <span class=""label"">Notes:</span>
                <p>@Model.Notes</p>
            </div>
        }
    </div>
</body>
</html>";

    /// <summary>
    /// Stores the current template content.
    /// Updated whenever the template is changed or saved.
    /// </summary>
    private string currentTemplate = string.Empty;

    /// <summary>
    /// Event handler called when the template content is changed in the editor.
    /// This is triggered on every keystroke in the editor.
    /// </summary>
    /// <param name="newTemplate">The new template content from the editor</param>
    private void OnTemplateChanged(string newTemplate)
    {
        currentTemplate = newTemplate;
        Console.WriteLine("Template changed");
    }

    /// <summary>
    /// Event handler called when the user clicks the Save button.
    /// In a production environment, this would save the template to a database.
    /// </summary>
    /// <param name="template">The template content to save</param>
    private async Task OnTemplateSaved(string template)
    {
        currentTemplate = template;

        // In production environment, this would save to the database
        Console.WriteLine("Template saved:");
        Console.WriteLine(template);

        // Show notification toast
        showSaveNotification = true;
        StateHasChanged();

        // Auto-hide notification after 3 seconds
        await Task.Delay(3000);
        showSaveNotification = false;
        StateHasChanged();
    }
}
