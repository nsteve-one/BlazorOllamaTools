using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using BlazorOllamaGlobal.Client.Models.Chats;

namespace BlazorOllamaGlobal.Client.Services;
public class OllamaService
{
    private readonly HttpClient _httpClient;
    
    public OllamaService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }
    
    /// <summary>
    /// Sends a chat request (with optional tool definitions) to the Ollama API.
    /// This method forces non-streaming mode to return a single JSON object for easier debugging.
    /// </summary>
    public async Task<ChatResponse> ChatAsync(ChatRequest request)
    {
        // Force non-streaming mode so we receive a complete JSON object.
        request.Stream = false;
        
        // Post the request to the /api/chat endpoint.
        var response = await _httpClient.PostAsJsonAsync("/api/chat", request);
        response.EnsureSuccessStatusCode();
        
        // Read the raw JSON response and log it.
        string rawJson = await response.Content.ReadAsStringAsync();
        Console.WriteLine("Raw JSON response: " + rawJson);
        
        // Use case-insensitive deserialization to avoid mismatches in property names.
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true
        };
        
        var chatResponse = JsonSerializer.Deserialize<ChatResponse>(rawJson, options);
        if (chatResponse == null)
        {
            throw new Exception("Failed to deserialize ChatResponse. Raw response was: " + rawJson);
        }
        return chatResponse;
    }
}
