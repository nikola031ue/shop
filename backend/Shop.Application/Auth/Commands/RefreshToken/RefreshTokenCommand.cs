using MediatR;
using Shop.Application.Auth.Dtos;

namespace Shop.Application.Auth.Commands.RefreshToken;

public record RefreshTokenCommand(string AccessToken, string RefreshToken) : IRequest<TokenDto?>;
