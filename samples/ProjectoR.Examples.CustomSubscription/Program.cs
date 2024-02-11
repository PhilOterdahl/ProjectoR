using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ProjectoR.Core.Projector;
using ProjectoR.Core.Registration;
using ProjectoR.EntityFrameworkCore.Registration;
using ProjectoR.Examples.Common;
using ProjectoR.Examples.Common.Data;
using ProjectoR.Examples.Common.Projectors;
using ProjectoR.Examples.CustomSubscription;
using ProjectoR.Examples.CustomSubscription.Data;

var builder = Host.CreateApplicationBuilder(args);

builder.Configuration.AddJsonFile("appsettings.json");
builder
    .Services
    .AddLogging(loggingBuilder => loggingBuilder.AddConsole())
    .AddDbContextPool<ApplicationContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("ApplicationContext")))
    .AddScoped<ISampleContext>(provider => provider.GetRequiredService<ApplicationContext>())
    .AddProjectoR(configurator =>
    {
        configurator
            .UseCustomSubscription<CustomSubscription, UserProjector>(configure =>
            {
                configure.Priority = ProjectorPriority.Highest;
                configure.BatchingOptions.BatchSize = 100;
                configure.BatchingOptions.BatchTimeout = TimeSpan.FromMilliseconds(100);
                configure.CheckpointingOptions.CheckpointAfterBatch();
                configure.SerializationOptions
                    .UseClassNameEventTypeResolver()
                    .UseSnakeCaseEventNaming();
            })
            .UseCustomSubscription<CustomSubscription, AmountOfUsersInCitiesProjector>(configure =>
            {
                configure.Priority = ProjectorPriority.Normal;
                configure.BatchingOptions.BatchSize = 100;
                configure.BatchingOptions.BatchTimeout = TimeSpan.FromMilliseconds(100);
                configure.CheckpointingOptions.CheckpointAfterBatch();
                configure.SerializationOptions
                    .UseClassNameEventTypeResolver()
                    .UseSnakeCaseEventNaming();
            })
            .UseCustomSubscription<CustomSubscription, AmountOfUsersInCountryProjector>(configure =>
            {
                configure.Priority = ProjectorPriority.Lowest;
                configure.BatchingOptions.BatchSize = 100;
                configure.BatchingOptions.BatchTimeout = TimeSpan.FromMilliseconds(100);
                configure.CheckpointingOptions.CheckpointAfterBatch();
                configure.SerializationOptions
                    .UseClassNameEventTypeResolver()
                    .UseSnakeCaseEventNaming();
            })
            .UseEntityFramework(frameworkConfigurator => frameworkConfigurator.UseEntityFrameworkCheckpointing<ApplicationContext>());
    });

var dbContext = builder
    .Services
    .BuildServiceProvider()
    .GetRequiredService<ApplicationContext>();

dbContext.Database.Migrate();

var app = builder.Build();
await Seeder.Seed(100, app.Services);
app.Run();