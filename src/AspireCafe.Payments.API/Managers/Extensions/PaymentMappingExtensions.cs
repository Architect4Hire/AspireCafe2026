using AspireCafe.Payments.API.Managers.Domain;
using AspireCafe.Payments.API.Managers.ServiceModels;
using AspireCafe.Payments.API.Managers.ViewModels;

namespace AspireCafe.Payments.API.Managers.Extensions;

public static class PaymentMappingExtensions
{
    /// <summary>
    /// Calculates tip given either an explicit amount or percent.
    /// If neither is provided, defaults to 18%.
    /// </summary>
    public static (decimal tipAmount, decimal tipPercent) ResolveTip(decimal subtotal, decimal? tipPercent, decimal? tipAmount)
    {
        if (subtotal <= 0) return (0m, 0m);

        if (tipAmount is { } amt && amt > 0)
        {
            var pct = decimal.Round(amt / subtotal * 100m, 2);
            return (decimal.Round(amt, 2), pct);
        }

        var percent = tipPercent ?? 18m; // default suggested tip
        var calculated = decimal.Round(subtotal * (percent / 100m), 2);
        return (calculated, decimal.Round(percent, 2));
    }

    public static Payment ToDomain(this PaymentViewModel vm)
    {
        var (tipAmount, tipPercent) = ResolveTip(vm.Subtotal, vm.TipPercent, vm.TipAmount);
        var now = DateTime.UtcNow;
        var method = Enum.TryParse<PaymentMethod>(vm.Method, ignoreCase: true, out var m) ? m : PaymentMethod.CreditCard;

        return new Payment
        {
            Id = Guid.NewGuid(),
            OrderId = vm.OrderId,
            TableNumber = vm.TableNumber,
            Subtotal = decimal.Round(vm.Subtotal, 2),
            TaxAmount = decimal.Round(vm.TaxAmount, 2),
            TipAmount = tipAmount,
            TipPercent = tipPercent,
            Total = decimal.Round(vm.Subtotal + vm.TaxAmount + tipAmount, 2),
            Method = method,
            Status = PaymentStatus.Pending,
            Last4 = vm.Last4 ?? string.Empty,
            CreatedUtc = now,
            UpdatedUtc = now
        };
    }

    public static PaymentServiceModel ToServiceModel(this Payment p) => new()
    {
        Id = p.Id,
        OrderId = p.OrderId,
        TableNumber = p.TableNumber,
        Subtotal = p.Subtotal,
        TaxAmount = p.TaxAmount,
        TipAmount = p.TipAmount,
        TipPercent = p.TipPercent,
        Total = p.Total,
        Method = p.Method.ToString(),
        Status = p.Status.ToString(),
        Last4 = p.Last4,
        AuthorizationCode = p.AuthorizationCode,
        CreatedUtc = p.CreatedUtc
    };

    public static TipSuggestionServiceModel BuildTipSuggestions(decimal subtotal) => new()
    {
        Subtotal = decimal.Round(subtotal, 2),
        Options = new List<TipOption>
        {
            new() { Percent = 15m, Amount = decimal.Round(subtotal * 0.15m, 2), Label = "Good" },
            new() { Percent = 18m, Amount = decimal.Round(subtotal * 0.18m, 2), Label = "Great" },
            new() { Percent = 20m, Amount = decimal.Round(subtotal * 0.20m, 2), Label = "Excellent" },
            new() { Percent = 25m, Amount = decimal.Round(subtotal * 0.25m, 2), Label = "Outstanding" }
        }
    };
}
