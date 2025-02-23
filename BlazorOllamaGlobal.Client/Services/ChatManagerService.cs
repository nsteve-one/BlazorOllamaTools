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
                    new ChatMessage { Role = "user", Content = message }
                };
                ChatMessages.Add(chatID, request.Messages);
            }
        
            var chatResponse = await ollamaService.ChatAsync(request);
    
            // Process tool calls if they exist.
            if (chatResponse.ResponseMessage.ToolCalls != null && chatResponse.ResponseMessage.ToolCalls.Any())
            {
                foreach (var toolCall in chatResponse.ResponseMessage.ToolCalls)
                {
                   var toolResult = await toolService.RunToolCalled(toolCall.Function.Name);
                    ChatMessages[chatID].Add(new ChatMessage { Role = "system", Content = toolResult });
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