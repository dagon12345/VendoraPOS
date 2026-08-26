import { Component, inject } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { ToastContainer } from './shared/toast/toast-container';
import { StockRealtimeService } from './core/services/stock-realtime';

@Component({
  imports: [RouterOutlet, RouterLink, RouterLinkActive, ToastContainer],
  selector: 'app-root',
  styleUrl: './app.scss',
  templateUrl: './app.html',
})
export class App {
  private readonly stockRealtime = inject(StockRealtimeService);

  constructor() {
    // Connected once app-wide, regardless of which page is active - cheap no-op for pages
    // that never read StockRealtimeService.lastChange.
    this.stockRealtime.connect();
  }
}
