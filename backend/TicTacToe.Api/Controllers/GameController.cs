using Microsoft.AspNetCore.Mvc;
using TicTacToe.Api.Models;
using TicTacToe.Api.Services;

namespace TicTacToe.Api.Controllers;

[ApiController]
[Route("api/games")]
public class GameController : ControllerBase
{
    private readonly IGameService _gameService;
    public GameController(IGameService gameService)
    {
        _gameService = gameService;
    }

    [HttpPost]
    [ProducesResponseType(typeof(GameState), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public ActionResult<GameState> CreateGame([FromBody] CreateGameRequest request)
    {
        var game = _gameService.CreateGame(request);

        return CreatedAtAction(
            nameof(GetGame),
            new { id = game.Id},
            game
        );
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(GameState), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<GameState> GetGame(Guid id)
    {
        var game = _gameService.GetGameState(id);
        if(game is null)
        {
            return NotFound(new {
                message = $"Game with ID {id} not found."
            });
        }

        return Ok(game); 
    }

    [HttpPost("{id:guid}/moves")]
    [ProducesResponseType(typeof(GameState), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<GameState> MakeMove(Guid id, [FromBody] MakeMoveRequest request)
    {
        try{
            var game = _gameService.MakeMove(id, request);
            return Ok(game);
        }catch(KeyNotFoundException ex)
        {
            return NotFound(new
            {
                message = ex.Message
            });
        }catch(ArgumentOutOfRangeException ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }catch(InvalidOperationException ex)
        {
            return BadRequest(new
            {
                Message = ex.Message
            });
        }
    }
}