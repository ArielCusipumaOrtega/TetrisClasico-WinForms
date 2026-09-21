using System.Drawing;
using TetrisApp.Models;

namespace TetrisApp.Rendering
{
    /// <summary>
    /// Paleta de colores oficial para los tetrominós y elementos visuales del tablero.
    /// </summary>
    public static class TetrisPalette
    {
        public static readonly Color BoardBackground = Color.FromArgb(17, 19, 24);
        public static readonly Color GridLine = Color.FromArgb(28, 32, 40);

        public static readonly Color ColorI = Color.FromArgb(0, 229, 255);  // Cian
        public static readonly Color ColorJ = Color.FromArgb(41, 121, 255); // Azul
        public static readonly Color ColorL = Color.FromArgb(255, 145, 0);  // Naranja
        public static readonly Color ColorO = Color.FromArgb(255, 214, 0);  // Amarillo
        public static readonly Color ColorS = Color.FromArgb(0, 230, 118);  // Verde
        public static readonly Color ColorT = Color.FromArgb(213, 0, 249);  // Púrpura
        public static readonly Color ColorZ = Color.FromArgb(255, 23, 68);   // Rojo

        public static Color GetColor(TetrominoType type) => type switch
        {
            TetrominoType.I => ColorI,
            TetrominoType.J => ColorJ,
            TetrominoType.L => ColorL,
            TetrominoType.O => ColorO,
            TetrominoType.S => ColorS,
            TetrominoType.T => ColorT,
            TetrominoType.Z => ColorZ,
            _ => BoardBackground
        };
    }
}
