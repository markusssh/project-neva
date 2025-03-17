using System.Collections.Generic;
using System.Linq;
using ProjectNeva.Main.NetworkingArchitecture.LobbyLogic;

namespace ProjectNeva.Main.NetworkingArchitecture.GamePhases;

public class LoadingDrawingPhase : ClosedGamePhase
{
    public LoadingDrawingPhase(LobbyManager lobbyManager) : base(lobbyManager)
    {
        _readiness = Lobby.Players.Keys.ToDictionary(key => key, key => false);
    }
    
    private readonly Dictionary<long, bool> _readiness;
    private bool EveryoneReady => _readiness.Values.All(ready => ready);

    public override void Enter()
    {
        base.Enter();
        MultiplayerController.Instance.Server_Broadcast(
            Lobby,
            MultiplayerController.MethodName.Client_LoadDrawingScene);
    }
    
}