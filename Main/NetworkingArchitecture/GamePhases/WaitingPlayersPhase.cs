using ProjectNeva.Main.NetworkingArchitecture.LobbyLogic;

namespace ProjectNeva.Main.NetworkingArchitecture.GamePhases;

public class WaitingPlayersPhase : ConnectableGamePhase
{
    public WaitingPlayersPhase(Lobby lobby) : base(lobby) {}

    public override void Enter()
    {
        base.Enter();
    }

    public override void Exit()
    {
        base.Exit();
    }

    protected override void HandlePlayerDisconnect(long playerId)
    {
        throw new System.NotImplementedException();
    }

    protected override void HandlePlayerConnect(AuthResponseDto authData)
    {
        if (lobby.State != Lobby.LobbyState.WaitingPlayers || lobby.Players.Count >= lobby.LobbySize)
        {
            //TODO: add ability to reconnect
            GD.PrintErr($"Cannot connect peer {newPeerId}! Lobby {lobby.LobbyId} is not accepting players.");
            Networking.Instance.Multiplayer.DisconnectPeer((int)newPeerId);
            return;
        }

        var player = new Player(newPeerId, peerAuthData.PlayerName);
        Networking.Instance.PeerAuthData.Remove((int)newPeerId);
        _playerIdToLobbyId.Add(newPeerId, lobby.LobbyId);
        lobby.Players.Add(newPeerId, player);

        RpcId(newPeerId, MethodName.SyncLobbySettingsOnClient,
            lobby.LobbySize,
            lobby.Topic);

        foreach (var playerId in lobby.Players.Keys)
        {
            if (newPeerId != playerId)
            {
                RpcId(newPeerId, MethodName.HandlePlayerConnectedOnClient, playerId,
                    lobby.Players[playerId].PlayerName);
            }

            RpcId(playerId, MethodName.HandlePlayerConnectedOnClient, newPeerId, lobby.Players[newPeerId].PlayerName);
        }

        Logger.LogNetwork($"Player {newPeerId} connected to lobby {lobby.LobbyId}");

        if (lobby.Players.Count == lobby.LobbySize)
        {
            Logger.LogNetwork($"Launching game in lobby {lobby.LobbyId}");
            lobby.State = Lobby.LobbyState.Playing;
            await OnPlaying(lobby);
        }
    }
}