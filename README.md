# Tic Tac Toe Full Stack

A browser-based Tic Tac Toe application built with Angular and ASP.NET Core Web API.

The application supports:
- Player vs Player
- Player vs Computer
- Move history
- Undo
- Scoreboard
- Reset game
- Reset scoreboard
- Win and draw detection
- Winning cell highlighting

The backend is the source of truth for game state and game rules.

---

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
- In-memory state management
- Swagger / OpenAPI

### Testing
- xUnit

### Source Control
- Git
- GitHub

---

## Architecture

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
       +-- Win / draw detection
       +-- Move history
       +-- Undo
       +-- Computer opponent
       +-- Scoreboard
       |
       v
In-Memory Storage