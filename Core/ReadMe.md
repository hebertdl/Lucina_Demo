# Core Project

## Overview

The Core project provides essential components and interfaces for logging, authentication, and other core
functionalities.

## Components

- **ILogger**: Interface for logging.
- **JwtAuthHeaderBuilder**: Builds JWT authentication headers for HTTP requests.

## Setup

1. Clone the repository:
    ```sh
    git clone https://github.com/hebertdl/Lucina_Demo.git
    cd Lucina_Demo/Core
    ```

2. Install dependencies:
    ```sh
    dotnet restore
    ```

3. Build the project:
    ```sh
    dotnet build
    ```

4. Run the tests:
    ```sh
    dotnet test
    ```

## Usage

To use the `JwtAuthHeaderBuilder`, create an instance and call the `BuildAuthHeaderAsync` method:

var httpClient = new HttpClient();
var logger = new ConsoleLogger(); // Assuming ConsoleLogger implements ILogger
var authHeaderBuilder = new JwtAuthHeaderBuilder(httpClient, logger, "clientId", "
clientSecret", "https://example.com/token");
var authHeader = await authHeaderBuilder.BuildAuthHeaderAsync();

## Lucina Demo Process Flow

```mermaid
graph TD
   A[Quartz Trigger<br>1 AM UTC] --> B[BatchProcessingJob.Execute]
   B --> C[BatchProcessor.RunBatchProcessAsync]
   C --> D[Log Start]
   C --> E[FdaEventDataProcessor.ExecuteDataProcessor]
   E --> F[GetBatchData<br>FDA API]
   F --> G[Save Raw Data<br>LocalFileStorage]
   G --> H[ConvertToFdaEvents<br>FdaDrugEventExtractor]
   H --> I[Save Processed Data<br>LocalFileStorage]
   I --> J[ApplyDataFilters<br>FdaEventsWomanFilter]
   J --> K[FdaEventsFirst10Filter]
   K --> L[BatchPusher.PostResults]
   L --> M[JwtAuthHeaderBuilder.BuildAuthHeaderAsync]
   M --> N[Post Data to reqres.in]
   N -->|Success| O[Log Success]
   N -->|Failure| P[Log Error] --> Q[Throw Exception]
   C -->|Failure| R[Log Error] --> S[Throw Exception]
```

## Lucina Demo Coupling Diagram

```mermaid
graph LR
    A[Program.cs] -->|Schedules| B[Quartz IScheduler]
    A -->|Configures| C[ServiceProvider]
    B -->|Triggers| D[BatchProcessingJob]
    C -->|DI| D
    D -->|Uses| E[IBatchProcessor]
    E --> F[BatchProcessor]
    F -->|Uses| G[BatchPusher]
    F -->|Uses| H[IDataProcessor]
    F -->|Uses| I[IFileStorage]
    F -->|Uses| J[ILogger]
    G -->|Uses| K[IAuthHeaderBuilder]
    G -->|Uses| J
    H --> L[FdaEventDataProcessor]
    I --> M[LocalFileStorage]
    J --> N[ConsoleLogger]
    K --> O[JwtAuthHeaderBuilder]
    C -->|Creates Jobs| P[MicrosoftDependencyInjectionJobFactory]
    L -->|Uses| Q[IHttpClientFactory]
    L -->|Uses| R[IFdaDrugEventExtractor]
    L -->|Uses| I
    L -->|Uses| J
    R --> S[FdaDrugEventExtractor]
    S -->|Uses| J
    L -->|Uses| T[FdaEventsWomanFilter]
    L -->|Uses| U[FdaEventsFirst10Filter]
    T -->|Chains| U
    O -->|Uses| V[HttpClient]
    O -->|Uses| J
```

