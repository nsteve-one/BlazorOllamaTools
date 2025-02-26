using BlazorOllamaGlobal.Client.Components.Tiles;
using BlazorOllamaGlobal.Client.Models.Chats;
using BlazorOllamaGlobal.Client.Models.ToolModels;
using Microsoft.AspNetCore.Components;

namespace BlazorOllamaGlobal.Client.Models.Tiles;

public class NoteTileModel : ITileModel
{
    private Note Note { get; set; }
    private NoteTile _noteTileReference;
    public NoteTileModel(Note note)
    {
        Note = note;
        Content = builder =>
        {
            builder.OpenComponent(0, typeof(NoteTile));
            builder.AddAttribute(1, "currentNote", Note);
            builder.AddAttribute(2, "OnNoteSaved", EventCallback.Factory.Create<Note>(this, HandleNoteSaved));
            builder.AddComponentReferenceCapture(3, capturedRef => _noteTileReference = (NoteTile)capturedRef);
            builder.CloseComponent();
        };
    }
    public Note GetNote()
    {
        return Note;
    }
    
    public void HandleNoteSaved(Note updatedNote)
    {
        Note = updatedNote;
        _noteTileReference?.StateChanged();
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
            },
            new ToolDefinition
            {
                Function = new ToolFunction
                {
                    Name = "EditCurrentNote",
                    Description = "Edits the current note on the screen for the user.",
                    Parameters = new
                    {
                        type = "object",
                        properties = new
                        {
                            Title = new
                            {
                                type = "string",
                                description = "The note title.",
                            },
                            Content = new
                            {
                                type = "string",
                                description = "The note content. Content should be in html format.",
                            }
                        }
                    }
                }
            }
        };
    }
}