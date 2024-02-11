using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using ProjectoR.Examples.Common.Data;
using ProjectoR.Examples.Common.Domain;
using ProjectoR.Examples.Common.Domain.User;

namespace ProjectoR.Examples.Common.Projectors;

public class AmountOfUsersInCountryProjector
{
    public static string ProjectionName => "AmountOfUsersPerCountry";
    
    public static async Task<IDbContextTransaction> PreProcess(
        ISampleContext context,
        CancellationToken cancellationToken) =>
        await context.Database.BeginTransactionAsync(IsolationLevel.ReadUncommitted, cancellationToken);

    public static async Task PostProcess(
        ISampleContext context,
        IDbContextTransaction transaction,
        CancellationToken cancellationToken)
    {
        await context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }
    
    public static async Task Project(
        UserWasEnrolled enrolled, 
        IDbContextTransaction transaction,
        ISampleContext context,
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