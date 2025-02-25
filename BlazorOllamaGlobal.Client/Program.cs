using BlazorOllamaGlobal.Client.Services;
using BlazorOllamaGlobal.Client.Services.Tiles;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Radzen;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddScoped<OllamaService>(sp =>
{
    // BaseAddress should point to your Ollama instance.
    var httpClient = new HttpClient { BaseAddress = new Uri("http://localhost:11434") };
    return new OllamaService(httpClient);
});
builder.Services.AddScoped<ChatManagerService>();
builder.Services.AddScoped<ToolService>();
builder.Services.AddSingleton<TileService>();
builder.Services.AddScoped<NoteService>();
builder.Services.AddRadzenComponents();

await builder.Build().RunAsync();