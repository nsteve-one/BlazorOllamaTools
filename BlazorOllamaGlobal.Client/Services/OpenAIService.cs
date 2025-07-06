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
        // Build OpenAI chat completion request
        var payload = new JsonObject
        {
            ["model"] = request.Model,
            ["messages"] = JsonSerializer.SerializeToNode(request.Messages),
            ["stream"] = false
        };
        if (request.Tools != null && request.Tools.Any())
        {
            payload["tools"] = JsonSerializer.SerializeToNode(request.Tools);
            payload["tool_choice"] = "auto";
        }

        var content = new StringContent(payload.ToJsonString(), Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync("v1/chat/completions", content);
        response.EnsureSuccessStatusCode();
        var rawJson = await response.Content.ReadAsStringAsync();

        var doc = JsonNode.Parse(rawJson)!.AsObject();
        var choice = doc["choices"]![0]!.AsObject();
        var message = choice["message"]!.AsObject();

        List<ToolCall>? parsedToolCalls = null;
        if (message["tool_calls"] is JsonArray callsArray)
        {
            parsedToolCalls = new List<ToolCall>();
            foreach (var callNode in callsArray)
            {
                var funcObj = callNode?["function"]?.AsObject();
                if (funcObj is null)
                    continue;

                var rawArgs = funcObj["arguments"]?.GetValue<string>() ?? "{}";
                JsonObject args;
                try
                {
                    args = JsonNode.Parse(rawArgs)?.AsObject() ?? new JsonObject();
                }
                catch
                {
                    args = new JsonObject();
                }

                parsedToolCalls.Add(new ToolCall
                {
                    Function = new ToolFunction
                    {
                        Name = funcObj["name"]!.GetValue<string>(),
                        Arguments = args
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
                ToolCalls = parsedToolCalls
            },
            Done = true,
            DoneReason = choice["finish_reason"]?.GetValue<string>() ?? string.Empty
        };
        return chatResponse;
    }
}
