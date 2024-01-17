using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using ProjectoR.Examples.EventStore;

namespace ProjectoR.Examples.Common;

public class AmountOfUsersInCitiesProjector
{
    public static string ProjectionName => "AmountOfUsersPerCity";
    
    public static async Task<IDbContextTransaction> PreProcess(
        ApplicationContext context,
        CancellationToken cancellationToken) =>
        await context.Database.BeginTransactionAsync(IsolationLevel.ReadUncommitted, cancellationToken);

    public static async Task PostProcess(
        ApplicationContext context,
        IDbContextTransaction transaction,
        CancellationToken cancellationToken)
    {
        await context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }
    
    public static async Task Project(
        User.Enrolled enrolled, 
        IDbContextTransaction transaction,
        ApplicationContext context,
        CancellationToken cancellationToken)
    {
        var projectionExists = await context
            .AmountOfUsersPerCityProjections
            .AnyAsync(projection => projection.City == enrolled.City, cancellationToken: cancellationToken);

        if (projectionExists)
        {
            await context
                .AmountOfUsersPerCityProjections
                .Where(projection => projection.City == enrolled.City)
                .ExecuteUpdateAsync(calls => calls.SetProperty(projection => projection.Amount, projection => projection.Amount + 1), cancellationToken: cancellationToken);
            return;
        }
        
        context.AmountOfUsersPerCityProjections.Add(new AmountOfUserPerCityProjection
        {
            City = enrolled.City,
            Amount = 1
        });

        await context.SaveChangesAsync(cancellationToken);
    }
}