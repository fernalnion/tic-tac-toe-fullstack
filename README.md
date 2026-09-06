# Tic Tac Toe – Full Stack Coding Exercise

A full-stack Tic Tac Toe application built with **Angular** and **ASP.NET Core Web API**.

The application supports **Player vs Player** and **Player vs Computer** modes. The backend manages the game rules and state, while the Angular frontend communicates with it through REST APIs.

## Features

- 3 × 3 Tic Tac Toe board
- Player vs Player mode
- Player vs Computer mode
- Player X always starts
- Turn validation
- Invalid move prevention
- Row, column and diagonal win detection
- Draw detection
- Winning cell highlighting
- Move history
- Undo
- Reset Game
- Play Again
- Session-level scoreboard
- Reset Scoreboard

### Player vs Computer

The human plays as **X** and the computer plays as **O**.

The computer follows this move priority:

1. Win if a winning move is available
2. Block Player X from winning
3. Take the center
4. Take an available corner
5. Take any remaining available cell

### Undo Behaviour

This implementation uses **Option A: Disable Undo After Completion**.

In **Player vs Player** mode, Undo removes the most recent move and restores the turn to the player whose move was removed.

In **Player vs Computer** mode, Undo removes both the computer's latest move and the human move immediately before it, returning the game to the human player's turn.

Once a game is won or drawn, Undo is disabled and the scoreboard result remains final for that game.

### Reset Game Behaviour

Reset Game creates a fresh game state while preserving:

- The existing game ID
- The selected game mode
- The session-level scoreboard

The board, move history, winner and winning combination are cleared. The game status returns to `InProgress` and Player X starts again.

## Tech Stack

### Frontend

- Angular 22
- TypeScript
- Angular Signals
- Standalone Components
- Angular HttpClient
- SCSS

### Backend

- .NET 10
- ASP.NET Core Web API
- C#
- REST API
- Swagger / OpenAPI
- In-memory game state

### Testing

- xUnit

## Architecture

The application follows a simple client-server architecture:

```text
Angular Frontend
      |
      | REST / JSON
      v
ASP.NET Core Web API
      |
      v
GameService
      |
      +-- Game state
      +-- Move validation
      +-- Turn management
      +-- Win / draw detection
      +-- Move history
      +-- Undo
      +-- Computer moves
      +-- Scoreboard
      |
      v
In-Memory Storage
```

The backend is the source of truth for the game.

Game rules such as move validation, turn switching, win detection, draw detection, Undo behaviour and computer moves are handled by the backend. The frontend sends player actions to the API and renders the returned game state.

## Project Structure

```text
tic-tac-toe-fullstack/
│
├── backend/
│   ├── TicTacToe.Api/
│   │   ├── Controllers/
│   │   ├── Enums/
│   │   ├── Models/
│   │   ├── Services/
│   │   └── Program.cs
│   │
│   ├── TicTacToe.Tests/
│   │   └── Services/
│   │
│   └── TicTacToe.slnx
│
├── frontend/
│   ├── src/
│   │   ├── app/
│   │   │   ├── components/
│   │   │   ├── models/
│   │   │   └── services/
│   │   └── environments/
│   │
│   └── package.json
│
└── README.md
```

## Getting Started

### Prerequisites

Install the following:

- .NET 10 SDK
- Node.js 22+
- npm
- Git

Verify the installations:

```bash
dotnet --version
node --version
npm --version
git --version
```

### Clone the Repository

```bash
git clone https://github.com/fernalnion/tic-tac-toe-fullstack.git
cd tic-tac-toe-fullstack
```

## Run the Backend

From the repository root:

```bash
cd backend
dotnet restore TicTacToe.slnx
dotnet run --project TicTacToe.Api
```

The backend runs at:

```text
http://localhost:5275
```

Swagger API documentation is available at:

```text
http://localhost:5275/docs
```

## Run the Frontend

Open another terminal from the repository root:

```bash
cd frontend
npm install
npm start
```

Open the application in a browser:

```text
http://localhost:4200
```

> The backend must be running before starting a game.

## Run Backend Tests

From the repository root:

```bash
cd backend
dotnet test TicTacToe.slnx
```

## Build the Frontend

```bash
cd frontend
npm run build
```

## API Endpoints

### Games

#### Create Game

```http
POST /api/games
```

Example request:

```json
{
  "mode": "PlayerVsPlayer"
}
```

Available modes:

```text
PlayerVsPlayer
PlayerVsComputer
```

#### Get Game State

```http
GET /api/games/{gameId}
```

#### Make Move

```http
POST /api/games/{gameId}/moves
```

Example request:

```json
{
  "player": "X",
  "row": 0,
  "column": 0
}
```

Rows and columns use zero-based indexing.

#### Undo Move

```http
POST /api/games/{gameId}/undo
```

#### Reset Game

```http
POST /api/games/{gameId}/reset
```

Reset creates a fresh game state while preserving the existing game ID, selected mode and session scoreboard.

### Scoreboard

#### Get Scoreboard

```http
GET /api/scoreboard
```

Example response:

```json
{
  "playerXWins": 2,
  "playerOWins": 1,
  "draws": 1
}
```

In **Player vs Computer** mode, Player O represents the computer.

#### Reset Scoreboard

```http
POST /api/scoreboard/reset
```

## Testing

The backend test suite covers the main game rules, state transitions and edge cases, including:

- Initial game state
- Valid move placement
- Turn switching
- Occupied cell validation
- Wrong-player validation
- Invalid board positions
- Row win detection
- Column win detection
- Diagonal win detection
- Draw detection
- Prevention of moves after game completion
- Player vs Player Undo
- Player vs Computer Undo
- Undo with no moves
- Undo after game completion
- Fresh game state after Reset Game
- Scoreboard preservation after Reset Game
- Scoreboard updates
- Scoreboard reset
- Automatic computer moves
- Computer taking the center when available
- Computer taking an available corner
- Computer blocking an immediate Player X win
- Computer taking a winning move when available
- Prevention of manual Player O moves in computer mode

Run the complete backend test suite with:

```bash
cd backend
dotnet test TicTacToe.slnx
```

## Design Decisions

### Backend as Source of Truth

Game rules are handled by the backend rather than duplicated in Angular.

This keeps validation and state transitions in one place and ensures that the game state returned by the API is authoritative.

### REST Instead of WebSockets

REST is sufficient for the current scope because game actions originate from the local client and each action receives the updated state in the API response.

SignalR or WebSockets would be more appropriate for remote multiplayer, where moves need to be pushed to another connected player in real time.

### In-Memory Storage

The exercise does not require persistent storage, so game sessions and scoreboard data are stored in memory.

This keeps the application simple to set up and run without requiring a database.

The trade-off is that restarting the backend clears all game sessions and scoreboard data.

### Board Representation

The board is represented as nine positions:

```text
0 | 1 | 2
---------
3 | 4 | 5
---------
6 | 7 | 8
```

A row and column are converted to a board index using:

```text
index = row * 3 + column
```

The flat representation keeps move handling and winning-combination checks straightforward.

### Computer Strategy

The computer uses a deterministic rule-based strategy:

```text
Win -> Block -> Center -> Corner -> Any Available Cell
```

The strategy intentionally remains simple for the scope of the exercise while ensuring that the computer:

- Makes only valid moves
- Takes an immediate winning opportunity
- Blocks an immediate Player X win
- Uses a predictable fallback strategy

### Undo Strategy

The implementation follows **Option A: Disable Undo After Completion**.

This keeps completed game results final and avoids changing scoreboard values after a win or draw has already been recorded.

In Player vs Computer mode, the human move and corresponding computer move are treated as one Undo cycle.

### Reset Strategy

Reset Game creates a new `GameState` for the existing game resource.

The game ID and selected mode are preserved, while the board, history and completion state are cleared.

The session-level scoreboard is intentionally unaffected by resetting an individual game.

### Angular Signals

Angular Signals are used for local UI state such as:

- Current game
- Scoreboard
- Selected mode
- Loading state
- Error messages

Computed signals are used for derived values such as the game status text and Undo availability.

A larger state-management library was not necessary for the scope of this application.

## Assumptions and Clarifications

- Player X always starts.
- The human is Player X in Player vs Computer mode.
- The computer is Player O.
- Player O wins therefore represent computer wins in Player vs Computer mode.
- Undo uses Option A and is disabled after game completion.
- Reset Game preserves the existing game ID and selected mode.
- Reset Game does not change the scoreboard.
- Reset Scoreboard explicitly clears all scoreboard values.
- The scoreboard is maintained at the backend session level.
- Persistent storage is not required.
- Game and scoreboard state are lost when the backend restarts.

## Error Handling

The backend validates cases including:

- Game not found
- Wrong player's turn
- Invalid row
- Invalid column
- Occupied cell
- Move after game completion
- Invalid Undo operation
- Manual Player O move in Player vs Computer mode

Errors returned by the API are displayed by the frontend.

## AI Tools and Prompt Summary

AI tools were used selectively as a supporting development aid during the exercise.

The main areas where AI assistance was used were:

- Requirement review
- Test-case brainstorming
- Troubleshooting specific build and integration issues
- Reviewing API and frontend implementation choices
- Documentation review

Example prompts used during development included:

- "Review the proposed Angular and .NET architecture against the assessment requirements and identify any gaps."
- "Review the Tic Tac Toe game rules and suggest additional edge cases to test."
- "Review the Undo behaviour for Player vs Player and Player vs Computer modes."
- "Help diagnose this failing xUnit test based on the error output."
- "Review the REST API endpoints against the stated requirements."
- "Review the README against the submission checklist and identify anything missing."

AI suggestions were reviewed before being applied. The game logic, API integration, computer-move behaviour, Undo behaviour and scoreboard interactions were manually verified through automated tests and browser testing.

Implementation choices were adjusted where needed to keep the solution aligned with the assessment requirements and intentionally simple for the scope of the exercise.

## Known Limitations

- Game state is stored only in memory.
- Scoreboard state is stored only in memory.
- Restarting the backend clears the current sessions and scoreboard.
- No database persistence.
- No remote multiplayer.
- No authentication or player accounts.
- The computer uses a rule-based strategy rather than Minimax.

## Future Improvements

Possible improvements include:

- Persistent storage using PostgreSQL or SQLite
- Remote multiplayer using SignalR
- Minimax-based computer opponent
- Per-game concurrency handling
- Centralized API exception-handling middleware
- API integration tests
- Additional Angular component tests
- Docker support
- CI/CD pipeline