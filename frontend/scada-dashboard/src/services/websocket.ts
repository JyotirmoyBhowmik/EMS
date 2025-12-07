import * as SignalR from '@microsoft/signalr';

class WebSocketClient {
    private connection: SignalR.HubConnection | null = null;
    private reconnectAttempts = 0;
    private maxReconnectAttempts = 5;

    constructor(private hubUrl: string) { }

    async connect(): Promise<void> {
        this.connection = new SignalR.HubConnectionBuilder()
            .withUrl(this.hubUrl)
            .withAutomaticReconnect({
                nextRetryDelayInMilliseconds: (retryContext) => {
                    if (retryContext.previousRetryCount >= this.maxReconnectAttempts) {
                        return null; // Stop reconnecting
                    }
                    return Math.min(1000 * Math.pow(2, retryContext.previousRetryCount), 30000);
                }
            })
            .configureLogging(SignalR.LogLevel.Information)
            .build();

        this.connection.onreconnecting((error) => {
            console.warn('WebSocket reconnecting...', error);
        });

        this.connection.onreconnected((connectionId) => {
            console.log('WebSocket reconnected:', connectionId);
            this.reconnectAttempts = 0;
        });

        this.connection.onclose((error) => {
            console.error('WebSocket connection closed', error);
        });

        try {
            await this.connection.start();
            console.log('WebSocket connected successfully');
        } catch (error) {
            console.error('Error connecting to WebSocket:', error);
            throw error;
        }
    }

    async subscribeToTags(tagNames: string[], callback: (data: TagUpdate) => void): Promise<void> {
        if (!this.connection) {
            throw new Error('WebSocket not connected');
        }

        // Register handler for tag updates
        this.connection.on('TagUpdate', callback);

        // Subscribe to tags
        await this.connection.invoke('SubscribeToTags', tagNames);
    }

    async unsubscribeFromTags(tagNames: string[]): Promise<void> {
        if (!this.connection) return;

        await this.connection.invoke('UnsubscribeFromTags', tagNames);
    }

    async subscribeToAlarms(callback: (alarm: Alarm) => void): Promise<void> {
        if (!this.connection) {
            throw new Error('WebSocket not connected');
        }

        this.connection.on('AlarmTriggered', callback);
        await this.connection.invoke('SubscribeToAlarms');
    }

    async disconnect(): Promise<void> {
        if (this.connection) {
            await this.connection.stop();
            this.connection = null;
        }
    }

    isConnected(): boolean {
        return this.connection?.state === SignalR.HubConnectionState.Connected;
    }
}

export interface TagUpdate {
    tagName: string;
    value: number;
    timestamp: string;
    quality: string;
}

export interface Alarm {
    id: number;
    tagName: string;
    type: string;
    priority: string;
    message: string;
    triggeredAt: string;
}

export const webSocketClient = new WebSocketClient(
    import.meta.env.VITE_WEBSOCKET_URL || 'http://localhost:5007/hubs/tags'
);

export default webSocketClient;
