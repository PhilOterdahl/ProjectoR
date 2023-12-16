using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ProjectoR.Core.Registration;
using ProjectoR.EventStoreDB.Checkpointing;
using ProjectoR.EventStoreDb.Registration;
using ProjectoR.Examples.EventStoreDB;
using ProjectoR.Examples.EventStoreDB.Data;

var builder = Host.CreateApplicationBuilder(args);

builder.Configuration.AddJsonFile("appsettings.json");
builder
    .Services
    .AddDbContext<UserContext>(options =>
    {
        options.UseNpgsql(builder.Configuration.GetConnectionString("UserContext"));
    })
    .AddProjectoR(configurator =>
    {
        configurator.UseEventStoreDB(
            builder.Configuration.GetConnectionString("EventStoreDB"),
            dbConfigurator =>
            {
                dbConfigurator
                    .UseEventStoreDBCheckpointing()
                    .UseProjector<UserProjector>(configure =>
                    {
                        configure.ProjectorOptions
                            .UseNameSpaceEventTypeResolver()
                            .UseKebabCaseEventNaming();
                    })
                    .UseProjector<NumberOfActiveUsersProjector>(configure =>
                    {
                        configure.ProjectorOptions
                            .UseNameSpaceEventTypeResolver()
                            .UseKebabCaseEventNaming();
                    });
            }
        );
    })
    .AddHostedService<UserSeeder>();

var userContext = builder
    .Services
    .BuildServiceProvider()
    .GetRequiredService<UserContext>();

userContext.Database.Migrate();

var app = builder.Build();
app.Run();
