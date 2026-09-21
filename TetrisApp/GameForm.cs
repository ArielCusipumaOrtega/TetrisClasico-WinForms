using System;
using System.Drawing;
using System.Windows.Forms;

namespace TetrisApp
{
    public partial class GameForm : Form
    {
        // CONSTANTES DEL JUEGO
        private const int FILAS = 20;
        private const int COLUMNAS = 10;
        private const int TAMANIO_CELDA = 30; // Tamaño en píxeles de cada bloque

        // MATRIZ DEL TABLERO Y ESTADO
        private int[,] tablero = new int[FILAS, COLUMNAS];
        private bool juegoActivo = false;
        private bool juegoPausado = false;

        // Puntuación, Líneas y Nivel
        private int puntuacion = 0;
        private int lineasTotales = 0;
        private int nivel = 1;

        // Temporizador de gravedad
        private System.Windows.Forms.Timer timerGravedad = null!;

        // PIEZA ACTIVA Y SIGUIENTE PIEZA
        private int[,]? piezaActual;
        private int tipoPiezaActual; // 1 a 7
        private int piezaFila;
        private int piezaCol;

        private int[,]? piezaSiguiente;
        private int tipoPiezaSiguiente;

        private Random random = new Random();

        // Paleta de colores para los 7 Tetrominós (Índice 1 a 7)
        private readonly Color[] coloresTetrominos = new Color[]
        {
            Color.FromArgb(17, 19, 24),   // 0: Fondo / Vacío
            Color.FromArgb(0, 229, 255),  // 1: I (Cian)
            Color.FromArgb(41, 121, 255), // 2: J (Azul)
            Color.FromArgb(255, 145, 0),  // 3: L (Naranja)
            Color.FromArgb(255, 214, 0),  // 4: O (Amarillo)
            Color.FromArgb(0, 230, 118),  // 5: S (Verde)
            Color.FromArgb(213, 0, 249),  // 6: T (Púrpura)
            Color.FromArgb(255, 23, 68)   // 7: Z (Rojo)
        };

        // Definiciones de las 7 piezas base
        private readonly int[][,] formasBase = new int[][,]
        {
            // 0: Vacío
            new int[,] { { 0 } },

            // 1: I (4x4)
            new int[,] {
                { 0, 0, 0, 0 },
                { 1, 1, 1, 1 },
                { 0, 0, 0, 0 },
                { 0, 0, 0, 0 }
            },

            // 2: J (3x3)
            new int[,] {
                { 2, 0, 0 },
                { 2, 2, 2 },
                { 0, 0, 0 }
            },

            // 3: L (3x3)
            new int[,] {
                { 0, 0, 3 },
                { 3, 3, 3 },
                { 0, 0, 0 }
            },

            // 4: O (2x2)
            new int[,] {
                { 4, 4 },
                { 4, 4 }
            },

            // 5: S (3x3)
            new int[,] {
                { 0, 5, 5 },
                { 5, 5, 0 },
                { 0, 0, 0 }
            },

            // 6: T (3x3)
            new int[,] {
                { 0, 6, 0 },
                { 6, 6, 6 },
                { 0, 0, 0 }
            },

            // 7: Z (3x3)
            new int[,] {
                { 7, 7, 0 },
                { 0, 7, 7 },
                { 0, 0, 0 }
            }
        };

        public GameForm()
        {
            InitializeComponent();
            ConfigurarMotor();
        }

        private void ConfigurarMotor()
        {
            // Habilitar doble búfer para evitar parpadeos
            this.DoubleBuffered = true;

            // Vincular eventos de dibujo de los PictureBoxes
            picTablero.Paint += PicTablero_Paint;
            picSiguiente.Paint += PicSiguiente_Paint;

            // Configurar el Timer del juego
            timerGravedad = new System.Windows.Forms.Timer();
            timerGravedad.Interval = 650; // Velocidad inicial (ms)
            timerGravedad.Tick += TimerGravedad_Tick;
        }

        // INICIO, PAUSA Y CONTROL DEL JUEGO

        private void IniciarJuego()
        {
            // Limpiar matriz del tablero
            Array.Clear(tablero, 0, tablero.Length);

            puntuacion = 0;
            lineasTotales = 0;
            nivel = 1;
            juegoPausado = false;
            juegoActivo = true;

            timerGravedad.Interval = 650;
            ActualizarEstadisticas();

            // Generar la primera y la siguiente pieza
            tipoPiezaSiguiente = random.Next(1, 8);
            piezaSiguiente = (int[,])formasBase[tipoPiezaSiguiente].Clone();

            AparecerSiguientePieza();
            timerGravedad.Start();

            btnIniciar.Text = "Reiniciar";
            picTablero.Invalidate();
            picSiguiente.Invalidate();
        }

        private void AlternarPausa()
        {
            if (!juegoActivo) return;

            juegoPausado = !juegoPausado;
            if (juegoPausado)
            {
                timerGravedad.Stop();
                btnPausa.Text = "Reanudar (P)";
            }
            else
            {
                timerGravedad.Start();
                btnPausa.Text = "Pausar (P)";
            }
            picTablero.Invalidate();
        }

        private void GameOver()
        {
            juegoActivo = false;
            timerGravedad.Stop();
            picTablero.Invalidate();

            MessageBox.Show($"¡Fin de la Partida!\n\nPuntuación Final: {puntuacion}\nLíneas: {lineasTotales}", 
                            "Game Over", MessageBoxButtons.OK, MessageBoxIcon.Information);
            btnIniciar.Text = "Iniciar Juego";
        }

        // GENERACIÓN Y MOVIMIENTO DE PIEZAS

        private void AparecerSiguientePieza()
        {
            tipoPiezaActual = tipoPiezaSiguiente;
            piezaActual = piezaSiguiente;

            // Posición de inicio en la parte superior central
            piezaFila = 0;
            piezaCol = (COLUMNAS / 2) - (piezaActual!.GetLength(1) / 2);

            // Generar la próxima pieza para la vista previa
            tipoPiezaSiguiente = random.Next(1, 8);
            piezaSiguiente = (int[,])formasBase[tipoPiezaSiguiente].Clone();

            picSiguiente.Invalidate();

            // Si la nueva pieza colisiona al nacer, el jugador ha perdido
            if (!PuedeMover(piezaActual, piezaFila, piezaCol))
            {
                GameOver();
            }
        }

        private void TimerGravedad_Tick(object? sender, EventArgs e)
        {
            if (!juegoActivo || juegoPausado) return;
            MoverAbajo();
        }

        private void MoverAbajo()
        {
            if (piezaActual == null) return;

            if (PuedeMover(piezaActual, piezaFila + 1, piezaCol))
            {
                piezaFila++;
                picTablero.Invalidate();
            }
            else
            {
                FijarPiezaYVerificarLineas();
                picTablero.Invalidate();
            }
        }

        private void MoverIzquierda()
        {
            if (piezaActual == null) return;

            if (PuedeMover(piezaActual, piezaFila, piezaCol - 1))
            {
                piezaCol--;
                picTablero.Invalidate();
            }
        }

        private void MoverDerecha()
        {
            if (piezaActual == null) return;

            if (PuedeMover(piezaActual, piezaFila, piezaCol + 1))
            {
                piezaCol++;
                picTablero.Invalidate();
            }
        }

        private void RotarPiezaActiva()
        {
            if (piezaActual == null) return;

            int[,] rotada = RotarMatrizHoraria(piezaActual);

            // Intento de rotación normal
            if (PuedeMover(rotada, piezaFila, piezaCol))
            {
                piezaActual = rotada;
                picTablero.Invalidate();
            }
            // "Wall Kick" básico: intenta desplazar 1 casilla a la izquierda si choca con la pared derecha
            else if (PuedeMover(rotada, piezaFila, piezaCol - 1))
            {
                piezaCol--;
                piezaActual = rotada;
                picTablero.Invalidate();
            }
            // Intenta desplazar 1 casilla a la derecha si choca con la pared izquierda
            else if (PuedeMover(rotada, piezaFila, piezaCol + 1))
            {
                piezaCol++;
                piezaActual = rotada;
                picTablero.Invalidate();
            }
        }

        private void CaidaInstantanea()
        {
            if (piezaActual == null) return;

            while (PuedeMover(piezaActual, piezaFila + 1, piezaCol))
            {
                piezaFila++;
                puntuacion += 2; // Bonificación por Hard Drop
            }
            FijarPiezaYVerificarLineas();
            ActualizarEstadisticas();
            picTablero.Invalidate();
        }

        private int[,] RotarMatrizHoraria(int[,] matrizOriginal)
        {
            int dim = matrizOriginal.GetLength(0);
            int[,] rotada = new int[dim, dim];
            for (int r = 0; r < dim; r++)
                for (int c = 0; c < dim; c++)
                    rotada[r, c] = matrizOriginal[dim - 1 - c, r];
            return rotada;
        }

        // COLISIONES, LIMPIEZA DE FILAS Y PUNTUACIÓN

        private bool PuedeMover(int[,] pieza, int filaDestino, int colDestino)
        {
            int dim = pieza.GetLength(0);
            for (int r = 0; r < dim; r++)
            {
                for (int c = 0; c < dim; c++)
                {
                    if (pieza[r, c] != 0)
                    {
                        int f = filaDestino + r;
                        int col = colDestino + c;

                        if (col < 0 || col >= COLUMNAS) return false;
                        if (f >= FILAS) return false;
                        if (f >= 0 && tablero[f, col] != 0) return false;
                    }
                }
            }
            return true;
        }

        private void FijarPiezaYVerificarLineas()
        {
            if (piezaActual == null) return;

            int dim = piezaActual.GetLength(0);
            for (int r = 0; r < dim; r++)
            {
                for (int c = 0; c < dim; c++)
                {
                    if (piezaActual[r, c] != 0)
                    {
                        int f = piezaFila + r;
                        int col = piezaCol + c;
                        if (f >= 0 && f < FILAS && col >= 0 && col < COLUMNAS)
                            tablero[f, col] = tipoPiezaActual;
                    }
                }
            }

            int lineasEliminadas = 0;
            for (int f = FILAS - 1; f >= 0; f--)
            {
                if (EsFilaCompleta(f))
                {
                    EliminarFila(f);
                    lineasEliminadas++;
                    f++; // Revisar de nuevo la misma posición tras caer los bloques superiores
                }
            }

            if (lineasEliminadas > 0)
            {
                CalcularPuntuacion(lineasEliminadas);
            }

            AparecerSiguientePieza();
        }

        private bool EsFilaCompleta(int f)
        {
            for (int c = 0; c < COLUMNAS; c++)
            {
                if (tablero[f, c] == 0) return false;
            }
            return true;
        }

        private void EliminarFila(int filaAEliminar)
        {
            for (int f = filaAEliminar; f > 0; f--)
            {
                for (int c = 0; c < COLUMNAS; c++)
                {
                    tablero[f, c] = tablero[f - 1, c];
                }
            }
            // La fila superior (0) queda vacía
            for (int c = 0; c < COLUMNAS; c++)
            {
                tablero[0, c] = 0;
            }
        }

        private void CalcularPuntuacion(int lineas)
        {
            lineasTotales += lineas;

            // Sistema de Puntos Clásico de Tetris
            int puntosBase = lineas switch
            {
                1 => 100,
                2 => 300,
                3 => 500,
                4 => 800, // ¡TETRIS!
                _ => 100
            };

            puntuacion += puntosBase * nivel;

            // Incrementar nivel cada 10 líneas
            nivel = (lineasTotales / 10) + 1;

            // Acelerar la caída conforme avanza el nivel (mínimo 100ms)
            int nuevoIntervalo = Math.Max(100, 650 - ((nivel - 1) * 55));
            timerGravedad.Interval = nuevoIntervalo;

            ActualizarEstadisticas();
        }

        private void ActualizarEstadisticas()
        {
            lblScore.Text = $"Puntos: {puntuacion}";
            lblLineas.Text = $"Líneas: {lineasTotales}";
            lblNivel.Text = $"Nivel: {nivel}";
        }

        // RENDERIZADO GDI+ (PAINT EVENTS)

        private void PicTablero_Paint(object? sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.Clear(Color.FromArgb(17, 19, 24)); // Fondo del tablero

            // 1. Dibujar cuadrícula tenue de fondo
            using (Pen penRejilla = new Pen(Color.FromArgb(28, 32, 40), 1))
            {
                for (int f = 0; f < FILAS; f++)
                    g.DrawLine(penRejilla, 0, f * TAMANIO_CELDA, COLUMNAS * TAMANIO_CELDA, f * TAMANIO_CELDA);
                for (int c = 0; c < COLUMNAS; c++)
                    g.DrawLine(penRejilla, c * TAMANIO_CELDA, 0, c * TAMANIO_CELDA, FILAS * TAMANIO_CELDA);
            }

            // 2. Dibujar bloques fijos del tablero
            for (int f = 0; f < FILAS; f++)
            {
                for (int c = 0; c < COLUMNAS; c++)
                {
                    int idColor = tablero[f, c];
                    if (idColor > 0 && idColor < coloresTetrominos.Length)
                    {
                        DibujarBloque(g, c * TAMANIO_CELDA, f * TAMANIO_CELDA, coloresTetrominos[idColor]);
                    }
                }
            }

            // 3. Dibujar la pieza activa
            if (juegoActivo && piezaActual != null)
            {
                int dim = piezaActual.GetLength(0);
                for (int r = 0; r < dim; r++)
                {
                    for (int c = 0; c < dim; c++)
                    {
                        if (piezaActual[r, c] != 0)
                        {
                            int drawX = (piezaCol + c) * TAMANIO_CELDA;
                            int drawY = (piezaFila + r) * TAMANIO_CELDA;
                            DibujarBloque(g, drawX, drawY, coloresTetrominos[tipoPiezaActual]);
                        }
                    }
                }
            }

            // 4. Mensaje de Pausa
            if (juegoPausado)
            {
                using (SolidBrush sombraBrush = new SolidBrush(Color.FromArgb(160, 0, 0, 0)))
                {
                    g.FillRectangle(sombraBrush, 0, 0, picTablero.Width, picTablero.Height);
                }
                using (Font fuentePausa = new Font("Segoe UI", 18, FontStyle.Bold))
                using (SolidBrush textoBrush = new SolidBrush(Color.White))
                {
                    string texto = "JUEGO PAUSADO";
                    SizeF tam = g.MeasureString(texto, fuentePausa);
                    g.DrawString(texto, fuentePausa, textoBrush, 
                                 (picTablero.Width - tam.Width) / 2, (picTablero.Height - tam.Height) / 2);
                }
            }
        }

        private void PicSiguiente_Paint(object? sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.Clear(Color.FromArgb(17, 19, 24));

            if (juegoActivo && piezaSiguiente != null)
            {
                int dim = piezaSiguiente.GetLength(0);
                int offsetX = (picSiguiente.Width - (dim * TAMANIO_CELDA)) / 2;
                int offsetY = (picSiguiente.Height - (dim * TAMANIO_CELDA)) / 2;

                for (int r = 0; r < dim; r++)
                {
                    for (int c = 0; c < dim; c++)
                    {
                        if (piezaSiguiente[r, c] != 0)
                        {
                            DibujarBloque(g, offsetX + (c * TAMANIO_CELDA), 
                                             offsetY + (r * TAMANIO_CELDA), 
                                             coloresTetrominos[tipoPiezaSiguiente]);
                        }
                    }
                }
            }
        }

        private void DibujarBloque(Graphics g, int x, int y, Color colorBase)
        {
            Rectangle rect = new Rectangle(x, y, TAMANIO_CELDA, TAMANIO_CELDA);

            using (SolidBrush brush = new SolidBrush(colorBase))
            {
                g.FillRectangle(brush, rect);
            }

            using (Pen penBorde = new Pen(Color.FromArgb(40, 0, 0, 0), 1))
            {
                g.DrawRectangle(penBorde, rect);
            }

            // Efecto Bisel / 3D
            using (Pen penBrillo = new Pen(Color.FromArgb(120, 255, 255, 255), 2))
            {
                g.DrawLine(penBrillo, x + 1, y + 1, x + TAMANIO_CELDA - 2, y + 1);
                g.DrawLine(penBrillo, x + 1, y + 1, x + 1, y + TAMANIO_CELDA - 2);
            }
        }

        // CAPTURA DE TECLAS

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (!juegoActivo || juegoPausado)
            {
                if (keyData == Keys.P) AlternarPausa();
                return base.ProcessCmdKey(ref msg, keyData);
            }

            switch (keyData)
            {
                case Keys.Left:
                case Keys.A:
                    MoverIzquierda();
                    return true;

                case Keys.Right:
                case Keys.D:
                    MoverDerecha();
                    return true;

                case Keys.Up:
                case Keys.W:
                    RotarPiezaActiva();
                    return true;

                case Keys.Down:
                case Keys.S:
                    MoverAbajo();
                    return true;

                case Keys.Space:
                    CaidaInstantanea();
                    return true;

                case Keys.P:
                    AlternarPausa();
                    return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        // EVENTOS DE BOTONES

        private void btnIniciar_Click(object? sender, EventArgs e)
        {
            IniciarJuego();
        }

        private void btnPausa_Click(object? sender, EventArgs e)
        {
            AlternarPausa();
        }
    }
}
