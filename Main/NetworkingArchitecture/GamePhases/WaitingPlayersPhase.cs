using ProjectNeva.Main.NetworkingArchitecture.LobbyLogic;

namespace ProjectNeva.Main.NetworkingArchitecture.GamePhases;

public class WaitingPlayersPhase : GamePhase, IConnectable
{
    public WaitingPlayersPhase(Lobby lobby) : base(lobby) {}

    public override void Enter()
    {
        throw new System.NotImplementedException();
    }

    public override void Exit()
    {
        throw new System.NotImplementedException();
    }

    public override void HandlePlayerDisconnect(long playerId)
    {
        throw new System.NotImplementedException();
    }

    public void HandlePlayerConnect(long playerId)
    {
        throw new System.NotImplementedException();
    }
}