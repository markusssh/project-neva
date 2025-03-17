namespace ProjectNeva.Main.NetworkingArchitecture.GamePhases;

public enum LobbyState
{
    WaitingPlayers,
    Loading,
    Drawing,
    FinishedDrawing,
    Rating,
    FinishedRating,
    ShowingResults,
    Finished
}