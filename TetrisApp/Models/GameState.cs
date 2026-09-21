namespace TetrisApp.Models
{
    /// <summary>
    /// Estados posibles del ciclo de vida del juego.
    /// </summary>
    public enum GameState
    {
        NotStarted,
        Playing,
        Paused,
        GameOver
    }
}
