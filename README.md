# BlazorHtmlEditor

A Blazor component library for editing Razor templates with Monaco Editor and live preview using RazorLight.

[![NuGet](https://img.shields.io/nuget/v/BlazorHtmlEditor.svg)](https://www.nuget.org/packages/BlazorHtmlEditor/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

## Features

- 🎨 **Monaco Editor Integration** - Professional code editor with Razor syntax highlighting
- 🔍 **Live Preview** - Real-time template rendering using RazorLight
- 📋 **Model Properties Panel** - Easy insertion of `@Model` properties
- ⚡ **Simple Architecture** - Clean, easy-to-understand codebase
- 🚀 **No Complex Dependencies** - Just Monaco and RazorLight

## Installation

Install via NuGet Package Manager:

```bash
dotnet add package BlazorHtmlEditor
```

Or via Package Manager Console:

```powershell
Install-Package BlazorHtmlEditor
```

## Quick Start

### 1. Register Services

In your `Program.cs`:

```csharp
using BlazorHtmlEditor.Services;

builder.Services.AddScoped<IModelMetadataProvider, ModelMetadataProvider>();
builder.Services.AddScoped<IRazorRenderService, RazorRenderService>();
```

### 2. Add Required Scripts

In your `App.razor` or `index.html`:

```html
<body>
    <!-- Your Blazor app -->
    <script src="_framework/blazor.web.js"></script>

    <!-- Monaco Editor from CDN -->
    <script src="https://cdnjs.cloudflare.com/ajax/libs/monaco-editor/0.45.0/min/vs/loader.min.js"></script>
    <script>
        require.config({ paths: { vs: 'https://cdnjs.cloudflare.com/ajax/libs/monaco-editor/0.45.0/min/vs' } });
    </script>

    <!-- BlazorHtmlEditor scripts -->
    <script type="module" src="_content/BlazorHtmlEditor/js/monaco-interop.js"></script>
</body>
```

### 3. Add Component to Your Page

```razor
@page "/template-editor"
@using BlazorHtmlEditor.Components
@using YourApp.Models

<TemplateEditor TModel="Customer"
                Title="Customer Template Editor"
                InitialContent="@initialTemplate"
                ShowSaveButton="true"
                ShowPropertiesPanel="true"
                DefaultTab="EditorTab.Code"
                OnSave="HandleSave" />

@code {
    private string initialTemplate = @"
@model Customer
<div class='customer-card'>
    <h1>@Model.FirstName @Model.LastName</h1>
    <p>Email: @Model.Email</p>
    <p>Phone: @Model.Phone</p>
</div>
";

    private void HandleSave(string template)
    {
        // Save your template
        Console.WriteLine("Template saved!");
    }
}
```

### 4. Define Your Model

```csharp
public class Customer
{
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string Email { get; set; } = "";
    public string Phone { get; set; } = "";
}
```

### 5. Configure Your .csproj

Add these properties to enable RazorLight:

```xml
<PropertyGroup>
  <PreserveCompilationContext>true</PreserveCompilationContext>
  <CopyRefAssembliesToPublishDirectory>true</CopyRefAssembliesToPublishDirectory>
</PropertyGroup>
```

## Configuration

### TemplateEditor Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `TModel` | Generic Type | - | The model type for the template |
| `Title` | string | "Razor Template Editor" | Editor title |
| `InitialContent` | string | "" | Initial Razor template content |
| `ShowSaveButton` | bool | true | Show/hide the Save button |
| `ShowPropertiesPanel` | bool | true | Show/hide Model Properties panel |
| `DefaultTab` | EditorTab | Code | Default tab (Code or Preview) |
| `OnContentChanged` | EventCallback<string> | - | Fired when content changes |
| `OnSave` | EventCallback<string> | - | Fired when Save button clicked |

## Architecture

```
BlazorHtmlEditor/
├── Components/
│   ├── TemplateEditor.razor        # Main editor with Code/Preview tabs
│   ├── TemplatePreview.razor       # Live preview with RazorLight
│   ├── RazorCodeEditor.razor       # Monaco editor wrapper
│   └── ModelPropertiesPanel.razor  # Model properties browser
│
├── Services/
│   ├── IRazorRenderService.cs      # Template rendering interface
│   ├── RazorRenderService.cs       # RazorLight implementation
│   └── ModelMetadataProvider.cs    # Model reflection service
│
└── Models/
    ├── ModelPropertyInfo.cs         # Property metadata
    └── TemplateModelMeta.cs         # Model metadata
```

## Example Use Cases

### Email Templates

```csharp
public class EmailTemplate
{
    public string Subject { get; set; } = "";
    public string RecipientName { get; set; } = "";
    public string Message { get; set; } = "";
}
```

```razor
<TemplateEditor TModel="EmailTemplate"
                Title="Email Template Editor"
                InitialContent="@emailTemplate" />
```

### Invoice Templates

```csharp
public class Invoice
{
    public string InvoiceNumber { get; set; } = "";
    public DateTime InvoiceDate { get; set; }
    public string CustomerName { get; set; } = "";
    public decimal TotalAmount { get; set; }
}
```

```razor
<TemplateEditor TModel="Invoice"
                Title="Invoice Template Editor"
                InitialContent="@invoiceTemplate" />
```

## Requirements

- .NET 8.0 or higher
- Blazor Server or Blazor WebAssembly

## Dependencies

- `Microsoft.AspNetCore.Components.Web` (8.0.0)
- `RazorLight` (2.3.1)

## Known Limitations

- RazorLight requires `PreserveCompilationContext=true` in your project
- Complex Razor features (sections, layouts) not supported in preview
- Preview uses in-memory compilation (no file system access)

## Troubleshooting

### "Can't load metadata reference" Error

Add to your `.csproj`:

```xml
<PropertyGroup>
  <PreserveCompilationContext>true</PreserveCompilationContext>
  <CopyRefAssembliesToPublishDirectory>true</CopyRefAssembliesToPublishDirectory>
</PropertyGroup>
```

### Monaco Editor Not Loading

Ensure Monaco JS files are included in your project. The component uses CDN by default.

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Acknowledgments

- [Monaco Editor](https://microsoft.github.io/monaco-editor/) - Microsoft's code editor
- [RazorLight](https://github.com/toddams/RazorLight) - Razor template engine

## Support

- 🐛 [Report a bug](https://github.com/zcloudcz/BlazorHtmlEditor/issues)
- 💡 [Request a feature](https://github.com/zcloudcz/BlazorHtmlEditor/issues)
- 📧 Contact: [GitHub Issues](https://github.com/zcloudcz/BlazorHtmlEditor/issues)
