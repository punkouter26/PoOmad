var builder = DistributedApplication.CreateBuilder(args);

// Add Azure Table Storage (uses Azurite locally)
var storage = builder.AddAzureStorage("storage")
    .RunAsEmulator();

var tables = storage.AddTables("tables");

// Add API project with dependencies
var api = builder.AddProject("api", @"..\..\PoOmad.Api\PoOmad.Api\PoOmad.Api.csproj")
    .WithReference(tables)
    .WithExternalHttpEndpoints();

// Add Blazor WASM client with reference to API
builder.AddProject("client", @"..\..\PoOmad.Client\PoOmad.Client\PoOmad.Client.csproj")
    .WithReference(api)
    .WithExternalHttpEndpoints();

builder.Build().Run();
