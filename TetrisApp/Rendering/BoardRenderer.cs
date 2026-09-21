using System.Drawing;
using TetrisApp.Models;

namespace TetrisApp.Rendering
{
    /// <summary>
    /// Se encarga del renderizado gráfico GDI+ del tablero, piezas y vista previa.
    /// Desacopla la lógica de dibujo de la clase del formulario.
    /// </summary>
    public class BoardRenderer
    {
        public int CellSize { get; set; } = 30;

        /// <summary>
        /// Dibuja el tablero completo: fondo, cuadrícula tenue, bloques fijos, pieza activa y cartel de pausa.
        /// </summary>
        public void DrawBoard(Graphics g, GameBoard board, Tetromino? activePiece, Position activePos, Size canvasSize, bool isPaused, Position? ghostPos = null)
        {
            g.Clear(TetrisPalette.BoardBackground);

            // 1. Dibujar cuadrícula tenue de fondo
            using (Pen penRejilla = new(TetrisPalette.GridLine, 1))
            {
                for (int f = 0; f < GameBoard.Rows; f++)
                {
                    g.DrawLine(penRejilla, 0, f * CellSize, GameBoard.Columns * CellSize, f * CellSize);
                }
                for (int c = 0; c < GameBoard.Columns; c++)
                {
                    g.DrawLine(penRejilla, c * CellSize, 0, c * CellSize, GameBoard.Rows * CellSize);
                }
            }

            // 2. Dibujar bloques fijos del tablero
            for (int f = 0; f < GameBoard.Rows; f++)
            {
                for (int c = 0; c < GameBoard.Columns; c++)
                {
                    var cellType = board[f, c];
                    if (cellType != TetrominoType.Empty)
                    {
                        DrawBlock(g, c * CellSize, f * CellSize, CellSize, TetrisPalette.GetColor(cellType));
                    }
                }
            }

            // 3. Dibujar la silueta translúcida de la pieza fantasma (Ghost Piece)
            if (activePiece != null && ghostPos.HasValue && ghostPos.Value != activePos)
            {
                int dim = activePiece.Dimension;
                Color baseColor = TetrisPalette.GetColor(activePiece.Type);
                Color fillColor = Color.FromArgb(45, baseColor.R, baseColor.G, baseColor.B);
                Color borderColor = Color.FromArgb(140, baseColor.R, baseColor.G, baseColor.B);

                for (int r = 0; r < dim; r++)
                {
                    for (int c = 0; c < dim; c++)
                    {
                        if (activePiece.Matrix[r, c] != 0)
                        {
                            int drawX = (ghostPos.Value.Column + c) * CellSize;
                            int drawY = (ghostPos.Value.Row + r) * CellSize;
                            DrawGhostBlock(g, drawX, drawY, CellSize, fillColor, borderColor);
                        }
                    }
                }
            }

            // 4. Dibujar la pieza activa
            if (activePiece != null)
            {
                int dim = activePiece.Dimension;
                for (int r = 0; r < dim; r++)
                {
                    for (int c = 0; c < dim; c++)
                    {
                        if (activePiece.Matrix[r, c] != 0)
                        {
                            int drawX = (activePos.Column + c) * CellSize;
                            int drawY = (activePos.Row + r) * CellSize;
                            DrawBlock(g, drawX, drawY, CellSize, TetrisPalette.GetColor(activePiece.Type));
                        }
                    }
                }
            }

            // 4. Mensaje de pausa superpuesto
            if (isPaused)
            {
                DrawPauseOverlay(g, canvasSize);
            }
        }

        /// <summary>
        /// Dibuja la vista previa centrada de la siguiente pieza.
        /// </summary>
        public void DrawNextPiecePreview(Graphics g, Tetromino? nextPiece, Size previewAreaSize)
        {
            g.Clear(TetrisPalette.BoardBackground);

            if (nextPiece == null) return;

            int dim = nextPiece.Dimension;
            int offsetX = (previewAreaSize.Width - (dim * CellSize)) / 2;
            int offsetY = (previewAreaSize.Height - (dim * CellSize)) / 2;

            for (int r = 0; r < dim; r++)
            {
                for (int c = 0; c < dim; c++)
                {
                    if (nextPiece.Matrix[r, c] != 0)
                    {
                        int drawX = offsetX + (c * CellSize);
                        int drawY = offsetY + (r * CellSize);
                        DrawBlock(g, drawX, drawY, CellSize, TetrisPalette.GetColor(nextPiece.Type));
                    }
                }
            }
        }

        /// <summary>
        /// Dibuja un bloque con relieve tridimensional (bisel / bevel).
        /// </summary>
        public void DrawBlock(Graphics g, int x, int y, int size, Color colorBase)
        {
            Rectangle rect = new(x, y, size, size);

            using (SolidBrush brush = new(colorBase))
            {
                g.FillRectangle(brush, rect);
            }

            using (Pen penBorde = new(Color.FromArgb(40, 0, 0, 0), 1))
            {
                g.DrawRectangle(penBorde, rect);
            }

            // Efecto Bisel / Brillo 3D
            using (Pen penBrillo = new(Color.FromArgb(120, 255, 255, 255), 2))
            {
                g.DrawLine(penBrillo, x + 1, y + 1, x + size - 2, y + 1);
                g.DrawLine(penBrillo, x + 1, y + 1, x + 1, y + size - 2);
            }
        }

        /// <summary>
        /// Dibuja una celda de la silueta fantasma como bloque translúcido con contorno visible.
        /// </summary>
        public void DrawGhostBlock(Graphics g, int x, int y, int size, Color fillColor, Color borderColor)
        {
            Rectangle rect = new(x, y, size, size);

            using (SolidBrush brush = new(fillColor))
            {
                g.FillRectangle(brush, rect);
            }

            using (Pen pen = new(borderColor, 1.5f))
            {
                g.DrawRectangle(pen, x + 1, y + 1, size - 2, size - 2);
            }
        }

        private static void DrawPauseOverlay(Graphics g, Size canvasSize)
        {
            using (SolidBrush sombraBrush = new(Color.FromArgb(160, 0, 0, 0)))
            {
                g.FillRectangle(sombraBrush, 0, 0, canvasSize.Width, canvasSize.Height);
            }

            using Font fuentePausa = new("Segoe UI", 18, FontStyle.Bold);
            using SolidBrush textoBrush = new(Color.White);

            string texto = "JUEGO PAUSADO";
            SizeF tam = g.MeasureString(texto, fuentePausa);
            g.DrawString(texto, fuentePausa, textoBrush,
                         (canvasSize.Width - tam.Width) / 2,
                         (canvasSize.Height - tam.Height) / 2);
        }
    }
}
