var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres");
var postgresDb = postgres.AddDatabase("db", "postgres");

builder
    .AddProject<Projects.DataLoader_State_GraphQL>("graphql")
    .WithReference(postgres)
    .WithReference(postgresDb);

builder.Build().Run();