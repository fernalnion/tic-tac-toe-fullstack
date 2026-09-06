using TicTacToe.Api.Enums;

namespace TicTacToe.Api.Models;
public class GameState
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public GameMode Mode { get; set; }
    public GameStatus Status { get; set; } = GameStatus.InProgress;
    public Player CurrentPlayer { get; set; } = Player.X;
    public Player? Winner { get; set; }
    public Player?[] Board { get; set; } = new Player?[9];
    public List<int> WinningCombination { get; set; } = new List<int>();
    public List<Move> MoveHistory { get; set; } = new List<Move>();
}