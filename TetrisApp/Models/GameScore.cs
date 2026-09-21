using System;

namespace TetrisApp.Models
{
    /// <summary>
    /// Gestiona las estadísticas de juego, puntuación clásica, líneas acumuladas y cálculo de nivel.
    /// </summary>
    public class GameScore
    {
        public int Score { get; private set; }
        public int Lines { get; private set; }
        public int Level { get; private set; } = 1;

        /// <summary>
        /// Reinicia las estadísticas a sus valores iniciales.
        /// </summary>
        public void Reset()
        {
            Score = 0;
            Lines = 0;
            Level = 1;
        }

        /// <summary>
        /// Agrega puntuación por líneas eliminadas según el sistema clásico y ajusta el nivel.
        /// </summary>
        public void AddLines(int count)
        {
            if (count <= 0) return;

            Lines += count;

            int puntosBase = count switch
            {
                1 => 100,
                2 => 300,
                3 => 500,
                4 => 800, // Tetris!
                _ => 100
            };

            Score += puntosBase * Level;

            // Cada 10 líneas se sube de nivel
            Level = (Lines / 10) + 1;
        }

        /// <summary>
        /// Bonificación por caída rápida instantánea (Hard Drop).
        /// </summary>
        public void AddHardDropBonus(int dropSteps)
        {
            if (dropSteps > 0)
            {
                Score += dropSteps * 2;
            }
        }

        /// <summary>
        /// Calcula el intervalo en milisegundos para el temporizador de gravedad según el nivel actual.
        /// </summary>
        public int CalculateDropIntervalMs()
        {
            return Math.Max(100, 650 - ((Level - 1) * 55));
        }
    }
}
