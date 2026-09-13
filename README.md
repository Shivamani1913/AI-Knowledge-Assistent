# AI Knowledge Assistant

A full-stack .NET application where users upload organization documents, ask questions in a chat interface, and receive grounded, cited answers. Admins get live document-processing status pushed in real time via SignalR.

Built to demonstrate a production-style RAG (Retrieval-Augmented Generation) architecture using ASP.NET Core, EF Core, JWT authentication, and SignalR — with AI integration (Azure OpenAI / local Ollama) layered on top of a clean, testable domain model.

## Architecture

\\\
Blazor / React Client
        |
   ASP.NET Core Web API (Controllers + SignalR Hubs)
        |
   +----+----------------------+
   |                           |
EF Core (SQL Server)     Background Worker (Channel<T> queue)
   |                           |
Identity + Roles + JWT    Document text extraction -> chunking -> embeddings -> vector index
                                |
                          RAG Service -> Chat completion with cited sources
\\\

**Projects:**
- \AIKnowledgeAssistant.Core\ — domain entities, enums, and interfaces (no external dependencies)
- \AIKnowledgeAssistant.Infrastructure\ — EF Core DbContext, document processing (extraction, chunking), AI/search implementations
- \AIKnowledgeAssistant.Api\ — ASP.NET Core Web API: auth, controllers, SignalR hubs, background processing
- \AIKnowledgeAssistant.Web\ — Blazor front end (chat UI, upload UI, admin dashboard)

## Features Implemented So Far

- **Authentication**: ASP.NET Core Identity with role-based authorization (Admin/User), JWT bearer tokens
- **Document upload**: multipart file upload, text extraction from PDF/DOCX/TXT, chunking
- **Async processing pipeline**: uploads are queued (\Channel<Guid>\) and processed by a background worker, not blocking the request
- **Live status updates**: SignalR pushes real-time processing status (Queued -> Extracting -> Chunking -> Completed/Failed) to connected clients
- **RAG pipeline** *(in progress)*: document chunk embeddings + vector retrieval + grounded chat completion with source citations

## Tech Stack

| Layer | Technology |
|---|---|
| Backend | ASP.NET Core 9 Web API |
| Auth | ASP.NET Core Identity + Roles + JWT Bearer |
| Database | SQL Server (LocalDB for local dev) + EF Core migrations |
| Real-time | SignalR |
| AI / RAG | Azure OpenAI or local Ollama models (pluggable via \Microsoft.Extensions.AI\ abstractions) |
| Frontend | Blazor Server |

## Getting Started

### Prerequisites
- .NET 9 SDK
- SQL Server LocalDB (ships with Visual Studio) or a Docker-hosted SQL Server
- (Optional, for AI features) An OpenAI API key or a local [Ollama](https://ollama.com) install

### Setup

1. Clone the repo:
   \\\
   git clone https://github.com/Shivamani1913/AI-Knowledge-Assistent.git
   cd AI-Knowledge-Assistent
   \\\

2. Apply database migrations:
   \\\
   dotnet ef database update --project AIKnowledgeAssistant.Infrastructure --startup-project AIKnowledgeAssistant.Api
   \\\

3. Run the API:
   \\\
   dotnet run --project AIKnowledgeAssistant.Api
   \\\

4. Open Swagger at \http://localhost:<port>/swagger\ to register a user, log in, and try the upload endpoint.

## Design Notes

- **Clean separation of concerns**: \Core\ has zero external dependencies, so the domain model can be tested in isolation and swapped between different infrastructure implementations (e.g., different vector stores) without touching business logic.
- **Async-first processing**: uploads return immediately; a background worker handles extraction, chunking, and (eventually) embedding/indexing, broadcasting progress over SignalR rather than making the client poll.
- **Provider-agnostic AI layer**: designed around \Microsoft.Extensions.AI\'s \IChatClient\/\IEmbeddingGenerator\ abstractions, so the underlying model provider (Azure OpenAI, OpenAI, or a local Ollama model) can be swapped without changing the RAG service logic.

## Roadmap

- [x] Auth (Identity + JWT + roles)
- [x] Document upload + async text extraction/chunking
- [x] Live SignalR processing status
- [ ] Vector embeddings + retrieval (local index or Azure AI Search)
- [ ] RAG chat with streamed, cited answers
- [ ] Admin analytics dashboard
- [ ] .NET Aspire orchestration
