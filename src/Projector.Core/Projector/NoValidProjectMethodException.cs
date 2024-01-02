namespace ProjectoR.Core.Projector;

public class NoValidProjectMethodException(string projectionName) : InvalidOperationException($"No valid project method for projector with projection: {projectionName}");