namespace ProjectoR.Core.Projector;

public class ProjectMethodWithOutParametersException(string projectionName) : InvalidOperationException(
    $"Projector with projection: {projectionName} has a project method without parameters, first parameters needs to be the event");