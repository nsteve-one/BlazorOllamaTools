using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace BlazorOllamaGlobal.Client.Models.Chats;

/// <summary>
/// Represents the request payload for the OpenAI chat completions API.
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

    [JsonPropertyName("tool_choice")]
    [JsonProperty("tool_choice")]
    public string? ToolChoice { get; set; }
    
    /// <summary>
    /// Whether to use streaming mode. (For debugging, we set this to false.)
    /// </summary>
    [JsonPropertyName("stream")]
    [JsonProperty("stream")]
    public bool Stream { get; set; } = false;
    
    /// <summary>
    /// Additional options for generation.
    /// </summary>
    public ChatOptions Options { get; set; } = new ChatOptions();
}

public class ChatOptions
{
    /*[JsonPropertyName("num_keep")]
    public int NumKeep { get; set; } = 5;

    [JsonPropertyName("seed")]
    public int Seed { get; set; } = 42;

    [JsonPropertyName("num_predict")]
    public int NumPredict { get; set; } = 100;

    [JsonPropertyName("top_k")]
    public int TopK { get; set; } = 20;

    [JsonPropertyName("top_p")]
    public double TopP { get; set; } = 0.9;

    [JsonPropertyName("min_p")]
    public double MinP { get; set; } = 0.0;

    [JsonPropertyName("typical_p")]
    public double TypicalP { get; set; } = 0.7;

    [JsonPropertyName("repeat_last_n")]
    public int RepeatLastN { get; set; } = 33;

    [JsonPropertyName("temperature")]
    public double Temperature { get; set; } = 0.8;

    [JsonPropertyName("repeat_penalty")]
    public double RepeatPenalty { get; set; } = 1.2;

    [JsonPropertyName("presence_penalty")]
    public double PresencePenalty { get; set; } = 1.5;

    [JsonPropertyName("frequency_penalty")]
    public double FrequencyPenalty { get; set; } = 1.0;

    [JsonPropertyName("mirostat")]
    public int Mirostat { get; set; } = 1;

    [JsonPropertyName("mirostat_tau")]
    public double MirostatTau { get; set; } = 0.8;

    [JsonPropertyName("mirostat_eta")]
    public double MirostatEta { get; set; } = 0.6;

    [JsonPropertyName("penalize_newline")]
    public bool PenalizeNewline { get; set; } = true;

    [JsonPropertyName("stop")]
    public List<string> Stop { get; set; } = new List<string> { "\n", "user:" };*/

    // <-- This is the context size parameter
    /*
    [JsonPropertyName("num_ctx")]
    public int NumCtx { get; set; } = 65536;
    */

    // ... Other options (num_batch, num_gpu, etc.) can be added similarly.
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
    public string? Content { get; set; }

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
}

public class ToolCallFunction
{
    [JsonPropertyName("name")]
    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonPropertyName("arguments")]
    [JsonProperty("arguments")]
    public string Arguments { get; set; }
}

/// <summary>
/// Represents a tool call returned by the model.
/// The raw arguments are provided as JSON.
/// </summary>
public class ToolCall
{
    [JsonPropertyName("id")]
    [JsonProperty("id")]
    public string Id { get; set; }

    [JsonPropertyName("type")]
    [JsonProperty("type")]
    public string Type { get; set; }

    [JsonPropertyName("function")]
    [JsonProperty("function")]
    public ToolCallFunction Function { get; set; }
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
    public string? Content { get; set; }

    /// <summary>
    /// Optional list of tool calls issued by the model.
    /// </summary>
    [JsonPropertyName("tool_calls")]
    [JsonProperty("tool_calls")]
    public List<ToolCall>? ToolCalls { get; set; }
}

/// <summary>
/// Represents the complete response from the OpenAI chat completions API.
/// </summary>
public class ChatResponse
{
    [JsonPropertyName("id")]
    [JsonProperty("id")]
    public string Id { get; set; }

    [JsonPropertyName("object")]
    [JsonProperty("object")]
    public string Object { get; set; }

    [JsonPropertyName("created")]
    [JsonProperty("created")]
    public long Created { get; set; }

    [JsonPropertyName("model")]
    [JsonProperty("model")]
    public string Model { get; set; }

    [JsonPropertyName("choices")]
    [JsonProperty("choices")]
    public List<ChatChoice> Choices { get; set; } = new();

    [JsonPropertyName("usage")]
    [JsonProperty("usage")]
    public Usage? Usage { get; set; }

    [System.Text.Json.Serialization.JsonIgnore]
    public ChatResponseMessage ResponseMessage => Choices.FirstOrDefault()?.Message;

    [System.Text.Json.Serialization.JsonIgnore]
    public string FinishReason => Choices.FirstOrDefault()?.FinishReason ?? string.Empty;
}

public class ChatChoice
{
    [JsonPropertyName("index")]
    [JsonProperty("index")]
    public int Index { get; set; }

    [JsonPropertyName("message")]
    [JsonProperty("message")]
    public ChatResponseMessage Message { get; set; }

    [JsonPropertyName("finish_reason")]
    [JsonProperty("finish_reason")]
    public string FinishReason { get; set; }
}

public class Usage
{
    [JsonPropertyName("prompt_tokens")]
    [JsonProperty("prompt_tokens")]
    public int PromptTokens { get; set; }

    [JsonPropertyName("completion_tokens")]
    [JsonProperty("completion_tokens")]
    public int CompletionTokens { get; set; }

    [JsonPropertyName("total_tokens")]
    [JsonProperty("total_tokens")]
    public int TotalTokens { get; set; }
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
