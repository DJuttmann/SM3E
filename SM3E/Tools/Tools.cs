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


    // Convert LoRom address to PC address.
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


    // Convert PC address to LoRom address.
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


    // Concatenate two bytes (little endian order) into an integer.
    public static int ConcatBytes (byte b0, byte b1)
    {
      return b0 + (b1 << 8);
    }


    // Concatenate three bytes (little endian order) into an integer.
    public static int ConcatBytes (byte b0, byte b1, byte b2)
    {
      return b0 + (b1 << 8) + (b2 << 16);
    }


    // Convert an integer to a hex string (always even number of digits in output).
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


    // Convert an integer to a hex string, specify the number of digits of output.
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


    // Convert a hex string to an integer.
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


    // Swap the values of two ints.
    public static void Swap (ref int x, ref int y)
    {
      int temp = y;
      y = x;
      x = temp;
    }


    // Swap if y < x, otherwise leave as is.
    public static void Order (ref int x, ref int y)
    {
      if (y < x)
      {
        int temp = y;
        y = x;
        x = temp;
      }
    }


    // Swapping for generic types.
    public static void Swap <T> (ref T x, ref T y)
    {
      T temp = y;
      y = x;
      x = temp;
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

//----------------------------------------------------------------------------------------

    // Extract a filename from a full path to a file.
    public static string FilenameFromPath (string path)
    {
      int i = path.Length - 1;
      while (i > 0 && path [i] != '\\')
        i--;
      if (path [i] == '\\')
        i++;
      return path.Substring (i, path.Length - i);
    }


    // Extract folder from a full path to a file.
    public static string FolderFromPath (string path)
    {
      int i = path.Length - 1;
      while (i > 0 && path [i] != '\\')
        i--;
      if (path [i] == '\\')
        i++;
      return path.Substring (0, i);
    }


    // Trim the extension from a filename, extension is returned as out variable.
    public static void TrimFileExtension (ref string filename, out string extension)
    {
      int i = filename.Length - 1;
      while (i >= 0 && filename [i] != '.')
        i--;
      if (filename [i] == '.')
      {
        extension = filename.Substring (i, filename.Length - i);
        filename = filename.Substring (0, i);
      }
      else
        extension = String.Empty;
    }

  } // class Tools

}