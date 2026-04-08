using ArtworkService.Application.Commands.AddArtwork;
using ArtworkService.Application.Commands.UpdateStock;
using ArtworkService.Application.Queries.GetAllArtworks;
using ArtworkService.Application.Queries.GetArtworkById;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ArtworkService.API.Controllers;

[ApiController]
[Route("api/artworks")]
public class ArtworksController : ControllerBase
{
    private readonly IMediator _mediator;
    public ArtworksController(IMediator mediator) => _mediator = mediator;

    [HttpPost]
    [Authorize(Roles = "Artist")]
    public async Task<IActionResult> Add([FromBody] AddArtworkCommand cmd)
    {
        var result = await _mediator.Send(cmd);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var list = await _mediator.Send(new GetAllArtworksQuery());
        return Ok(list);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var art = await _mediator.Send(new GetArtworkByIdQuery(id));
        if (art == null) return NotFound();
        return Ok(art);
    }

    [HttpPatch("{id}/stock")]
    [Authorize(Roles = "Artist")]
    public async Task<IActionResult> UpdateStock(Guid id, [FromBody] UpdateStockRequest request)
    {
        var cmd = new UpdateStockCommand { ArtworkId = id, Quantity = request.Quantity };
        var result = await _mediator.Send(cmd);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }
}
