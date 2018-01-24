﻿using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Reflection;
//using MBuild.Properties;
using System.Collections.Generic;


namespace SM3E
{

//========================================================================================
// CLASS LEVEL DATA 
//========================================================================================


  class LevelData: Data
  {
    int ScreenCount;

    public List <UInt16> Layer1;
    public List <UInt16> Layer2;
    public List <byte> BTS;

    public List <RoomState> MyRoomStates;

    protected byte [] CompressedData;

    public override int Size
    {
      get {return CompressedData != null ? CompressedData.Length : 0;}
    }

    public bool HasLayer2
    {
      get {return Layer2.Count > 0;}
    }


    // Constructor.
    public LevelData (): base ()
    {
      ScreenCount = 0;

      Layer1 = new List <UInt16> ();
      Layer2 = new List <UInt16> ();
      BTS = new List <byte> ();

      MyRoomStates = new List <RoomState> ();
      CompressedData = null;

      startAddressPC = 0;
    }


    // Read data from ROM at given PC address.
    public override bool ReadFromROM (Rom rom, int addressPC)
    {
      rom.Seek (addressPC, null);
      int compressedSize = rom.Decompress (out List <byte> buffer);
      CompressedData = new byte [compressedSize];
      rom.Seek (addressPC, null);
      rom.Read (CompressedData, 0, compressedSize);
      int decompressedSize = buffer.Count;

      int Layer1Size = Tools.ConcatBytes (buffer [0], buffer [1]);
      int BtsSize = Layer1Size / 2;         // half the amount of data of layer one
      int Layer2Size;
      if (Layer1Size + BtsSize + 2 < decompressedSize)
        Layer2Size = Layer1Size;       // check if layer 2 data exists,
      else
        Layer2Size = 0;                  // if not, set its size to zero.
      ScreenCount = BtsSize / 256;        // divide by 256 tiles per screen.

      int Layer1Counter = 2;
      int BtsCounter = 2 + Layer1Size;
      int Layer2Counter = 2 + Layer1Size + BtsSize;
      for (int n = 0; n < BtsSize; n++) {
        Layer1.Add ((ushort) Tools.ConcatBytes (buffer [Layer1Counter], 
                                                buffer [Layer1Counter + 1]));
        BTS.Add (buffer [BtsCounter]);
        if (Layer2Size > 0)
          Layer2.Add ((ushort) Tools.ConcatBytes (buffer [Layer2Counter],
                                                  buffer [Layer2Counter + 1]));
        Layer1Counter += 2;
        BtsCounter += 1;
        Layer2Counter += 2;
      }

//      CompressedDataSize = 0; // [wip] is this is a thing?
      startAddressPC = addressPC;
      return decompressedSize > 0;
    }


    // Compress the level data [wip].
    public bool Compress ()
    {
      int L1Size = 2 * Layer1.Count;
      int UncompressedSize = 2 + 2 * Layer1.Count + BTS.Count + 2 * Layer2.Count;
      byte [] buffer = new byte [UncompressedSize];

      Tools.CopyBytes (L1Size, buffer, 0, 2);
      int index = 2;
      for (int n = 0; n < Layer1.Count; n++) {
        Tools.CopyBytes (Layer1 [n], buffer, index, 2);
        index += 2;
      }
      for (int n = 0; n < BTS.Count; n++) {
        Tools.CopyBytes (BTS [n], buffer, index, 1);
        index ++;
      }
      for (int n = 0; n < Layer2.Count; n++) {
        Tools.CopyBytes (Layer2 [n], buffer, index, 2);
        index += 2;
      }  

      // CompressedData = Compression.CompressData (buffer);
      return CompressedData != null;
    }


    // Write data to ROM at current position (addressPC), which is updated.
    public override bool WriteToROM (Stream rom, ref int addressPC)
    {
      if (CompressedData == null)
        return false;
      rom.Write (CompressedData, 0, Size);
      addressPC += Size;
      return true;
    }


    // Set default values.
    public override void SetDefault ()
    {
      int Layer1Size = 16 * 16;
      Layer1.Clear ();
      Layer2.Clear ();
      BTS.Clear ();
      for (int n = 0; n < Layer1Size; n++) {
        Layer1.Add (0x805F);
        BTS.Add (0);
      }

      startAddressPC = -1;
      Compress ();
    }


    // Log data to text file.
    public override void Log ()
    {
      Logging.WriteLine (
        "LEVEL DATA at $" + Tools.IntToHex (startAddressPC) +
        " of size " + Size + " byte");
      int width = 16;
      if (!Logging.Verbose)
        return;

      Logging.WriteLine ("-- Layer 1 --");
      for (int n = 0; n < Layer1.Count; n++)
      {
        Logging.Write (" " + Tools.IntToHex (Layer1 [n], 4));
        if ((n + 1) % width == 0)
          Logging.WriteLine ("");
      }

      Logging.WriteLine ("-- BTS --");
      for (int n = 0; n < Layer1.Count; n++)
      {
        Logging.Write (" " + Tools.IntToHex (Layer1 [n], 2));
        if ((n + 1) % width == 0)
          Logging.WriteLine ("");
      }

      Logging.WriteLine ("-- Layer 2 --");
      for (int n = 0; n < Layer1.Count; n++)
      {
        Logging.Write (" " + Tools.IntToHex (Layer1 [n], 4));
        if ((n + 1) % width == 0)
          Logging.WriteLine ("");
      }

      Logging.WriteLine ("");
    }

//----------------------------------------------------------------------------------------

    public int GetLayer1Tile (int index)
    {
      if (index < 0 || index >= Layer1.Count)
        return 0;
      return Layer1 [index] & 1023;
    }


    public bool GetLayer1HFlip (int index) {
      if (index < 0 || index >= Layer1.Count)
        return false;
      return ((Layer1 [index] >> 10) & 1) > 0;
    }


    public bool GetLayer1VFlip (int index) {
      if (index < 0 || index >= Layer1.Count)
        return false;
      return ((Layer1 [index] >> 11) & 1) > 0;
    }


    public int GetBtsType (int index) {
      if (index < 0 || index >= Layer1.Count)
        return 0;
      return (Layer1 [index] >> 12) & 15;
    }


    public int GetBtsValue (int index) {
      if (index < 0 || index >= BTS.Count)
        return 0;
      return BTS [index];
    }


    public int GetLayer2Tile (int index) {
      if (index < 0 || index >= Layer2.Count)
        return 0;
      return Layer2 [index] & 1023;
    }


    public bool GetLayer2HFlip (int index) {
      if (index < 0 || index >= Layer2.Count)
        return false;
      return ((Layer2 [index] >> 10) & 1) > 0;
    }
    

    public bool GetLayer2VFlip (int index) {
      if (index < 0 || index >= Layer2.Count)
        return false;
      return ((Layer2 [index] >> 11) & 1) > 0;
    }

//----------------------------------------------------------------------------------------

    public void SetLayer1 (int index, int tile, bool h_flip, bool v_flip) {
      if (index < 0 || index >= Layer1.Count)
        return;
      Layer1 [index] &= 0xF000;
      if (v_flip)
        Layer1 [index] |= 0x800;
      if (h_flip)
        Layer1 [index] |= 0x400;
      Layer1 [index] |= (ushort) tile;
    }


    public void SetLayer2 (int index, int tile, bool h_flip, bool v_flip) {
      if (index < 0 || index >= Layer2.Count)
        return;
      Layer2 [index] &= 0xF000;
      if (v_flip)
        Layer2 [index] |= 0x800;
      if (h_flip)
        Layer2 [index] |= 0x400;
      Layer2 [index] |= (ushort) tile;
    }


    public void SetBts (int index, int type, int value) {
      if (index < 0 || index >= BTS.Count)
        return;
      Layer1 [index] &= 0xFFF;
      Layer1 [index] |= (ushort) (type << 12);
      BTS [index] = (byte) value;
    }

  } // class LevelData

}