public enum GameMode { PassAndPlay, VsComputer }

public static class GameSettings
{
    public static GameMode Mode = GameMode.VsComputer;
    public static int SearchDepth = 3; // Standard-Tiefe
}