using ProjectoR.Examples.Common.Domain;
using ProjectoR.Examples.Common.Domain.User;

namespace ProjectoR.Examples.Common.Data;

public interface IUserRepository
{
    Task<User?> TryLoad(string id, CancellationToken cancellationToken);
    
    Task<User> Load(string id, CancellationToken cancellationToken);
    
    Task CommitEvents(User user, CancellationToken cancellationToken);
}