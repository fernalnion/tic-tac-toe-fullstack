using System.Collections.Concurrent;
using TicTacToe.Api.Enums;
using TicTacToe.Api.Models;

namespace TicTacToe.Api.Services;

public class GameService : IGameService
{
    private readonly ConcurrentDictionary<Guid, GameState> _games = new();
    private readonly Scoreboard _scoreboard = new();

    private static readonly int[][] WinningCombinations =
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

    private static readonly int[] Corners = [0, 2, 6, 8];

    public GameState CreateGame(CreateGameRequest request)
    {
        var game = new GameState
        {
            Mode = request.Mode,
            CurrentPlayer = Player.X,
            Status = GameStatus.InProgress
        };

        _games[game.Id] = game;

        return game;
    }

    public GameState? GetGameState(Guid gameId)
    {
        _games.TryGetValue(gameId, out var gameState);

        return gameState;
    }

    public GameState MakeMove(
        Guid gameId,
        MakeMoveRequest moveRequest)
    {
        if (!_games.TryGetValue(gameId, out var gameState))
        {
            throw new KeyNotFoundException(
                $"Game with ID {gameId} not found.");
        }

        ValidateMove(gameState, moveRequest);

        ApplyMove(
            gameState,
            moveRequest.Player,
            moveRequest.Row,
            moveRequest.Column);

        // If the human/user move completes the game,
        // do not switch turn or allow a computer move.
        if (CompleteGameStateIfRequired(
            gameState,
            moveRequest.Player))
        {
            return gameState;
        }

        gameState.CurrentPlayer =
            moveRequest.Player == Player.X
                ? Player.O
                : Player.X;

        // In computer mode the backend automatically
        // performs Player O's move.
        if (gameState.Mode == GameMode.PlayerVsComputer)
        {
            MakeComputerMove(gameState);
        }

        return gameState;
    }

    private static void ValidateMove(
        GameState gameState,
        MakeMoveRequest moveRequest)
    {
        if (gameState.Status != GameStatus.InProgress)
        {
            throw new InvalidOperationException(
                "Game is already completed.");
        }

        if (gameState.Mode == GameMode.PlayerVsComputer &&
            moveRequest.Player != Player.X)
        {
            throw new InvalidOperationException(
                "Only Player X can submit moves in computer mode.");
        }

        if (moveRequest.Player != gameState.CurrentPlayer)
        {
            throw new InvalidOperationException(
                $"It is Player {gameState.CurrentPlayer}'s turn.");
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

        var cellIndex =
            moveRequest.Row * 3 + moveRequest.Column;

        if (gameState.Board[cellIndex].HasValue)
        {
            throw new InvalidOperationException(
                "The selected cell is already occupied.");
        }
    }

    private static void ApplyMove(
        GameState gameState,
        Player player,
        int row,
        int column)
    {
        var cellIndex = row * 3 + column;

        gameState.Board[cellIndex] = player;

        gameState.MoveHistory.Add(new Move
        {
            MoveNumber = gameState.MoveHistory.Count + 1,
            Player = player,
            Row = row,
            Column = column
        });
    }

    private bool CompleteGameStateIfRequired(
        GameState gameState,
        Player player)
    {
        if (TryGetWinningCombination(
            gameState.Board,
            player,
            out var winningCombination))
        {
            gameState.Status = GameStatus.Won;
            gameState.Winner = player;
            gameState.WinningCombination = winningCombination;

            if (player == Player.X)
            {
                _scoreboard.PlayerXWins++;
            }
            else
            {
                _scoreboard.PlayerOWins++;
            }

            return true;
        }

        if (IsBoardFull(gameState))
        {
            gameState.Status = GameStatus.Draw;
            gameState.Winner = null;
            gameState.WinningCombination.Clear();

            _scoreboard.Draws++;

            return true;
        }

        return false;
    }

    private void MakeComputerMove(GameState gameState)
    {
        if (gameState.Status != GameStatus.InProgress)
        {
            return;
        }

        var cellIndex = GetBestComputerMove(gameState);

        var row = cellIndex / 3;
        var column = cellIndex % 3;

        ApplyMove(
            gameState,
            Player.O,
            row,
            column);

        if (CompleteGameStateIfRequired(
            gameState,
            Player.O))
        {
            return;
        }

        gameState.CurrentPlayer = Player.X;
    }

    private static int GetBestComputerMove(
        GameState gameState)
    {
        // Priority 1: Win if possible.
        var winningMove =
            FindWinningMove(
                gameState.Board,
                Player.O);

        if (winningMove.HasValue)
        {
            return winningMove.Value;
        }

        // Priority 2: Block Player X.
        var blockingMove =
            FindWinningMove(
                gameState.Board,
                Player.X);

        if (blockingMove.HasValue)
        {
            return blockingMove.Value;
        }

        // Priority 3: Take center.
        if (!gameState.Board[4].HasValue)
        {
            return 4;
        }

        // Priority 4: Take an available corner.
        foreach (var corner in Corners)
        {
            if (!gameState.Board[corner].HasValue)
            {
                return corner;
            }
        }

        // Priority 5: Take any remaining cell.
        for (var i = 0; i < gameState.Board.Length; i++)
        {
            if (!gameState.Board[i].HasValue)
            {
                return i;
            }
        }

        throw new InvalidOperationException(
            "No valid computer move is available.");
    }

    private static int? FindWinningMove(
        Player?[] board,
        Player player)
    {
        foreach (var combination in WinningCombinations)
        {
            var playerCells =
                combination.Count(
                    index => board[index] == player);

            var emptyCells =
                combination
                    .Where(index => !board[index].HasValue)
                    .ToList();

            if (playerCells == 2 &&
                emptyCells.Count == 1)
            {
                return emptyCells[0];
            }
        }

        return null;
    }

    private static bool TryGetWinningCombination(
        Player?[] board,
        Player player,
        out List<int> winningCombination)
    {
        foreach (var combination in WinningCombinations)
        {
            if (combination.All(
                index => board[index] == player))
            {
                winningCombination =
                    combination.ToList();

                return true;
            }
        }

        winningCombination = [];

        return false;
    }

    private static bool IsBoardFull(
        GameState gameState)
    {
        return gameState.Board.All(
            cell => cell.HasValue);
    }

    public GameState UndoMove(Guid gameId)
    {
        if (!_games.TryGetValue(gameId, out var gameState))
        {
            throw new KeyNotFoundException(
                $"Game with ID {gameId} not found.");
        }

        // Option A from the assessment:
        // Undo is disabled after completion.
        if (gameState.Status != GameStatus.InProgress)
        {
            throw new InvalidOperationException(
                "Undo is not allowed after the game is completed.");
        }

        if (gameState.MoveHistory.Count == 0)
        {
            throw new InvalidOperationException(
                "There are no moves to undo.");
        }

        if (gameState.Mode == GameMode.PlayerVsComputer)
        {
            /*
             * Computer mode:
             * remove the computer's previous O move and
             * the human's previous X move together.
             */

            var movesToUndo =
                Math.Min(
                    2,
                    gameState.MoveHistory.Count);

            for (var i = 0; i < movesToUndo; i++)
            {
                RemoveLastMove(gameState);
            }

            gameState.CurrentPlayer = Player.X;
        }
        else
        {
            /*
             * Player-vs-player:
             * remove exactly one move and return
             * the turn to the player whose move
             * was removed.
             */

            var removedPlayer =
                gameState.MoveHistory[^1].Player;

            RemoveLastMove(gameState);

            gameState.CurrentPlayer =
                removedPlayer;
        }

        gameState.Status = GameStatus.InProgress;
        gameState.Winner = null;
        gameState.WinningCombination.Clear();

        return gameState;
    }

    private static void RemoveLastMove(
        GameState gameState)
    {
        var lastMove =
            gameState.MoveHistory[^1];

        var cellIndex =
            lastMove.Row * 3 +
            lastMove.Column;

        gameState.Board[cellIndex] = null;

        gameState.MoveHistory.RemoveAt(
            gameState.MoveHistory.Count - 1);
    }

    public GameState ResetGame(Guid gameId)
    {
        if (!_games.TryGetValue(
            gameId,
            out var existingGame))
        {
            throw new KeyNotFoundException(
                $"Game with ID {gameId} not found.");
        }

        /*
         * Create a fresh game state while preserving:
         * - Existing game resource ID
         * - Selected game mode
         * - Session-level scoreboard
         */

        var resetGame = new GameState
        {
            Id = existingGame.Id,
            Mode = existingGame.Mode,
            CurrentPlayer = Player.X,
            Status = GameStatus.InProgress,
            Winner = null
        };

        _games[gameId] = resetGame;

        return resetGame;
    }

    public Scoreboard GetScoreboard()
    {
        return _scoreboard;
    }

    public Scoreboard ResetScoreboard()
    {
        _scoreboard.PlayerXWins = 0;
        _scoreboard.PlayerOWins = 0;
        _scoreboard.Draws = 0;

        return _scoreboard;
    }
}