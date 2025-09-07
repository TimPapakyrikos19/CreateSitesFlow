using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureAppConfiguration(config =>
    {
        config.AddJsonFile("local.settings.json", optional: true)
              .AddEnvironmentVariables();
    })
    .ConfigureServices((ctx, services) =>
    {
        services.AddHttpClient("graph", c =>
        {
            c.BaseAddress = new Uri("https://graph.microsoft.com/");
        });

        services.AddSingleton<AppOptions>(sp =>
        {
            var cfg = sp.GetRequiredService<IConfiguration>();
            return new AppOptions(
                cfg["TenantId"]!,
                cfg["ClientId"]!,
                cfg["ClientSecret"]!,
                cfg["GraphScopes"]!.Split(' ', StringSplitOptions.RemoveEmptyEntries)
            );
        });

        services.AddSingleton<TokenHelper>();
        services.AddSingleton<GraphHelper>(sp =>
        {
            var http = sp.GetRequiredService<IHttpClientFactory>().CreateClient("graph");
            return new GraphHelper(http);
        });
    })
    .Build();

host.Run();
