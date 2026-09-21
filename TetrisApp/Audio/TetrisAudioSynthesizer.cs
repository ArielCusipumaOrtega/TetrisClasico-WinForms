using System;
using System.IO;
using System.Text;

namespace TetrisApp.Audio
{
    /// <summary>
    /// Generador de audio chiptune retro (8-bit) en memoria formato WAV (PCM 16-bit Mono).
    /// Sintetiza de forma procedimental el Tema A de Tetris (Korobeiniki) y efectos de sonido.
    /// </summary>
    public static class TetrisAudioSynthesizer
    {
        public const int DefaultSampleRate = 22050;

        /// <summary>
        /// Genera el archivo WAV completo de la melodía de Korobeiniki con melodía líder y línea de bajo.
        /// </summary>
        public static byte[] GenerateKorobeinikiBgm(int sampleRate = DefaultSampleRate)
        {
            // Melodía principal: pares (frecuencia en Hz, duración en ms)
            (double Freq, int DurationMs)[] melody =
            {
                // Compás 1
                (659.25, 400), // E5
                (493.88, 200), // B4
                (523.25, 200), // C5
                (587.33, 400), // D5
                (523.25, 200), // C5
                (493.88, 200), // B4

                // Compás 2
                (440.00, 400), // A4
                (440.00, 200), // A4
                (523.25, 200), // C5
                (659.25, 400), // E5
                (587.33, 200), // D5
                (523.25, 200), // C5

                // Compás 3
                (493.88, 600), // B4
                (523.25, 200), // C5
                (587.33, 400), // D5
                (659.25, 400), // E5

                // Compás 4
                (523.25, 400), // C5
                (440.00, 400), // A4
                (440.00, 400), // A4
                (0, 400),      // Silencio

                // Compás 5
                (587.33, 600), // D5
                (698.46, 200), // F5
                (880.00, 400), // A5
                (783.99, 200), // G5
                (698.46, 200), // F5

                // Compás 6
                (659.25, 600), // E5
                (523.25, 200), // C5
                (659.25, 400), // E5
                (587.33, 200), // D5
                (523.25, 200), // C5

                // Compás 7
                (493.88, 400), // B4
                (493.88, 200), // B4
                (523.25, 200), // C5
                (587.33, 400), // D5
                (659.25, 400), // E5

                // Compás 8
                (523.25, 400), // C5
                (440.00, 400), // A4
                (440.00, 400), // A4
                (0, 400)       // Silencio
            };

            // Línea de acompañamiento de bajo (4 notas de 400 ms por compás)
            (double Freq, int DurationMs)[] bass =
            {
                // Compás 1: E3, B2, E3, B2
                (164.81, 400), (123.47, 400), (164.81, 400), (123.47, 400),
                // Compás 2: A2, E3, A2, E3
                (110.00, 400), (164.81, 400), (110.00, 400), (164.81, 400),
                // Compás 3: G#2, E3, G#2, E3
                (103.83, 400), (164.81, 400), (103.83, 400), (164.81, 400),
                // Compás 4: A2, E3, A2, Silencio
                (110.00, 400), (164.81, 400), (110.00, 400), (0, 400),
                // Compás 5: D3, A2, D3, A2
                (146.83, 400), (110.00, 400), (146.83, 400), (110.00, 400),
                // Compás 6: C3, G2, C3, G2
                (130.81, 400), (98.00, 400), (130.81, 400), (98.00, 400),
                // Compás 7: B2, F#2, B2, F#2
                (123.47, 400), (92.50, 400), (123.47, 400), (92.50, 400),
                // Compás 8: A2, E3, A2, Silencio
                (110.00, 400), (164.81, 400), (110.00, 400), (0, 400)
            };

            int totalMs = 0;
            foreach (var note in melody) totalMs += note.DurationMs;
            int totalSamples = (int)((long)totalMs * sampleRate / 1000);

            short[] samples = new short[totalSamples];

            // Renderizar melodía líder (onda cuadrada 50% / onda pulso)
            RenderTrack(samples, melody, sampleRate, volume: 5500, dutyCycle: 0.5);

            // Renderizar bajo mezclado (onda cuadrada con volumen más suave)
            RenderTrack(samples, bass, sampleRate, volume: 3000, dutyCycle: 0.5, mix: true);

            return BuildWavFile(samples, sampleRate);
        }

        /// <summary>
        /// Efecto de sonido para rotación: barrido de tono ascendente corto y nítido (~55 ms).
        /// </summary>
        public static byte[] GenerateRotateSfx(int sampleRate = DefaultSampleRate)
        {
            int durationMs = 55;
            int sampleCount = (int)((long)durationMs * sampleRate / 1000);
            short[] samples = new short[sampleCount];

            double startFreq = 480;
            double endFreq = 820;
            double phase = 0;

            for (int i = 0; i < sampleCount; i++)
            {
                double t = (double)i / sampleCount;
                double currentFreq = startFreq + ((endFreq - startFreq) * t);
                phase += 2.0 * Math.PI * currentFreq / sampleRate;

                // Onda cuadrada con caída de volumen suave al final
                double envelope = 1.0 - (0.5 * t);
                short value = Math.Sin(phase) >= 0 ? (short)6500 : (short)-6500;
                samples[i] = (short)(value * envelope);
            }

            return BuildWavFile(samples, sampleRate);
        }

        /// <summary>
        /// Efecto de sonido para limpieza de filas (1-3 líneas): arpegio ascendente alegre (~280 ms).
        /// </summary>
        public static byte[] GenerateLineClearSfx(int sampleRate = DefaultSampleRate)
        {
            (double Freq, int DurationMs)[] notes =
            {
                (392.00, 60),  // G4
                (523.25, 60),  // C5
                (659.25, 60),  // E5
                (783.99, 100)  // G5
            };

            int totalMs = 0;
            foreach (var n in notes) totalMs += n.DurationMs;
            int sampleCount = (int)((long)totalMs * sampleRate / 1000);
            short[] samples = new short[sampleCount];

            RenderTrack(samples, notes, sampleRate, volume: 7000, dutyCycle: 0.5);

            return BuildWavFile(samples, sampleRate);
        }

        /// <summary>
        /// Efecto de sonido para Tetris (4 líneas): fanfarria festiva de 6 notas (~400 ms).
        /// </summary>
        public static byte[] GenerateTetrisClearSfx(int sampleRate = DefaultSampleRate)
        {
            (double Freq, int DurationMs)[] notes =
            {
                (523.25, 50),   // C5
                (659.25, 50),   // E5
                (783.99, 50),   // G5
                (1046.50, 75),  // C6
                (783.99, 75),   // G5
                (1046.50, 100)  // C6
            };

            int totalMs = 0;
            foreach (var n in notes) totalMs += n.DurationMs;
            int sampleCount = (int)((long)totalMs * sampleRate / 1000);
            short[] samples = new short[sampleCount];

            RenderTrack(samples, notes, sampleRate, volume: 7500, dutyCycle: 0.5);

            return BuildWavFile(samples, sampleRate);
        }

        /// <summary>
        /// Efecto de sonido para Game Over: caída de tono retro descendente (~450 ms).
        /// </summary>
        public static byte[] GenerateGameOverSfx(int sampleRate = DefaultSampleRate)
        {
            int durationMs = 450;
            int sampleCount = (int)((long)durationMs * sampleRate / 1000);
            short[] samples = new short[sampleCount];

            double startFreq = 480;
            double endFreq = 120;
            double phase = 0;

            for (int i = 0; i < sampleCount; i++)
            {
                double t = (double)i / sampleCount;
                double currentFreq = startFreq + ((endFreq - startFreq) * t * t);
                phase += 2.0 * Math.PI * currentFreq / sampleRate;

                double envelope = 1.0 - t;
                short value = Math.Sin(phase) >= 0 ? (short)6500 : (short)-6500;
                samples[i] = (short)(value * envelope);
            }

            return BuildWavFile(samples, sampleRate);
        }

        private static void RenderTrack(short[] buffer, (double Freq, int DurationMs)[] notes, int sampleRate, short volume, double dutyCycle, bool mix = false)
        {
            int sampleOffset = 0;

            foreach (var (freq, durationMs) in notes)
            {
                int noteSamples = (int)((long)durationMs * sampleRate / 1000);
                int soundSamples = (freq > 0) ? (int)(noteSamples * 0.88) : 0; // Ligero staccato retro
                int fadeSamples = Math.Min(sampleRate * 4 / 1000, soundSamples / 4);

                double period = freq > 0 ? (double)sampleRate / freq : 0;

                for (int i = 0; i < noteSamples && (sampleOffset + i) < buffer.Length; i++)
                {
                    short sampleValue = 0;

                    if (freq > 0 && i < soundSamples)
                    {
                        double phase = (i % period) / period;
                        short raw = phase < dutyCycle ? volume : (short)-volume;

                        // Envolvente de entrada y salida rápida para evitar "clics" de fase
                        if (i < fadeSamples && fadeSamples > 0)
                        {
                            raw = (short)(raw * i / fadeSamples);
                        }
                        else if (i > soundSamples - fadeSamples && fadeSamples > 0)
                        {
                            raw = (short)(raw * (soundSamples - i) / fadeSamples);
                        }

                        sampleValue = raw;
                    }

                    int targetIndex = sampleOffset + i;
                    if (mix)
                    {
                        int mixed = buffer[targetIndex] + sampleValue;
                        buffer[targetIndex] = (short)Math.Clamp(mixed, short.MinValue, short.MaxValue);
                    }
                    else
                    {
                        buffer[targetIndex] = sampleValue;
                    }
                }

                sampleOffset += noteSamples;
            }
        }

        public static byte[] BuildWavFile(short[] samples, int sampleRate)
        {
            int subchunk2Size = samples.Length * sizeof(short);
            int chunkSize = 36 + subchunk2Size;

            using MemoryStream ms = new(44 + subchunk2Size);
            using BinaryWriter bw = new(ms, Encoding.ASCII);

            // 1. RIFF Chunk Descriptor
            bw.Write(Encoding.ASCII.GetBytes("RIFF"));
            bw.Write(chunkSize);
            bw.Write(Encoding.ASCII.GetBytes("WAVE"));

            // 2. "fmt " Subchunk
            bw.Write(Encoding.ASCII.GetBytes("fmt "));
            bw.Write(16);             // Subchunk1Size para PCM
            bw.Write((short)1);       // AudioFormat: 1 = PCM
            bw.Write((short)1);       // NumChannels: 1 = Mono
            bw.Write(sampleRate);     // SampleRate
            bw.Write(sampleRate * 2); // ByteRate (SampleRate * NumChannels * BitsPerSample/8)
            bw.Write((short)2);       // BlockAlign (NumChannels * BitsPerSample/8)
            bw.Write((short)16);      // BitsPerSample: 16-bit

            // 3. "data" Subchunk
            bw.Write(Encoding.ASCII.GetBytes("data"));
            bw.Write(subchunk2Size);

            // Muestras de audio
            for (int i = 0; i < samples.Length; i++)
            {
                bw.Write(samples[i]);
            }

            bw.Flush();
            return ms.ToArray();
        }
    }
}
