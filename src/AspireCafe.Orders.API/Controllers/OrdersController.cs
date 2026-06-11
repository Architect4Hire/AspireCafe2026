using AspireCafe.Orders.API.Managers.Facades;
using AspireCafe.Orders.API.Managers.ServiceModels;
using AspireCafe.Orders.API.Managers.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace AspireCafe.Orders.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class OrdersController(IOrderFacade facade) : ControllerBase
{
    /// <summary>Get all orders.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<OrderServiceModel>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken ct) => Ok(await facade.GetOrdersAsync(ct));

    /// <summary>Get active (not delivered/cancelled) orders for the kitchen.</summary>
    [HttpGet("active")]
    [ProducesResponseType(typeof(IEnumerable<OrderServiceModel>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetActive(CancellationToken ct) => Ok(await facade.GetActiveAsync(ct));

    /// <summary>Get all orders routed to a specific table number.</summary>
    [HttpGet("table/{tableNumber:int}")]
    [ProducesResponseType(typeof(IEnumerable<OrderServiceModel>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByTable(int tableNumber, CancellationToken ct)
        => Ok(await facade.GetByTableAsync(tableNumber, ct));

    /// <summary>Get a single order by id.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(OrderServiceModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var order = await facade.GetByIdAsync(id, ct);
        return order is null ? NotFound() : Ok(order);
    }

    /// <summary>Submit a new order routed to a table number.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(OrderServiceModel), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Submit([FromBody] OrderViewModel vm, CancellationToken ct)
    {
        try
        {
            var created = await facade.SubmitOrderAsync(vm, ct);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>Update an order's status (Submitted, Preparing, Ready, Delivered, Cancelled).</summary>
    [HttpPatch("{id:guid}/status")]
    [ProducesResponseType(typeof(OrderServiceModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] OrderStatusUpdateViewModel vm, CancellationToken ct)
    {
        try
        {
            var updated = await facade.ChangeStatusAsync(id, vm.Status, ct);
            return updated is null ? NotFound() : Ok(updated);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
