﻿using System;
using ProjectNeva.Main.NetworkingArchitecture.GamePhases;

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
            /*LobbyState.FinishedRating => expr,
            LobbyState.ShowingResults => expr,
            LobbyState.Finished => expr,*/
            _ => throw new ArgumentOutOfRangeException(nameof(newState))
        };

        _currentPhase.Enter();
    }
}