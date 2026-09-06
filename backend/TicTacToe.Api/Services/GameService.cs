using System.Collections.Concurrent;
using TicTacToe.Api.Enums;
using TicTacToe.Api.Models;

namespace TicTacToe.Api.Services;

public class GameService : IGameService
{
    private readonly ConcurrentDictionary<Guid, GameState> _games = new ConcurrentDictionary<Guid, GameState>();

    private static readonly int[][] _winningCombinations =
    [
        [0, 1, 2],
        [3, 4, 5],
        [6, 7, 8],

        [0, 3, 6],
        [1, 4, 7],
        [2, 5, 8],

        [0, 4, 8],
        [2, 4, 6]
    ];

    public GameState CreateGame(CreateGameRequest request)
    {
        var game = new GameState
        {
            Mode = request.Mode,
            Status = GameStatus.InProgress,
            CurrentPlayer = Player.X,
        };

        _games[game.Id] = game;
        return game;
    }

    public GameState? GetGameState(Guid gameId)
    {
        _games.TryGetValue(gameId, out var gameState);
        return gameState;
    }

    public GameState MakeMove(Guid gameId, MakeMoveRequest moveRequest)
    {
        if(!_games.TryGetValue(gameId, out var gameState))
        {
            throw new KeyNotFoundException($"Game with ID {gameId} not found.");
        }

        ValidateMove(gameState, moveRequest);

        var index = moveRequest.Row * 3 + moveRequest.Column;
        gameState.Board[index] = moveRequest.Player;

        gameState.MoveHistory.Add(new Move{
            MoveNumber = gameState.MoveHistory.Count + 1,
            Player = moveRequest.Player,
            Row = moveRequest.Row,
            Column = moveRequest.Column
        });

        if(TryGetWinningCombination(gameState.Board, moveRequest.Player, out var winningCombination))
        {
            gameState.Status = GameStatus.Won;
            gameState.Winner = moveRequest.Player;
            gameState.WinningCombination = winningCombination;
        }
        else if(gameState.Board.All(cell => cell.HasValue))
        {
            gameState.Status = GameStatus.Draw;
        }
        else
        {
            gameState.CurrentPlayer = moveRequest.Player == Player.X ? Player.O : Player.X;
        }

        return gameState;
    }

    private static void ValidateMove(GameState gameState, MakeMoveRequest moveRequest)
    {
        if(gameState.Status != GameStatus.InProgress)
        {
            throw new InvalidOperationException("Game already finished.");
        }

        if(moveRequest.Player != gameState.CurrentPlayer)
        {
            throw new InvalidOperationException($"It is {gameState.CurrentPlayer}'s turn.");
        }   

        if (moveRequest.Row < 0 || moveRequest.Row > 2)
        {
            throw new ArgumentOutOfRangeException(
                nameof(moveRequest.Row),
                "Row must be between 0 and 2.");
        }

        if (moveRequest.Column < 0 || moveRequest.Column > 2)
        {
            throw new ArgumentOutOfRangeException(
                nameof(moveRequest.Column),
                "Column must be between 0 and 2.");
        }

        var cellIndex = moveRequest.Row * 3 + moveRequest.Column;

        if(gameState.Board[cellIndex].HasValue)
        {
            throw new InvalidOperationException("Cell is already occupied.");
        }
    }

    private static bool TryGetWinningCombination(Player?[] board, Player player, out List<int> winningCombination)
    {
        foreach(var combination in _winningCombinations)
        {
            if(combination.All(index => board[index] == player))
            {
                winningCombination = combination.ToList();
                return true;
            }
        }
        winningCombination = new List<int>();
        return false;
    }
}