# DatingApp - AI-Powered Smart Dating Platform

This project is a feature-rich, full-stack web application built with a **.NET 10** backend and an **Angular 21** frontend.

It goes beyond standard CRUD applications by implementing a **Hybrid RAG (Retrieval-Augmented Generation)** architecture. It uses a Vector Database and local AI models to enable semantic matchmaking, allowing users to find matches based on meaning and context rather than just keyword matching.

**Live Demo:** [https://dating-2025.azurewebsites.net/](https://dating-2025.azurewebsites.net/)
_(Note: The free-tier Azure App Service may experience a cold start.)_

## Application Preview

![Member listing with filtering options.](demo/filtering.gif)
_Member listing with filtering options._

## 🧠 AI & Hybrid RAG Features (Refactored)

This application implements a high-performance **AI Engineer stack** within a Clean Architecture:

- **Local Semantic Matchmaking:**
  - Users can search for matches using natural language.
  - **How it works:** User profiles are vectorized locally on the server using the **ONNX Runtime** and the **all-MiniLM-L6-v2** model (384 dimensions). This eliminates privacy concerns and API costs for embedding generation.
  - Vectors are stored in a **Qdrant** Vector Database.
  - The system performs a cosine similarity search to find profiles that match the _intent_ of the query.
- **Cerebras-Powered Chat Intelligence:**
  - Integrates **Cerebras Inference** (using `gpt-oss-120b`) for near-instant AI responses.
  - Uses **Chain-of-Thought (CoT)** reasoning to analyze conversation context and suggest high-quality ice-breakers.
- **High-Performance Sync:**
  - Synchronizes structured SQL Server data with Qdrant automatically during the database seeding process.

## Key Features

- **User Authentication:** Secure registration/login using JWT and ASP.NET Core Identity (with deterministic ConcurrencyStamps for stable migrations).
- **Real-time Presence:** Built with **SignalR** for live status and instant messaging.
- **Geolocation:** Filtering by physical distance using **NetTopologySuite** and **OpenCage API**.
- **Photo Management:** Cloud-based image storage and transformation using **Cloudinary**.
- **Advanced Filtering:** Sort by age, gender, and last active status.

## Technology Stack

### Backend (.NET 10)

- **Framework:** ASP.NET Core 10.0 Web API.
- **AI & Vectors:**
  - **Microsoft.Extensions.AI:** Standardized AI integration.
  - **ONNX Runtime:** Local embedding generation (`model.onnx`).
  - **Cerebras SDK:** High-speed LLM inference (OpenAI-compatible).
  - **Qdrant Client:** Vector search via gRPC (port 6334).
- **Database:** SQL Server (EF Core 10.0).

### Frontend (Angular 21)

- **Framework:** Angular 21 with Signals for state management.
- **Styling:** Tailwind CSS + DaisyUI.

## Local Development Setup

1.  **Infrastructure (Docker)**
    Ensure Docker Desktop is running (WSL 2 recommended). Start the vector database:

    ```bash
    docker compose up -d qdrant
    ```

    The dashboard is available at http://localhost:6333.

2.  **Configuration**
    Update `appsettings.Development.json` in the Presentation project:

    ```json
    {
      "ConnectionStrings": {
        "DefaultConnection": "Server=YOUR_SERVER;Database=datingdb;Trusted_Connection=True;"
      },
      "Qdrant": {
        "Url": "http://localhost:6334"
      },
      "CerebrasSettings": {
        "ApiKey": "your_api_key"
      }
    }
    ```

3.  **Local AI Models**
    Place `model.onnx` and `vocab.txt` into the `DatingApp.Infrastructure/Data/` folder. Ensure they are set as "Copy to Output Directory" in the `.csproj` file.

4.  **Run Application**

    ```bash
    # Build Frontend
    cd DatingApp.Client && npm run build

    # Run Backend
    cd ../DatingApp.Presentation && dotnet watch run
    ```

## Project Goal

This project serves as a case study on bridging the gap between traditional enterprise applications and modern AI workloads. It demonstrates how to use local SLMs (Small Language Models) for embeddings and lightning-fast LLM providers like Cerebras to create a seamless, intelligent user experience.
