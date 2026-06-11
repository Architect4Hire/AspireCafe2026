using AspireCafe.Menu.API.Managers.Facades;
using AspireCafe.Menu.API.Managers.ServiceModels;
using AspireCafe.Menu.API.Managers.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace AspireCafe.Menu.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class MenuController(IMenuFacade facade) : ControllerBase
{
    /// <summary>Get the entire menu.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<MenuItemServiceModel>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get(CancellationToken ct)
        => Ok(await facade.GetMenuAsync(ct));

    /// <summary>Get menu items by category (Coffee, Tea, Pastry, Food).</summary>
    [HttpGet("category/{category}")]
    [ProducesResponseType(typeof(IEnumerable<MenuItemServiceModel>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByCategory(string category, CancellationToken ct)
        => Ok(await facade.GetByCategoryAsync(category, ct));

    /// <summary>Get a single menu item by id.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(MenuItemServiceModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var item = await facade.GetItemAsync(id, ct);
        return item is null ? NotFound() : Ok(item);
    }

    /// <summary>Create a new menu item.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(MenuItemServiceModel), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] MenuItemViewModel vm, CancellationToken ct)
    {
        var created = await facade.AddItemAsync(vm, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>Update an existing menu item.</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(MenuItemServiceModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] MenuItemViewModel vm, CancellationToken ct)
    {
        var updated = await facade.UpdateItemAsync(id, vm, ct);
        return updated is null ? NotFound() : Ok(updated);
    }

    /// <summary>Delete a menu item.</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
        => await facade.RemoveItemAsync(id, ct) ? NoContent() : NotFound();
}
