using System.Collections.Generic;
using System.Linq;

namespace ProjectNeva.Main.NetworkingArchitecture.GamePhases;

public class RatingPhase : ClosedGamePhase
{
    private const int AvgScore = 5;
    private const int MaxScore = 10;
    
    public RatingPhase(LobbyManager lobbyManager) : base(lobbyManager)
    {
        _ratings = Lobby.Players.Keys.ToDictionary(key => key, _ => AvgScore * Lobby.Players.Count);
    }
    
    private readonly Dictionary<long, int> _ratings;
    
}