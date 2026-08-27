namespace WordoGuessr.Words.Contract;

public sealed record WordsDataRequest(
    RequestOperation.TotalWords? TotalWords = null,
    RequestOperation.DistancesToWords? DistancesToWords = null,
    RequestOperation.DistanceToWord? DistanceToWord = null,
    RequestOperation.WordByDistance? WordByDistance = null,
    RequestOperation.ClosestWords? ClosestWords = null
);