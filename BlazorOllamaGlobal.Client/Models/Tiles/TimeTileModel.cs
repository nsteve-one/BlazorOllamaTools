using BlazorOllamaGlobal.Client.Components.Tiles;
using BlazorOllamaGlobal.Client.Models.Chats;
using Microsoft.AspNetCore.Components;

namespace BlazorOllamaGlobal.Client.Models.Tiles;

public class TimeTileModel : ITileModel
{
    public TimeTileModel(string toolResult)
    {
        Content = builder =>
        {
            builder.OpenComponent(0, typeof(TimeTile));
            builder.AddAttribute(1, "TimeToDisplay", DateTime.Parse(toolResult));
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