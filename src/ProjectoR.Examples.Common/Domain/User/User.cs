namespace ProjectoR.Examples.Common.Domain.User;

public class User : Aggregate<UserState>
{
    public static User Enroll(
        string id,
        string firstName,
        string lastName, 
        string email, 
        string mobile,
        string countryCode, 
        string city, 
        string postalCode, 
        string street) =>
        new(id, firstName, lastName, email, mobile, countryCode, city, postalCode, street);
    
    private User(  
        string id,
        string firstName,
        string lastName,
        string email,
        string mobile,
        string countryCode,
        string city,
        string postalCode,
        string street)
    {
        var enrolled = new UserWasEnrolled(
            id, 
            firstName, 
            lastName, 
            email, 
            mobile, 
            countryCode, 
            city, 
            postalCode, 
            street,
            DateTimeOffset.UtcNow
        );
        
        AddEvent(enrolled);
    }

    public User Move(
        string countryCode,
        string city,
        string postalCode,
        string street)
    {
        if (State.QuitDate.HasValue)
            throw new InvalidOperationException("Can not modify a user that has quit");
        
        var moved = new UserMoved(State.Id, countryCode, city, postalCode, street);
        
        AddEvent(moved);
        return this;
    }
    
    public User ChangeContactInformation(
        string mobile,
        string email)
    {
        if (State.QuitDate.HasValue)
            throw new InvalidOperationException("Can not modify a user that has quit");
        
        var changedContactInformation = new UserChangedContactInformation(State.Id, mobile, email);
        
        AddEvent(changedContactInformation);
        return this;
    }
    
    public User Quit()
    {
        if (State.QuitDate.HasValue)
            throw new InvalidOperationException("Can not modify a user that has quit");
        
        var quit = new UserQuit(State.Id, State.CountryCode, DateTimeOffset.UtcNow);
        
        AddEvent(quit);
        return this;
    }

    protected override UserState ApplyEvent(UserState currentState, object @event) =>
        @event switch
        {   
            UserWasEnrolled userWasEnrolled => new UserState
            {
                Id = userWasEnrolled.Id,
                FirstName = userWasEnrolled.FirstName,
                CountryCode = userWasEnrolled.CountryCode,
                LastName = userWasEnrolled.LastName,
                PostalCode = userWasEnrolled.PostalCode,
                City = userWasEnrolled.City,
                Email = userWasEnrolled.Email,
                Mobile = userWasEnrolled.Mobile,
                Street = userWasEnrolled.Street,
                EnrolledDate = userWasEnrolled.EnrolledDate
            },
            UserMoved userMoved => currentState with
            {
                City = userMoved.City,
                PostalCode = userMoved.PostalCode,
                Street = userMoved.Street,
                CountryCode = userMoved.CountryCode
            },
            UserChangedContactInformation userChangedContactInformation => currentState with
            {
                Email = userChangedContactInformation.Email,
                Mobile = userChangedContactInformation.Mobile
            },
            UserQuit userQuit => currentState with
            {
                QuitDate = userQuit.QuitDate
            },
            _ => throw new ArgumentOutOfRangeException(nameof(@event), $"Event of type: {@event.GetType().Name} is not handled by applyEvent method")
        };
    
}