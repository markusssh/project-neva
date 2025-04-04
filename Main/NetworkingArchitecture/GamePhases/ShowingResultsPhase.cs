using ProjectNeva.Main.NetworkingArchitecture.GamePhases.AbstractPhase;

namespace ProjectNeva.Main.NetworkingArchitecture.GamePhases;

public class ShowingResultsPhase : ClosedGamePhase
{
    public ShowingResultsPhase(LobbyManager lobbyManager) : base(lobbyManager)
    {
    }
}