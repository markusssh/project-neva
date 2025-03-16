using System;
using Godot;

namespace ProjectNeva.Main.NetworkingArchitecture;

public partial class Player : RefCounted
{
    public Player()
    {
    }

    public Player(long playerId, string playerName)
    {
        PlayerId = playerId;
        PlayerName = playerName;
    }

    public enum PlayerState
    {
        Loading,
        Playing
    }
    
    public long PlayerId { get; set; }

    public string PlayerName { get; set; }
    public int Score { get; set; }
    public int PlayersVoted { get; set; } 
    public byte[] FinalImageData { get; set; }
    public PlayerState State { get; set; } = PlayerState.Loading;

    public void AddScore(int score)
    {
        Score += score;
        PlayersVoted++;
    }

    public double GetAvgScore()
    {
        return Math.Round((double) Score / PlayersVoted, 2);
    }
}