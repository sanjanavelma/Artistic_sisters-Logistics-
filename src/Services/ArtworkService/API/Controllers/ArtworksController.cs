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
    public async Task<IActionResult> GetAll([FromQuery] string? artworkType = null, [FromQuery] string? medium = null)
    {
        var list = await _mediator.Send(new GetAllArtworksQuery { ArtworkType = artworkType, Medium = medium });
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

    [HttpDelete("{id}")]
    [Authorize(Roles = "Artist,Admin")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var cmd = new ArtworkService.Application.Commands.DeleteArtwork.DeleteArtworkCommand { ArtworkId = id };
        var result = await _mediator.Send(cmd);
        if (!result.Success) return NotFound(result);
        return Ok(result);
    }

    [HttpPatch("{id}/coming-soon")]
    [Authorize(Roles = "Artist,Admin")]
    public async Task<IActionResult> ToggleComingSoon(Guid id, [FromBody] ComingSoonRequest request)
    {
        var cmd = new ArtworkService.Application.Commands.ToggleComingSoon.ToggleComingSoonCommand
        {
            ArtworkId = id,
            IsComingSoon = request.IsComingSoon
        };
        var result = await _mediator.Send(cmd);
        if (!result.Success) return NotFound(result);
        return Ok(result);
    }
}

public record ComingSoonRequest(bool IsComingSoon);
