using Godot;

namespace ProjectNeva.Main.Utils;

[GlobalClass]
public partial class HttpHelper : RefCounted
{
    public static string GetMainServerAddress() 
    {
        return "http://localhost:8080";
    }

    public static string CreateUri() 
    {
        return "/lobby/new-lobby";
    }

    public static string JoinUri() 
    {
        return "/lobby/join-lobby";
    }
}