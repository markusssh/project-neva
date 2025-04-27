using ProjectNeva.Main.NetworkingArchitecture.GamePhases.AbstractPhase;
using ProjectNeva.Main.Utils.Logger;

namespace ProjectNeva.Main.NetworkingArchitecture.GamePhases;

public class ShowingResultsPhase : ClosedGamePhase
{
    public ShowingResultsPhase(LobbyManager lobbyManager) : base(lobbyManager)
    {
    }

    public override void Enter()
    {
        base.Enter();
        Logger.LogNetwork($"Lobby: {Lobby.LobbyId}. Results scene started.");
    }
}