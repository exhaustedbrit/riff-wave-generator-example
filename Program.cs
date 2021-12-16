/*
 * Written by Lily Hall
 * lily@nyaa.sh
 */

using System;
using System.Collections.Generic;
using System.IO;

namespace SoundGenerator
{
    /// <summary>
    /// Encapsulates the various components of a WAVE file.
    /// </summary>
    public class RIFFWaveFile {
        /// <summary>
        /// Literally the word 'RIFF' in ASCII.
        /// </summary>
        public int ChunkID = 0x52494646;
        /// <summary>
        /// The size of this chunk.
        /// Effectively the rest of the file minus 8 bytes.
        /// </summary>
        public int ChunkSize
        {
            get { return this.data.Length + 44; }
        }
        /// <summary>
        /// Contains the letters "WAVE" in big endian.
        /// </summary>
        public int Format = 0x57415645;
        /// <summary>
        /// Contains the letters "fmt " in big endian.
        /// </summary>
        public int Subchunk1ID = 0x666d7420;
        /// <summary>
        /// The size of this chunk, which for PCM is 16.
        /// </summary>
        public int Subchunk1Size = 16;
        /// <summary>
        /// The format, PCM is 1 - other formats have different values.
        /// </summary>
        public short AudioFormat = 1;
        /// <summary>
        /// How many channels in the audio file (mono/stereo?)
        /// </summary>
        public short NumChannels = 1;
        /// <summary>
        /// How many samples per second of audio?
        /// This is the cycle rate in Hz of the sound.
        /// </summary>
        public int SampleRate = 44100;
        /// <summary>
        /// How many bytes per second of audio including all channels.
        /// </summary>
        public int ByteRate => SampleRate * SampleRate * BitsPerSample / 8;
        /// <summary>
        /// How many bytes per sample (mono/stereo impacts this)
        /// </summary>
        public short BlockAlign => (short)(NumChannels * BitsPerSample / 8);
        /// <summary>
        /// How many bits in each sample, 8 or 16.
        /// </summary>
        public short BitsPerSample = 16;
        /// <summary>
        /// Contains the letters 'data' in big endian.
        /// </summary>
        public int Subchunk2ID = 0x64617461;
        /// <summary>
        /// The size of data, basically how many bytes in the rest of the file.
        /// </summary>
        public int Subchunk2Size
        {
            get { return this.data.Length; }
        }
        /// <summary>
        /// The actual sound data, this is what you hear.
        /// </summary>
        public byte[] data;
        /// <summary>
        /// Returns this instances properties with endian-correctness as a byte
        /// array ready for writing to a file.
        /// </summary>
        public byte[] header => getHeaderBytes();
        /// <summary>
        /// Assembles the properties specified as a single byte array with the
        /// appropriate order and endianness.
        /// </summary>
        /// <returns></returns>
        private byte[] getHeaderBytes ()
        {
            // Create the byte array, this will store the header bytes that preced the data.
            // See the following page which reflects this breakdown.
            // http://soundfile.sapp.org/doc/WaveFormat/
            List<byte> bytes = new List<byte>();

            bool bConv = BitConverter.IsLittleEndian;

            // RIFF Chunk Descriptor
            bytes.AddRange(getBytesEndian(ChunkID, true));
            bytes.AddRange(getBytesEndian(ChunkSize, false));
            bytes.AddRange(getBytesEndian(Format, true));

            // The "fmt" (format) sub-chunk.
            bytes.AddRange(getBytesEndian(Subchunk1ID, true));
            bytes.AddRange(getBytesEndian(Subchunk1Size, false));
            bytes.AddRange(getBytesEndian(AudioFormat, false));
            bytes.AddRange(getBytesEndian(NumChannels, false));
            bytes.AddRange(getBytesEndian(SampleRate, false));
            bytes.AddRange(getBytesEndian(ByteRate, false));
            bytes.AddRange(getBytesEndian(BlockAlign, false));
            bytes.AddRange(getBytesEndian(BitsPerSample, false));

            // The "data" sub-chunk.
            bytes.AddRange(getBytesEndian(Subchunk2ID, true));
            bytes.AddRange(getBytesEndian(Subchunk2Size, false));

            return bytes.ToArray();
        }
        /// <summary>
        /// Gets the bytes for the provided value in the correct byte order.
        /// For 32-bit integers.
        /// </summary>
        /// <param name="value">The value to transform.</param>
        /// <param name="isBigEndian">Indicates whether is big-endian.</param>
        /// <returns>Correctly ordered value.</returns>
        private byte[] getBytesEndian (int value, bool isBigEndian)
        {
            // Get the bytes from the input integer.
            byte[] bytes = BitConverter.GetBytes(value);

            return setEndianness(bytes, isBigEndian);
        }
        /// <summary>
        /// Gets the bytes for the provided value in the correct byte order.
        /// For 16-bit integers.
        /// </summary>
        /// <param name="value">The value to transform.</param>
        /// <param name="isBigEndian">Indicates whether is big-endian.</param>
        /// <returns>Correctly ordered value.</returns>
        private byte[] getBytesEndian(short value, bool isBigEndian)
        {
            // Get the bytes from the input integer.
            byte[] bytes = BitConverter.GetBytes(value);

            return setEndianness(bytes, isBigEndian);
        }
        /// <summary>
        /// Reverses bytes if required to match desired byte order.
        /// </summary>
        /// <param name="isBigEndian">Indicates whether the value should be big.</param>
        /// <returns>Correctly ordered byte sequence.</returns>
        private byte[] setEndianness (byte[] bytes, bool isBigEndian)
        {
            // Work out what the default byte-order is for this system.
            bool isSystemLittle = BitConverter.IsLittleEndian;

            // If the system endian-ness doesn't match the target, reverse the bytes.
            if (isSystemLittle && isBigEndian || !isSystemLittle && !isBigEndian)
            {
                Array.Reverse(bytes);
            }

            return bytes;
        }
        /// <summary>
        /// Gets the byte data for the entire file, combining the header and payload.
        /// </summary>
        /// <returns>Bytes for the entire WAVE file.</returns>
        public byte[] GetBytes()
        {
            // Create a list to combine our header and payload.
            List<byte> bytes = new List<byte>();
            bytes.AddRange(getHeaderBytes());
            bytes.AddRange(data);
            // Return as a single array for writing to a file.
            return bytes.ToArray();
        }
        /// <summary>
        /// Instantiates a new instance of the RIFF wave file container with the
        /// provided sample rate.
        /// </summary>
        /// <param name="sampleRate">The sample rate in Hz</param>
        public RIFFWaveFile(int sampleRate)
        {
            SampleRate = sampleRate;
        }
    }
    class Program
    {
        static void Main(string[] args)
        {
            // Create a wave file with a 44.1KHz sample rate.
            RIFFWaveFile wave = new RIFFWaveFile(44100);

            // Use our audio generator to generate a 2 second sine wave at 261.6Hz, which is Middle C.
            wave.data = AudioGenerator.MakeSineWave(261.6256f, wave.SampleRate * 2);

            // Use our audio generator generate a bass sweep two octaves lower than Middle C.
            // wave.data = AudioGenerator.MakeTransientSineWave(261.6256f, 65.4064f, wave.SampleRate * 2);

            // Write the wave file.
            File.WriteAllBytes("sound.wav", wave.GetBytes());
            Console.WriteLine($"Saved sound file.");
        }
    }
    /// <summary>
    /// Generates 16-bit PCM audio in various ways with a variable sample rate.
    /// </summary>
    public class AudioGenerator
    {
        public static int SampleRate = 44100;
        /// <summary>
        /// Returns PCM byte data with a sine wave at 0dB amplitude for a given
        /// number of samples.
        /// </summary>
        /// <param name="frequencyHz">The frequency of the sine wave in Hz</param>
        /// <param name="samples">The number of samples</param>
        /// <returns>The PCM byte data for the given sine wave.</returns>
        public static byte[] MakeSineWave(float frequencyHz, int samples) {
            byte[] bytes = new byte[samples * 2];
            double mod = (SampleRate / Math.PI / frequencyHz);

            for (int i = 0; i < samples * 2; i+= 2)
            {
                // Use a simple sine function multiplied by 2^16 / 2
                int sinMod = (int)((1-Math.Sin(i/mod)) * 32768);
                // Offset the sinusoidal wave so that it's anchored at 0.
                Int16 value = Convert.ToInt16(sinMod - 32768);
                // Get bytes for the wave and put in array.
                byte[] sample = BitConverter.GetBytes(value);
                // 16-bit so need to set two bites.
                bytes[i] = sample[0];
                bytes[i + 1] = sample[1];
            }

            return bytes;
        }
        /// <summary>
        /// Returns PCM byte data with a sine wave at 0dB amplitude for a given
        /// number of samples. The sine wave will bend towards the target
        /// frequency over the given number of numbers.
        /// </summary>
        /// <param name="frequencyHz">The frequency of the sine wave in Hz</param>
        /// <param name="samples">The number of samples</param>
        /// <returns>The PCM byte data for the given sine wave.</returns>
        public static byte[] MakeTransientSineWave(
            float startFrequencyHz, float endFrequencyHz, int samples)
        {
            byte[] bytes = new byte[samples * 2];
            double hold = (SampleRate / Math.PI);

            for (int i = 0; i < samples * 2; i += 2)
            {
                double freq = startFrequencyHz - ((startFrequencyHz - endFrequencyHz) * (i > 0 ? (float)i / (samples * 4) : 0));
                double mod = hold / freq;
                // Use a simple sine function multiplied by 2^16 / 2
                int sinMod = (int)((1 - Math.Sin(i / mod)) * 32768);
                // Offset the sinusoidal wave so that it's anchored at 0.
                Int16 value = Convert.ToInt16(sinMod - 32768);
                // Get bytes for the wave and put in array.
                byte[] sample = BitConverter.GetBytes(value);
                // 16-bit so need to set two bites.
                bytes[i] = sample[0];
                bytes[i + 1] = sample[1];
            }

            return bytes;
        }
    }
}
