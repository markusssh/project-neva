using System;
using ProjectNeva.Main.NetworkingArchitecture.GamePhases;

namespace ProjectNeva.Main.NetworkingArchitecture.LobbyLogic;

public class LobbyManager
{
    private readonly Lobby _lobby;
    private GamePhase _currentPhase;

    public LobbyManager(Lobby lobby)
    {
        _lobby = lobby;
    }

    public void TransitionTo(LobbyState newState)
    {
        _currentPhase?.Exit();

        _currentPhase = newState switch
        {
            LobbyState.WaitingPlayers => new WaitingPlayersPhase(_lobby),
            LobbyState.Loading => expr,
            LobbyState.Drawing => expr,
            LobbyState.FinishedDrawing => expr,
            LobbyState.Rating => expr,
            LobbyState.FinishedRating => expr,
            LobbyState.ShowingResults => expr,
            LobbyState.Finished => expr,
            _ => throw new ArgumentOutOfRangeException(nameof(newState))
        };

        _currentPhase.Enter();
    }

    public void HandlePlayerConnect(AuthResponseDto authData)
    {
        _lobby.OnPlayerConnected(authData);
    }
}