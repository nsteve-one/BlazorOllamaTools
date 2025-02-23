using BlazorOllamaGlobal.Client.Pages;
using BlazorOllamaGlobal.Client.Services;
using BlazorOllamaGlobal.Components;
using Radzen;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveWebAssemblyComponents();

builder.Services.AddScoped<OllamaService>(sp =>
{
    // BaseAddress should point to your Ollama instance.
    var httpClient = new HttpClient { BaseAddress = new Uri("http://localhost:11434") };
    return new OllamaService(httpClient);
});
builder.Services.AddScoped<ChatManagerService>();
builder.Services.AddScoped<ToolService>();
builder.Services.AddSingleton<TileService>();
builder.Services.AddRadzenComponents();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(BlazorOllamaGlobal.Client._Imports).Assembly);

app.Run();