using BlazorOllamaGlobal.Client.Pages;
using BlazorOllamaGlobal.Client.Services;
using BlazorOllamaGlobal.Client.Services.DataAccess;
using BlazorOllamaGlobal.Client.Services.Tiles;
using BlazorOllamaGlobal.Components;
using BlazorOllamaGlobal.Services.DataAccess;
using Radzen;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveWebAssemblyComponents();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve;
        options.JsonSerializerOptions.DefaultIgnoreCondition = 
            System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    });
builder.Services.AddHttpClient();

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
builder.Services.AddScoped<IDapperService, DapperService>();
builder.Services.AddScoped<DapperClient>();
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
app.MapControllers();

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(BlazorOllamaGlobal.Client._Imports).Assembly);

app.Run();