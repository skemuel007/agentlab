# AgentLab

Full-stack AI agent playground — .NET 10 backend + React/Vite frontend.

## Stack

| Layer    | Technology                          |
|----------|-------------------------------------|
| Backend  | ASP.NET Core 10, Microsoft.Agents.AI, OpenAI SDK |
| Frontend | React 19, TypeScript, Vite          |

## Project structure

```
agentlab/
├── backend/
│   ├── AgentLab.slnx
│   └── src/
│       └── AgentLab.Api/        # ASP.NET Core Web API
├── frontend/                    # React + Vite SPA
└── README.md
```

## Getting started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- Node 20+

### Backend

```bash
cd backend/src/AgentLab.Api

# Copy env template and fill in secrets
cp .env.example .env

dotnet run
# Listens on http://localhost:5080
```

### Frontend

```bash
cd frontend
npm install
npm run dev
# Listens on http://localhost:5176
```

## Environment variables

See [backend/src/AgentLab.Api/.env.example](backend/src/AgentLab.Api/.env.example) for all required variables. Copy to `.env` and never commit it.

## API

| Method | Path         | Description          |
|--------|--------------|----------------------|
| GET    | /api/health  | Health check         |
| POST   | /api/chat    | Send a chat message  |

## License

MIT
