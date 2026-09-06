using TicTacToe.Api.Enums;

namespace TicTacToe.Api.Models;

public class MakeMoveRequest
{
    public Player Player { get; set; }
    public int Row { get; set; }
    public int Column { get; set; }
}