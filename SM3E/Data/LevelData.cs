using System;
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


  class LevelData: Data, ICompressed, IReusable, IReferenceableBy <RoomState>
  {
    int ScreenCount;

    public List <UInt16> Layer1;
    public List <UInt16> Layer2;
    public List <byte> Bts;

    public List <RoomState> MyRoomStates;

    protected List <byte> CompressedData;
    protected bool CompressionUpToDate = false;

    public override int Size
    {
      get
      {
        Compress ();
        return CompressedData.Count;
      }
    }

    public bool HasLayer2
    {
      get {return Layer2.Count > 0;}
      set
      {
        if (value == false)
          Layer2.Clear ();
        else
        {
          Layer2 = new List <ushort> ();
          Layer2.Capacity = Layer1.Count;
          for (int i = 0; i < Layer1.Count; i++)
            Layer2.Add (0x80);
        }
      }
    }

    public int ReferenceCount {get {return MyRoomStates.Count;}}


    // Constructor.
    public LevelData (): base ()
    {
      ScreenCount = 0;

      Layer1 = new List <UInt16> ();
      Layer2 = new List <UInt16> ();
      Bts = new List <byte> ();

      MyRoomStates = new List <RoomState> ();
      CompressedData = new List <byte> ();
    }


    // Constructor, given width and height.
    public LevelData (int width, int height): base ()
    {
      ScreenCount = width * height;

      Layer1 = new List <UInt16> ();
      Layer2 = new List <UInt16> ();
      Bts = new List <byte> ();
      int tileCount = ScreenCount * 256;
      for (int n = 0; n < tileCount; n++)
        Layer1.Add (0x805F);
        Bts.Add (0x00);

      MyRoomStates = new List <RoomState> ();
      CompressedData = new List <byte> ();
    }


    // Constructor, copy from exisiting level data.
    public LevelData (LevelData source): base ()
    {
      ScreenCount = 0;

      Layer1 = new List <UInt16> (source.Layer1);
      Layer2 = new List <UInt16> (source.Layer2);
      Bts = new List <byte> (source.Bts);

      MyRoomStates = new List <RoomState> ();
      CompressedData = new List <byte> ();
    }



    // Read data from ROM at given PC address.
    public override bool ReadFromROM (Rom rom, int addressPC)
    {
      rom.Seek (addressPC);
      int compressedSize = rom.Decompress (out List <byte> buffer);
      CompressedData.Clear ();
      rom.Seek (addressPC);
      rom.Read (CompressedData, compressedSize);
      CompressionUpToDate = true;
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
        Bts.Add (buffer [BtsCounter]);
        if (Layer2Size > 0)
          Layer2.Add ((ushort) Tools.ConcatBytes (buffer [Layer2Counter],
                                                  buffer [Layer2Counter + 1]));
        Layer1Counter += 2;
        BtsCounter += 1;
        Layer2Counter += 2;
      }

      startAddressPC = addressPC;
      return decompressedSize > 0;
    }


    // Compress the level data.
    public bool Compress ()
    {
      if (CompressionUpToDate)
        return true;
      int L1Size = 2 * Layer1.Count;
      int UncompressedSize = 2 + 2 * Layer1.Count + Bts.Count + 2 * Layer2.Count;
      byte [] buffer = new byte [UncompressedSize];

      Tools.CopyBytes (L1Size, buffer, 0, 2);
      int index = 2;
      for (int n = 0; n < Layer1.Count; n++) {
        Tools.CopyBytes (Layer1 [n], buffer, index, 2);
        index += 2;
      }
      for (int n = 0; n < Bts.Count; n++) {
        Tools.CopyBytes (Bts [n], buffer, index, 1);
        index ++;
      }
      for (int n = 0; n < Layer2.Count; n++) {
        Tools.CopyBytes (Layer2 [n], buffer, index, 2);
        index += 2;
      }  

      Compressor c = new Compressor (buffer);
      CompressedData = c.Compress ();
      CompressionUpToDate = CompressedData != null;
      return CompressionUpToDate;
    }


    // Write data to ROM at current position (addressPC), which is updated.
    public override bool WriteToROM (Stream rom, ref int addressPC)
    {
      if (CompressedData == null)
        return false;
      if (!CompressionUpToDate)
        Compress ();
      rom.Write (CompressedData.ToArray (), 0, Size);
      addressPC += Size;
      return true;
    }


    // Set default values.
    public override void SetDefault ()
    {
      int Layer1Size = 16 * 16;
      Layer1.Clear ();
      Layer2.Clear ();
      Bts.Clear ();
      for (int n = 0; n < Layer1Size; n++) {
        Layer1.Add (0x805F);
        Bts.Add (0);
      }

      startAddressPC = DefaultStartAddress;
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


    public bool ReferenceMe (RoomState source)
    {
      MyRoomStates.Add (source);
      return true;
    }


    public int UnreferenceMe (RoomState source)
    {
      MyRoomStates.Remove (source);
      return MyRoomStates.Count;
    }


    public void DetachAllReferences ()
    {
      foreach (RoomState r in MyRoomStates)
        r.SetLevelData (null, out var ignore);
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
      if (index < 0 || index >= Bts.Count)
        return 0;
      return Bts [index];
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
      CompressionUpToDate = false;
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
      CompressionUpToDate = false;
    }


    public void SetBts (int index, int type, int value) {
      if (index < 0 || index >= Bts.Count)
        return;
      Layer1 [index] &= 0xFFF;
      Layer1 [index] |= (ushort) (type << 12);
      Bts [index] = (byte) value;
      CompressionUpToDate = false;
    }


    // Resize level data; (x,y) is new top left corner relative to current corner.
    // Current width must be provided so room size can be infered.
    public void SetSize (int currentWidth, int newX, int newY, int newW, int newH)
    {
      if (newW < 1 || newH < 1 || newW > 15 || newH > 15 || newW * newH > 50)
        return;
      List <ushort> newLayer1 = new List <ushort> ();
      List <ushort> newLayer2 = new List <ushort> ();
      List <byte> newBts = new List <byte> ();
      bool L2 = HasLayer2;

      currentWidth *= 16;
      int currentHeight = Layer1.Count / currentWidth;
      newX *= 16;
      newY *= 16;
      newW *= 16;
      newH *= 16;
      for (int y = newY; y < newY + newH; y++)
      {
        for (int x = newX; x < newX + newW; x++)
        {
          // int indexNew = newW * (y - newY) + (x - newX);
          if (x >= 0 && x < currentWidth && y >= 0 && y < currentHeight)
          {
            int indexOld = (currentWidth * y + x);
            newLayer1.Add (Layer1 [indexOld]);
            newBts.Add (Bts [indexOld]);
            if (L2)
              newLayer2.Add (Layer2 [indexOld]);
          }
          else {
            newLayer1.Add (0x805F);
            newBts.Add (0);
            if (L2)
              newLayer2.Add (0x0FF);
          }
        }
      }
      Layer1 = newLayer1;
      Layer2 = newLayer2;
      Bts = newBts;

      CompressionUpToDate = false;
    }

  } // class LevelData

}