using Microsoft.AspNetCore.Mvc;
using TicTacToe.Api.Models;
using TicTacToe.Api.Services;

namespace TicTacToe.Api.Controllers;

[ApiController]
[Route("api/scoreboard")]
public class ScoreboardController : ControllerBase
{
    private readonly IGameService _gameService;

    public ScoreboardController(IGameService gameService)
    {
        _gameService = gameService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(Scoreboard), StatusCodes.Status200OK)]
    public ActionResult<Scoreboard> GetScoreboard()
    {
        var scoreboard = _gameService.GetScoreboard();
        return Ok(scoreboard);
    }

    [HttpPost("reset")]
    [ProducesResponseType(typeof(Scoreboard), StatusCodes.Status200OK)]
    public ActionResult<Scoreboard> ResetScoreboard()
    {
        var scoreboard = _gameService.ResetScoreboard();
        return Ok(scoreboard);
    }
}