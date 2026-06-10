using System;
using System.IO;
using UnityEngine;

public static class WavUtility
{
    public static AudioClip ToAudioClip(byte[] wavFile, string clipName = "wav")
    {
        using (MemoryStream stream = new MemoryStream(wavFile))
        using (BinaryReader reader = new BinaryReader(stream))
        {
            // RIFF header
            string riff = new string(reader.ReadChars(4));
            if (riff != "RIFF") throw new Exception("Invalid WAV file: Missing RIFF");

            int chunkSize = reader.ReadInt32();
            string wave = new string(reader.ReadChars(4));
            if (wave != "WAVE") throw new Exception("Invalid WAV file: Missing WAVE");

            // Read format chunk
            string fmt = new string(reader.ReadChars(4));
            int subchunk1Size = reader.ReadInt32();
            int audioFormat = reader.ReadInt16();
            int numChannels = reader.ReadInt16();
            int sampleRate = reader.ReadInt32();
            int byteRate = reader.ReadInt32();
            int blockAlign = reader.ReadInt16();
            int bitsPerSample = reader.ReadInt16();

            // Skip extra parameters
            if (subchunk1Size > 16)
                reader.ReadBytes(subchunk1Size - 16);

            // Find data chunk
            string dataID = new string(reader.ReadChars(4));
            while (dataID != "data")
            {
                int chunkSizeData = reader.ReadInt32();
                reader.ReadBytes(chunkSizeData);
                dataID = new string(reader.ReadChars(4));
            }

            int dataSize = reader.ReadInt32();
            byte[] data = reader.ReadBytes(dataSize);

            // Convert byte[] → float[]
            float[] samples = new float[dataSize / 2];
            for (int i = 0; i < samples.Length; i++)
            {
                short value = BitConverter.ToInt16(data, i * 2);
                samples[i] = value / 32768f;
            }

            AudioClip audioClip = AudioClip.Create(clipName, samples.Length, numChannels, sampleRate, false);
            audioClip.SetData(samples, 0);

            return audioClip;
        }
    }
}
