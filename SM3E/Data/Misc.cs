using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Reflection;
//using MBuild.Properties;
using System.Collections.Generic;


namespace SM3E
{

  public enum ScrollColor: byte
  {
    Red = 0x00,
    Green = 0x01,
    Blue = 0x02,
    Unchanged = 0x03,
    None = 0xFF
  }


//========================================================================================
// CLASS SCROLL SET 
//========================================================================================


  class ScrollSet: RawData
  {
    public const int AllBlue = 0x8F0000;
    public const int AllGreen = 0x8F0001;

    public HashSet <RoomState> MyRoomStates;


    // Constructor
    public ScrollSet (): base ()
    {
      MyRoomStates = new HashSet <RoomState> ();
    }


    // Set default values.
    public override void SetDefault ()
    {
      Bytes.Clear ();
      Bytes.Add (1);

      startAddressPC = -1;
    }

//----------------------------------------------------------------------------------------

    public ScrollColor GetScroll (int index)
    {
      if (index >= 0 && index < Bytes.Count)
        return (ScrollColor) Bytes [index];
      return ScrollColor.None;
    }

  } // class ScrollSet


//========================================================================================
// CLASS BACKGROUND 
//========================================================================================


  class Background: Data
  {
    public const int TerminatorSize = 2;

    public List <byte> Bytes;

    public HashSet <RoomState> MyRoomStates;

    public override int Size
    {
      get {return Bytes.Count + TerminatorSize;}
    }


    // Constructor.
    public Background (): base ()
    {
      Bytes = new List <byte> ();
      MyRoomStates = new HashSet <RoomState> ();
    }


    // Read data from ROM at given PC address.
    public override bool ReadFromROM (Rom rom, int addressPC)
    {
      byte [] b = new byte [11];
      rom.Seek (addressPC, SeekOrigin.Begin);
      if (!rom.Read (b, 0, 2))
        return false;

      int blockSize;
      int totalSize = 0;
      while (b [0] != 0 || b [1] != 0)
      {
        int type = Tools.ConcatBytes (b [0], b [1]);
        Bytes.Add (b [0]);
        Bytes.Add (b [1]);
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
        totalSize += blockSize;
        if (!rom.Read (b, 0, 2))
          return false;
      }
      startAddressPC = addressPC;
      return true;
    }


    // Write data to ROM at current position (addressPC), which is updated.
    public override bool WriteToROM (Stream rom, ref int addressPC)
    {
      rom.Write (Bytes.ToArray (), 0, Bytes.Count);
      addressPC += Size;
      return true;
    }


    // Set default values.
    public override void SetDefault ()
    {
      Bytes.Clear ();
      Bytes.Add (2);
      Bytes.Add (2);

      startAddressPC = -1;
    }

  } // Class Background


//========================================================================================
// CLASS FX 
//========================================================================================


  class Fx: Data, IRepointable
  {
    public const int DefaultSize = 16;
    public const int NullSize = 2;
    public const byte TerminatorByte = 0xFF;

    public bool NotNull;
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

    public Door MyDoor;
    public HashSet <RoomState> MyRoomStates;

    public override int Size
    {
      get {return NotNull ? DefaultSize : NullSize;}
    }


    // Constructor.
    public Fx (): base ()
    {
      NotNull = false;
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

      MyDoor = null;
      MyRoomStates = new HashSet <RoomState> ();
    }


    // Read data from ROM at given PC address.
    public override bool ReadFromROM (Rom rom, int addressPC)
    {
      byte [] b = new byte [DefaultSize];
      rom.Seek (addressPC, SeekOrigin.Begin);
      if (!rom.Read (b, 0, 2))
        return false;
      if (b [0] == TerminatorByte && b [1] == TerminatorByte)
      {
        NotNull = false;
        startAddressPC = addressPC;
        return true;
      }

      if (!rom.Read (b, 0, DefaultSize - 2))
        return false;
      NotNull = true;
      DoorPtr               = Tools.ConcatBytes (b [0], b [1]); // [wip] no bank arg?
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
      if (NotNull)
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
      }
      else
        rom.Write (new byte [] {TerminatorByte, TerminatorByte}, 0, 2);

      addressPC += Size;
      return true;
    }


    // Set default values.
    public override void SetDefault ()
    {
      NotNull = false;

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

      startAddressPC = -1;
    }


    public bool Connect (List <Door> Doors)
    {
      if (NotNull && DoorPtr >= 0x8F8000) {     // != 0x8F0000) {
        MyDoor = Doors.Find (x => x.StartAddressLR == DoorPtr);
        return (MyDoor != null);
      }
      MyDoor = null;
      return true;
    }


    public void Repoint ()
    {
      if (MyDoor != null)
        DoorPtr = MyDoor.StartAddressLR;
    }

  } // class Fx


//========================================================================================
// CLASS SCROLL ASM 
//========================================================================================


  class ScrollAsm: Data
  {
    public const int ColourCommandSize = 2;
    public const int ScrollCommandSize = 4;
    public const int HeaderTerminatorSize = 5;

    public List <int> Entries;
    public HashSet <Door> MyDoors;

    public override int Size
    {
      get
      {
        int ColourCount = 0;
        byte CurrentColour = 0xFF;

        Entries.Sort ();
        for (int n = 0; n < Entries.Count; n++) {
          byte NewColour = (byte) (Entries [n] >> 8);
          if (NewColour != CurrentColour) {
            ColourCount++;
            CurrentColour = NewColour;
          }
        }
        return ColourCommandSize * ColourCount +
               ScrollCommandSize * Entries.Count +
               HeaderTerminatorSize;
      }
    }


    // Constructor.
    public ScrollAsm (): base ()
    {
      Entries = new List <int> ();
      MyDoors = new HashSet <Door> ();
    }



    // Read data from ROM at given PC address.
    public override bool ReadFromROM (Rom rom, int addressPC)
    {
      byte ScrollColour = 0x00;

      byte [] b = new byte [3];
      rom.Seek (addressPC, SeekOrigin.Begin);
      if (!rom.Read (b, 0, 3) || 
          b [0] != 0x08 || b [1] != 0xE2 || b [2] != 0x20)
        return false;

      Entries.Clear ();
      do
      {
        if (!rom.Read (b, 0, 2))
          return false;
        switch (b [0])
        {
        case 0xA9: // set scroll colour
          ScrollColour = b [1];
          break;
        case 0x8F: // set scroll
          Entries.Add (Tools.ConcatBytes ((byte) (b [1] - 0x20), ScrollColour));
          if (!rom.Read (b, 0, 2) || (b [0] != 0xCD && b [1] != 0x7E))
            return false;
          break;
        case 0x28:
          if (b [1] != 0x60)
            return false;
          break;
        default:
          return false;
        }
      } while (b [0] != 0x28);

      startAddressPC = addressPC;
      return true;
    }


    // Write data to ROM at current position (addressPC), which is updated.
    public override bool WriteToROM (Stream rom, ref int addressPC)
    {
      byte CurrentColour = 0xFF;
      byte NewColour;
      byte Scroll;
      // byte [] b = new byte [4];

      addressPC += Size;
      rom.Write (new byte [] {0x08, 0xE2, 0x20}, 0, 3);
      for (int n = 0; n < Entries.Count; n++)
      {
        Scroll = (byte) Entries [n];
        NewColour = (byte) (Entries [n] >> 8);
        if (NewColour != CurrentColour)
        {
          rom.Write (new byte [] {0xA9, NewColour}, 0, 2);
          CurrentColour = NewColour;
        }
        rom.Write (new byte [] {0x8F, (byte) (Scroll + 0x20), 0xCD, 0x7E}, 0, 4);
      }
      rom.Write (new byte [] {0x28, 0x60}, 0, 2);
      return true;
    }


    // Set default values.
    public override void SetDefault ()
    {
      Entries.Clear ();
    }

  }

}