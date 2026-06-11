using AspireCafe.Payments.API.Managers.Facades;
using AspireCafe.Payments.API.Managers.ServiceModels;
using AspireCafe.Payments.API.Managers.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace AspireCafe.Payments.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class PaymentsController(IPaymentFacade facade) : ControllerBase
{
    /// <summary>Get all payments.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<PaymentServiceModel>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken ct) => Ok(await facade.GetAllAsync(ct));

    /// <summary>Get a payment by id.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(PaymentServiceModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var p = await facade.GetPaymentAsync(id, ct);
        return p is null ? NotFound() : Ok(p);
    }

    /// <summary>Get payments for a specific order.</summary>
    [HttpGet("order/{orderId:guid}")]
    [ProducesResponseType(typeof(IEnumerable<PaymentServiceModel>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByOrder(Guid orderId, CancellationToken ct)
        => Ok(await facade.GetPaymentsByOrderAsync(orderId, ct));

    /// <summary>
    /// Get suggested tip amounts for a given subtotal. Returns 15%, 18%, 20%, 25% options.
    /// </summary>
    [HttpGet("tip-suggestions")]
    [ProducesResponseType(typeof(TipSuggestionServiceModel), StatusCodes.Status200OK)]
    public IActionResult GetTipSuggestions([FromQuery] decimal subtotal)
        => Ok(facade.GetTipSuggestions(subtotal));

    /// <summary>Process a payment with automatic tip calculation.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(PaymentServiceModel), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Process([FromBody] PaymentViewModel vm, CancellationToken ct)
    {
        var processed = await facade.ProcessPaymentAsync(vm, ct);
        return CreatedAtAction(nameof(GetById), new { id = processed.Id }, processed);
    }
}
