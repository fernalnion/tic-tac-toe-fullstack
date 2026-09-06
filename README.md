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
- Scoreboard
- Reset Scoreboard

### Player vs Computer

The human plays as **X** and the computer plays as **O**.

The computer follows a simple move priority:

1. Win if a winning move is available
2. Block Player X from winning
3. Take the center
4. Take an available corner
5. Take any remaining available cell

### Undo Behaviour

In **Player vs Player** mode, Undo removes the most recent move and restores that player's turn.

In **Player vs Computer** mode, Undo removes both the computer's latest move and the human move immediately before it.

Undo is disabled after the game has completed.

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

The application uses a simple client-server architecture:

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

Game rules such as move validation, turn switching, win detection, draw detection and computer moves are handled by the backend. The frontend sends player actions to the API and renders the returned game state.

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

### Run the Backend

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

### Run the Frontend

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

### Run Backend Tests

```bash
cd backend
dotnet test TicTacToe.slnx
```

### Build the Frontend

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

Example:

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

#### Get Game

```http
GET /api/games/{gameId}
```

#### Make Move

```http
POST /api/games/{gameId}/moves
```

Example:

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

In Player vs Computer mode, Player O represents the computer.

#### Reset Scoreboard

```http
POST /api/scoreboard/reset
```

## Testing

The backend test suite covers the main game rules and edge cases, including:

- Initial game state
- Valid moves
- Turn switching
- Occupied cell validation
- Wrong player validation
- Invalid board positions
- Row wins
- Column wins
- Diagonal wins
- Draw detection
- Moves after game completion
- Player vs Player undo
- Player vs Computer undo
- Game reset
- Scoreboard updates
- Scoreboard reset
- Computer automatic moves
- Computer blocking behaviour

Run the complete test suite with:

```bash
cd backend
dotnet test TicTacToe.slnx
```

## Design Decisions

### Backend as Source of Truth

Game rules are handled by the backend rather than duplicated in Angular.

This keeps validation and state transitions in one place and ensures the state returned by the API is authoritative.

### REST Instead of WebSockets

REST is sufficient for the current requirements because game actions originate from the local client.

WebSockets or SignalR would be more useful for remote multiplayer, where moves need to be pushed to another connected player in real time.

### In-Memory Storage

The exercise does not require persistent storage, so game sessions and scoreboard data are stored in memory.

This keeps the application simple to run without requiring a database. The trade-off is that restarting the backend clears the current games and scoreboard.

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

This flat representation keeps board rendering and winning-combination checks straightforward.

### Computer Strategy

The computer uses a simple deterministic strategy rather than Minimax.

Its priority is:

```text
Win -> Block -> Center -> Corner -> Available Cell
```

This keeps the implementation simple while ensuring the computer always makes a valid move and blocks immediate winning opportunities.

### Angular Signals

Angular Signals are used for local UI state including:

- Current game
- Scoreboard
- Selected game mode
- Loading state
- Error messages

Computed signals are used for derived state such as the game status text and Undo availability.

A larger state-management library was not necessary for the scope of this application.

## Assumptions

- Player X always starts.
- The human is Player X in Player vs Computer mode.
- The computer is Player O.
- Undo is disabled after game completion.
- Reset Game keeps the scoreboard unchanged.
- Reset Scoreboard clears all scoreboard values.
- Game and scoreboard state are maintained for the current backend session.
- Persistent storage is not required.

## Error Handling

The backend validates cases including:

- Game not found
- Wrong player's turn
- Invalid row or column
- Occupied cell
- Move after game completion
- Invalid Undo operation

Errors returned by the API are displayed by the frontend.

## Known Limitations

- Game state is lost when the backend restarts.
- Scoreboard state is lost when the backend restarts.
- No database persistence.
- No remote multiplayer.
- No authentication or player accounts.
- The computer uses a simple rule-based strategy rather than Minimax.
- Concurrent updates to the same individual game are not explicitly synchronized at the game-object level.

## Development Note

AI tools were used selectively for troubleshooting and documentation support. The solution was implemented, reviewed and tested before submission.