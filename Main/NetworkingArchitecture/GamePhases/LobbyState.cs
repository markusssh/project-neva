namespace ProjectNeva.Main.NetworkingArchitecture.GamePhases;

public enum LobbyState
{
    WaitingPlayers,
    LoadingDrawing,
    Drawing,
    LoadingRating,
    Rating,
    FinishedRating,
    ShowingResults,
    Finished
}