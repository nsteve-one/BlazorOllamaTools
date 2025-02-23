using System.Text.Json;
using System.Text.Json.Nodes;
using BlazorOllamaGlobal.Client.Components.Tiles;
using BlazorOllamaGlobal.Client.Models.Chats;
using BlazorOllamaGlobal.Client.Models.ToolModels;

namespace BlazorOllamaGlobal.Client.Services;

public class ToolService
{
    
    public readonly TileService tileService;

    public ToolService(TileService tileService)
    {
        this.tileService = tileService;
    }
    public async Task<string> RunToolCalled(string toolName, JsonObject args)
    {
        string toolResult = string.Empty;
        switch (toolName)
        {
            case "GetCurrentTime":
                toolResult = GetCurrentTime();
                tileService.RequestTile(builder =>
                {
                    builder.OpenComponent(0, typeof(TimeTile));
                    builder.AddAttribute(1, "TimeToDisplay", DateTime.Parse(toolResult));
                    builder.CloseComponent();
                });
                break;
            case "CreateNewNote":
                toolResult = "success";
                var NewNote = new Note() { Id = Guid.NewGuid() };
                
                NewNote.Title = args["title"]?.GetValue<string>() ?? "";
                NewNote.Content = args["content"]?.GetValue<string>() ?? "";
                
                
                tileService.RequestTile(builder =>
                {
                    builder.OpenComponent(0, typeof(NoteTile));
                    builder.AddAttribute(1, "currentNote", NewNote);
                    builder.CloseComponent();
                });
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
        return new List<ToolDefinition>
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
                    Description = "Displays a new note to the user with an optional title and content",
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
                                description = "The note content.",
                            }
                        }
                    }
                }
            }
        };
    }
}