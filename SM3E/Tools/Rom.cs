﻿using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Reflection;
//using MBuild.Properties;
using System.Text;
using System.Collections.Generic;


namespace SM3E
{

//========================================================================================
// CLASS ROM
//========================================================================================


  class Rom
  {
    public byte [] Data;
    private int Position = 0;
    private Compressor Comp = new Compressor ();


    // Constructor, read bytes from file.
    public Rom (string fileName)
    {
      Data = File.ReadAllBytes (fileName);
    }


    // Set the reading Position in the ROM.
    public void Seek (int position, object o) // [wip] remove arg 2!
    {
      Position = position;
    }


    // Read <count> bytes from ROM and save in array at <offset>.
    public bool Read (byte [] dest, int offset, int count)
    {
      if (Position + count > Data.Length)
        return false;
      for (int i = 0; i < count; i++)
        dest [offset + i] = Data [Position + i];
      Position += count;
      return true;
    }


    // Decompress data starting at the current position, save it in output list.
    // Returns size of compressed data, or -1 if decompression fails.
    // More info on format at http://old.smwiki.net/wiki/LC_LZ5
    public int Decompress (out List <byte> output)
    {
      int startPosition = Position; // start position in Rom data for decompression.
      byte CopyByte1; // byte for repeated copying.
      byte CopyByte2; // byte for repeated copying.
      int address; // start address in output stream for byte copying.
      bool longLenth; // 0b111 command was encountered signifying 10 length bits
      output = new List <byte> ();

      while (Position < Data.Length && Position < startPosition + 0x8000)
      {
        byte currentByte = Data [Position];
        Position++;
        int command = currentByte >> 5;
        int length = (currentByte & 0b11111) + 1;
        if (currentByte == 0xFF) // End of compressed data
          return Position - startPosition;

        do
        {
          longLenth = false;
          switch (command)
          {
          case 0b000: // Copy source bytes.
            if (Position + length > Data.Length)
              return -1;
            for (int i = 0; i < length; i++)
              output.Add (Data [Position + i]);
            Position += length;
            break;

          case 0b001: // Repeat one byte <length> times.
            if (Position + 1 > Data.Length)
              return -1;
            CopyByte1 = Data [Position];
            Position++;
            for (int i = 0; i < length; i++)
              output.Add (CopyByte1);
            break;

          case 0b010: // Alternate between two bytes <length> times.
            if (Position + 2 > Data.Length)
              return -1;
            CopyByte1 = Data [Position];
            CopyByte2 = Data [Position + 1];
            Position += 2;
            for (int i = 0; i < length; i++)
              output.Add (i % 2 == 0 ? CopyByte1 : CopyByte2);
            break;

          case 0b011: // Sequence of increasing bytes.
            if (Position + 1 > Data.Length)
              return -1;
            CopyByte1 = Data [Position];
            Position++;
            for (int i = 0; i < length; i++)
              output.Add (CopyByte1++);
            break;

          case 0b100: // Copy from output stream.
            if (Position + 2 > Data.Length)
              return -1;
            address = Tools.ConcatBytes (Data [Position], Data [Position + 1]);
            Position += 2;
            for (int i = 0; i < length; i++)
              output.Add (output [address + i]);
            break;

          case 0b101: // Copy from output stream, flip bits.
            if (Position + 2 > Data.Length)
              return -1;
            address = Tools.ConcatBytes (Data [Position], Data [Position + 1]);
            Position += 2;
            for (int i = 0; i < length; i++)
              output.Add ((byte) (output [address + i] ^ 0xFF));
            break;

          case 0b110: // Copy from output stream, relative to current index.
            if (Position + 1 > Data.Length)
              return -1;
            address = output.Count - Data [Position];
            Position++;
            for (int i = 0; i < length; i++)
              output.Add (output [address + i]);
            break;

          case 0b111: // Long length (10 bits) command.
            if (Position + 1 > Data.Length)
              return -1;
            command = (currentByte >> 2) & 0b111;
            length = ((currentByte & 0b11) << 8) + Data [Position] + 1;
            Position++;

            if (command == 0b111) // Copy output relative to current index, flip bits.
            {
              if (Position + 1 > Data.Length)
                return -1;
              address = output.Count - Data [Position];
              Position++;
              for (int i = 0; i < length; i++)
                output.Add ((byte) (output [address + i] ^ 0xFF));
            }
            else
              longLenth = true;
            break;
          }
        } while (longLenth);
      }
      return -1; // fail because ROM data ended before decompression was finished.
    }


    public List <byte> Compress (byte [] data)
    {
      Comp.Input = data;
      return Comp.Compress ();
    }

  } // class Rom


//========================================================================================
// CLASS COMPRESSOR
//========================================================================================


  class Compressor
  {
    // An interval of a byte stream, defined by its address and length.
    private struct Interval
    {
      public int Address;
      public int Length;

      public void Reset ()
      {
        Address = 0;
        Length = 0;
      }
    }


    public byte [] Input; // input data, set before compressing.

    private int [] ByteFillLengths;
    private int [] WordFillLengths;
    private int [] ByteIncrementLengths;
    private Interval [] CopyLengths;
    private Interval [] XorCopyLengths;

    private List <int> [] Addresses; // list of candidate start addresses for copy


    public Compressor ()
    {
      ByteFillLengths      = new int [Input.Length];
      WordFillLengths      = new int [Input.Length];
      ByteIncrementLengths = new int [Input.Length];
      CopyLengths          = new Interval [Input.Length];
      XorCopyLengths       = new Interval [Input.Length];

      Addresses = new List <int> [256];
      for (int i = 0; i < 256; i++)
        Addresses [i] = new List <int> ();
    }


    // Reset for new compression job.
    private void Reset ()
    {
      for (int i = 0; i < 256; i++)
        Addresses [i].Clear ();
    }


    // Compress the data in the Input array. 
    public List <byte> Compress ()
    {
      if (Input == null)
        return null;
      var output = new List <byte> ();
      Reset ();
      CalculateByteFill ();
      CalculateWordFill ();
      CalculateByteIncrement ();
      CalculateCopy ();
      CalculateXorCopy ();

      int i = 0;
      while (i < Input.Length)
      {
        int length = Max (ByteFillLengths [i],
                          WordFillLengths [i],
                          ByteIncrementLengths [i],
                          CopyLengths [i].Length,
                          XorCopyLengths [i].Length);
        if (length < 3)
        {
          int j = i;
          while (j < Input.Length && length < 3)
          {
            j++;
            length = Max (ByteFillLengths [j],
                          WordFillLengths [j],
                          ByteIncrementLengths [j],
                          CopyLengths [j].Length,
                          XorCopyLengths [j].Length);
          }
          length = j - i;
          WriteUncompressed (output, i, length);
        }
        else if (length == ByteFillLengths [i])
        {
          length = Math.Min (length, 1024);
          WriteByteFill (output, Input [i], length);
        }
        else if (length == WordFillLengths [i])
        {
          length = Math.Min (length, 1024);
          WriteWordFill (output, Input [i], Input [i + 1], length);
        }
        else if (length == ByteIncrementLengths [i])
        {
          length = Math.Min (length, 1024);
          WriteByteIncement (output, Input [i], length);
        }
        else if (length == CopyLengths [i].Length)
        {
          length = Math.Min (length, 1024);
          if (i - CopyLengths [i].Address < 32)
            WriteNegativeCopy (output, i - CopyLengths [i].Address, length);
          else
            WriteCopy (output, CopyLengths [i].Address, length);
        }
        else if (length == XorCopyLengths [i].Length)
        {
          length = Math.Min (length, 512);
          if (i - XorCopyLengths [i].Address < 32)
            WriteNegativeXorCopy (output, i - XorCopyLengths [i].Address, length);
          else
            WriteXorCopy (output, XorCopyLengths [i].Address, length);
        }
        i += length;
      }
      return output;
    }


    // Maximum of five integers.
    private int Max (int a, int b, int c, int d, int e)
    {
      return Math.Max (Math.Max (Math.Max (a, b), Math.Max (c, d)), e);
    }


    // Write a chunk header.
    private void WriteChunkHeader (List <byte> output, int type, int length)
    {
      length--;
      if (length < 32)
      {
        output.Add ((byte) (type << 5 | length));
      }
      else
      {
        output.Add ((byte) (0b111 | type << 2 | length >> 8));
        output.Add ((byte) (length & 0xFF));
      }
    }


    // Write an uncompressed chunk.
    private void WriteUncompressed (List <byte> output, int index, int length)
    {
      WriteChunkHeader (output, 0b000, length);
      for (int i = 0; i < length; i++)
        output.Add (Input [index + i]);
    }


    // Write a byte fill chunk.
    private void WriteByteFill (List <byte> output, byte b, int length)
    {
      WriteChunkHeader (output, 0b001, length);
      output.Add (b);
    }


    // Write a word fill chunk.
    private void WriteWordFill (List <byte> output, byte b1, byte b2, int length)
    {
      WriteChunkHeader (output, 0b010, length);
      output.Add (b1);
      output.Add (b2);
    }


    // Write a byte fill chunk.
    private void WriteByteIncement (List <byte> output, byte b, int length)
    {
      WriteChunkHeader (output, 0b011, length);
      output.Add (b);
    }


    // Write a word fill chunk.
    private void WriteCopy (List <byte> output, int address, int length)
    {
      WriteChunkHeader (output, 0b100, length);
      output.Add ((byte) (address & 0xFF));
      output.Add ((byte) (address >> 8));
    }


    // Write a word fill chunk.
    private void WriteXorCopy (List <byte> output, int negOffset, int length)
    {
      WriteChunkHeader (output, 0b101, length);
      output.Add ((byte) negOffset);
    }


    // Write a word fill chunk.
    private void WriteNegativeCopy (List <byte> output, int address, int length)
    {
      WriteChunkHeader (output, 0b110, length);
      output.Add ((byte) (address & 0xFF));
      output.Add ((byte) (address >> 8));
    }


    // Write a word fill chunk.
    private void WriteNegativeXorCopy (List <byte> output, int negOffset, int length)
    {
      WriteChunkHeader (output, 0b111, length);
      output.Add ((byte) negOffset);
    }

//----------------------------------------------------------------------------------------
// Calculating max lengths of commands at all positions in the input stream.

    // Calculate lengths of maximal byte fill commands.
    private void CalculateByteFill ()
    {
      int carry = 0;
      for (int i = 0; i < Input.Length; i++)
      {
        if (carry == 0)
        {
          byte value = Input [i];
          while (i + carry < Input.Length && Input [i + carry] == value)
            carry++;
        }
        ByteFillLengths [i] = carry;
        carry--;
      }
    }


    // Calculate lengths of maximal word fill commands.
    private void CalculateWordFill ()
    {
      int carry = 1;
      for (int i = 0; i < Input.Length - 1; i++)
      {
        if (carry == 1)
        {
          byte [] value = new byte [] {Input [i], Input [i + 1]};
          while (i + carry < Input.Length &&
                 Input [i + carry] == value [(i + carry) & 1])
            carry++;
        }
        WordFillLengths [i] = carry;
        carry--;
      }
    }


    // Calculate lengths of maximal byte fill commands.
    private void CalculateByteIncrement ()
    {
      int carry = 0;
      for (int i = 0; i < Input.Length; i++)
      {
        if (carry == 0)
        {
          byte value = Input [i];
          while (i + carry < Input.Length && Input [i + carry] == value)
          {
            carry++;
            value++;
          }
        }
        ByteIncrementLengths [i] = carry;
        carry--;
      }
    }


    // Calculate lengths of maximal byte fill commands.
    private void CalculateCopy ()
    {
      Interval maxInterval = new Interval ();
      for (int i = 0; i < Input.Length; i++)
      {
        maxInterval.Reset ();
        foreach (int address in Addresses [Input [i]])
        {
          int length = MatchSubSequences (address, i);
          if (length > maxInterval.Length)
          {
            maxInterval.Address = address;
            maxInterval.Length = length;
          }
        }
        CopyLengths [i] = maxInterval;
        Addresses [Input [i]].Add (i);
      }
    }


    // Calculate lengths of maximal byte fill commands.
    private void CalculateXorCopy ()
    {
      Interval maxInterval = new Interval ();
      for (int i = 0; i < Input.Length; i++)
      {
        maxInterval.Reset ();
        foreach (int address in Addresses [Input [~i]])
        {
          int length = XorMatchSubSequences (address, i);
          if (length > maxInterval.Length)
          {
            maxInterval.Address = address;
            maxInterval.Length = length;
          }
        }
        CopyLengths [i] = maxInterval;
      }
    }


    // Find the max length of two matching sequences starting at a and b in Input array.
    // Make sure that 0 <= a < b, otherwise bad stuff will happen.
    private int MatchSubSequences (int a, int b)
    {
      int bStart = b;

      // Check if max byte fill and word fill lengths match.
      if (b >= Input.Length || Input [a] != Input [b])
        return 0;
      if (ByteFillLengths [a] != ByteFillLengths [b])
        return (Math.Min (ByteFillLengths [a], ByteFillLengths [b]));
      if (b + 1 >= Input.Length || Input [a + 1] != Input [b + 1])
        return 1;
      if (WordFillLengths [a] != WordFillLengths [b])
        return (Math.Min (WordFillLengths [a], WordFillLengths [b]));

      // Check for matching data after identical byte/word runs.
      int step = Math.Max (ByteFillLengths [a], WordFillLengths [a]);
      a += step;
      b += step;
      while (b < Input.Length && Input [a] == Input [b])
      {
        a++;
        b++;
      }
      return b - bStart;
    }


    // Find the max length of two xor-matching sequences starting at a and b.
    // Make sure that 0 <= a < b, otherwise bad stuff will happen.
    private int XorMatchSubSequences (int a, int b)
    {
      int bStart = b;

      // Check if max byte fill and word fill lengths match.
      if (b >= Input.Length || Input [a] != ~Input [b])
        return 0;
      if (ByteFillLengths [a] != ByteFillLengths [b])
        return (Math.Min (ByteFillLengths [a], ByteFillLengths [b]));
      if (b + 1 >= Input.Length || Input [a + 1] != ~Input [b + 1])
        return 1;
      if (WordFillLengths [a] != WordFillLengths [b])
        return (Math.Min (WordFillLengths [a], WordFillLengths [b]));

      // Check for matching data after identical byte/word runs.
      int step = Math.Max (ByteFillLengths [a], WordFillLengths [a]);
      a += step;
      b += step;
      while (b < Input.Length && Input [a] == ~Input [b])
      {
        a++;
        b++;
      }
      return b - bStart;
    }

  } // class Compressor

}