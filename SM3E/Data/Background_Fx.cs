using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Reflection;
//using MBuild.Properties;
using System.Collections.Generic;


namespace SM3E
{

//========================================================================================
// CLASS BACKGROUND 
//========================================================================================


  class Background: Data, IReusable, IReferenceableBy <RoomState>
  {
    public const int TerminatorSize = 2;

    public List <byte> Bytes;
    public string Name;

    public BackgroundTiles MyBackgroundTiles;

    public HashSet <RoomState> MyRoomStates;

    public override int Size
    {
      get {return Bytes.Count + TerminatorSize;}
    }

    public int ReferenceCount {get {return MyRoomStates.Count;}}


    // Constructor.
    public Background (): base ()
    {
      Bytes = new List <byte> ();
      MyRoomStates = new HashSet <RoomState> ();
    }


    // Read data from ROM at given PC address.
    public override bool ReadFromROM (Rom rom, int addressPC)
    {
      int TilesPtr = 0;
      byte [] b = new byte [11];
      rom.Seek (addressPC);
      if (!rom.Read (b, 0, 2))
        return false;

      int blockSize;
      int totalSize = 0;
      while (b [0] != 0 || b [1] != 0)
      {
        int type = Tools.ConcatBytes (b [0], b [1]);
        // Bytes.Add (b [0]);
        // Bytes.Add (b [1]);
        switch (type)
        {
        default:
          Console.WriteLine ("Unknown BG block type found: {0}", Tools.IntToHex (type));
          return false;
        case 0x000A:
        case 0x000C:
          blockSize = 2;
          break;
        case 0x0004:
          blockSize = 7;
          break;
        case 0x0002:
        case 0x0008:
          blockSize = 9;
          break;
        case 0x000E:
          blockSize = 11;
          break;
        }
        if (!rom.Read (b, 2, blockSize - 2))
          return false;
        for (int i = 0; i < blockSize; i++)
          Bytes.Add (b [i]);
        if (type == 0x0004)
        {
          if (TilesPtr == 0)
            TilesPtr = Tools.ConcatBytes (b [2], b [3], b [4]);
          else
            TilesPtr = 0; // [wip] still an ugly hack to skip Kraid background
        }
        totalSize += blockSize;
        if (!rom.Read (b, 0, 2))
          return false;
      }
      if (TilesPtr != 0)
      {
        MyBackgroundTiles = new BackgroundTiles ();
        MyBackgroundTiles.ReadFromROM (rom, Tools.LRtoPC (TilesPtr));
      }
      startAddressPC = addressPC;
      return true;
    }


    // Write data to ROM at current position (addressPC), which is updated.
    public override bool WriteToROM (Stream rom, ref int addressPC)
    {
      rom.Write (Bytes.ToArray (), 0, Bytes.Count);
      rom.Write (new byte [] {0, 0}, 0, TerminatorSize);
      addressPC += Size;
      return true;
    }


    // Set default values.
    public override void SetDefault ()
    {
      Bytes.Clear ();
      Bytes.Add (2);
      Bytes.Add (2);

      startAddressPC = DefaultStartAddress;
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
        r.SetBackground (null, out var ignore);
    }

  } // Class Background


//========================================================================================
// CLASS BACKGROUND TILES
//========================================================================================


  class BackgroundTiles: RawData
  {
    protected List <byte> CompressedData;
    protected bool CompressionUpToDate = false;
    

    // Constructor.
    public BackgroundTiles (): base ()
    {
      CompressedData = new List <byte> ();
    }


    // Read data from ROM at given PC address.
    public override bool ReadFromROM (Rom rom, int addressPC)
    {

      rom.Seek (addressPC);
      int compressedSize = rom.Decompress (out Bytes);
      CompressedData.Clear ();
      rom.Seek (addressPC);
      rom.Read (CompressedData, compressedSize);
      CompressionUpToDate = true;

      return true;
    }

//----------------------------------------------------------------------------------------

    public int GetTile (int x, int y)
    {
      int index = 32 * y + x;
      index *= 2;
      if (index < Bytes.Count)
        return (Bytes [index] + Bytes [index + 1] * 256) & 1023;
      return 0;
    }


    public void GetData (int x, int y, out int tile, out int paletteRow,
                                      out bool hFlip, out bool vFlip)
    {
      int index = 64 * y + 2 * x;
      if (index < Bytes.Count)
      {
        int value = Tools.ConcatBytes (Bytes [index], Bytes [index + 1]);
        tile = value & 1023;
        paletteRow = (value >> 10) & 0xF;
        hFlip = (value & 0x4000) > 0;
        vFlip = (value & 0x8000) > 0;
      }
      else
      {
        tile = 0;
        paletteRow = 0;
        hFlip = false;
        vFlip = false;
      }
    }


    public byte [] Render (TileSet activeTileSet)
    {
      TileSheet cre = activeTileSet.MyCreSheet;
      TileSheet sce = activeTileSet.MySceSheet;
      Palette p = activeTileSet.MyPalette;
      int firstCreTileIndex = 0x280;

      byte [] image = new byte [256 * 256 * 4];
      for (int y = 0; y < 32; y++)
      {
        for (int x = 0; x < 32; x++)
        {
          GetData (x, y, out int tile, out int row, out bool hFlip, out bool vFlip);
          // int tile = GetTile (x, y);
          if (tile < firstCreTileIndex)
            sce.DrawTile (image, 256, 256, p, row, 8 * x, 8 * y,
                          tile, hFlip, vFlip);
          else
            cre.DrawTile (image, 256, 256, p, row, 8 * x, 8 * y,
                          tile - firstCreTileIndex, hFlip, vFlip);
        }
      }
      return image;
    }
  }


//========================================================================================
// CLASS FX DATA
//========================================================================================


  public enum FxType: byte
  {
    None        = 0x00,
    Lava        = 0x02,
    Acid        = 0x04,
    Water       = 0x06,
    Spores      = 0x08,
    Rain        = 0x0A,
    Fog         = 0x0C,
    BgScroll    = 0x20,
    BgGlow      = 0x24,
    Statues     = 0x26,
    CeresRidley = 0x28,
    CeresMode7  = 0x2A,
    Haze        = 0x2C,
    Unknown     = 0xFF
  }


  class FxData: Data
  {
    public const int DefaultSize = 16;

    public int DoorPtr; // LoROM address
    public int LiquidSurfaceStart;
    public int LiquidSurfaceNew;
    public int LiquidSurfaceSpeed;
    public byte LiquidSurfaceDelay;
    public byte FxType;
    public byte FxBitA;
    public byte FxBitB;
    public byte FxBitC;
    public byte PaletteFxBitflags;
    public byte TileAnimationBitflags;
    public byte PaletteBlend;


    public override int Size
    {
      get {return DefaultSize;}
    }


    // Constructor.
    public FxData (): base ()
    {
      DoorPtr               = 0xCCCC;
      LiquidSurfaceStart    = 0;
      LiquidSurfaceNew      = 0;
      LiquidSurfaceSpeed    = 0;
      LiquidSurfaceDelay    = 0;
      FxType                = 0;
      FxBitA                = 0;
      FxBitB                = 0;
      FxBitC                = 0;
      PaletteFxBitflags     = 0;
      TileAnimationBitflags = 0;
      PaletteBlend          = 0;
    }


    public FxData (FxData source): base ()
    {
      DoorPtr               = source.DoorPtr;
      LiquidSurfaceStart    = source.LiquidSurfaceStart;
      LiquidSurfaceNew      = source.LiquidSurfaceNew;
      LiquidSurfaceSpeed    = source.LiquidSurfaceSpeed;
      LiquidSurfaceDelay    = source.LiquidSurfaceDelay;
      FxType                = source.FxType;
      FxBitA                = source.FxBitA;
      FxBitB                = source.FxBitB;
      FxBitC                = source.FxBitC;
      PaletteFxBitflags     = source.PaletteFxBitflags;
      TileAnimationBitflags = source.TileAnimationBitflags;
      PaletteBlend          = source.PaletteBlend;
    }


    // Read data from ROM at given PC address.
    public override bool ReadFromROM (Rom rom, int addressPC)
    {
      byte [] b = new byte [DefaultSize];
      rom.Seek (addressPC);

      if (!rom.Read (b, 0, DefaultSize))
        return false;
      DoorPtr               = Tools.ConcatBytes (b [0], b [1], 0x83);
      LiquidSurfaceStart    = Tools.ConcatBytes (b [2], b [3]);
      LiquidSurfaceNew      = Tools.ConcatBytes (b [4], b [5]);
      LiquidSurfaceSpeed    = Tools.ConcatBytes (b [6], b [7]);
      LiquidSurfaceDelay    = b [8];
      FxType                = b [9];
      FxBitA                = b [10];
      FxBitB                = b [11];
      FxBitC                = b [12];
      PaletteFxBitflags     = b [13];
      TileAnimationBitflags = b [14];
      PaletteBlend          = b [15];

      startAddressPC = addressPC;
      return true;
    }


    // Write data to ROM at current position (addressPC), which is updated.
    public override bool WriteToROM (Stream rom, ref int addressPC)
    {
      byte [] b = new byte [DefaultSize];
      Tools.CopyBytes (DoorPtr              , b,  0, 2);
      Tools.CopyBytes (LiquidSurfaceStart   , b,  2, 2);
      Tools.CopyBytes (LiquidSurfaceNew     , b,  4, 2);
      Tools.CopyBytes (LiquidSurfaceSpeed   , b,  6, 2);
      Tools.CopyBytes (LiquidSurfaceDelay   , b,  8, 1);
      Tools.CopyBytes (FxType               , b,  9, 1);
      Tools.CopyBytes (FxBitA               , b, 10, 1);
      Tools.CopyBytes (FxBitB               , b, 11, 1);
      Tools.CopyBytes (FxBitC               , b, 12, 1);
      Tools.CopyBytes (PaletteFxBitflags    , b, 13, 1);
      Tools.CopyBytes (TileAnimationBitflags, b, 14, 1);
      Tools.CopyBytes (PaletteBlend         , b, 15, 1);
      rom.Write (b, 0, DefaultSize);

      addressPC += Size;
      return true;
    }


    // Set default values.
    public override void SetDefault ()
    {
      DoorPtr               = 0x830000;
      LiquidSurfaceStart    = 0xFFFF;
      LiquidSurfaceNew      = 0xFFFF;
      LiquidSurfaceSpeed    = 0x0000;
      LiquidSurfaceDelay    = 0x00;
      FxType                = 0x00;
      FxBitA                = 0x00;
      FxBitB                = 0x00;
      FxBitC                = 0x00;
      PaletteFxBitflags     = 0x00;
      TileAnimationBitflags = 0x00;
      PaletteBlend          = 0x00;

      startAddressPC = DefaultStartAddress;
    }

  } // class FxData


//========================================================================================
// CLASS FX 
//========================================================================================


  class Fx: Data, IRepointable, IReusable, IReferenceableBy <RoomState>
  {
    public const int NullSize = 2;
    public const byte TerminatorByte = 0xFF;

    public bool NotNull;
    public List <Door> FxDoors;
    public List <FxData> FxDatas;

    public HashSet <RoomState> MyRoomStates;

    public override int Size
    {
      get {return NotNull ? FxDatas.Count * FxData.DefaultSize : NullSize;}
    }
    
    public int FxDataCount
    {
      get {return FxDoors.Count;}
    }

    public int ReferenceCount {get {return MyRoomStates.Count;}}


    // Constructor.
    public Fx (): base ()
    {
      NotNull = false;
      FxDatas = new List <FxData> ();
      FxDoors = new List <Door> ();

      MyRoomStates = new HashSet <RoomState> ();
    }

    
    // Constructor, copy from existing Fx.
    public Fx (Fx source): base ()
    {
      NotNull = source.NotNull;
      if (NotNull)
      {
        for (int i = 0; i < source.FxDoors.Count; i++)
        {
          if (source.FxDoors [i].ReferenceMe (this))
          {
            FxDoors.Add (source.FxDoors [i]);
            FxDatas.Add (new FxData (source.FxDatas [i]));
          }
        }
      }

      MyRoomStates = new HashSet <RoomState> ();
    }


    // Read data from ROM at given PC address.
    public override bool ReadFromROM (Rom rom, int addressPC)
    {
      int startAddress = addressPC;
      byte [] b = new byte [2];
      rom.Seek (addressPC);
      if (!rom.Read (b, 0, 2))
        return false;
      if (b [0] == TerminatorByte && b [1] == TerminatorByte)
      {
        NotNull = false;
        startAddressPC = addressPC;
        return true;
      }

      NotNull = true;
      FxData newFxData;
      int adPC = addressPC;
      do
      {
        newFxData = new FxData ();
        rom.Seek (adPC);
        newFxData.ReadFromROM (rom, adPC);
        FxDatas.Add (newFxData);
        adPC += newFxData.Size;
      }
      while (newFxData.DoorPtr != 0x830000);

      startAddressPC = startAddress;
      return true;
    }


    // Write data to ROM at current position (addressPC), which is updated.
    public override bool WriteToROM (Stream rom, ref int addressPC)
    {
      if (NotNull)
      {
        foreach (FxData d in FxDatas)
          d.WriteToROM (rom, ref addressPC);
      }
      else
      {
        rom.Write (new byte [] {TerminatorByte, TerminatorByte}, 0, 2);
        addressPC += 2;
      }

      return true;
    }


    // Set default values.
    public override void SetDefault ()
    {
      NotNull = false;

      FxDoors.Clear ();
      FxDatas.Clear ();

      startAddressPC = DefaultStartAddress;
    }


    public bool Connect (List <Data> Doors)
    {
      FxDoors.Clear ();
      if (NotNull) {
        foreach (FxData d in FxDatas)
          FxDoors.Add ((Door) Doors.Find (x => x.StartAddressLR == d.DoorPtr));
      }
      return true;
    }


    public void Repoint ()
    {
      for (int i = 0; i < FxDatas.Count; i++)
        FxDatas [i].DoorPtr = FxDoors [i]?.StartAddressLR ?? 0x830000;
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
        r.SetFx (null, out var ignore);
    }

//----------------------------------------------------------------------------------------

    public void DeleteDoorFx (Door d)
    {
      if (NotNull)
      {
        for (int i = 0; i < FxDoors.Count;)
        {
          if (FxDoors [i] == d)
          {
            FxDoors.RemoveAt (i);
            FxDatas.RemoveAt (i);
          }
          else
            i++;
        }
      }
    }

  } // class Fx

}