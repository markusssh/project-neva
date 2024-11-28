using Godot;

namespace ProjectNeva.Main.NetworkingArchitecture;

[GlobalClass]
public partial class RoundTheme : Resource
{
    public string ThemeName { get; private set; }
    public string Author { get; private set; }

    public RoundTheme()
    {
    }

    public RoundTheme(string themeName, string author)
    {
        ThemeName = themeName;
        Author = author;
    }
}