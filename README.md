# Tic Tac Toe Full Stack

A browser-based Tic Tac Toe application built using Angular and ASP.NET Core Web API.

## Architecture

Angular frontend communicates with the ASP.NET Core backend through REST APIs.

The backend is the source of truth for:

- Game state
- Move validation
- Turn management
- Win and draw detection
- Move history
- Undo
- Computer opponent
- Scoreboard

## Planned Tech Stack

- Angular
- TypeScript
- ASP.NET Core Web API
- C#
- REST
- In-memory storage
- xUnit
- GitHub

## Game Modes

- Two Player
- Player vs Computer

## Design Decisions

- Backend owns all game rules and state.
- REST is used instead of WebSockets because all game actions are client initiated.
- In-memory storage is used because persistent storage is not required.
- Undo after a completed game will be disabled to keep scoreboard state deterministic.