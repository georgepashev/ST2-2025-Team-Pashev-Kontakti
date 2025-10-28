using System.Text.Json.Serialization;

namespace Kontakti.Services;
// OpenAI-compatible Chat Completions request
public class ChatCompletionsRequest
{
    [JsonPropertyName("model")]
    public string Model { get; set; } = "mistral-7b-instruct";
    [JsonPropertyName("messages")]
    public List<ChatMessage> Messages { get; set; } = new();
    [JsonPropertyName("temperature")]
    public double? Temperature { get; set; } = 0.2;
}
public class ChatMessage
{
    [JsonPropertyName("role")]
    public string Role { get; set; } = "user"; // "system" | "user" | "assistant"
    [JsonPropertyName("content")]
    public string Content { get; set; } = "";
}
// Minimal response we need
public class ChatCompletionsResponse
{
    [JsonPropertyName("choices")]
    public List<ChatChoice> Choices { get; set; } = new();
}
public class ChatChoice
{
    [JsonPropertyName("message")]
    public ChatMessage Message { get; set; } = new();
}