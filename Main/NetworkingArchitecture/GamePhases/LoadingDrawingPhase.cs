using ProjectNeva.Main.NetworkingArchitecture.LobbyLogic;

namespace ProjectNeva.Main.NetworkingArchitecture.GamePhases;

public class LoadingDrawingPhase : ClosedGamePhase
{
    public LoadingDrawingPhase(LobbyManager lobbyManager) : base(lobbyManager) {}

    public override void Enter()
    {
        base.Enter();
        MultiplayerController.Instance.Server_BroadcastDrawingStart(Lobby);
    }
}