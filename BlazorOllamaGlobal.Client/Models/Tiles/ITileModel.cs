using BlazorOllamaGlobal.Client.Models.Chats;
using Microsoft.AspNetCore.Components;

namespace BlazorOllamaGlobal.Client.Models.Tiles;

public interface ITileModel
{
    public RenderFragment Content { get; set; }
    public bool IsExiting { get; set; }
    public List<ToolDefinition> GetTileTools();
}