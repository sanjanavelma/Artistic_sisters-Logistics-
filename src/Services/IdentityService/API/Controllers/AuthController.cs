using IdentityService.Application.Commands.Login;
using IdentityService.Application.Commands.Register;
using IdentityService.Application.Commands.RegisterDeliveryAgent;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace IdentityService.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;
    public AuthController(IMediator mediator) => _mediator = mediator;

    [HttpPost("register")]
    public async Task<IActionResult> Register(
        [FromBody] RegisterCommand command)
    {
        var result = await _mediator.Send(command);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [HttpPost("register-artist")]
    public async Task<IActionResult> RegisterArtist(
        [FromBody] IdentityService.Application.Commands.RegisterArtist.RegisterArtistCommand command)
    {
        var result = await _mediator.Send(command);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [HttpPost("register-agent")]
    public async Task<IActionResult> RegisterDeliveryAgent(
        [FromBody] RegisterDeliveryAgentCommand command)
    {
        var result = await _mediator.Send(command);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(
        [FromBody] LoginCommand command)
    {
        var result = await _mediator.Send(command);
        if (!result.Success) return Unauthorized(result);
        return Ok(result);
    }
}

