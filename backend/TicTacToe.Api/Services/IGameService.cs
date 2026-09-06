using TicTacToe.Api.Enums;
using TicTacToe.Api.Models;

namespace TicTacToe.Api.Services;

public interface IGameService
{
    GameState CreateGame(CreateGameRequest request);
    GameState? GetGameState(Guid gameId);
    GameState MakeMove(Guid gameId, MakeMoveRequest moveRequest);
    // Scoreboard GetScoreboard();

    GameState UndoMove(Guid gameId);
    GameState ResetGame(Guid gameId);
}