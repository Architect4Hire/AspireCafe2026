namespace AspireCafe.Payments.API.Managers.ServiceModels;

public class PaymentServiceModel
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public int TableNumber { get; set; }
    public decimal Subtotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TipAmount { get; set; }
    public decimal TipPercent { get; set; }
    public decimal Total { get; set; }
    public string Method { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Last4 { get; set; } = string.Empty;
    public string AuthorizationCode { get; set; } = string.Empty;
    public DateTime CreatedUtc { get; set; }
}

public class TipSuggestionServiceModel
{
    public decimal Subtotal { get; set; }
    public List<TipOption> Options { get; set; } = new();
}

public class TipOption
{
    public decimal Percent { get; set; }
    public decimal Amount { get; set; }
    public string Label { get; set; } = string.Empty;
}
