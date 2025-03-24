# Tests Project

This project contains the unit tests for the Collectors application, which automates the nightly processing of FDA drug
event data. The tests ensure the reliability of components like `FdaEventDataProcessor`, `BatchPusher`, and
`JwtAuthHeaderBuilder`, as well as the overall process flow.

## Project Structure

- **Collectors.Tests/**: Contains all test files.
    - **Unit Tests**: Test individual components in isolation (e.g., `JwtAuthHeaderBuilderTests.cs`).
    - **Integration Tests**: Test the end-to-end flow (e.g., `BatchProcessorIntegrationTests.cs`).

## Dependencies

- **xUnit**: Testing framework.
- **Moq**: Mocking library for isolating dependencies.
- **coverlet**: For collecting test coverage metrics.
- **ReportGenerator**: For generating a detailed coverage report.

## Prerequisites

- .NET 8.0 SDK or later.
- Visual Studio 2022 (or another IDE like VS Code with the .NET CLI).
- Git (to clone the repository).

Install the following global tools for coverage reporting:

```bash
dotnet tool install -g coverlet.console
dotnet tool install -g dotnet-reportgenerator-globaltool
```

## Setup

Clone the Repository:

```bash
git clone https://github.com/your-repo/collectors.git
cd collectors
```

Restore Dependencies:
Navigate to the test project directory:

```bash
cd Collectors.Tests
```

Restore the NuGet packages:

```bash
dotnet restore
```

## Running Tests

Run All Tests:
From the `Collectors.Tests` directory, run:

```bash
dotnet test
```

This executes all unit and integration tests and outputs the results to the console.

Run Specific Tests:
To run a specific test (e.g., `JwtAuthHeaderBuilderTests`):

```bash
dotnet test --filter "FullyQualifiedName~JwtAuthHeaderBuilderTests"
```

## Generating and Viewing the Test Coverage Report

The test project uses coverlet to collect coverage data and ReportGenerator to create a detailed HTML report.

### Step 1: Generate the Coverage Report

Run Tests with Coverage Collection:
From the `Collectors.Tests` directory, run the following command to collect coverage data:

```bash
dotnet test --no-restore --collect:"XPlat Code Coverage" --settings coverlet.runsettings
```

This generates a `coverage.cobertura.xml` file in the `TestResults` directory (e.g.,
`TestResults/<guid>/coverage.cobertura.xml`).

Generate the HTML Report:
Use ReportGenerator to convert the coverage data into an HTML report:

```bash
reportgenerator -reports:"TestResults/*/coverage.cobertura.xml" -targetdir:"coveragereport" -reporttypes:Html
```

This creates a `coveragereport` directory with the HTML report files.

### Step 2: View the Coverage Report

Open the Report:
Navigate to the `coveragereport` directory and open `index.html` in a web browser:

```bash
cd coveragereport
start index.html  # On Windows
open index.html   # On macOS
xdg-open index.html  # On Linux
```

The report displays detailed coverage metrics, including line and branch coverage for each file, class, and method.

## Test Coverage Goals

- **Unit Tests**: Ensure each component (e.g., `JwtAuthHeaderBuilder`, `FdaEventsWomanFilter`) is thoroughly tested in
  isolation.
- **Integration Tests**: Validate the end-to-end flow from Quartz scheduling to posting data to the mock endpoint (
  reqres.in).
- **Coverage Target**: Achieve 80%+ line and branch coverage, as reported by the coverage report.

## Troubleshooting

- **Tests Fail**: Check the test output for details. Ensure mocks are set up correctly (e.g., for `IHttpClientFactory`
  in `FdaEventDataProcessorTests`).
- **Coverage Report Not Generated**: Verify that coverlet and ReportGenerator are installed globally and that the
  `TestResults` directory contains the `coverage.cobertura.xml` file.
- **Low Coverage**: Add more tests for uncovered code paths, such as error handling or edge cases.

```