using System.Text;
using System.Text.Json;
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
        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        var payloadJson = JsonSerializer.Serialize(request, options);
        var content = new StringContent(payloadJson, Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync("v1/chat/completions", content);
        response.EnsureSuccessStatusCode();
        var rawJson = await response.Content.ReadAsStringAsync();

        var chatResponse = JsonSerializer.Deserialize<ChatResponse>(rawJson, options);
        if (chatResponse == null)
        {
            throw new Exception("Failed to deserialize ChatResponse");
        }
        return chatResponse;
    }
}
