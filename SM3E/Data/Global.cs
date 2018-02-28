using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Reflection;
//using MBuild.Properties;
using System.Collections.Generic;


namespace SM3E
{

//========================================================================================
// CLASS SAVE STATION 
//========================================================================================


  class SaveStation: Data, IRepointable
  {
    public const int SaveStationsAddress = 0x0044C5; // Address where save stations are stored
    public const int DefaultSize = 14;
    public const int Count = 151;
    public static readonly int [] AreaOffsets = 
      new int [] {0, 19, 38, 61, 79, 99, 117, 134, Count};

    public int RoomPtr; // LoROM address
    public int DoorPtr; // LoROM address
    public int DoorBts;
    public int ScreenX;
    public int ScreenY;
    public int SamusX;
    public int SamusY;

    public Room MyRoom;
    public Door MyDoor;

    public override int Size
    {
      get {return DefaultSize;}
    }


    // Constructor
    public SaveStation (): base ()
    {
      RoomPtr = 0;
      DoorPtr = 0;
      DoorBts = 0;
      ScreenX = 0;
      ScreenY = 0;
      SamusX  = 0;
      SamusY  = 0;

      MyRoom = null;
      MyDoor = null;
    }


    // Read data from ROM at given PC address.
    public override bool ReadFromROM (Rom rom, int addressPC)
    {
      byte [] b = new byte [DefaultSize];
      rom.Seek (addressPC);
      if (!rom.Read (b, 0, DefaultSize))
        return false;
      RoomPtr = Tools.ConcatBytes (b [ 0], b [ 1], 0x8F);
      DoorPtr = Tools.ConcatBytes (b [ 2], b [ 3], 0x83);
      DoorBts = Tools.ConcatBytes (b [ 4], b [ 5]);
      ScreenX = Tools.ConcatBytes (b [ 6], b [ 7]);
      ScreenY = Tools.ConcatBytes (b [ 8], b [ 9]);
      SamusX  = Tools.ConcatBytes (b [10], b [11]);
      SamusY  = Tools.ConcatBytes (b [12], b [13]);

      startAddressPC = addressPC;
      return true;
    }


    // Write data to ROM at current position (addressPC), which is updated.
    public override bool WriteToROM (Stream rom, ref int addressPC)
    {
      byte [] b = new byte [DefaultSize];
      Tools.CopyBytes (RoomPtr, b,  0, 2);
      Tools.CopyBytes (DoorPtr, b,  2, 2);
      Tools.CopyBytes (DoorBts, b,  4, 2);
      Tools.CopyBytes (ScreenX, b,  6, 2);
      Tools.CopyBytes (ScreenY, b,  8, 2);
      Tools.CopyBytes (SamusX , b, 10, 2);
      Tools.CopyBytes (SamusY , b, 12, 2);
      rom.Write (b, 0, DefaultSize);
      addressPC += Size;
      return true;
    }


    // Set default values.
    public override void SetDefault ()
    {
      RoomPtr = 0;
      DoorPtr = 0;
      DoorBts = 0;
      ScreenX = 0x0400;
      ScreenY = 0x0400;
      SamusX  = 0x0000;
      SamusY  = 0x00B0;

      // startAddressPC = DefaultStartAddress; // [wip] add this line?
    }


    public bool Connect (List <Data> Rooms, List <Data> Doors)
    {
      bool success = true;
      MyRoom = null;
      if (RoomPtr != 0x8F0000)
      {
        MyRoom = (Room) Rooms.Find (x => x.StartAddressLR == RoomPtr);
        success &= (MyRoom != null);
      }
      MyDoor = null;
      if (DoorPtr != 0x830000)
      {
        MyDoor = (Door) Doors.Find (x => x.StartAddressLR == DoorPtr);
        success &= (MyDoor != null);
      }
      return success;
    }


    public void Repoint ()
    {
      if (MyRoom != null)
        RoomPtr = MyRoom.StartAddressLR;
      if (MyDoor != null)
        DoorPtr = MyDoor.StartAddressLR;
    }

//----------------------------------------------------------------------------------------

    public void SetRoom (Room target)
    {
      MyRoom?.UnreferenceMe (this);
      MyRoom = null;
      if (target?.ReferenceMe (this) ?? false)
        MyRoom = target;
    }


    public void SetDoor (Door target)
    {
      MyDoor?.UnreferenceMe (this);
      MyDoor = null;
      if (target?.ReferenceMe (this) ?? false)
        MyDoor = target;
    }

  } // class SaveRoom


//========================================================================================
// CLASS AREA MAP 
//========================================================================================


  class AreaMap: Data
  {
    public const int Count = 8; // Total number of area maps.
    public const int DefaultSize = 4096;
    public const int TileCount = DefaultSize / 2;
    public const int Width = 64;
    public const int Height = 32;
    public static readonly int [] Addresses = new int [] {0x1A9000,
                                                          0x1A8000,
                                                          0x1AA000,
                                                          0x1AB000,
                                                          0x1AC000,
                                                          0x1AD000,
                                                          0x1AE000,
                                                          0x1AF000};

    public byte [] Tiles;
    public byte [] Properties;

    public override int Size
    {
      get {return DefaultSize;}
    }


    // Constructor,
    public AreaMap (): base ()
    {
      Tiles = new byte [TileCount];
      Properties = new byte [TileCount];
    }


    // Read data from ROM at given PC address.
    public override bool ReadFromROM (Rom rom, int addressPC)
    {
      byte [] b = new byte [DefaultSize];
      rom.Seek (addressPC);
      if (!rom.Read (b, 0, DefaultSize))
        return false;

      // int TileCount = DefaultSize / 2;
      for (int n = 0; n < TileCount; n++) {
        int index = (n & 31) | ((n & 992) << 1) | ((n & 1024) >> 5);
        Tiles [index] = b [2 * n];
        Properties [index] = b [2 * n + 1];
      }
      startAddressPC = addressPC;
      return true;
    }


    // Write data to ROM at current position (addressPC), which is updated.
    public override bool WriteToROM (Stream rom, ref int addressPC)
    {
      byte [] b = new byte [DefaultSize];
      for (int n = 0; n < Tiles.Length; n++) {
        int index = (n & 31) | ((n & 992) << 1) | ((n & 1024) >> 5);
        b [2 * n] = Tiles [index];
        b [2 * n + 1] = Properties [index];
      }
      rom.Write (b, 0, DefaultSize);
      addressPC += Size;
      return true;
    }


    // Set default values.
    public override void SetDefault ()
    {
      // Do nothing
    }

//----------------------------------------------------------------------------------------

    public int GetTile (int index) {
      if (index >= 0 && index < Tiles.Length)
        return Tiles [index];
      return 0;
    }


    public bool GetHFlip (int index) {
      if (index >= 0 && index < Tiles.Length)
        return ((Properties [index] >> 6) & 1) > 0;
      return false;
    }


    public bool GetVFlip (int index) {
      if (index >= 0 && index < Tiles.Length)
        return ((Properties [index] >> 7) & 1) > 0;
      return false;
    }


    public int GetPalette (int index) {
      if (index >= 0 && index < Tiles.Length)
        return (Properties [index] >> 2) & 7;
      return 0;
    }


    public void SetTile (int index, int tile) {
      if (index >= 0 && index < Tiles.Length)
        Tiles [index] = (byte) tile;
    }


    public void SetHFlip (int index, bool h_flip) {
      if (index >= 0 && index < Tiles.Length) {
        Properties [index] &= 0xBF;
        if (h_flip)
          Properties [index] |= 0x40;
      }
    }


    public void SetVFlip (int index, bool v_flip) {
      if (index >= 0 && index < Tiles.Length) {
        Properties [index] &= 0x7F;
        if (v_flip)
          Properties [index] |= 0x80;
      }
    }


    public void SetPalette (int index, int palette) {
      if (index >= 0 && index < Tiles.Length) {
        Properties [index] &= 0xE3;
        Properties [index] |= (byte) ((palette & 7) << 2);
      }
    }

  } // class AreaMap

}