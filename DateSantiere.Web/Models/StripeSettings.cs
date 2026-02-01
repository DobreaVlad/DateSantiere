namespace DateSantiere.Web.Models;

public class StripeSettings
{
    public string PublishableKey { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty;
    public string WebhookSecret { get; set; } = string.Empty;
    
    public string BasicPriceId { get; set; } = string.Empty;
    public string ProPriceId { get; set; } = string.Empty;
    public string EnterprisePriceId { get; set; } = string.Empty;
}
