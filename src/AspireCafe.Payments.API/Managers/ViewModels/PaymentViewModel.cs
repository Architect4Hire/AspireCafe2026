using System.ComponentModel.DataAnnotations;

namespace AspireCafe.Payments.API.Managers.ViewModels;

public class PaymentViewModel
{
    [Required]
    public Guid OrderId { get; set; }

    [Range(1, 999)]
    public int TableNumber { get; set; }

    [Range(0.0, 99999.99)]
    public decimal Subtotal { get; set; }

    [Range(0.0, 99999.99)]
    public decimal TaxAmount { get; set; }

    /// <summary>Tip as a percent of subtotal (0-100). Mutually exclusive with TipAmount.</summary>
    [Range(0.0, 100.0)]
    public decimal? TipPercent { get; set; }

    /// <summary>Explicit tip amount. Mutually exclusive with TipPercent.</summary>
    [Range(0.0, 99999.99)]
    public decimal? TipAmount { get; set; }

    [Required]
    public string Method { get; set; } = "CreditCard";

    /// <summary>For card payments — last 4 digits only. NEVER send the full PAN.</summary>
    [StringLength(4, MinimumLength = 4)]
    public string Last4 { get; set; } = string.Empty;
}

public class TipCalculationRequestViewModel
{
    [Range(0.0, 99999.99)]
    public decimal Subtotal { get; set; }
}
