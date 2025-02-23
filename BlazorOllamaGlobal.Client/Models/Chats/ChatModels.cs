using System.Text.Json;
using System.Text.Json.Serialization;

namespace BlazorOllamaGlobal.Client.Models.Chats;

/// <summary>
/// Represents the request to the Ollama chat endpoint.
/// </summary>
public class ChatRequest
{
    public string Model { get; set; }
    
    /// <summary>
    /// Conversation messages (e.g. user and assistant messages).
    /// </summary>
    public List<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
    
    /// <summary>
    /// Optional tool definitions that allow the model to instruct your client to execute functions.
    /// </summary>
    public List<ToolDefinition>? Tools { get; set; }
    
    /// <summary>
    /// Whether to use streaming mode. (For debugging, we set this to false.)
    /// </summary>
    public bool Stream { get; set; } = false;
}

/// <summary>
/// Represents a message in the request (for sending prompts).
/// </summary>
public class ChatMessage
{
    public string Role { get; set; }
    public string Content { get; set; }
}

/// <summary>
/// Represents a tool definition that you include in your chat request.
/// When provided, the model can return tool calls that instruct your client to execute a function.
/// </summary>
public class ToolDefinition
{
    public string Type { get; set; } = "function";
    public ToolFunction Function { get; set; }
}

/// <summary>
/// Describes the function that may be called by the model.
/// </summary>
public class ToolFunction
{
    public string Name { get; set; }
    public string Description { get; set; }
    public object? Parameters { get; set; }
}

/// <summary>
/// Represents a tool call returned by the model.
/// The raw arguments are provided as JSON.
/// </summary>
public class ToolCall
{
    public ToolFunction Function { get; set; }
    public JsonElement Arguments { get; set; }
}

/// <summary>
/// Represents the message in the response from the API.
/// Note that this includes the tool_calls field.
/// </summary>
public class ChatResponseMessage
{
    public string Role { get; set; }
    public string Content { get; set; }
    
    /// <summary>
    /// Optional list of tool calls issued by the model.
    /// </summary>
    [JsonPropertyName("tool_calls")]
    public List<ToolCall>? ToolCalls { get; set; }
}

/// <summary>
/// Represents the complete response from the Ollama API chat endpoint.
/// </summary>
public class ChatResponse
{
    public string Model { get; set; }
    
    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }
    
    /// <summary>
    /// The primary message object returned by the model, which includes tool_calls if any.
    /// </summary>
    [JsonPropertyName("message")]
    public ChatResponseMessage ResponseMessage { get; set; }
    
    public bool Done { get; set; }
    
    [JsonPropertyName("done_reason")]
    public string DoneReason { get; set; }
    
    // Additional fields from the response:
    [JsonPropertyName("total_duration")]
    public long TotalDuration { get; set; }
    
    [JsonPropertyName("load_duration")]
    public long LoadDuration { get; set; }
    
    [JsonPropertyName("prompt_eval_count")]
    public int PromptEvalCount { get; set; }
    
    [JsonPropertyName("prompt_eval_duration")]
    public long PromptEvalDuration { get; set; }
    
    [JsonPropertyName("eval_count")]
    public int EvalCount { get; set; }
    
    [JsonPropertyName("eval_duration")]
    public long EvalDuration { get; set; }
}

/// <summary>
/// (Optional) Represents a generate response if you use the /api/generate endpoint.
/// Not used in the chat flow.
/// </summary>
public class GenerateResponse
{
    public string Model { get; set; }
    public DateTime CreatedAt { get; set; }
    public string Response { get; set; }
    public bool Done { get; set; }
}
