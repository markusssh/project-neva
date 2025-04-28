using System;
using Godot;

namespace ProjectNeva.Main.NetworkingArchitecture;

[GlobalClass]
public partial class RoundTopic : Resource
{
    public static string GetNewTopic => RoundTopic.Topics[new Random().Next(RoundTopic.Topics.Length - 1)];

    private static readonly string[] Topics = {
        "Яблоко",
        "Луна",
        "Рыба",
        "Поляна",
        "Мяч",
        "Солнце"
    };
    
}