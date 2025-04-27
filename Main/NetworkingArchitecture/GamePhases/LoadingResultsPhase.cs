using ProjectNeva.Main.NetworkingArchitecture.GamePhases.AbstractPhase;
using ProjectNeva.Main.Utils.Logger;

namespace ProjectNeva.Main.NetworkingArchitecture.GamePhases;

public class LoadingResultsPhase : LoadingGamePhase
{
    public LoadingResultsPhase(LobbyManager lobbyManager) : base(lobbyManager,
        MultiplayerController.MethodName.Client_LoadResultsScene)
    {
    }

    public override void Enter()
    {
        base.Enter();
        Logger.LogNetwork($"Lobby: {Lobby.LobbyId}. Loading results scene.");
    }

    protected override void OnEveryoneReady()
    {
        LobbyManager.TransitionTo(LobbyState.ShowingResults);
    }
}