using System.Text.Json;
using System.Text.Json.Nodes;
using BlazorOllamaGlobal.Client.Components.Tiles;
using BlazorOllamaGlobal.Client.Models.Chats;
using BlazorOllamaGlobal.Client.Models.Tiles;
using BlazorOllamaGlobal.Client.Models.ToolModels;
using BlazorOllamaGlobal.Client.Services.Tiles;

namespace BlazorOllamaGlobal.Client.Services;

public class ToolService
{
    
    public readonly TileService tileService;
    public readonly NoteService noteService;
    public ToolService(TileService tileService, NoteService noteService)
    {
        this.tileService = tileService;
        this.noteService = noteService;
    }
    public async Task<string> RunToolCalled(string toolName, JsonObject args)
    {
        string toolResult = string.Empty;
        switch (toolName)
        {
            case "GetCurrentTime":
                toolResult = GetCurrentTime();
                tileService.RequestTile(new TimeTileModel(toolResult));
                break;
            case "CreateNewNote":
                toolResult = "success";
                var NewNote = new Note() { Id = Guid.NewGuid() };
                
                NewNote.Title = args["title"]?.GetValue<string>() ?? "";
                NewNote.Content = args["content"]?.GetValue<string>() ?? "";
                
                tileService.RequestTile(new NoteTileModel(NewNote));
                break;
            case "SaveNote":
                var note =
                    (tileService.ActiveTiles.FirstOrDefault(x =>
                        x.IsExiting is not true && x is NoteTileModel) as NoteTileModel)?.GetNote();
                if (note is not null)
                {
                    await noteService.SaveNote(note);
                    toolResult = "Note saved successfully.";
                }
                else
                {
                    toolResult = "No note found to save.";
                }
                break;
            default:
                toolResult = "Unknown tool";
                break;
        }
        
        
        return $"Tool {toolName} was called. Tool result: {toolResult}. Always follow up with a short response to the user with the result of this call, as they cannot see this message.";
    }
    
    public string GetCurrentTime()
    {
        return DateTime.Now.ToString("T");
    }

    public async Task<List<ToolDefinition>> GetToolDefinitions()
    {
        var toolDefinitions = new List<ToolDefinition>
        {
            new ToolDefinition
            {
                Function = new ToolFunction
                {
                    Name = "GetCurrentTime",
                    Description = "Returns the current time on the server",
                    Parameters = new { } // Define parameters schema if needed
                }
            },
            new ToolDefinition()
            {
                Function = new ToolFunction()
                {
                    Name = "CreateNewNote",
                    Description = "Displays a new note to the user with an optional title and content. Do not add content unless explicitly requested by the user.",
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
        
        var activeTileTools = tileService.ActiveTiles.Where(x => x.IsExiting is not true).SelectMany(t => t.GetTileTools()).ToList();
        toolDefinitions.AddRange(activeTileTools);
        return toolDefinitions;
    }
}