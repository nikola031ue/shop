using MediatR;
using Shop.Application.Auth.Dtos;

namespace Shop.Application.Auth.Commands.Login;

public record LoginCommand(string Username, string Password) : IRequest<TokenDto?>;
