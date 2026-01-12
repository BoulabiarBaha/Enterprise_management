public class GenerateServiceTokenRequest
{
    public string ServiceName { get; set; } = "n8n-agent";
    public int ExpiryInDays { get; set; } = 90;
}

