import { Component, signal } from '@angular/core';
import { GameBoard } from "./components/game-board/game-board";

@Component({
  imports: [GameBoard],
  selector: 'app-root',
  styleUrl: './app.scss',
  templateUrl: './app.html',
})
export class App {
  protected readonly title = signal('frontend');
}
