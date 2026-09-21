using System;
using System.Drawing;
using System.Windows.Forms;
using TetrisApp.Audio;
using TetrisApp.Engine;
using TetrisApp.Models;
using TetrisApp.Rendering;

namespace TetrisApp
{
    /// <summary>
    /// Capa de presentación visual (UI) de Windows Forms.
    /// Su única responsabilidad es capturar interacciones del usuario (teclado/botones),
    /// sincronizar el temporizador visual y delegar el pintado a BoardRenderer y las reglas a TetrisEngine.
    /// </summary>
    public partial class GameForm : Form
    {
        private readonly TetrisEngine _engine;
        private readonly BoardRenderer _renderer;
        private readonly System.Windows.Forms.Timer _timerGravedad;
        private readonly ITetrisSoundService _soundService;

        public GameForm(ITetrisSoundService? soundService = null)
        {
            InitializeComponent();

            _engine = new TetrisEngine();
            _renderer = new BoardRenderer();
            _timerGravedad = new System.Windows.Forms.Timer();
            _soundService = soundService ?? new TetrisSoundService();

            ConfigurarComponentes();
        }

        private void ConfigurarComponentes()
        {
            // Habilitar doble búfer para evitar parpadeos al redibujar
            DoubleBuffered = true;

            // Ajustar tamaño de celda según el área disponible en picTablero
            AjustarEscalaTablero();
            picTablero.Resize += (s, e) => AjustarEscalaTablero();

            // Vincular eventos de dibujo de los PictureBoxes
            picTablero.Paint += PicTablero_Paint;
            picSiguiente.Paint += PicSiguiente_Paint;

            // Configurar el temporizador de gravedad
            _timerGravedad.Interval = _engine.DropIntervalMs;
            _timerGravedad.Tick += TimerGravedad_Tick;

            // Sincronizar estado inicial del botón de sonido del diseñador
            btnSonido.Text = _soundService.IsMuted ? "Sonido: OFF (M)" : "Sonido: ON (M)";

            // Suscribirse a los eventos del motor desacoplado
            _engine.BoardChanged += (s, e) =>
            {
                picTablero.Invalidate();
                picSiguiente.Invalidate();
            };

            _engine.ScoreChanged += (s, e) => ActualizarEstadisticas();
            _engine.StateChanged += (s, e) => ActualizarEstadoJuego();
            _engine.GameOver += Engine_GameOver;

            // Eventos sonoros retro
            _engine.PieceRotated += (s, e) => _soundService.PlayRotate();
            _engine.LinesCleared += (s, count) => _soundService.PlayLineClear(count);
        }

        private void AjustarEscalaTablero()
        {
            if (picTablero.ClientSize.Width > 0 && picTablero.ClientSize.Height > 0)
            {
                int escalaX = picTablero.ClientSize.Width / GameBoard.Columns;
                int escalaY = picTablero.ClientSize.Height / GameBoard.Rows;
                int tamCalculado = Math.Min(escalaX, escalaY);

                _renderer.CellSize = tamCalculado > 0 ? tamCalculado : 30;
            }
        }

        // ============================================================
        // CONTROL DE FLUJO Y ACTUALIZACIÓN VISUAL
        // ============================================================

        private void ActualizarEstadisticas()
        {
            lblScore.Text = $"Puntos: {_engine.Score.Score}";
            lblLineas.Text = $"Líneas: {_engine.Score.Lines}";
            lblNivel.Text = $"Nivel: {_engine.Score.Level}";
        }

        private void ActualizarEstadoJuego()
        {
            switch (_engine.State)
            {
                case GameState.Playing:
                    _timerGravedad.Interval = _engine.DropIntervalMs;
                    _timerGravedad.Start();
                    btnIniciar.Text = "Reiniciar";
                    btnPausa.Text = "Pausar (P)";
                    _soundService.StartBgm();
                    break;

                case GameState.Paused:
                    _timerGravedad.Stop();
                    btnPausa.Text = "Reanudar (P)";
                    _soundService.PauseBgm();
                    break;

                case GameState.GameOver:
                case GameState.NotStarted:
                    _timerGravedad.Stop();
                    btnIniciar.Text = "Iniciar Juego";
                    _soundService.StopBgm();
                    break;
            }

            picTablero.Invalidate();
        }

        private void Engine_GameOver(object? sender, EventArgs e)
        {
            _timerGravedad.Stop();
            _soundService.PlayGameOver();
            picTablero.Invalidate();

            MessageBox.Show(
                $"¡Fin de la Partida!\n\nPuntuación Final: {_engine.Score.Score}\nLíneas: {_engine.Score.Lines}",
                "Game Over",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        private void TimerGravedad_Tick(object? sender, EventArgs e)
        {
            if (_engine.State != GameState.Playing) return;

            // Ajustar dinámicamente la velocidad por si cambió de nivel
            _timerGravedad.Interval = _engine.DropIntervalMs;
            _engine.Tick();
        }

        // ============================================================
        // EVENTOS DE PINTADO (GDI+)
        // ============================================================

        private void PicTablero_Paint(object? sender, PaintEventArgs e)
        {
            _renderer.DrawBoard(
                e.Graphics,
                _engine.Board,
                _engine.CurrentPiece,
                _engine.CurrentPosition,
                picTablero.ClientSize,
                _engine.State == GameState.Paused,
                _engine.GetGhostPosition());
        }

        private void PicSiguiente_Paint(object? sender, PaintEventArgs e)
        {
            _renderer.DrawNextPiecePreview(
                e.Graphics,
                _engine.NextPiece,
                picSiguiente.ClientSize);
        }

        // ============================================================
        // CAPTURA DE TECLAS DEL SISTEMA
        // ============================================================

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            // Atajo global para silenciar/activar música y sonido (tecla M)
            if (keyData == Keys.M)
            {
                AlternarSonido();
                return true;
            }

            // Si el juego está pausado o no iniciado, solo admitir alternar pausa
            if (_engine.State != GameState.Playing)
            {
                if (keyData == Keys.P && _engine.State == GameState.Paused)
                {
                    _engine.TogglePause();
                    return true;
                }
                return base.ProcessCmdKey(ref msg, keyData);
            }

            switch (keyData)
            {
                case Keys.Left:
                case Keys.A:
                    _engine.MoveLeft();
                    return true;

                case Keys.Right:
                case Keys.D:
                    _engine.MoveRight();
                    return true;

                case Keys.Up:
                case Keys.W:
                    _engine.Rotate();
                    return true;

                case Keys.Down:
                case Keys.S:
                    _engine.MoveDown();
                    return true;

                case Keys.Space:
                    _engine.HardDrop();
                    return true;

                case Keys.P:
                    _engine.TogglePause();
                    return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void AlternarSonido()
        {
            _soundService.ToggleMute();
            btnSonido.Text = _soundService.IsMuted ? "Sonido: OFF (M)" : "Sonido: ON (M)";
        }

        // ============================================================
        // EVENTOS DE BOTONES Y CICLO DE VIDA
        // ============================================================

        private void btnIniciar_Click(object? sender, EventArgs e)
        {
            _engine.StartGame();
        }

        private void btnPausa_Click(object? sender, EventArgs e)
        {
            _engine.TogglePause();
        }

        private void btnSonido_Click(object? sender, EventArgs e)
        {
            AlternarSonido();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            _soundService.Dispose();
        }
    }
}
