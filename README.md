# DatingApp - A Modern Full-Stack Dating Application

This project is a feature-rich, full-stack web application built with a .NET backend and an Angular frontend. It was developed as a portfolio piece to showcase skills in building complex, real-time, service-oriented applications from the ground up.

**Live Demo:** [https://dating-2025.azurewebsites.net/](https://dating-2025.azurewebsites.net/)
*(Note: The free-tier Azure App Service may experience a cold start, leading to a slower initial load time.)*

## Key Features

*   **User Authentication & Profile Management:** Secure user registration and login using JWT (JSON Web Token) authentication. Users can create and edit their profiles, including a personal description, photos, and location details.
*   **Photo Uploads & Management:** Users can upload photos to their profiles. Image storage and delivery are handled by **Cloudinary**, a cloud-based image management service. Photos can be set as the main profile picture or deleted.
*   **Admin & Moderation:** An admin panel allows for user role management (`Admin`, `Moderator`, `Member`). A dedicated photo moderation queue enables admins and moderators to approve or reject new photos.
*   **Geolocation-based Distance Filtering:** Users can find others based on distance. Addresses are converted to geographic coordinates using the **OpenCage Geocoding API**, and the data is stored in the database using a `geography` spatial data type for efficient querying.
*   **Real-time Presence:** Users can see who is currently online, implemented using **SignalR** for real-time, low-latency communication between the client and server.
*   **Real-time Messaging:** A private, one-on-one messaging system allows users to chat in real-time, also built with SignalR. The system includes indicators for unread messages.
*   **AI-Powered Chat Suggestions:** If a user gets stuck in a conversation, they can request a chat suggestion from the **Google Gemini API** to help keep the conversation flowing.
*   **Advanced Filtering & Sorting:** The member list can be filtered by age, gender, and last active time, in addition to distance.

## Technology Stack

This application follows Clean Architecture principles, separating concerns into distinct Domain, Application, Infrastructure, and Presentation layers.

### Backend (.NET 9)

*   **Framework:** ASP.NET Core
*   **Database:** SQL Server
*   **ORM:** Entity Framework Core
*   **Spatial Data:** NetTopologySuite for handling geolocation data.
*   **Authentication:** ASP.NET Core Identity & JWT (JSON Web Tokens)
*   **Real-time Communication:** SignalR
*   **Image Management:** Cloudinary
*   **Geocoding:** OpenCage Geocoding API
*   **Artificial Intelligence:** Google Gemini API
*   **Architecture:** Clean Architecture
*   **Hosting:** Microsoft Azure App Service & Azure SQL Database

### Frontend (Angular 20)

*   **Framework:** Angular
*   **Language:** TypeScript
*   **Styling:** Tailwind CSS
*   **UI Components:** DaisyUI (a component library for Tailwind CSS)
*   **State Management:** Angular Signals
*   **HTTP Client:** Angular's `HttpClient` for communicating with the backend API.
*   **Real-time Communication:** SignalR Client

## Project Goal

The primary goal of this project is to demonstrate a comprehensive understanding of modern web development techniques and technologies within a complex, well-structured application. It emphasizes clean code, a maintainable architecture, and the professional integration of third-party services.



