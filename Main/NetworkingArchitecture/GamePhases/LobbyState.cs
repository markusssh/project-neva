namespace ProjectNeva.Main.NetworkingArchitecture.GamePhases;

public enum LobbyState
{
    WaitingPlayers,
    LoadingDrawing,
    Drawing,
    FinishedDrawing,
    Rating,
    FinishedRating,
    ShowingResults,
    Finished
}