namespace FarlaTweaks.Core.Monitoring;

public sealed record CopilotObservation(
    string State,
    string Title,
    string Detail,
    DateTimeOffset Timestamp);
