using TicTacToe.Api.Enums;
using TicTacToe.Api.Models;
using TicTacToe.Api.Services;

namespace TicTacToe.Tests.Services;

public class GameServiceTests
{
    private readonly GameService _gameService = new GameService();

    private GameState CreateGame()
    {
        return _gameService.CreateGame(new CreateGameRequest{
            Mode = GameMode.PlayerVsPlayer
        });
    }

    [Fact]
    public void CreateGame_ShouldStartWithPlayerX()
    {
        var game = CreateGame();

        Assert.Equal(Player.X, game.CurrentPlayer);
        Assert.Equal(GameStatus.InProgress, game.Status);
        Assert.Null(game.Winner);
        Assert.All(game.Board, cell => Assert.Null(cell));
        Assert.Empty(game.WinningCombination);
        Assert.Empty(game.MoveHistory);
    }

    [Fact]
    public void MakeMove_ShouldPlacePlayerAndSwitchTurn()
    {
        var game = CreateGame();

        var result = _gameService.MakeMove(game.Id, new MakeMoveRequest
        {
            Player = Player.X,
            Row = 0,
            Column = 0
        });

        Assert.Equal(Player.X, result.Board[0]);
        Assert.Equal(Player.O, result.CurrentPlayer);

        Assert.Single(result.MoveHistory);

        Assert.Equal(Player.X, result.MoveHistory[0].Player);
        Assert.Equal(0, result.MoveHistory[0].Row);
        Assert.Equal(0, result.MoveHistory[0].Column);
    }

    [Fact]
    public void MakeMove_OnOccupiedCell_ShouldThrowException()
    {
        var game = CreateGame();

        _gameService.MakeMove(game.Id, new MakeMoveRequest
        {
            Player = Player.X,
            Row = 0,
            Column = 0
        });

        Assert.Throws<InvalidOperationException>(()=>
        _gameService.MakeMove(
            game.Id,
            new MakeMoveRequest
            {
                Player = Player.O,
                Row = 0,
                Column = 0
            }
        ));
    }

    [Fact]
    public void MakeMove_WhenRowCompleted_ShouldDeclareWinner()
    {
        var game = CreateGame();

        _gameService.MakeMove(game.Id, new MakeMoveRequest { Player = Player.X, Row = 0, Column = 0 });
        _gameService.MakeMove(game.Id, new MakeMoveRequest { Player = Player.O, Row = 1, Column = 0 });
        _gameService.MakeMove(game.Id, new MakeMoveRequest { Player = Player.X, Row = 0, Column = 1 });
        _gameService.MakeMove(game.Id, new MakeMoveRequest { Player = Player.O, Row = 1, Column = 1 });

        var result = _gameService.MakeMove(game.Id, new MakeMoveRequest { Player = Player.X, Row = 0, Column = 2 });

        Assert.Equal(GameStatus.Won, result.Status);
        Assert.Equal(Player.X, result.Winner);
        Assert.Equal(new List<int> { 0, 1, 2 }, result.WinningCombination);
        Assert.Equal(Player.X, result.CurrentPlayer);
    }

    [Fact]
    public void MakeMove_ByWrongPlayer_ShouldThrowException()
    {
        var game = CreateGame();

        Assert.Throws<InvalidOperationException>(()=>
        _gameService.MakeMove(game.Id, new MakeMoveRequest
        {
            Player = Player.O,
            Row = 0,
            Column = 0
        }));
    }

    [Fact]
    public void MakeMove_WithInvalidPosition_ShouldThrowException()
    {
        var game = CreateGame();

        Assert.Throws<ArgumentOutOfRangeException>(()=>
        _gameService.MakeMove(game.Id, new MakeMoveRequest
        {
            Player = Player.X,
            Row = 3,
            Column = 0
        }));
    }

    [Fact]
    public void MakeMove_WhenColumnCompleted_ShouldDeclareWinner()
    {
        var game = CreateGame();

        _gameService.MakeMove(game.Id, new MakeMoveRequest {Player = Player.X, Row = 0, Column = 0});
        _gameService.MakeMove(game.Id, new MakeMoveRequest {Player = Player.O, Row = 0, Column = 1});
        _gameService.MakeMove(game.Id, new MakeMoveRequest {Player = Player.X, Row = 1, Column = 0});
        _gameService.MakeMove(game.Id, new MakeMoveRequest {Player = Player.O, Row = 1, Column = 1});

        var result = _gameService.MakeMove(game.Id, new MakeMoveRequest {Player = Player.X, Row = 2, Column = 0});
        Assert.Equal(GameStatus.Won, result.Status);
        Assert.Equal(Player.X, result.Winner);
        Assert.Equal(new List<int> {0, 3, 6}, result.WinningCombination);
    }

    [Fact]
    public void MakeMove_WhenDiagonalCompleted_ShouldDeclareWinner()
    {
        var game = CreateGame();

        _gameService.MakeMove(game.Id, new MakeMoveRequest {Player = Player.X, Row = 0, Column = 0});
        _gameService.MakeMove(game.Id, new MakeMoveRequest {Player = Player.O, Row = 0, Column = 1});
        _gameService.MakeMove(game.Id, new MakeMoveRequest {Player = Player.X, Row = 1, Column = 1});
        _gameService.MakeMove(game.Id, new MakeMoveRequest {Player = Player.O, Row = 0, Column = 2});

        var result = _gameService.MakeMove(game.Id, new MakeMoveRequest {Player = Player.X, Row = 2, Column = 2});
        Assert.Equal(GameStatus.Won, result.Status);
        Assert.Equal(Player.X, result.Winner);
        Assert.Equal(new List<int> {0, 4, 8}, result.WinningCombination);
    }

    [Fact]
    public void MakeMove_WhenBoardFull_ShouldDeclareDraw()
    {
        var game = CreateGame();

        _gameService.MakeMove(game.Id, new MakeMoveRequest {Player = Player.X, Row = 0, Column = 0});
        _gameService.MakeMove(game.Id, new MakeMoveRequest {Player = Player.O, Row = 0, Column = 1});
        _gameService.MakeMove(game.Id, new MakeMoveRequest {Player = Player.X, Row = 0, Column = 2});

        _gameService.MakeMove(game.Id, new MakeMoveRequest {Player = Player.O, Row = 1, Column = 1});
        _gameService.MakeMove(game.Id, new MakeMoveRequest {Player = Player.X, Row = 1, Column = 0});
        _gameService.MakeMove(game.Id, new MakeMoveRequest {Player = Player.O, Row = 1, Column = 2});

        _gameService.MakeMove(game.Id, new MakeMoveRequest {Player = Player.X, Row = 2, Column = 1});
        _gameService.MakeMove(game.Id, new MakeMoveRequest {Player = Player.O, Row = 2, Column = 0});

        var result = _gameService.MakeMove(game.Id, new MakeMoveRequest {Player = Player.X, Row = 2, Column = 2});

        Assert.Equal(GameStatus.Draw, result.Status);
        Assert.Null(result.Winner);
        Assert.All(result.Board, cell => Assert.NotNull(cell));
    }

    [Fact]
    public void MakeMove_AfterGameCompleted_ShouldThrowException()
    {
        var game = CreateGame();

        _gameService.MakeMove(game.Id, new MakeMoveRequest {Player = Player.X, Row = 0, Column = 0});
        _gameService.MakeMove(game.Id, new MakeMoveRequest {Player = Player.O, Row = 1, Column = 0});
        _gameService.MakeMove(game.Id, new MakeMoveRequest {Player = Player.X, Row = 0, Column = 1});
        _gameService.MakeMove(game.Id, new MakeMoveRequest {Player = Player.O, Row = 1, Column = 1});
        _gameService.MakeMove(game.Id, new MakeMoveRequest {Player = Player.X, Row = 0, Column = 2});

        Assert.Throws<InvalidOperationException>(()=>
        _gameService.MakeMove(game.Id, new MakeMoveRequest
        {
            Player = Player.O,
            Row = 2,
            Column = 2
        }));
    }  

    [Fact]
    public void UndoMove_ShouldRemoveLastMoveAndRestorePreviousState()
    {
        var game = CreateGame();

        _gameService.MakeMove(game.Id, new MakeMoveRequest {Player = Player.X, Row = 0, Column = 0});
        _gameService.MakeMove(game.Id, new MakeMoveRequest {Player = Player.O, Row = 1, Column = 1});

        var result = _gameService.UndoMove(game.Id);

        Assert.Equal(Player.O, result.CurrentPlayer);
        Assert.Null(result.Board[1 * 3 + 1]); // Cell (1,1) should be empty
        Assert.Single(result.MoveHistory); // Only one move should remain
        Assert.Equal(Player.X, result.MoveHistory[0].Player);
    }  

    [Fact]
    public void Undo_WithNoMoves_ShouldThrowException()
    {
        var game = CreateGame();

        Assert.Throws<InvalidOperationException>(()=>
        _gameService.UndoMove(game.Id));
    }   

    [Fact]
    public void ResetGame_SHouldClearBoardAndRestoreInitialState()
    {
        var game = CreateGame();

        _gameService.MakeMove(game.Id, new MakeMoveRequest {Player = Player.X, Row = 0, Column = 0});
        _gameService.MakeMove(game.Id, new MakeMoveRequest {Player = Player.O, Row = 1, Column = 1});

        var result = _gameService.ResetGame(game.Id);

        Assert.Equal(Player.X, result.CurrentPlayer);
        Assert.Equal(GameStatus.InProgress, result.Status);
        Assert.Null(result.Winner);
        Assert.All(result.Board, cell => Assert.Null(cell));
        Assert.Empty(result.WinningCombination);
        Assert.Empty(result.MoveHistory);
    }

    [Fact]
    public void ResetGame_ForUnknownGame_ShouldThrowException()
    {
        var unknownGameId = Guid.NewGuid();

        Assert.Throws<KeyNotFoundException>(()=>
        _gameService.ResetGame(unknownGameId));
    }

    [Fact]
    public void Scoreboard_ShouldUpdateWhenPlayerWins()
    {
        var game = CreateGame();

        _gameService.MakeMove(game.Id, new MakeMoveRequest {Player = Player.X, Row = 0, Column = 0});
        _gameService.MakeMove(game.Id, new MakeMoveRequest {Player = Player.O, Row = 1, Column = 0});
        _gameService.MakeMove(game.Id, new MakeMoveRequest {Player = Player.X, Row = 0, Column = 1});
        _gameService.MakeMove(game.Id, new MakeMoveRequest {Player = Player.O, Row = 1, Column = 1});
        _gameService.MakeMove(game.Id, new MakeMoveRequest {Player = Player.X, Row = 0, Column = 2}); 

        var scoreboard = _gameService.GetScoreboard();

        Assert.Equal(1, scoreboard.PlayerXWins);
        Assert.Equal(0, scoreboard.PlayerOWins);
        Assert.Equal(0, scoreboard.Draws);
     }

     [Fact]
     public void Scoreboard_ShouldUpdateWhenGameDraws()
     {
         var game = CreateGame();

         _gameService.MakeMove(game.Id, new MakeMoveRequest {Player = Player.X, Row = 0, Column = 0});
         _gameService.MakeMove(game.Id, new MakeMoveRequest {Player = Player.O, Row = 0, Column = 1});
         _gameService.MakeMove(game.Id, new MakeMoveRequest {Player = Player.X, Row = 0, Column = 2});

         _gameService.MakeMove(game.Id, new MakeMoveRequest {Player = Player.O, Row = 1, Column = 1});
         _gameService.MakeMove(game.Id, new MakeMoveRequest {Player = Player.X, Row = 1, Column = 0});
         _gameService.MakeMove(game.Id, new MakeMoveRequest {Player = Player.O, Row = 1, Column = 2});

         _gameService.MakeMove(game.Id, new MakeMoveRequest {Player = Player.X, Row = 2, Column = 1});
         _gameService.MakeMove(game.Id, new MakeMoveRequest {Player = Player.O, Row = 2, Column = 0});
         _gameService.MakeMove(game.Id, new MakeMoveRequest {Player = Player.X, Row = 2, Column = 2}); 

         var scoreboard = _gameService.GetScoreboard();

         Assert.Equal(0, scoreboard.PlayerXWins);
         Assert.Equal(0, scoreboard.PlayerOWins);
         Assert.Equal(1, scoreboard.Draws);
     } 

     [Fact]
     public void ResetGame_ShouldNotAffectScoreboard()
     {
         var game = CreateGame();

         _gameService.MakeMove(game.Id, new MakeMoveRequest {Player = Player.X, Row = 0, Column = 0});
         _gameService.MakeMove(game.Id, new MakeMoveRequest {Player = Player.O, Row = 1, Column = 0});
         _gameService.MakeMove(game.Id, new MakeMoveRequest {Player = Player.X, Row = 0, Column = 1});
         _gameService.MakeMove(game.Id, new MakeMoveRequest {Player = Player.O, Row = 1, Column = 1});
         _gameService.MakeMove(game.Id, new MakeMoveRequest {Player = Player.X, Row = 0, Column = 2}); 

         var scoreboardBeforeReset = _gameService.GetScoreboard();
         Assert.Equal(1, scoreboardBeforeReset.PlayerXWins);

         _gameService.ResetGame(game.Id);

         var scoreboardAfterReset = _gameService.GetScoreboard();
         Assert.Equal(1, scoreboardAfterReset.PlayerXWins);
     } 

     [Fact]
    public void ResetScoreboard_ShouldClearAllScores()
    {
        var game = CreateGame();

        _gameService.MakeMove(game.Id, new MakeMoveRequest { Player = Player.X, Row = 0, Column = 0 });
        _gameService.MakeMove(game.Id, new MakeMoveRequest { Player = Player.O, Row = 1, Column = 0 });
        _gameService.MakeMove(game.Id, new MakeMoveRequest { Player = Player.X, Row = 0, Column = 1 });
        _gameService.MakeMove(game.Id, new MakeMoveRequest { Player = Player.O, Row = 1, Column = 1 });
        _gameService.MakeMove(game.Id, new MakeMoveRequest { Player = Player.X, Row = 0, Column = 2 });

        var scoreboard = _gameService.ResetScoreboard();

        Assert.Equal(0, scoreboard.PlayerXWins);
        Assert.Equal(0, scoreboard.PlayerOWins);
        Assert.Equal(0, scoreboard.Draws);
    }
    
}