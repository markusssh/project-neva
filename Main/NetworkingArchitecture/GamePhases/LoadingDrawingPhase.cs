using ProjectNeva.Main.NetworkingArchitecture.GamePhases.AbstractPhase;
using ProjectNeva.Main.Utils.Logger;

namespace ProjectNeva.Main.NetworkingArchitecture.GamePhases;

public class LoadingDrawingPhase : LoadingGamePhase
{
    public LoadingDrawingPhase(LobbyManager lobbyManager) : base(lobbyManager,
        MultiplayerController.MethodName.Client_LoadDrawingScene)
    {
    }

    public override void Enter()
    {
        base.Enter();
        Logger.LogNetwork($"Lobby: {Lobby.LobbyId}. Loading drawing scene.");
    }

    protected override void OnEveryoneReady()
    {
        LobbyManager.TransitionTo(LobbyState.Drawing);
    }
}