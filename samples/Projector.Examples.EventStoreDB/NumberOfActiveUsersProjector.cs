using Microsoft.EntityFrameworkCore;
using ProjectoR.Core.Projector;
using ProjectoR.Examples.EventStoreDB.Data;

namespace ProjectoR.Examples.EventStoreDB;

public class NumberOfActiveUsersProjector(UserContext connection, IServiceProvider serviceProvider) : BatchProjector<UserContext>(connection, serviceProvider)
{
    public override string ProjectionName => "NumberOfActiveUsers";

    public override Type[] EventTypes => new[]
    {
        typeof(UserEnrolled),
        typeof(UserQuit)
    };

    private readonly Dictionary<string, NumberOfActiveUsersProjection> _cache = new();
    
    protected override async Task ProjectBatch(
        UserContext connection, 
        IEnumerable<object> events, 
        CancellationToken cancellationToken = default)
    {
        foreach (var @event in events)
        {
            switch (@event)
            {
                case UserEnrolled userEnrolled:
                    await IncreaseNumberOfActiveUsers(connection, userEnrolled, cancellationToken);
                    break;
                
                case UserQuit userQuit:
                    await DecreaseNumberOfActiveUsers(connection, userQuit, cancellationToken);
                    break;
            }
        }

        await connection.SaveChangesAsync(cancellationToken);
    }

    private async Task DecreaseNumberOfActiveUsers(
        UserContext connection, 
        UserQuit userQuit,
        CancellationToken cancellationToken)
    {
        var projection = await LoadExistingElseAdd(userQuit.Country, connection, cancellationToken);
        projection.Number--;
    }

    private async Task IncreaseNumberOfActiveUsers(
        UserContext connection,
        UserEnrolled userEnrolled,
        CancellationToken cancellationToken)
    {
        var projection = await LoadExistingElseAdd(userEnrolled.Country, connection, cancellationToken);
        projection.Number++;
    }

    private async Task<NumberOfActiveUsersProjection> LoadExistingElseAdd(
        string country,
        UserContext connection,
        CancellationToken cancellationToken)
    {
        if (_cache.TryGetValue(country, out var projection))
            return projection;
        
        projection = await connection
            .NumberOfActiveUsersProjections
            .AsTracking()
            .SingleOrDefaultAsync(projection => projection.Country == country, cancellationToken: cancellationToken);

        if (projection is not null)
        {
            AddToCache(projection);
            return projection;
        }
        
        projection = new NumberOfActiveUsersProjection
        {
            Country = country
        };
        AddToCache(projection);
        
        connection.NumberOfActiveUsersProjections.Add(projection);
        return projection;
    }

    private void AddToCache(NumberOfActiveUsersProjection projection)
    {
        _cache[projection.Country] = projection;
    }
}