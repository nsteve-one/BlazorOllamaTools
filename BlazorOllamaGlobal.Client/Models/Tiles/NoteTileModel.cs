using BlazorOllamaGlobal.Client.Components.Tiles;
using BlazorOllamaGlobal.Client.Models.Chats;
using BlazorOllamaGlobal.Client.Models.ToolModels;
using Microsoft.AspNetCore.Components;

namespace BlazorOllamaGlobal.Client.Models.Tiles;

public class NoteTileModel : ITileModel
{
    private Note Note { get; set; }
    public NoteTileModel(Note note)
    {
        Note = note;
        Content = builder =>
        {
            builder.OpenComponent(0, typeof(NoteTile));
            builder.AddAttribute(1, "currentNote", Note);
            builder.CloseComponent();
        };
    }
    public Note GetNote()
    {
        return Note;
    }
    public RenderFragment Content { get; set; }
    public bool IsExiting { get; set; }
    public List<ToolDefinition> GetTileTools()
    {
        return new List<ToolDefinition>
        {
            new ToolDefinition
            {
                Function = new ToolFunction
                {
                    Name = "SaveNote",
                    Description = "Saves the current note for the user.",
                    Parameters = new { } // Define parameters schema if needed
                }
            }
        };
    }
}