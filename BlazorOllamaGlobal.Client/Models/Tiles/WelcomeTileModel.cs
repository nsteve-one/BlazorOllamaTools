using BlazorOllamaGlobal.Client.Components.Tiles;
using BlazorOllamaGlobal.Client.Models.Chats;
using Microsoft.AspNetCore.Components;

namespace BlazorOllamaGlobal.Client.Models.Tiles;

public class WelcomeTileModel : ITileModel
{
    public WelcomeTileModel()
    {
        Content = builder =>
        {
            builder.OpenComponent(0, typeof(WelcomeTile));
            builder.CloseComponent();
        };
    }
    public RenderFragment Content { get; set; }
    public bool IsExiting { get; set; }
    
    public List<ToolDefinition> GetTileTools()
    {
        return new List<ToolDefinition>();
    }
}