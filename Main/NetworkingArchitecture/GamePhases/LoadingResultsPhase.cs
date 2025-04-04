using Godot;
using ProjectNeva.Main.NetworkingArchitecture.GamePhases.AbstractPhase;

namespace ProjectNeva.Main.NetworkingArchitecture.GamePhases;

public class LoadingResultsPhase : LoadingGamePhase
{
    public LoadingResultsPhase(LobbyManager lobbyManager) : base(lobbyManager,
        MultiplayerController.MethodName.Client_LoadResultsScene)
    {
    }

    protected override void OnEveryoneReady()
    {
        LobbyManager.TransitionTo(LobbyState.ShowingResults);
    }
}