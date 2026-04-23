var builder = DistributedApplication.CreateBuilder(args);

var api = builder.AddProject<Projects.EcommerceApi>("api")
    .WithExternalHttpEndpoints();

builder.AddProject<Projects.EcommerceMvc>("mvc")
    .WithExternalHttpEndpoints()
    .WaitFor(api);

builder.Build().Run();
