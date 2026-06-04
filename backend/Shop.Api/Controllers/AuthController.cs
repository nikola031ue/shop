using MediatR;
using Microsoft.AspNetCore.Mvc;
using Shop.Application.Auth.Commands.Login;
using Shop.Application.Auth.Commands.RefreshToken;

namespace Shop.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(IMediator mediator) : ControllerBase
{
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginCommand command, CancellationToken cancellationToken)
    {
        var token = await mediator.Send(command, cancellationToken);
        return token is null ? Unauthorized("Pogrešno korisničko ime ili lozinka.") : Ok(token);
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenCommand command, CancellationToken cancellationToken)
    {
        var token = await mediator.Send(command, cancellationToken);
        return token is null ? Unauthorized("Nevažeći ili istekli refresh token.") : Ok(token);
    }
}
