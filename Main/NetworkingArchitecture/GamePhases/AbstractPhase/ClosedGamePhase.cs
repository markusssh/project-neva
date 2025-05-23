﻿namespace ProjectNeva.Main.NetworkingArchitecture.GamePhases.AbstractPhase;

public abstract class ClosedGamePhase : GamePhase
{
    protected ClosedGamePhase(LobbyManager lobbyManager) : base(lobbyManager)
    {
    }

    protected override void HandlePlayerConnect(long playerId, JwtValidationResult authData)
    {
        //TODO: add user notification
        //TODO: add ability to reconnect
        Networking.Instance.Multiplayer.DisconnectPeer((int)playerId);
    }
}