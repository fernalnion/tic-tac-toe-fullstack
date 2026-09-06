using TicTacToe.Api.Enums;
using TicTacToe.Api.Models;
using TicTacToe.Api.Services;

namespace TicTacToe.Tests.Services;

public class GameServiceTests
{
    private readonly GameService _gameService = new();

    private GameState CreateGame()
    {
        return _gameService.CreateGame(
            new CreateGameRequest
            {
                Mode = GameMode.PlayerVsPlayer
            });
    }

    private GameState CreateComputerGame()
    {
        return _gameService.CreateGame(
            new CreateGameRequest
            {
                Mode = GameMode.PlayerVsComputer
            });
    }

    private GameState Play(
        Guid gameId,
        Player player,
        int row,
        int column)
    {
        return _gameService.MakeMove(
            gameId,
            new MakeMoveRequest
            {
                Player = player,
                Row = row,
                Column = column
            });
    }

    [Fact]
    public void CreateGame_ShouldStartWithPlayerX()
    {
        var game = CreateGame();

        Assert.NotEqual(Guid.Empty, game.Id);
        Assert.Equal(GameMode.PlayerVsPlayer, game.Mode);
        Assert.Equal(Player.X, game.CurrentPlayer);
        Assert.Equal(GameStatus.InProgress, game.Status);
        Assert.Null(game.Winner);

        Assert.All(
            game.Board,
            cell => Assert.Null(cell));

        Assert.Empty(game.WinningCombination);
        Assert.Empty(game.MoveHistory);
    }

    [Fact]
    public void MakeMove_ShouldPlacePlayerAndSwitchTurn()
    {
        var game = CreateGame();

        var result = Play(
            game.Id,
            Player.X,
            0,
            0);

        Assert.Equal(Player.X, result.Board[0]);
        Assert.Equal(Player.O, result.CurrentPlayer);

        Assert.Single(result.MoveHistory);

        Assert.Equal(
            Player.X,
            result.MoveHistory[0].Player);

        Assert.Equal(
            0,
            result.MoveHistory[0].Row);

        Assert.Equal(
            0,
            result.MoveHistory[0].Column);
    }

    [Fact]
    public void MakeMove_OnOccupiedCell_ShouldThrowException()
    {
        var game = CreateGame();

        Play(
            game.Id,
            Player.X,
            0,
            0);

        Assert.Throws<InvalidOperationException>(() =>
            Play(
                game.Id,
                Player.O,
                0,
                0));
    }

    [Fact]
    public void MakeMove_ByWrongPlayer_ShouldThrowException()
    {
        var game = CreateGame();

        Assert.Throws<InvalidOperationException>(() =>
            Play(
                game.Id,
                Player.O,
                0,
                0));
    }

    [Fact]
    public void MakeMove_WithInvalidPosition_ShouldThrowException()
    {
        var game = CreateGame();

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            Play(
                game.Id,
                Player.X,
                3,
                0));
    }

    [Fact]
    public void MakeMove_WhenRowCompleted_ShouldDeclareWinner()
    {
        var game = CreateGame();

        Play(game.Id, Player.X, 0, 0);
        Play(game.Id, Player.O, 1, 0);
        Play(game.Id, Player.X, 0, 1);
        Play(game.Id, Player.O, 1, 1);

        var result =
            Play(game.Id, Player.X, 0, 2);

        Assert.Equal(GameStatus.Won, result.Status);
        Assert.Equal(Player.X, result.Winner);

        Assert.Equal(
            new List<int> { 0, 1, 2 },
            result.WinningCombination);

        Assert.Equal(
            Player.X,
            result.CurrentPlayer);
    }

    [Fact]
    public void MakeMove_WhenColumnCompleted_ShouldDeclareWinner()
    {
        var game = CreateGame();

        Play(game.Id, Player.X, 0, 0);
        Play(game.Id, Player.O, 0, 1);
        Play(game.Id, Player.X, 1, 0);
        Play(game.Id, Player.O, 1, 1);

        var result =
            Play(game.Id, Player.X, 2, 0);

        Assert.Equal(GameStatus.Won, result.Status);
        Assert.Equal(Player.X, result.Winner);

        Assert.Equal(
            new List<int> { 0, 3, 6 },
            result.WinningCombination);
    }

    [Fact]
    public void MakeMove_WhenDiagonalCompleted_ShouldDeclareWinner()
    {
        var game = CreateGame();

        Play(game.Id, Player.X, 0, 0);
        Play(game.Id, Player.O, 0, 1);
        Play(game.Id, Player.X, 1, 1);
        Play(game.Id, Player.O, 0, 2);

        var result =
            Play(game.Id, Player.X, 2, 2);

        Assert.Equal(GameStatus.Won, result.Status);
        Assert.Equal(Player.X, result.Winner);

        Assert.Equal(
            new List<int> { 0, 4, 8 },
            result.WinningCombination);
    }

    [Fact]
    public void MakeMove_WhenBoardFull_ShouldDeclareDraw()
    {
        var game = CreateGame();

        Play(game.Id, Player.X, 0, 0);
        Play(game.Id, Player.O, 0, 1);
        Play(game.Id, Player.X, 0, 2);

        Play(game.Id, Player.O, 1, 1);
        Play(game.Id, Player.X, 1, 0);
        Play(game.Id, Player.O, 1, 2);

        Play(game.Id, Player.X, 2, 1);
        Play(game.Id, Player.O, 2, 0);

        var result =
            Play(game.Id, Player.X, 2, 2);

        Assert.Equal(GameStatus.Draw, result.Status);
        Assert.Null(result.Winner);

        Assert.All(
            result.Board,
            cell => Assert.NotNull(cell));
    }

    [Fact]
    public void MakeMove_AfterGameCompleted_ShouldThrowException()
    {
        var game = CreateGame();

        Play(game.Id, Player.X, 0, 0);
        Play(game.Id, Player.O, 1, 0);
        Play(game.Id, Player.X, 0, 1);
        Play(game.Id, Player.O, 1, 1);
        Play(game.Id, Player.X, 0, 2);

        Assert.Throws<InvalidOperationException>(() =>
            Play(
                game.Id,
                Player.O,
                2,
                2));
    }

    [Fact]
    public void UndoMove_ShouldRemoveLastMoveAndRestorePreviousState()
    {
        var game = CreateGame();

        Play(game.Id, Player.X, 0, 0);
        Play(game.Id, Player.O, 1, 1);

        var result =
            _gameService.UndoMove(game.Id);

        Assert.Equal(
            Player.O,
            result.CurrentPlayer);

        Assert.Null(result.Board[4]);

        Assert.Single(result.MoveHistory);

        Assert.Equal(
            Player.X,
            result.MoveHistory[0].Player);

        Assert.Equal(
            GameStatus.InProgress,
            result.Status);

        Assert.Null(result.Winner);
    }

    [Fact]
    public void UndoMove_WithNoMoves_ShouldThrowException()
    {
        var game = CreateGame();

        Assert.Throws<InvalidOperationException>(() =>
            _gameService.UndoMove(game.Id));
    }

    [Fact]
    public void Undo_AfterCompletedGame_ShouldThrowException()
    {
        var game = CreateGame();

        Play(game.Id, Player.X, 0, 0);
        Play(game.Id, Player.O, 1, 0);
        Play(game.Id, Player.X, 0, 1);
        Play(game.Id, Player.O, 1, 1);
        Play(game.Id, Player.X, 0, 2);

        Assert.Equal(
            GameStatus.Won,
            game.Status);

        Assert.Throws<InvalidOperationException>(() =>
            _gameService.UndoMove(game.Id));
    }

    [Fact]
    public void ResetGame_ShouldStartFreshGameState()
    {
        var game = CreateGame();

        Play(game.Id, Player.X, 0, 0);
        Play(game.Id, Player.O, 1, 1);

        var originalId = game.Id;
        var originalMode = game.Mode;

        var result =
            _gameService.ResetGame(game.Id);

        // A fresh GameState instance is created.
        Assert.NotSame(game, result);

        // Game resource identity and selected mode remain unchanged.
        Assert.Equal(originalId, result.Id);
        Assert.Equal(originalMode, result.Mode);

        Assert.Equal(
            Player.X,
            result.CurrentPlayer);

        Assert.Equal(
            GameStatus.InProgress,
            result.Status);

        Assert.Null(result.Winner);

        Assert.All(
            result.Board,
            cell => Assert.Null(cell));

        Assert.Empty(result.WinningCombination);
        Assert.Empty(result.MoveHistory);

        // Ensure the dictionary now returns the reset state.
        var storedGame =
            _gameService.GetGameState(originalId);

        Assert.Same(result, storedGame);
    }

    [Fact]
    public void ResetGame_ForUnknownGame_ShouldThrowException()
    {
        var unknownGameId =
            Guid.NewGuid();

        Assert.Throws<KeyNotFoundException>(() =>
            _gameService.ResetGame(
                unknownGameId));
    }

    [Fact]
    public void Scoreboard_ShouldUpdateWhenPlayerWins()
    {
        var game = CreateGame();

        Play(game.Id, Player.X, 0, 0);
        Play(game.Id, Player.O, 1, 0);
        Play(game.Id, Player.X, 0, 1);
        Play(game.Id, Player.O, 1, 1);
        Play(game.Id, Player.X, 0, 2);

        var scoreboard =
            _gameService.GetScoreboard();

        Assert.Equal(
            1,
            scoreboard.PlayerXWins);

        Assert.Equal(
            0,
            scoreboard.PlayerOWins);

        Assert.Equal(
            0,
            scoreboard.Draws);
    }

    [Fact]
    public void Scoreboard_ShouldUpdateWhenGameDraws()
    {
        var game = CreateGame();

        Play(game.Id, Player.X, 0, 0);
        Play(game.Id, Player.O, 0, 1);
        Play(game.Id, Player.X, 0, 2);

        Play(game.Id, Player.O, 1, 1);
        Play(game.Id, Player.X, 1, 0);
        Play(game.Id, Player.O, 1, 2);

        Play(game.Id, Player.X, 2, 1);
        Play(game.Id, Player.O, 2, 0);
        Play(game.Id, Player.X, 2, 2);

        var scoreboard =
            _gameService.GetScoreboard();

        Assert.Equal(
            0,
            scoreboard.PlayerXWins);

        Assert.Equal(
            0,
            scoreboard.PlayerOWins);

        Assert.Equal(
            1,
            scoreboard.Draws);
    }

    [Fact]
    public void ResetGame_ShouldNotAffectScoreboard()
    {
        var game = CreateGame();

        Play(game.Id, Player.X, 0, 0);
        Play(game.Id, Player.O, 1, 0);
        Play(game.Id, Player.X, 0, 1);
        Play(game.Id, Player.O, 1, 1);
        Play(game.Id, Player.X, 0, 2);

        var scoreboardBeforeReset =
            _gameService.GetScoreboard();

        Assert.Equal(
            1,
            scoreboardBeforeReset.PlayerXWins);

        _gameService.ResetGame(game.Id);

        var scoreboardAfterReset =
            _gameService.GetScoreboard();

        Assert.Equal(
            1,
            scoreboardAfterReset.PlayerXWins);

        Assert.Equal(
            0,
            scoreboardAfterReset.PlayerOWins);

        Assert.Equal(
            0,
            scoreboardAfterReset.Draws);
    }

    [Fact]
    public void ResetScoreboard_ShouldClearAllScores()
    {
        var game = CreateGame();

        Play(game.Id, Player.X, 0, 0);
        Play(game.Id, Player.O, 1, 0);
        Play(game.Id, Player.X, 0, 1);
        Play(game.Id, Player.O, 1, 1);
        Play(game.Id, Player.X, 0, 2);

        var scoreboard =
            _gameService.ResetScoreboard();

        Assert.Equal(
            0,
            scoreboard.PlayerXWins);

        Assert.Equal(
            0,
            scoreboard.PlayerOWins);

        Assert.Equal(
            0,
            scoreboard.Draws);
    }

    [Fact]
    public void Scoreboard_ShouldUpdateOnlyOnceForCompletedGame()
    {
        var game = CreateGame();

        Play(game.Id, Player.X, 0, 0);
        Play(game.Id, Player.O, 1, 0);
        Play(game.Id, Player.X, 0, 1);
        Play(game.Id, Player.O, 1, 1);

        // X wins
        Play(game.Id, Player.X, 0, 2);

        var scoreboardAfterWin = _gameService.GetScoreboard();

        Assert.Equal(1, scoreboardAfterWin.PlayerXWins);
        Assert.Equal(0, scoreboardAfterWin.PlayerOWins);
        Assert.Equal(0, scoreboardAfterWin.Draws);

        // Further move must be rejected
        Assert.Throws<InvalidOperationException>(() =>
            Play(game.Id, Player.O, 2, 2));

        // Scoreboard must not increment again
        var scoreboardAfterRejectedMove = _gameService.GetScoreboard();

        Assert.Equal(1, scoreboardAfterRejectedMove.PlayerXWins);
        Assert.Equal(0, scoreboardAfterRejectedMove.PlayerOWins);
        Assert.Equal(0, scoreboardAfterRejectedMove.Draws);
    }

    [Fact]
    public void ComputerMode_ShouldAutomaticallyMakeOMove()
    {
        var game = CreateComputerGame();

        var result =
            Play(
                game.Id,
                Player.X,
                0,
                0);

        Assert.Equal(
            2,
            result.MoveHistory.Count);

        Assert.Equal(
            Player.X,
            result.MoveHistory[0].Player);

        Assert.Equal(
            Player.O,
            result.MoveHistory[1].Player);

        Assert.Equal(
            Player.X,
            result.CurrentPlayer);
    }

    [Fact]
    public void Computer_ShouldTakeCenterWhenAvailable()
    {
        var game = CreateComputerGame();

        var result =
            Play(
                game.Id,
                Player.X,
                0,
                0);

        Assert.Equal(
            Player.O,
            result.Board[4]);
    }

    [Fact]
    public void Computer_ShouldTakeCornerWhenCenterIsOccupied()
    {
        var game = CreateComputerGame();

        // Human takes center.
        var result =
            Play(
                game.Id,
                Player.X,
                1,
                1);

        // Computer should select the first available corner.
        Assert.Equal(
            Player.O,
            result.Board[0]);
    }

    [Fact]
    public void Computer_ShouldBlockPlayerXWinningMove()
    {
        var game = CreateComputerGame();

        // X takes 0, computer takes center 4.
        Play(
            game.Id,
            Player.X,
            0,
            0);

        // X takes 1, computer must block position 2.
        var result =
            Play(
                game.Id,
                Player.X,
                0,
                1);

        Assert.Equal(
            Player.O,
            result.Board[2]);
    }

    [Fact]
    public void Computer_ShouldTakeWinningMoveBeforeBlockingPlayerX()
    {
        var game = CreateComputerGame();

        /*
         * X -> 0
         * O -> 4 (center)
         *
         * X -> 1
         * O -> 2 (blocks X)
         *
         * X -> 6
         * O -> 3 (blocks X on 0,3,6)
         *
         * X -> 8
         *
         * O now has 3 and 4.
         * O can win immediately by taking 5.
         *
         * X also threatens 7 via 6,7,8,
         * but winning has higher priority than blocking.
         */

        Play(
            game.Id,
            Player.X,
            0,
            0);

        Play(
            game.Id,
            Player.X,
            0,
            1);

        Play(
            game.Id,
            Player.X,
            2,
            0);

        var result =
            Play(
                game.Id,
                Player.X,
                2,
                2);

        Assert.Equal(
            Player.O,
            result.Board[5]);

        Assert.Equal(
            GameStatus.Won,
            result.Status);

        Assert.Equal(
            Player.O,
            result.Winner);

        Assert.Equal(
            new List<int> { 3, 4, 5 },
            result.WinningCombination);
    }

    [Fact]
    public void ComputerMode_ShouldRejectManualPlayerOMove()
    {
        var game = CreateComputerGame();

        Assert.Throws<InvalidOperationException>(() =>
            Play(
                game.Id,
                Player.O,
                0,
                0));
    }

    [Fact]
    public void Undo_InComputerMode_ShouldRemoveHumanAndComputerMoves()
    {
        var game = CreateComputerGame();

        Play(
            game.Id,
            Player.X,
            0,
            0);

        Assert.Equal(
            2,
            game.MoveHistory.Count);

        var result =
            _gameService.UndoMove(
                game.Id);

        Assert.Empty(result.MoveHistory);

        Assert.All(
            result.Board,
            cell => Assert.Null(cell));

        Assert.Equal(
            Player.X,
            result.CurrentPlayer);

        Assert.Equal(
            GameStatus.InProgress,
            result.Status);

        Assert.Null(result.Winner);
    }
}