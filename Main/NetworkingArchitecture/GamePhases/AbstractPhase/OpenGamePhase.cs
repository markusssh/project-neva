using ProjectNeva.Main.Utils.Logger;

namespace ProjectNeva.Main.NetworkingArchitecture.GamePhases.AbstractPhase;

public class OpenGamePhase : GamePhase
{
    protected OpenGamePhase(LobbyManager lobbyManager) : base(lobbyManager)
    {
    }

    protected override void HandlePlayerConnect(long playerId, JwtValidationResult authData)
    {
        if (Lobby.Players.Count >= Lobby.LobbySize)
        {
            //TODO: add user notification
            Networking.Instance.Multiplayer.DisconnectPeer((int)playerId);
            return;
        }

        var player = new Player(playerId, authData.PlayerName);
        Lobby.Players.Add(playerId, player);

        MultiplayerController.Instance.Server_SendLobbySettings(playerId, Lobby);
        MultiplayerController.Instance.Server_BroadcastLobbyNewPlayer(playerId, Lobby);

        Logger.LogNetwork($"Player {playerId} joined lobby {Lobby.LobbyId}");
        
        if (Lobby.Players.Count == Lobby.LobbySize) LobbyManager.TransitionTo(LobbyState.LoadingDrawing);
    }
}