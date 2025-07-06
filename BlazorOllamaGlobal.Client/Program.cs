using BlazorOllamaGlobal.Client.Services;
using BlazorOllamaGlobal.Client.Services.DataAccess;
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
builder.Services.AddScoped<OpenAIService>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var apiKey = config["OpenAI:ApiKey"] ?? string.Empty;
    var httpClient = new HttpClient { BaseAddress = new Uri("https://api.openai.com/") };
    if (!string.IsNullOrEmpty(apiKey))
    {
        httpClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);
    }
    return new OpenAIService(httpClient);
});
builder.Services.AddScoped(sp => 
    new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddScoped<ChatManagerService>();
builder.Services.AddScoped<ToolService>();
builder.Services.AddSingleton<TileService>();
builder.Services.AddScoped<NoteService>();
builder.Services.AddScoped<DapperClient>();
builder.Services.AddRadzenComponents();

await builder.Build().RunAsync();