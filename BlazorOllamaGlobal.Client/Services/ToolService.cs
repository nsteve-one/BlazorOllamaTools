using BlazorOllamaGlobal.Client.Components;
using BlazorOllamaGlobal.Client.Models.Chats;

namespace BlazorOllamaGlobal.Client.Services;

public class ToolService
{
    
    public readonly TileService tileService;

    public ToolService(TileService tileService)
    {
        this.tileService = tileService;
    }
    public async Task<string> RunToolCalled(string toolName)
    {
        string toolResult = string.Empty;
        switch (toolName)
        {
            case "GetCurrentTime":
                toolResult = GetCurrentTime();
                tileService.RequestTile(builder =>
                {
                    builder.OpenComponent(0, typeof(TimePanel));
                    builder.AddAttribute(1, "TimeToDisplay", DateTime.Parse(toolResult));
                    builder.CloseComponent();
                });
                break;
            default:
                toolResult = "Unknown tool";
                break;
        }
        
        
        return $"Tool {toolName} was called. Tool result: {toolResult}. Always write a response to the user with the result of this call, as they cannot see this message.";
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
            }
        };
    }
}