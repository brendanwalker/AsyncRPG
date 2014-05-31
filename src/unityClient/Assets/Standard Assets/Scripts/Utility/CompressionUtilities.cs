using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CompressionUtilities
{
    public enum eEncodeType
    {
        Zeros,
        Ones
    }

    public static byte[] RunLengthEncodeByteArray(
        byte[] uncompressed,
        eEncodeType encodeType,
        byte[] header)
    {
        List<byte> compressed = new List<byte>();
        byte encodeValue = (encodeType == eEncodeType.Zeros) ? (byte)0 : (byte)255;
        int readIndex = 0;

        // Add header bytes, uncompressed
        if (header != null && header.Length > 0)
        {
            foreach (byte headerByte in header)
            {
                compressed.Add(headerByte);
            }
        }

        // Apply RLE compression on the rest of the buffer
        while (readIndex < uncompressed.Length)
        {
            byte uncompressedByte = uncompressed[readIndex++];

            compressed.Add(uncompressedByte);

            if (uncompressedByte == encodeValue)
            {
                byte encodeByteRunCount = 0;

                while (readIndex < uncompressed.Length && uncompressed[readIndex] == encodeValue && encodeByteRunCount < 255)
                {
                    encodeByteRunCount++;
                    readIndex++;
                }

                compressed.Add(encodeByteRunCount);
            }
        }

        return compressed.ToArray();
    }

    public static byte[] RunLengthDecodeByteArray(
        byte[] compressed,
        eEncodeType encodeType,
        byte[] header)
    {
        List<byte> uncompressed = new List<byte>();
        byte encodeValue = (encodeType == eEncodeType.Zeros) ? (byte)0 : (byte)255;
        int readIndex = 0;

        // Extract the uncompressed header first
        while (header != null && readIndex < header.Length)
        {
            header[readIndex] = compressed[readIndex];
            readIndex++;
        }

        // Decompress the rest of the buffer
        while (readIndex < compressed.Length)
        {
            if (compressed[readIndex] != encodeValue)
            {
                uncompressed.Add(compressed[readIndex++]);
            }
            else
            {
                for (int encodeBytesLeft = compressed[readIndex + 1] + 1; encodeBytesLeft != 0; encodeBytesLeft--)
                {
                    uncompressed.Add(encodeValue);
                }

                readIndex += 2;
            }
        }

        return uncompressed.ToArray();
    }
}