using ProjectNeva.Main.NetworkingArchitecture.GamePhases.AbstractPhase;
using ProjectNeva.Main.Utils.Logger;

namespace ProjectNeva.Main.NetworkingArchitecture.GamePhases;

public class LoadingRatingPhase : LoadingGamePhase
{
    public LoadingRatingPhase(LobbyManager lobbyManager) : base(
        lobbyManager, MultiplayerController.MethodName.Client_LoadRatingScene)
    {
    }

    public override void Enter()
    {
        base.Enter();
        Logger.LogNetwork($"Lobby: {Lobby.LobbyId}. Loading rating scene.");
    }

    protected override void OnEveryoneReady()
    {
        LobbyManager.TransitionTo(LobbyState.Rating);
    }
}