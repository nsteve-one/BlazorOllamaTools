using System.Text.Json;
using BlazorOllamaGlobal.Client.Components;
using BlazorOllamaGlobal.Client.Models.Chats;

namespace BlazorOllamaGlobal.Client.Services;

public class ChatManagerService
{
    private readonly OllamaService ollamaService;
    private readonly ToolService toolService;
    private readonly TileService tileService;

    public ChatManagerService(OllamaService ollamaService, ToolService toolService, TileService tileService)
    {
        this.ollamaService = ollamaService;
        this.toolService = toolService;
        this.tileService = tileService;
    }

    private Dictionary<string, List<ChatMessage>> ChatMessages { get; set; } = new();

    private string CurrentChatID { get; set; }

    public async Task<string> SendChat(string chatID, string message, string model = "qwen2.5:32b")
    {
        try
        {
            ChatRequest request;
            
            request = new ChatRequest
            {
                Model = model,
                Tools = await toolService.GetToolDefinitions(),
                Stream = false 
            };

            if (ChatMessages.ContainsKey(chatID))
            {
                ChatMessages[chatID].Add(new ChatMessage { Role = "user", Content = message });
                request.Messages = ChatMessages[chatID];
            }
            else
            {
                request.Messages = new List<ChatMessage>
                {
                    new ChatMessage { Role = "system", Content = "You are a pleasant, helpful, AI assistant prepared to help you with any questions the user may have. Please feel free to use tools to help the user with their questions when appropriate. Every time you call a tool, the user's screen will populate with info regarding the surrounding tool. ONLY use the tools provided. For reference, here is an example of a properly formatted tool call:   \"message\" : {\n    \"role\" : \"assistant\",\n    \"content\" : \"\",\n    \"tool_calls\" : [ {\n      \"function\" : {\n        \"name\" : \"CreateNewNote\",\n        \"arguments\" : {\n          \"content\" : \"<p>hello world</p>\",\n          \"title\" : \"hello world\"\n        }\n      }\n    } ]\n  },"},
                    new ChatMessage { Role = "user", Content = message }
                };
                ChatMessages.Add(chatID, request.Messages);
            }

            var tileHasContent = tileService.ActiveTiles.FirstOrDefault(x => x.IsExiting is not true) is not null;
            if (tileHasContent)
            {
                ChatMessages[chatID].Add(new ChatMessage { Role = "system", Content = $"A JSON representation of the content on the current screen: {tileService.ActiveTiles.FirstOrDefault(x =>
                    x.IsExiting is not true).GetAsJSON()}" });
            }
            
            var chatResponse = await ollamaService.ChatAsync(request);

            if (tileHasContent)
                ChatMessages[chatID].RemoveAt(ChatMessages[chatID].Count - 1);
            
    
            // Process tool calls if they exist.
            if (chatResponse.ResponseMessage.ToolCalls != null && chatResponse.ResponseMessage.ToolCalls.Any())
            {
                ChatMessages[chatID].Add(new ChatMessage { Role = "assistant", Content = JsonSerializer.Serialize(chatResponse?.ResponseMessage?.ToolCalls) });
                foreach (var toolCall in chatResponse.ResponseMessage.ToolCalls)
                {
                    var toolResult = await toolService.RunToolCalled(toolCall.Function.Name, toolCall.Function.Arguments);
                    ChatMessages[chatID].Add(new ChatMessage { Role = "system", Content = toolResult });
                    ChatMessages[chatID].Add(new ChatMessage { Role = "system", Content = "Please briefly tell the user what the result of the tool call was." });
                    request.Messages = ChatMessages[chatID];
                    var chatAfterTool = await ollamaService.ChatAsync(request);
                    ChatMessages[chatID].Add(new ChatMessage { Role = "assistant", Content = chatAfterTool.ResponseMessage.Content });
                    return chatAfterTool.ResponseMessage.Content;
                    
                }
    
                return null; //TODO: Need to make the model continue to recall to get a proper response.
            }
            else
            {
                ChatMessages[chatID].Add(new ChatMessage { Role = "assistant", Content = chatResponse.ResponseMessage.Content });
                // If no tool calls, display the regular response.
                return chatResponse.ResponseMessage.Content;
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
    
    
}