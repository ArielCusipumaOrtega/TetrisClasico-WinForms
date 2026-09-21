using System;
using System.IO;
using System.Text;
using TetrisApp.Audio;
using Xunit;

namespace TetrisApp.Tests.Audio
{
    public class TetrisAudioSynthesizerTests
    {
        [Fact]
        public void GenerateKorobeinikiBgm_ProducesValidWavHeaderAndData()
        {
            byte[] wavBytes = TetrisAudioSynthesizer.GenerateKorobeinikiBgm();

            Assert.NotNull(wavBytes);
            Assert.True(wavBytes.Length > 100000, "BGM track should contain full 8-measure audio");

            ValidateWavStructure(wavBytes, TetrisAudioSynthesizer.DefaultSampleRate);
        }

        [Fact]
        public void GenerateRotateSfx_ProducesValidShortWav()
        {
            byte[] wavBytes = TetrisAudioSynthesizer.GenerateRotateSfx();

            Assert.NotNull(wavBytes);
            Assert.True(wavBytes.Length > 44);

            ValidateWavStructure(wavBytes, TetrisAudioSynthesizer.DefaultSampleRate);
        }

        [Fact]
        public void GenerateLineClearSfx_ProducesValidWav()
        {
            byte[] wavBytes = TetrisAudioSynthesizer.GenerateLineClearSfx();

            Assert.NotNull(wavBytes);
            Assert.True(wavBytes.Length > 44);

            ValidateWavStructure(wavBytes, TetrisAudioSynthesizer.DefaultSampleRate);
        }

        [Fact]
        public void GenerateTetrisClearSfx_ProducesValidWav()
        {
            byte[] wavBytes = TetrisAudioSynthesizer.GenerateTetrisClearSfx();

            Assert.NotNull(wavBytes);
            Assert.True(wavBytes.Length > 44);

            ValidateWavStructure(wavBytes, TetrisAudioSynthesizer.DefaultSampleRate);
        }

        [Fact]
        public void GenerateGameOverSfx_ProducesValidWav()
        {
            byte[] wavBytes = TetrisAudioSynthesizer.GenerateGameOverSfx();

            Assert.NotNull(wavBytes);
            Assert.True(wavBytes.Length > 44);

            ValidateWavStructure(wavBytes, TetrisAudioSynthesizer.DefaultSampleRate);
        }

        [Theory]
        [InlineData(11025)]
        [InlineData(22050)]
        [InlineData(44100)]
        public void BuildWavFile_SupportsDifferentSampleRates(int sampleRate)
        {
            short[] samples = new short[sampleRate]; // 1 second of audio
            for (int i = 0; i < samples.Length; i++)
            {
                samples[i] = (short)(Math.Sin(2 * Math.PI * 440 * i / sampleRate) * 5000);
            }

            byte[] wavBytes = TetrisAudioSynthesizer.BuildWavFile(samples, sampleRate);

            ValidateWavStructure(wavBytes, sampleRate);
        }

        private static void ValidateWavStructure(byte[] wavBytes, int expectedSampleRate)
        {
            using var ms = new MemoryStream(wavBytes);
            using var reader = new BinaryReader(ms, Encoding.ASCII);

            // RIFF
            string riff = Encoding.ASCII.GetString(reader.ReadBytes(4));
            Assert.Equal("RIFF", riff);

            int fileSize = reader.ReadInt32();
            Assert.Equal(wavBytes.Length - 8, fileSize);

            string wave = Encoding.ASCII.GetString(reader.ReadBytes(4));
            Assert.Equal("WAVE", wave);

            // fmt
            string fmt = Encoding.ASCII.GetString(reader.ReadBytes(4));
            Assert.Equal("fmt ", fmt);

            int fmtSize = reader.ReadInt32();
            Assert.Equal(16, fmtSize);

            short audioFormat = reader.ReadInt16();
            Assert.Equal(1, audioFormat); // PCM

            short numChannels = reader.ReadInt16();
            Assert.Equal(1, numChannels); // Mono

            int sampleRate = reader.ReadInt32();
            Assert.Equal(expectedSampleRate, sampleRate);

            int byteRate = reader.ReadInt32();
            Assert.Equal(expectedSampleRate * 2, byteRate);

            short blockAlign = reader.ReadInt16();
            Assert.Equal(2, blockAlign);

            short bitsPerSample = reader.ReadInt16();
            Assert.Equal(16, bitsPerSample);

            // data
            string data = Encoding.ASCII.GetString(reader.ReadBytes(4));
            Assert.Equal("data", data);

            int dataSize = reader.ReadInt32();
            Assert.Equal(wavBytes.Length - 44, dataSize);
        }
    }
}
