using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace BlazorOllamaGlobal.Client.Models.Chats;

/// <summary>
/// Represents the request to the Ollama chat endpoint.
/// </summary>
public class ChatRequest
{
    [JsonPropertyName("model")]
    [JsonProperty("model")]
    public string Model { get; set; }
    
    /// <summary>
    /// Conversation messages (e.g. user and assistant messages).
    /// </summary>
    [JsonPropertyName("messages")]
    [JsonProperty("messages")]
    public List<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
    
    /// <summary>
    /// Optional tool definitions that allow the model to instruct your client to execute functions.
    /// </summary>
    [JsonPropertyName("tools")]
    [JsonProperty("tools")]
    public List<ToolDefinition>? Tools { get; set; }
    
    /// <summary>
    /// Whether to use streaming mode. (For debugging, we set this to false.)
    /// </summary>
    [JsonPropertyName("stream")]
    [JsonProperty("stream")]
    public bool Stream { get; set; } = false;
}

/// <summary>
/// Represents a message in the request (for sending prompts).
/// </summary>
public class ChatMessage
{
    [JsonPropertyName("role")]
    [JsonProperty("role")]
    public string Role { get; set; }

    [JsonPropertyName("content")]
    [JsonProperty("content")]
    public string Content { get; set; }

    [JsonPropertyName("tool_calls")]
    [JsonProperty("tool_calls")]
    public List<ToolCall>? ToolCalls { get; set; }
}

/// <summary>
/// Represents a tool definition that you include in your chat request.
/// When provided, the model can return tool calls that instruct your client to execute a function.
/// </summary>
public class ToolDefinition
{
    [JsonPropertyName("type")]
    [JsonProperty("type")]
    public string Type { get; set; } = "function";

    [JsonPropertyName("function")]
    [JsonProperty("function")]
    public ToolFunction Function { get; set; }
}

/// <summary>
/// Describes the function that may be called by the model.
/// </summary>
public class ToolFunction
{
    [JsonPropertyName("name")]
    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonPropertyName("description")]
    [JsonProperty("description")]
    public string Description { get; set; }

    [JsonPropertyName("parameters")]
    [JsonProperty("parameters")]
    public object? Parameters { get; set; }
    
    [JsonPropertyName("arguments")]
    [JsonProperty("arguments")]
    public JsonObject Arguments { get; set; } = new JsonObject();
}

/// <summary>
/// Represents a tool call returned by the model.
/// The raw arguments are provided as JSON.
/// </summary>
public class ToolCall
{
    [JsonPropertyName("function")]
    [JsonProperty("function")]
    public ToolFunction Function { get; set; }
}

/// <summary>
/// Represents the message in the response from the API.
/// Note that this includes the tool_calls field.
/// </summary>
public class ChatResponseMessage
{
    [JsonPropertyName("role")]
    [JsonProperty("role")]
    public string Role { get; set; }

    [JsonPropertyName("content")]
    [JsonProperty("content")]
    public string Content { get; set; }
    
    /// <summary>
    /// Optional list of tool calls issued by the model.
    /// </summary>
    [JsonPropertyName("tool_calls")]
    [JsonProperty("tool_calls")]
    public List<ToolCall>? ToolCalls { get; set; }
}

/// <summary>
/// Represents the complete response from the Ollama API chat endpoint.
/// </summary>
public class ChatResponse
{
    [JsonPropertyName("model")]
    [JsonProperty("model")]
    public string Model { get; set; }
    
    [JsonPropertyName("created_at")]
    [JsonProperty("created_at")]
    public DateTime CreatedAt { get; set; }
    
    /// <summary>
    /// The primary message object returned by the model, which includes tool_calls if any.
    /// </summary>
    [JsonPropertyName("message")]
    [JsonProperty("message")]
    public ChatResponseMessage ResponseMessage { get; set; }
    
    [JsonPropertyName("done")]
    [JsonProperty("done")]
    public bool Done { get; set; }
    
    [JsonPropertyName("done_reason")]
    [JsonProperty("done_reason")]
    public string DoneReason { get; set; }
    
    // Additional fields from the response:
    [JsonPropertyName("total_duration")]
    [JsonProperty("total_duration")]
    public long TotalDuration { get; set; }
    
    [JsonPropertyName("load_duration")]
    [JsonProperty("load_duration")]
    public long LoadDuration { get; set; }
    
    [JsonPropertyName("prompt_eval_count")]
    [JsonProperty("prompt_eval_count")]
    public int PromptEvalCount { get; set; }
    
    [JsonPropertyName("prompt_eval_duration")]
    [JsonProperty("prompt_eval_duration")]
    public long PromptEvalDuration { get; set; }
    
    [JsonPropertyName("eval_count")]
    [JsonProperty("eval_count")]
    public int EvalCount { get; set; }
    
    [JsonPropertyName("eval_duration")]
    [JsonProperty("eval_duration")]
    public long EvalDuration { get; set; }
}

/// <summary>
/// (Optional) Represents a generate response if you use the /api/generate endpoint.
/// Not used in the chat flow.
/// </summary>
public class GenerateResponse
{
    [JsonPropertyName("model")]
    [JsonProperty("model")]
    public string Model { get; set; }

    [JsonPropertyName("created_at")]
    [JsonProperty("created_at")]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("response")]
    [JsonProperty("response")]
    public string Response { get; set; }

    [JsonPropertyName("done")]
    [JsonProperty("done")]
    public bool Done { get; set; }
}
