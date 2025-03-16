using Godot;

namespace ProjectNeva.Main.NetworkingArchitecture;

[GlobalClass]
public partial class RoundTopic : Resource
{
    public static readonly string[] Topics = {
        "Яблоко",
        "Луна",
        "Рыба",
        "Поляна",
        "Мяч",
        "Солнце"
    };
    
}