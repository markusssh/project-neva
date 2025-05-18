using System;
using Godot;

namespace ProjectNeva.Main.NetworkingArchitecture;

[GlobalClass]
public partial class RoundTopic : Resource
{
    public static string GetNewTopic => Topics[new Random().Next(Topics.Length - 1)];

    //TODO: ALERT!!!!!!!!
    private static readonly string[] Topics = {
        "Яблоко",
        //"Луна",
        //"Рыба",
        //"Поляна",
        //"Мяч",
        //"Солнце"
    };
    
}