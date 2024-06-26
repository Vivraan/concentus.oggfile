﻿using Concentus;
using Concentus.Enums;
using Concentus.Oggfile;
using Concentus.Structs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace OggTest
{
    public class Program
    {
        public static void Main(string[] args)
        {
            string opusfile = @"C:\Code\concentus.oggfile\Warning call.opus";
            string rawFile = @"C:\Code\concentus.oggfile\Warning call.raw";
            string rawFile2 = @"C:\Code\concentus.oggfile\Warning call_out.raw";
            using (FileStream fileOut = new FileStream(opusfile, FileMode.Create))
            {
                IOpusEncoder encoder = OpusCodecFactory.CreateEncoder(48000, 2, OpusApplication.OPUS_APPLICATION_AUDIO, Console.Out);
                encoder.Bitrate = 96000;

                OpusTags tags = new OpusTags();
                tags.Fields[OpusTagName.Title] = "Warning Call";
                tags.Fields[OpusTagName.Artist] = "CHVRCHES";
                OpusOggWriteStream oggOut = new OpusOggWriteStream(encoder, fileOut, tags, inputSampleRate: 44100);

                byte[] allInput = File.ReadAllBytes(rawFile);
                short[] samples = BytesToShorts(allInput);

                oggOut.WriteSamples(samples, 0, samples.Length);
                oggOut.Finish();
            }

            using (FileStream fileIn = new FileStream(opusfile, FileMode.Open))
            {
                using (FileStream fileOut = new FileStream(rawFile2, FileMode.Create))
                {
                    IOpusDecoder decoder = OpusCodecFactory.CreateDecoder(16000, 2, Console.Out);
                    OpusOggReadStream oggIn = new OpusOggReadStream(decoder, fileIn);
                    while (oggIn.HasNextPacket)
                    {
                        short[] packet = oggIn.DecodeNextPacket();
                        if (packet != null)
                        {
                            byte[] binary = ShortsToBytes(packet);
                            fileOut.Write(binary, 0, binary.Length);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Converts interleaved byte samples (such as what you get from a capture device)
        /// into linear short samples (that are much easier to work with)
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static short[] BytesToShorts(byte[] input)
        {
            return BytesToShorts(input, 0, input.Length);
        }

        /// <summary>
        /// Converts interleaved byte samples (such as what you get from a capture device)
        /// into linear short samples (that are much easier to work with)
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static short[] BytesToShorts(byte[] input, int offset, int length)
        {
            short[] processedValues = new short[length / 2];
            for (int c = 0; c < processedValues.Length; c++)
            {
                processedValues[c] = (short)(((int)input[(c * 2) + offset]) << 0);
                processedValues[c] += (short)(((int)input[(c * 2) + 1 + offset]) << 8);
            }

            return processedValues;
        }

        /// <summary>
        /// Converts linear short samples into interleaved byte samples, for writing to a file, waveout device, etc.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static byte[] ShortsToBytes(short[] input)
        {
            return ShortsToBytes(input, 0, input.Length);
        }

        /// <summary>
        /// Converts linear short samples into interleaved byte samples, for writing to a file, waveout device, etc.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static byte[] ShortsToBytes(short[] input, int in_offset, int samples)
        {
            byte[] processedValues = new byte[samples * 2];
            ShortsToBytes(input, in_offset, processedValues, 0, samples);
            return processedValues;
        }

        /// <summary>
        /// Converts linear short samples into interleaved byte samples, for writing to a file, waveout device, etc.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static void ShortsToBytes(short[] input, int in_offset, byte[] output, int out_offset, int samples)
        {
            for (int c = 0; c < samples; c++)
            {
                output[(c * 2) + out_offset] = (byte)(input[c + in_offset] & 0xFF);
                output[(c * 2) + out_offset + 1] = (byte)((input[c + in_offset] >> 8) & 0xFF);
            }
        }
    }
}
