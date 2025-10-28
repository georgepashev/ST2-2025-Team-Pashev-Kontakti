namespace Kontakti.Services;
public class LlmOptions
{
    public string BaseUrl { get; set; } = "http://localhost:1234/v1";
    public string Model { get; set; } = "mistral-7b-instruct";
    public string? ApiKey { get; set; } = "lm-studio"; // често не е нужен реален ключ
    public int TimeoutSeconds { get; set; } = 30;
}