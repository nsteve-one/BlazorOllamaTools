using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using BlazorOllamaGlobal.Client.Models.Chats;

namespace BlazorOllamaGlobal.Client.Services;

public class OpenAIService
{
    private readonly HttpClient _httpClient;

    public OpenAIService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ChatResponse> ChatAsync(ChatRequest request)
    {
        request.Stream = false;

        // Manually build the request JSON using the property names expected by the
        // OpenAI Chat Completions API. Using our chat models directly results in
        // Pascal-cased property names which cause a BadRequest response.

        var messagesArray = new JsonArray();
        foreach (var msg in request.Messages)
        {
            var msgObj = new JsonObject
            {
                ["role"] = msg.Role,
                ["content"] = msg.Content
            };

            if (msg.ToolCalls != null && msg.ToolCalls.Any())
            {
                var tcArray = new JsonArray();
                foreach (var tc in msg.ToolCalls)
                {
                    var tcObj = new JsonObject
                    {
                        ["type"] = "function",
                        ["function"] = new JsonObject
                        {
                            ["name"] = tc.Function.Name,
                            ["arguments"] = tc.Function.Arguments.ToJsonString()
                        }
                    };
                    tcArray.Add(tcObj);
                }
                msgObj["tool_calls"] = tcArray;
            }

            messagesArray.Add(msgObj);
        }

        var payload = new JsonObject
        {
            ["model"] = request.Model,
            ["messages"] = messagesArray,
            ["stream"] = false
        };

        if (request.Tools != null && request.Tools.Any())
        {
            var toolsArray = new JsonArray();
            foreach (var tool in request.Tools)
            {
                var toolObj = new JsonObject
                {
                    ["type"] = tool.Type,
                    ["function"] = new JsonObject
                    {
                        ["name"] = tool.Function.Name,
                        ["description"] = tool.Function.Description,
                        ["parameters"] = JsonSerializer.SerializeToNode(tool.Function.Parameters ?? new { })
                    }
                };
                toolsArray.Add(toolObj);
            }

            payload["tools"] = toolsArray;
            payload["tool_choice"] = "auto";
        }

        var content = new StringContent(payload.ToJsonString(), Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync("v1/chat/completions", content);
        response.EnsureSuccessStatusCode();
        var rawJson = await response.Content.ReadAsStringAsync();

        var doc = JsonNode.Parse(rawJson)!.AsObject();
        var choice = doc["choices"]![0]!.AsObject();
        var message = choice["message"]!.AsObject();

        var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        List<ToolCall>? toolCalls = null;
        if (message["tool_calls"] != null)
        {
            toolCalls = new List<ToolCall>();
            foreach (var tc in message["tool_calls"]!.AsArray())
            {
                var func = tc!["function"]!.AsObject();
                var argsStr = func["arguments"]!.GetValue<string>();
                var argsObj = JsonNode.Parse(argsStr)?.AsObject() ?? new JsonObject();
                toolCalls.Add(new ToolCall
                {
                    Function = new ToolFunction
                    {
                        Name = func["name"]!.GetValue<string>(),
                        Description = string.Empty,
                        Parameters = null,
                        Arguments = argsObj
                    }
                });
            }
        }

        var chatResponse = new ChatResponse
        {
            Model = doc["model"]!.GetValue<string>(),
            CreatedAt = DateTimeOffset.FromUnixTimeSeconds(doc["created"]!.GetValue<long>()).DateTime,
            ResponseMessage = new ChatResponseMessage
            {
                Role = message["role"]!.GetValue<string>(),
                Content = message["content"]?.GetValue<string>() ?? string.Empty,
                ToolCalls = toolCalls
            },
            Done = true,
            DoneReason = choice["finish_reason"]?.GetValue<string>() ?? string.Empty
        };
        return chatResponse;
    }
}
