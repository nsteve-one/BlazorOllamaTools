using BlazorOllamaGlobal.Client.Components.Tiles;
using BlazorOllamaGlobal.Client.Models.Chats;
using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;

namespace BlazorOllamaGlobal.Client.Models.Tiles;

public class TimeTileModel : ITileModel
{
    private string TimeString { get; set; }
    public TimeTileModel(string toolResult)
    {
        TimeString = toolResult;
        Content = builder =>
        {
            builder.OpenComponent(0, typeof(TimeTile));
            builder.AddAttribute(1, "TimeToDisplay", DateTime.Parse(toolResult));
            builder.CloseComponent();
        };
    }
    public string GetAsJSON()
    {
        return JsonConvert.SerializeObject(new
        {
            Time = TimeString,
            TileName = "Time Tile"
        });
    }
    public RenderFragment Content { get; set; }
    public bool IsExiting { get; set; }
    public List<ToolDefinition> GetTileTools()
    {
        return new List<ToolDefinition>();
    }
}