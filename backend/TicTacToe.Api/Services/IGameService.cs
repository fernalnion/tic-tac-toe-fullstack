using TicTacToe.Api.Enums;
using TicTacToe.Api.Models;

namespace TicTacToe.Api.Services;

public interface IGameService
{
    // Create a new game with the specified mode (Player vs Player or Player vs AI).
    GameState CreateGame(CreateGameRequest request);
    // Retrieve the current state of the game with the specified ID.
    GameState? GetGameState(Guid gameId);
    // Make a move in the specified game with the provided move request.
    GameState MakeMove(Guid gameId, MakeMoveRequest moveRequest);

    // Undo the last move made in the specified game.
    GameState UndoMove(Guid gameId);
    // Reset the specified game to its initial state.
    GameState ResetGame(Guid gameId);

    // Retrieve the current scoreboard, which includes the number of wins for each player and the number of draws.
    Scoreboard GetScoreboard();
    // Reset the scoreboard to its initial state, clearing all win and draw counts.
    Scoreboard ResetScoreboard();
}