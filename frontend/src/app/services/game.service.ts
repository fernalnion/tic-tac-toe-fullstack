import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { CreateGameRequest, GameState, MakeMoveRequest, Scoreboard } from '../models/game.models';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root',
})
export class GameService {
  private readonly apiUrl = environment.apiUrl;

  constructor(private readonly http: HttpClient) {}

  createGame(request: CreateGameRequest): Observable<GameState> {
    return this.http.post<GameState>(`${this.apiUrl}/games`, request);
  }

  getGame(id: string): Observable<GameState> {
    return this.http.get<GameState>(`${this.apiUrl}/games/${id}`);
  }

  makeMove(id: string, request: MakeMoveRequest): Observable<GameState> {
    return this.http.post<GameState>(`${this.apiUrl}/games/${id}/moves`, request);
  }

  undoMove(id: string): Observable<GameState> {
    return this.http.post<GameState>(`${this.apiUrl}/games/${id}/undo`, {});
  }

  resetGame(id: string): Observable<GameState> {
    return this.http.post<GameState>(`${this.apiUrl}/games/${id}/reset`, {});
  }

  getScoreboard(): Observable<Scoreboard> {
    return this.http.get<Scoreboard>(`${this.apiUrl}/scoreboard`);
  }

  resetScoreboard(): Observable<Scoreboard> {
    return this.http.post<Scoreboard>(`${this.apiUrl}/scoreboard/reset`, {});
  }
}
