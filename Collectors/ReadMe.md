# Collectors Project

## Overview

The Collectors project processes FDA drug event data nightly, filters it (women-only, first 10 records), stores results, and posts to a mock endpoint, built for a .NET interview assignment.

## Components

- **BatchProcessor**: Orchestrates the batch processing workflow.
- **BatchPusher**: Posts processed data to an endpoint with authentication.
- **JwtAuthHeaderBuilder**: Builds JWT headers (in `Core`).
- **FdaEventDataProcessor**: Fetches and processes FDA data.
- **FdaDrugEventExtractor**: Converts raw data to `FdaEvents`.
- **FdaEventsWomanFilter/First10Filter**: Filters `FdaEvents`.
- **LocalFileStorage**: Stores raw, processed, and final data.
- **ConsoleLogger**: Logs execution details.


## Setup

1. Clone the repository:
    ```sh
    git clone https://github.com/your-repo/collectors.git
    cd collectors
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

To run the batch processing, use the following command:
```sh
dotnet run --project Collectors
```

## Batch Processor Process Flow

The diagram below illustrates the flow of the batch processing system, from initiation to completion or error handling:

```mermaid
graph TD
    A[Start Batch Processing] --> B[RunBatchProcessAsync]
    B --> H[Log Start]
    B --> C[ExecuteDataProcessor]
    C --> D[PostResults]
    D --> E[BuildAuthHeaderAsync]
    E --> F[Post Data to Endpoint]
    F --> G[Log Success]
    F --> I[Log Error] --> J[Throw Exception]
```

## Batch Processor Coupling Diagram
```mermaid

graph LR
    A[BatchProcessor] -->|uses| B[BatchPusher]
    A -->|uses| C[IDataProcessor]
    A -->|uses| D[IFileStorage]
    A -->|uses| E[ILogger]
    B -->|uses| F[JwtAuthHeaderBuilder]
    B -->|uses| E[ILogger]
    F -->|uses| G[HttpClient]
    F -->|uses| E[ILogger]
```

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
