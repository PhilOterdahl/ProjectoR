using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using ProjectoR.Examples.EventStore;

namespace ProjectoR.Examples.Common;

public class AmountOfUsersInCountryProjector
{
    public static string ProjectionName => "AmountOfUsersPerCountry";
    
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
            .AmountOfUsersPerCountryProjections
            .AnyAsync(projection => projection.CountryCode == enrolled.CountryCode, cancellationToken: cancellationToken);

        if (projectionExists)
        {
            await context
                .AmountOfUsersPerCountryProjections
                .Where(projection => projection.CountryCode == enrolled.CountryCode)
                .ExecuteUpdateAsync(calls => calls.SetProperty(projection => projection.Amount, projection => projection.Amount + 1), cancellationToken: cancellationToken);
            return;
        }
        
        context.AmountOfUsersPerCountryProjections.Add(new AmountOfUsersPerCountryProjection
        {
            CountryCode = enrolled.CountryCode,
            Amount = 1
        });

        await context.SaveChangesAsync(cancellationToken);
    }
}