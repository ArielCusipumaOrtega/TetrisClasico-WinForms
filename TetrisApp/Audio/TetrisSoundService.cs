using System;
using System.IO;
using System.Media;
using System.Runtime.InteropServices;

namespace TetrisApp.Audio
{
    public interface ITetrisSoundService : IDisposable
    {
        bool IsMuted { get; set; }
        bool IsBgmPlaying { get; }
        void StartBgm();
        void PauseBgm();
        void StopBgm();
        void PlayRotate();
        void PlayLineClear(int linesCount);
        void PlayGameOver();
        void ToggleMute();
    }

    /// <summary>
    /// Servicio de audio que gestiona la reproducción continua de Korobeiniki mediante System.Media.SoundPlayer
    /// y efectos sonoros retro independientes que no interrumpen ni reinician la música de fondo.
    /// </summary>
    public class TetrisSoundService : ITetrisSoundService
    {
        [DllImport("winmm.dll", EntryPoint = "mciSendStringA", CharSet = CharSet.Ansi)]
        private static extern int mciSendString(string command, System.Text.StringBuilder? buffer, int bufferSize, IntPtr hwndCallback);

        private readonly MemoryStream _bgmStream;
        private readonly SoundPlayer _bgmPlayer;

        // Reproductores de respaldo SoundPlayer
        private readonly MemoryStream _rotateStream;
        private readonly MemoryStream _lineClearStream;
        private readonly MemoryStream _tetrisClearStream;
        private readonly MemoryStream _gameOverStream;
        private readonly SoundPlayer _rotatePlayer;
        private readonly SoundPlayer _lineClearPlayer;
        private readonly SoundPlayer _tetrisClearPlayer;
        private readonly SoundPlayer _gameOverPlayer;

        // Canales independientes MCI para efectos simultáneos (evita pausar o reiniciar el BGM)
        private readonly bool _mciInitialized;
        private readonly string _aliasRotate;
        private readonly string _aliasLineClear;
        private readonly string _aliasTetris;
        private readonly string _aliasGameOver;
        private readonly string _rotatePath = string.Empty;
        private readonly string _lineClearPath = string.Empty;
        private readonly string _tetrisClearPath = string.Empty;
        private readonly string _gameOverPath = string.Empty;

        private bool _isDisposed;
        private bool _isMuted;
        private bool _isBgmPlaying;

        public bool IsMuted
        {
            get => _isMuted;
            set
            {
                _isMuted = value;
                if (_isMuted)
                {
                    StopAll();
                }
                else if (_isBgmPlaying)
                {
                    ResumeBgm();
                }
            }
        }

        public bool IsBgmPlaying => _isBgmPlaying;

        public TetrisSoundService()
        {
            // 1. Sintetizar música de Korobeiniki y configurar SoundPlayer principal
            _bgmStream = new MemoryStream(TetrisAudioSynthesizer.GenerateKorobeinikiBgm());
            _bgmPlayer = new SoundPlayer(_bgmStream);

            // 2. Sintetizar efectos de sonido
            byte[] rotBytes = TetrisAudioSynthesizer.GenerateRotateSfx();
            byte[] lcBytes = TetrisAudioSynthesizer.GenerateLineClearSfx();
            byte[] tetBytes = TetrisAudioSynthesizer.GenerateTetrisClearSfx();
            byte[] goBytes = TetrisAudioSynthesizer.GenerateGameOverSfx();

            _rotateStream = new MemoryStream(rotBytes);
            _lineClearStream = new MemoryStream(lcBytes);
            _tetrisClearStream = new MemoryStream(tetBytes);
            _gameOverStream = new MemoryStream(goBytes);

            _rotatePlayer = new SoundPlayer(_rotateStream);
            _lineClearPlayer = new SoundPlayer(_lineClearStream);
            _tetrisClearPlayer = new SoundPlayer(_tetrisClearStream);
            _gameOverPlayer = new SoundPlayer(_gameOverStream);

            // 3. Inicializar canales multimedia independientes para reproducir efectos sin cortar el BGM
            string id = Guid.NewGuid().ToString("N")[..8];
            _aliasRotate = $"sfx_rot_{id}";
            _aliasLineClear = $"sfx_lc_{id}";
            _aliasTetris = $"sfx_tet_{id}";
            _aliasGameOver = $"sfx_go_{id}";

            try
            {
                string tempDir = Path.GetTempPath();
                _rotatePath = Path.Combine(tempDir, $"{_aliasRotate}.wav");
                _lineClearPath = Path.Combine(tempDir, $"{_aliasLineClear}.wav");
                _tetrisClearPath = Path.Combine(tempDir, $"{_aliasTetris}.wav");
                _gameOverPath = Path.Combine(tempDir, $"{_aliasGameOver}.wav");

                File.WriteAllBytes(_rotatePath, rotBytes);
                File.WriteAllBytes(_lineClearPath, lcBytes);
                File.WriteAllBytes(_tetrisClearPath, tetBytes);
                File.WriteAllBytes(_gameOverPath, goBytes);

                mciSendString($"open \"{_rotatePath}\" type waveaudio alias {_aliasRotate}", null, 0, IntPtr.Zero);
                mciSendString($"open \"{_lineClearPath}\" type waveaudio alias {_aliasLineClear}", null, 0, IntPtr.Zero);
                mciSendString($"open \"{_tetrisClearPath}\" type waveaudio alias {_aliasTetris}", null, 0, IntPtr.Zero);
                mciSendString($"open \"{_gameOverPath}\" type waveaudio alias {_aliasGameOver}", null, 0, IntPtr.Zero);

                _mciInitialized = true;
            }
            catch
            {
                _mciInitialized = false;
            }
        }

        public void StartBgm()
        {
            if (_isDisposed) return;
            _isBgmPlaying = true;

            if (_isMuted) return;

            try
            {
                _bgmStream.Position = 0;
                _bgmPlayer.PlayLooping();
            }
            catch
            {
            }
        }

        public void PauseBgm()
        {
            if (_isDisposed) return;
            _isBgmPlaying = false;

            try
            {
                _bgmPlayer.Stop();
            }
            catch
            {
            }
        }

        public void StopBgm()
        {
            PauseBgm();
        }

        public void PlayRotate()
        {
            if (_isDisposed || _isMuted) return;

            if (_mciInitialized)
            {
                mciSendString($"play {_aliasRotate} from 0", null, 0, IntPtr.Zero);
            }
            else
            {
                _rotatePlayer.Play();
            }
        }

        public void PlayLineClear(int linesCount)
        {
            if (_isDisposed || _isMuted) return;

            string alias = linesCount >= 4 ? _aliasTetris : _aliasLineClear;

            if (_mciInitialized)
            {
                mciSendString($"play {alias} from 0", null, 0, IntPtr.Zero);
            }
            else
            {
                if (linesCount >= 4) _tetrisClearPlayer.Play();
                else _lineClearPlayer.Play();
            }
        }

        public void PlayGameOver()
        {
            if (_isDisposed || _isMuted) return;
            _isBgmPlaying = false;

            try
            {
                _bgmPlayer.Stop();
                if (_mciInitialized)
                {
                    mciSendString($"play {_aliasGameOver} from 0", null, 0, IntPtr.Zero);
                }
                else
                {
                    _gameOverPlayer.Play();
                }
            }
            catch
            {
            }
        }

        public void ToggleMute()
        {
            IsMuted = !IsMuted;
        }

        private void ResumeBgm()
        {
            if (_isDisposed || _isMuted || !_isBgmPlaying) return;

            try
            {
                _bgmPlayer.PlayLooping();
            }
            catch
            {
            }
        }

        private void StopAll()
        {
            try
            {
                _bgmPlayer.Stop();

                if (_mciInitialized)
                {
                    mciSendString($"stop {_aliasRotate}", null, 0, IntPtr.Zero);
                    mciSendString($"stop {_aliasLineClear}", null, 0, IntPtr.Zero);
                    mciSendString($"stop {_aliasTetris}", null, 0, IntPtr.Zero);
                    mciSendString($"stop {_aliasGameOver}", null, 0, IntPtr.Zero);
                }
                else
                {
                    _rotatePlayer.Stop();
                    _lineClearPlayer.Stop();
                    _tetrisClearPlayer.Stop();
                    _gameOverPlayer.Stop();
                }
            }
            catch
            {
            }
        }

        public void Dispose()
        {
            if (_isDisposed) return;
            _isDisposed = true;
            _isBgmPlaying = false;

            try
            {
                StopAll();

                if (_mciInitialized)
                {
                    mciSendString($"close {_aliasRotate}", null, 0, IntPtr.Zero);
                    mciSendString($"close {_aliasLineClear}", null, 0, IntPtr.Zero);
                    mciSendString($"close {_aliasTetris}", null, 0, IntPtr.Zero);
                    mciSendString($"close {_aliasGameOver}", null, 0, IntPtr.Zero);

                    TryDeleteFile(_rotatePath);
                    TryDeleteFile(_lineClearPath);
                    TryDeleteFile(_tetrisClearPath);
                    TryDeleteFile(_gameOverPath);
                }

                _bgmPlayer.Dispose();
                _rotatePlayer.Dispose();
                _lineClearPlayer.Dispose();
                _tetrisClearPlayer.Dispose();
                _gameOverPlayer.Dispose();

                _bgmStream.Dispose();
                _rotateStream.Dispose();
                _lineClearStream.Dispose();
                _tetrisClearStream.Dispose();
                _gameOverStream.Dispose();
            }
            catch
            {
            }
        }

        private static void TryDeleteFile(string path)
        {
            try
            {
                if (!string.IsNullOrEmpty(path) && File.Exists(path))
                {
                    File.Delete(path);
                }
            }
            catch
            {
            }
        }
    }
}
