using TicTacToe.Api.Enums;

namespace TicTacToe.Api.Models;

public class CreateGameRequest
{
    public GameMode Mode { get; set; }
}