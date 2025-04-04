using System;
using ProjectNeva.Main.NetworkingArchitecture.GamePhases;
using ProjectNeva.Main.NetworkingArchitecture.GamePhases.AbstractPhase;

namespace ProjectNeva.Main.NetworkingArchitecture;

public class LobbyManager
{
    public readonly Lobby Lobby;
    private GamePhase _currentPhase;

    public LobbyManager(Lobby lobby)
    {
        Lobby = lobby;
    }

    public void TransitionTo(LobbyState newState)
    {
        _currentPhase?.Exit();

        _currentPhase = newState switch
        {
            LobbyState.WaitingPlayers => new WaitingPlayersPhase(this),
            LobbyState.LoadingDrawing => new LoadingDrawingPhase(this),
            LobbyState.Drawing => new DrawingPhase(this),
            LobbyState.LoadingRating => new LoadingRatingPhase(this),
            LobbyState.Rating => new RatingPhase(this),
            LobbyState.LoadingResults => new LoadingResultsPhase(this),
            LobbyState.ShowingResults => new ShowingResultsPhase(this),
            _ => throw new ArgumentOutOfRangeException(nameof(newState))
        };

        _currentPhase.Enter();
    }
}