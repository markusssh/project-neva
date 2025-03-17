using ProjectNeva.Main.NetworkingArchitecture.LobbyLogic;

namespace ProjectNeva.Main.NetworkingArchitecture.GamePhases;

public class DrawingPhase : ClosedGamePhase
{
    public DrawingPhase(LobbyManager lobbyManager) : base(lobbyManager)
    {
    }

    public override void Enter()
    {
        base.Enter();
        MultiplayerController.Instance.Server_Broadcast(
            Lobby, 
            MultiplayerController.MethodName.Client_ShootDrawingScene);
    }
}