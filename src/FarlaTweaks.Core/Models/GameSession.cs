namespace FarlaTweaks.Core.Models;

public sealed record GameSession(
    string Game,
    DateTimeOffset StartedAt,
    DateTimeOffset EndedAt,
    TimeSpan Duration);
