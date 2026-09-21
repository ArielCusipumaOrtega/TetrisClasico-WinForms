namespace TetrisApp.Models
{
    /// <summary>
    /// Representa una coordenada inmutable (fila, columna) en el tablero.
    /// </summary>
    public readonly record struct Position(int Row, int Column)
    {
        public Position Down() => new(Row + 1, Column);
        public Position Left() => new(Row, Column - 1);
        public Position Right() => new(Row, Column + 1);
        public Position Up() => new(Row - 1, Column);
    }
}
