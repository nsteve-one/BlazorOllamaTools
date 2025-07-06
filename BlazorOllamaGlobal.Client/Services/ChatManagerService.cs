using System.Text.Json;
using BlazorOllamaGlobal.Client.Components;
using BlazorOllamaGlobal.Client.Models.Chats;

namespace BlazorOllamaGlobal.Client.Services;

public class ChatManagerService
{
    private readonly OllamaService ollamaService;
    private readonly OpenAIService openAIService;
    private readonly ToolService toolService;
    private readonly TileService tileService;

    public ChatManagerService(OllamaService ollamaService, ToolService toolService, TileService tileService, OpenAIService openAIService)
    {
        this.ollamaService = ollamaService;
        this.toolService = toolService;
        this.tileService = tileService;
        this.openAIService = openAIService;
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
                    new ChatMessage { Role = "system", Content = @"You are a pleasant, helpful, AI assistant prepared to help with any questions. 
                                                                        IMPORTANT FORMATTING INSTRUCTIONS:
                                                                        1. When you need to use a tool, ALWAYS use the tool_calls field, NEVER include tool calls in the content field.
                                                                        2. NEVER use formats like <tool_call> or similar HTML-like tags.
                                                                        3. Leave the content field empty when making tool calls.
                                                                        4. Wait for tool results before continuing.

                                                                        CORRECT FORMAT EXAMPLE:
                                                                        {
                                                                          ""role"": ""assistant"",
                                                                          ""content"": """",
                                                                          ""tool_calls"": [ {
                                                                            ""function"": {
                                                                              ""name"": ""CreateNewNote"",
                                                                              ""arguments"": {
                                                                                ""content"": ""<p>hello world</p>"",
                                                                                ""title"": ""hello world""
                                                                              }
                                                                            }
                                                                          } ]
                                                                        }

                                                                        INCORRECT FORMATS (NEVER DO THESE):
                                                                        - DO NOT put tool calls in content: ""content"": ""I'll create a note using CreateNewNote...""
                                                                        - DO NOT use HTML tags: ""content"": ""<tool_call>CreateNewNote...</tool_call>""
                                                                        - DO NOT format arguments incorrectly: ""arguments"": ""title=hello, content=world"""},
                    new ChatMessage { Role = "user", Content = message }
                };
                ChatMessages.Add(chatID, request.Messages);
            }

            var tileHasContent = tileService.ActiveTiles.FirstOrDefault(x => x.IsExiting is not true) is not null;
            if (tileHasContent)
            {
                ChatMessages[chatID].Add(new ChatMessage { Role = "system", Content = $"Here is a JSON representation of the content on the current screen (this may or may not be useful or relevant): {tileService.ActiveTiles.FirstOrDefault(x =>
                    x.IsExiting is not true).GetAsJSON()}" });
            }
            
            var chatResponse = model.Contains("gpt")
                ? await openAIService.ChatAsync(request)
                : await ollamaService.ChatAsync(request);

            if (tileHasContent)
                ChatMessages[chatID].RemoveAt(ChatMessages[chatID].Count - 1);
            
            if (ChatMessages[chatID].Count > 10 && ChatMessages[chatID].Count % 5 == 0)
            {
                ChatMessages[chatID].Add(new ChatMessage { 
                    Role = "system", 
                    Content = "Reminder: When using tools, always put them in the tool_calls field with empty content, never in the content field. " +
                              "If you need to use a tool or the user asks for something, please use one. If you don't need to use a tool, please continue the conversation." +
                              "NEVER use formats like <tool_call> or similar HTML-like tags. NEVER include tool calls in the content field. " +
                              "ALWAYS RESPOND BY CALLING A TOOL when A TOOL SHOULD BE USED TO DO AN ACTION A USER ASKED FOR. TELLING THE USER YOU DID SOMETHING WITHOUT CALLING A TOOL IS WRONG. "
                });
            }
    
            // Process tool calls if they exist.
            if (chatResponse.ResponseMessage.ToolCalls != null && chatResponse.ResponseMessage.ToolCalls.Any())
            {
                ChatMessages[chatID].Add(new ChatMessage { Role = "assistant", Content = "", ToolCalls = chatResponse?.ResponseMessage?.ToolCalls });
                foreach (var toolCall in chatResponse.ResponseMessage.ToolCalls)
                {
                    var toolResult = await toolService.RunToolCalled(toolCall.Function.Name, toolCall.Function.Arguments);
                    ChatMessages[chatID].Add(new ChatMessage { Role = "system", Content = toolResult });
                    ChatMessages[chatID].Add(new ChatMessage { Role = "system", Content = "Assistant, ALWAYS tell the user if the tool was successful and it's result. Try to keep it less than 100 words. ALWAYS at least tell the user you finished using the tool. The user can see the result. Your next response should NOT be a tool and do NOT mention tool use." });
                    request.Messages = ChatMessages[chatID];
                    var chatAfterTool = model.Contains("gpt")
                        ? await openAIService.ChatAsync(request)
                        : await ollamaService.ChatAsync(request);
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