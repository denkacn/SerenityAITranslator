using System.IO;
using System.Text;

namespace SerenityAITranslator.Editor.Services.Tts.Converters
{
    public class AudioConverter
    {
        public static void ConvertPcmToWav(string pcmPath, string wavPath, int sampleRate = 24000, int channels = 1, int bitsPerSample = 16)
        {
            var pcmData = File.ReadAllBytes(pcmPath);

            using var stream = new FileStream(wavPath, FileMode.Create);
            using var writer = new BinaryWriter(stream);
            
            // WAV header
            writer.Write(Encoding.UTF8.GetBytes("RIFF"));
            writer.Write(36 + pcmData.Length);
            writer.Write(Encoding.UTF8.GetBytes("WAVE"));
            
            // fmt chunk
            writer.Write(Encoding.UTF8.GetBytes("fmt "));
            writer.Write(16); // Subchunk size
            writer.Write((short)1); // PCM format
            writer.Write((short)channels);
            writer.Write(sampleRate);
            writer.Write(sampleRate * channels * bitsPerSample / 8); // Byte rate
            writer.Write((short)(channels * bitsPerSample / 8)); // Block align
            writer.Write((short)bitsPerSample);
            
            // data chunk
            writer.Write(Encoding.UTF8.GetBytes("data"));
            writer.Write(pcmData.Length);
            writer.Write(pcmData);
        }
    }
}