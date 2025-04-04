using ProjectNeva.Main.NetworkingArchitecture.GamePhases.AbstractPhase;

namespace ProjectNeva.Main.NetworkingArchitecture.GamePhases;

public class LoadingDrawingPhase : LoadingGamePhase
{
    public LoadingDrawingPhase(LobbyManager lobbyManager) : base(lobbyManager,
        MultiplayerController.MethodName.Client_LoadDrawingScene)
    {
    }

    protected override void OnEveryoneReady()
    {
        LobbyManager.TransitionTo(LobbyState.Drawing);
    }
}