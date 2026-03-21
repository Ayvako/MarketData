# MarketData

A .NET 8 microservice for integration with the Fintacharts platform. It implements asset synchronization, historical data retrieval, and real-time price updates via WebSocket.

## Tech Stack
- **Backend:** .NET 8 (Web API)
- **Database:** PostgreSQL + Entity Framework Core
- **Real-time:** WebSockets (ClientWebSocket)
- **Containerization:** Docker + Docker Compose

## Getting Started

1. **Clone the repository.**
2. **Configure credentials:** Update the `appsettings.json` file with your Fintacharts API `Username` and `Password`.
3. **Run via Docker Compose:**
   ```bash
   docker-compose up --build
