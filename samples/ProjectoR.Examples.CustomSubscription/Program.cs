using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ProjectoR.Core.Registration;
using ProjectoR.EntityFrameworkCore.Registration;
using ProjectoR.Examples.Common;
using ProjectoR.Examples.CustomSubscription;
using ProjectoR.Examples.CustomSubscription.Data;

var builder = Host.CreateApplicationBuilder(args);

builder.Configuration.AddJsonFile("appsettings.json");
builder
    .Services
    .AddLogging(loggingBuilder => loggingBuilder.AddConsole())
    .AddDbContext<CustomSubscriptionContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("UserContext")))
    .AddProjectoR(configurator =>
    {
        configurator
            .UseCustomSubscription<CustomSubscription, UserProjector>()
            .UseEntityFramework(frameworkConfigurator => frameworkConfigurator.UseEntityFrameworkCheckpointing<CustomSubscriptionContext>());
    });

var userContext = builder
    .Services
    .BuildServiceProvider()
    .GetRequiredService<CustomSubscriptionContext>();

userContext.Database.Migrate();

var app = builder.Build();
app.Run();