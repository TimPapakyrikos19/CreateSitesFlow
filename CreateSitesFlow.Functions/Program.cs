using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults() // uses isolated worker
    .ConfigureLogging(lb => lb.AddConsole())
    .Build();

host.Run();
