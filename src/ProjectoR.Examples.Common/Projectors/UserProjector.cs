using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using ProjectoR.Examples.Common.Data;
using ProjectoR.Examples.Common.Domain.User;

namespace ProjectoR.Examples.Common.Projectors;

public class UserProjector
{
    public static string ProjectionName => "User";
    
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
        context.UsersProjections.Add(new UserProjection
        {
            Id = enrolled.Id,
            FirstName = enrolled.FirstName,
            LastName = enrolled.LastName,
            Address = new Address
            {
                CountryCode = enrolled.CountryCode,
                City = enrolled.City,
                Street = enrolled.Street,
                PostalCode = enrolled.PostalCode
            },
            ContactInformation = new ContactInformation
            {
                Email = enrolled.Email,
                Mobile = enrolled.Mobile
            }
        });

        await context.SaveChangesAsync(cancellationToken);
    }
    
    public async Task Project(UserMoved moved, ISampleContext context, CancellationToken cancellationToken) =>
        await context
            .UsersProjections
            .Where(user => user.Id == moved.Id)
            .ExecuteUpdateAsync(calls => calls
                    .SetProperty(projection => projection.Address.City, moved.City)
                    .SetProperty(projection => projection.Address.PostalCode, moved.PostalCode)
                    .SetProperty(projection => projection.Address.Street, moved.Street),
                cancellationToken
            );

    public async Task Project(UserChangedContactInformation changedContactInformation, ISampleContext context, CancellationToken cancellationToken) =>
        await context
            .UsersProjections
            .Where(user => user.Id == changedContactInformation.Id)
            .ExecuteUpdateAsync(calls => calls
                    .SetProperty(projection => projection.ContactInformation.Email, changedContactInformation.Email)
                    .SetProperty(projection => projection.ContactInformation.Mobile, changedContactInformation.Mobile),
                cancellationToken
            );

    public async Task Project(UserQuit userQuit, ISampleContext context, CancellationToken cancellationToken) =>
        await context
            .UsersProjections
            .Where(user => user.Id == userQuit.Id)
            .ExecuteDeleteAsync(cancellationToken);
}