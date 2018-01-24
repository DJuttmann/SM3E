using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Reflection;
//using MBuild.Properties;
using System.Text;
using System.Collections.Generic;


namespace SM3E
{

//========================================================================================
// CLASS TOOLS
//========================================================================================


  static class Tools
  {
    const string hexDigits = "0123456789ABCDEF";


    public static int LRtoPC (int B)
    {
      int B_1 = B >> 16;
      int B_2 = B & 0xFFFF;
      if (B_1 < 0x80 || B_1 > 0xFFFFFF || B_2 < 0x8000) // return 0 if invalid LoROM address
        return 0;
      int A_1 = (B_1 - 0x80) >> 1;
      int A_2 = (B_1 & 1) == 0 ? B_2 & 0x7FFF : B_2; // if B_1 is even, remove most significant bit
      return (A_1 << 16) | A_2;
    }


    public static int PCtoLR (int A)
    {
      if (A > 0x3FFFFF)
        return 0; // return 0 if address would be outside of LoROM range.
      int A_1 = A >> 16;
      int A_2 = A & 0xFFFF;
      int B_1 = (A_1 << 1) + (A_2 >> 15) + 0x80;
      int B_2 = A_2 | 0x8000;
      return (B_1 << 16) | B_2;
    }


    public static int ConcatBytes (byte b0, byte b1)
    {
      return b0 + (b1 << 8);
    }


    public static int ConcatBytes (byte b0, byte b1, byte b2)
    {
      return b0 + (b1 << 8) + (b2 << 16);
    }


    public static String IntToHex (int n)
    {
      StringBuilder hexString = new StringBuilder (string.Empty);
      StringBuilder hexReverse = new StringBuilder (string.Empty);
      
      if (n == 0)
        return "00";

      while (n > 0)
      {
        hexReverse.Append (hexDigits [n & 0xF]);
        n >>= 4;
      }
      if (hexReverse.Length % 2 == 1)
        hexReverse.Append ('0');
      for (int i = 0; i < hexReverse.Length; i++)
        hexString.Append (hexReverse [hexReverse.Length - 1 - i]);
      return hexString.ToString ();
    }


    public static String IntToHex (int n, int digitCount)
    {
      StringBuilder hexString = new StringBuilder (string.Empty);
      StringBuilder hexReverse = new StringBuilder (string.Empty);

      for (int i = 0; i < digitCount; i++)
      {
        hexReverse.Append (hexDigits [n & 0xF]);
        n >>= 4;
      }
      for (int i = 0; i < digitCount; i++)
        hexString.Append (hexReverse [hexReverse.Length - 1 - i]);
      return hexString.ToString ();
    }


    public static int HexToInt (string s)
    {
      int n = 0;
      for (int i = 0; i < s.Length; i++) {
        n <<= 4;
        char c = s [i];
        if (c >= '0' && c <= '9') 
          n += c - '0';
        else if (c >= 'A' && c <= 'F')
          n += c - 'A' + 10;
        else if (c >= 'a' && c <= 'f')
          n += c - 'a' + 10;
        else
          return 0;
      }
      return n;
    }


    // Convert decimal string to integer.
    public static int DecToInt (string dec_str) {
      int n = 0;
      char c;
      for (int i = 0; i < dec_str.Length; i++) 
      {
        n *= 10;
        c = dec_str [i];
        if (c >= '0' && c <= '9')
          n += c - '0';
        else
          return 0;
      }
      return n;
    }


    /* Convert integer to decimal string.
    public static string IntToDec (int n) {
      static const std::string dec_digits = "0123456789";
      std::string dec_str = "";
      std::string dec_str_reverse = "";

      if (n == 0)
        return "0";

      while (n > 0) {
        dec_str_reverse.push_back (dec_digits [n % 10]);
        n /= 10;
      }
      dec_str.resize (dec_str_reverse.size ());
      for (unsigned int i = 0; i < dec_str.size (); i++) {
        dec_str [i] = dec_str_reverse [dec_str.size () - 1 - i];
      }
      return dec_str;
    }*/


    // Copies <count> bytes from an integer to an array at position <offset>.
    public static void CopyBytes (int value, byte [] bytes, int offset, int count)
    {
      for (int i = 0; i < count; i++)
      {
        bytes [offset + i] = (byte) value;
        value >>= 8;
      }
    }


//----------------------------------------------------------------------------------------

    // Removes duplicates from a List, List is sorted in the process.
    public static void RemoveDuplicates <T> (List <T> Data)
    {
      Data.Sort ();
      for (int n = 0; n < Data.Count; n++)
      {
        T currentValue = Data [n];
        int i = 1;
        while (n + i < Data.Count && Data [n + i].Equals (currentValue))
          i++;
        if (i > 1)
          Data.RemoveRange (n + 1, i - 1);
      }
    }


    // Split a string into segments separated by spaces or tabs (unless between "").
    public static List <string> SplitString (string str)
    {
      var segments = new List <string> ();
      var newSegment = new StringBuilder ();
      bool quotedText = false;
      for (int i = 0; i < str.Length; i++)
      {
        char c = str [i];
        switch (c)
        {
        case ' ':
        case '\t':
          if (quotedText)
          {
            newSegment.Append (c);
          }
          else if (newSegment.Length != 0)
          {
            segments.Add (newSegment.ToString ());
            newSegment.Clear ();
          }
          break;
        case '"':
          quotedText = !quotedText;
          break;
        default:
          newSegment.Append (c);
          break;
        }
      }
      if (newSegment.Length != 0)
        segments.Add (newSegment.ToString ());
      return segments;
    }


  } // class Tools


//========================================================================================
// CLASS ROM
//========================================================================================


  class Rom
  {
    public byte [] Data;
    private int Position = 0;


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


  } // class Rom


}