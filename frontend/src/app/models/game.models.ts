export type Player = 'X' | 'O';
export type GameStatus = 'InProgress' | 'Draw' | 'Won';
export type GameMode = 'PlayerVsPlayer' | 'PlayerVsComputer';

export interface Move{
    moveNumber: number;
    player: Player;
    row: number;
    column: number;
}

export interface GameState {
    id: string;
    mode: GameMode;
    status: GameStatus;
    currentPlayer: Player;
    winner: Player | null;
    board: (Player | null)[];
    winningCombination: number[];
    moveHistory: Move[];
}

export interface Scoreboard{
    playerXWins: number;
    playerOWins: number;
    draws: number;
}

export interface CreateGameRequest{
    mode: GameMode;
}

export interface MakeMoveRequest{
    player: Player;
    row: number;
    column: number;
}