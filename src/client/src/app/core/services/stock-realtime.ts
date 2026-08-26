import { Injectable, signal } from '@angular/core';
import * as signalR from '@microsoft/signalr';

export interface StockChangedEvent {
  productId: string;
  quantityOnHand: number;
}

@Injectable({ providedIn: 'root' })
export class StockRealtimeService {
  private connection?: signalR.HubConnection;
  readonly lastChange = signal<StockChangedEvent | null>(null);

  connect(): void {
    if (this.connection) return;

    this.connection = new signalR.HubConnectionBuilder()
      .withUrl('/hubs/stock')
      .withAutomaticReconnect()
      .build();

    this.connection.on('StockChanged', (event: StockChangedEvent) => this.lastChange.set(event));

    // Real-time sync is a nice-to-have, not required for the app to function - if the hub can't
    // be reached, screens just keep working off their last-known values silently.
    this.connection.start().catch(() => {});
  }
}
