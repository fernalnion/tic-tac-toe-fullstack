import { CommonModule } from '@angular/common';
import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { GameMode, GameState, Player, Scoreboard } from '../../models/game.models';
import { GameService } from '../../services/game.service';

@Component({
  imports: [CommonModule],
  standalone: true,
  selector: 'app-game-board',
  styleUrl: './game-board.scss',
  templateUrl: './game-board.html',
})
export class GameBoard implements OnInit {
  private readonly gameService = inject(GameService);

  readonly game = signal<GameState | null>(null);

  readonly scoreboard = signal<Scoreboard>({
    playerXWins: 0,
    playerOWins: 0,
    draws: 0,
  });

  readonly selectedMode = signal<GameMode>('PlayerVsPlayer');
  readonly errorMessage = signal<string | null>(null);
  readonly isLoading = signal<boolean>(false);

  readonly statusText = computed(() => {
    const game = this.game();
    if (!game) {
      return '';
    }

    if (game.status === 'Won') {
      return `Player ${game.winner} wins!`;
    }

    if (game.status === 'Draw') {
      return 'Game is a draw.';
    }

    if (game.mode === 'PlayerVsComputer') {
      return 'Your turn — Player X';
    }

    return `Current turn: Player ${game.currentPlayer}`;
  });

  readonly canUndo = computed(() => {
    const game = this.game();

    return !!game && game.status === 'InProgress' && game.moveHistory.length > 0;
  });

  ngOnInit(): void {
    this.loadScoreboard();
    this.startNewGame();
  }

  startNewGame(mode: GameMode = this.selectedMode()): void {
    this.selectedMode.set(mode);
    this.errorMessage.set(null);
    this.isLoading.set(true);

    this.gameService.createGame({ mode }).subscribe({
      next: (game) => {
        this.game.set(game);
        this.isLoading.set(false);
      },
      error: (error) => {
        this.handleError(error);
        this.isLoading.set(false);
      },
    });
  }

  selectCell(index: number): void {
    const game = this.game();
    if (!game || this.isLoading() || game.status !== 'InProgress' || game.board[index] !== null) {
      return;
    }

    this.errorMessage.set(null);
    this.isLoading.set(true);

    const player: Player = game.mode === 'PlayerVsComputer' ? 'X' : game.currentPlayer;
    const row = Math.floor(index / 3);
    const column = index % 3;

    this.gameService.makeMove(game.id, { player, row, column }).subscribe({
      next: (updatedGame) => {
        this.game.set(updatedGame);
        this.isLoading.set(false);
      },
      error: (error) => {
        this.handleError(error);
        this.isLoading.set(false);
      },
    });
  }

  undoMove(): void {
    const game = this.game();
    if (!game || this.isLoading() || !this.canUndo()) {
      return;
    }

    this.errorMessage.set(null);
    this.isLoading.set(true);

    this.gameService.undoMove(game.id).subscribe({
      next: (updatedGame) => {
        this.game.set(updatedGame);
        this.isLoading.set(false);
      },
      error: (error) => {
        this.handleError(error);
        this.isLoading.set(false);
      },
    });
  }

  resetGame(): void {
    const game = this.game();
    if (!game || this.isLoading()) {
      return;
    }

    this.errorMessage.set(null);
    this.isLoading.set(true);

    this.gameService.resetGame(game.id).subscribe({
      next: (resetGame) => {
        this.game.set(resetGame);
        this.isLoading.set(false);
      },
      error: (error) => {
        this.handleError(error);
        this.isLoading.set(false);
      },
    });
  }

  resetScoreboard(): void {
    this.errorMessage.set(null);
    this.isLoading.set(true);

    this.gameService.resetScoreboard().subscribe({
      next: (resetScoreboard) => {
        this.scoreboard.set(resetScoreboard);
        this.isLoading.set(false);
      },
      error: (error) => {
        this.handleError(error);
        this.isLoading.set(false);
      },
    });
  }

  isWinningCell(index: number): boolean {
    const game = this.game();
    return !!game && game.winningCombination.includes(index);
  }

  private loadScoreboard(): void {
    this.isLoading.set(true);

    this.gameService.getScoreboard().subscribe({
      next: (scoreboard) => {
        this.scoreboard.set(scoreboard);
        this.isLoading.set(false);
      },
      error: (error) => {
        this.handleError(error);
        this.isLoading.set(false);
      },
    });
  }

  private handleError(error: any): void {
    const errorMessage = error?.error?.message || 'An unexpected error occurred.';
    this.errorMessage.set(errorMessage);
  }

  getRow(index: number): number {
    return Math.floor(index / 3) + 1;
  }

  getColumn(index: number): number {
    return (index % 3) + 1;
  }
}
