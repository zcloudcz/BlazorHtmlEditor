using BlazorHtmlEditor.Demo.Server.Components;
using BlazorHtmlEditor.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// This sets up Blazor Server with interactive components support
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Register BlazorHtmlEditor services
// IModelMetadataProvider: Provides reflection-based metadata about model properties
// IRazorRenderService: Handles Razor template compilation and rendering using RazorLight
builder.Services.AddScoped<IModelMetadataProvider, ModelMetadataProvider>();
builder.Services.AddScoped<IRazorRenderService, RazorRenderService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
