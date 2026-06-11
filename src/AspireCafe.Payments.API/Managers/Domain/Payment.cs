namespace AspireCafe.Payments.API.Managers.Domain;

public enum PaymentStatus
{
    Pending = 0,
    Authorized = 1,
    Captured = 2,
    Failed = 9
}

public enum PaymentMethod
{
    Cash = 0,
    CreditCard = 1,
    DebitCard = 2,
    MobileWallet = 3
}

public class Payment
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public int TableNumber { get; set; }
    public decimal Subtotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TipAmount { get; set; }
    public decimal TipPercent { get; set; }
    public decimal Total { get; set; }
    public PaymentMethod Method { get; set; }
    public PaymentStatus Status { get; set; }
    public string Last4 { get; set; } = string.Empty;
    public string AuthorizationCode { get; set; } = string.Empty;
    public DateTime CreatedUtc { get; set; }
    public DateTime UpdatedUtc { get; set; }
}
