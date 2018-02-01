using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Reflection;
//using MBuild.Properties;
using System.Collections.Generic;


namespace SM3E
{

//========================================================================================
// CLASS ROOM STATE HEADER
//========================================================================================


  class RoomStateHeader: Data
  {
    public const int SizeStandard = 2;
    public const int SizeEvents = 5;
    public const int SizeItems = 4;

    public StateType HeaderType;
    public byte Value;
    public int RoomStatePtr; // LoROM address

    public override int Size
    {
      get
      {
        switch (HeaderType)
        {
        default:
        case StateType.Standard:
          return SizeStandard;
        case StateType.Events:
        case StateType.Bosses:
          return SizeEvents;
        case StateType.TourianBoss:
        case StateType.Morph:
        case StateType.MorphMissiles:
        case StateType.PowerBombs:
        case StateType.SpeedBooster:
          return SizeItems;
        }
      }
    }


    // Constructor.
    public RoomStateHeader (): base ()
    {
      HeaderType = StateType.Standard;
      Value = 0;
      RoomStatePtr = 0;
    }


    // Read room state header from ROM at given PC address.
    public override bool ReadFromROM (Rom rom, int addressPC)
    {
      byte [] b = new byte [3];

      rom.Seek (addressPC, SeekOrigin.Begin);
      if (!rom.Read (b, 0, 2))
        return false;

      HeaderType = (StateType) Tools.ConcatBytes (b [0], b [1]);
      switch (HeaderType)
      {
      case StateType.Standard:
        RoomStatePtr = Tools.PCtoLR (addressPC + 2);
        break;
      case StateType.Events:
      case StateType.Bosses:
        if (!rom.Read (b, 0, 3))
          return false;
        Value = b [0];
        RoomStatePtr = Tools.ConcatBytes (b [1], b [2], 0x8F);
        break;
      case StateType.TourianBoss:
      case StateType.Morph:
      case StateType.MorphMissiles:
      case StateType.PowerBombs:
      case StateType.SpeedBooster:
        if (!rom.Read (b, 0, 2))
          return false;
        Value = b [0];
        RoomStatePtr = Tools.ConcatBytes (b [0], b [1], 0x8F);
        break;
      default:
        Console.WriteLine ("Error: unknown state type encountered: ${0}", 
                           Tools.IntToHex ((int) HeaderType));
        return false;
      }
      startAddressPC = addressPC;
      return true;
    }


    // Write raw data to ROM at current position (addressPC), which is updated.
    public override bool WriteToROM (Stream rom, ref int addressPC)
    {
      byte [] b = new byte [SizeEvents];
      Tools.CopyBytes ((int) HeaderType, b, 0, 2);

      switch (HeaderType)
      {
      case StateType.Standard:
        rom.Write (b, 0, SizeStandard);
        addressPC += SizeStandard;
        break;
      case StateType.Events:
      case StateType.Bosses:
        Tools.CopyBytes (Value, b, 2, 1);
        Tools.CopyBytes (RoomStatePtr, b, 3, 2);
        rom.Write (b, 0, SizeEvents);
        addressPC += SizeEvents;
        break;
      case StateType.TourianBoss:
      case StateType.Morph:
      case StateType.MorphMissiles:
      case StateType.PowerBombs:
      case StateType.SpeedBooster:
        Tools.CopyBytes (RoomStatePtr, b, 2, 2);
        rom.Write (b, 0, SizeItems);
        addressPC += SizeItems;
        break;
      default:
        Console.WriteLine ("Error: unknown state type encountered: ${0}", 
                           Tools.IntToHex ((int) HeaderType));
        return false;
      }
      return true;
    }


    // Set default values.
    public override void SetDefault ()
    {
      HeaderType = StateType.Standard;
      Value = 0;
      RoomStatePtr = 0;
      
      startAddressPC = -1;
    }


    // Log data to text file.
    public override void Log ()
    {
      Logging.WriteLine (
        "ROOM STATE HEADER at $" + Tools.IntToHex (startAddressPC) +
        " of size " + Size + " byte");
      Logging.WriteLine ("  HeaderType  : " + Tools.IntToHex ((int) HeaderType, 4));
      Logging.WriteLine ("  Value       : " + Tools.IntToHex (Value, 2));
      Logging.WriteLine ("  RoomStatePtr: " + Tools.IntToHex (RoomStatePtr, 4));
      Logging.WriteLine ("");
    }

  } // class RoomStateHeader


//========================================================================================
// CLASS ROOM STATE
//========================================================================================


  class RoomState: Data, IRepointable
  {
    public const int DefaultSize = 26;
    
    public int LevelDataPtr; // LoROM address
    public byte TileSet;
    public byte SongSet;
    public byte PlayIndex;
    public int FxPtr; // LoROM address
    public int EnemySetPtr; // LoROM address
    public int EnemyGfxPtr; // LoROM address
    public int BackgroundScrolling;
    public int RoomScrollsPtr; // LoROM address
    public int UnusedPtr; // LoROM address
    public int MainAsmPtr; // LoROM address
    public int PlmSetPtr; // LoROM address
    public int BackgroundPtr; // LoROM address
    public int SetupAsmPtr; // LoROM address

    public PlmSet MyPlmSet;
    public ScrollSet MyScrollSet;
    public Background MyBackground;
    public Fx MyFx;
    public LevelData MyLevelData;
    public EnemySet MyEnemySet;
    public EnemyGfx MyEnemyGfx;
    public Room MyRoom;

    public override int Size
    {
      get {return DefaultSize;}
    }


    // Constructor.
    public RoomState (): base ()
    {
      LevelDataPtr        = 0;
      TileSet             = 0;
      SongSet             = 0;
      PlayIndex           = 0;
      FxPtr               = 0;
      EnemySetPtr         = 0;
      EnemyGfxPtr         = 0;
      BackgroundScrolling = 0;
      RoomScrollsPtr      = 0;
      UnusedPtr           = 0;
      MainAsmPtr          = 0;
      PlmSetPtr           = 0;
      BackgroundPtr       = 0;
      SetupAsmPtr         = 0; 

      MyPlmSet = null;
      MyScrollSet = null;
      MyBackground = null;
      MyFx = null;
      MyLevelData = null;
      MyEnemySet = null;
      MyEnemyGfx = null;
      MyRoom = null;
    }


    public override bool ReadFromROM (Rom rom, int addressPC)
    {
      byte [] b = new byte [DefaultSize];
      rom.Seek (addressPC, SeekOrigin.Begin);
      if (!rom.Read (b, 0, DefaultSize))
        return false;

      LevelDataPtr        = Tools.ConcatBytes (b [0], b [1], b [2]);
      TileSet             = b [3];
      SongSet             = b [4];
      PlayIndex           = b [5];
      FxPtr               = Tools.ConcatBytes (b [6], b [7], 0x83);
      EnemySetPtr         = Tools.ConcatBytes (b [8], b [9], 0xA1);
      EnemyGfxPtr         = Tools.ConcatBytes (b [10], b [11], 0xB4);
      BackgroundScrolling = Tools.ConcatBytes (b [12], b [13]);
      RoomScrollsPtr      = Tools.ConcatBytes (b [14], b [15], 0x8F);
      UnusedPtr           = Tools.ConcatBytes (b [16], b [17], 0x8F); // almost always null
      MainAsmPtr          = Tools.ConcatBytes (b [18], b [19], 0x8F);
      PlmSetPtr           = Tools.ConcatBytes (b [20], b [21], 0x8F);
      BackgroundPtr       = Tools.ConcatBytes (b [22], b [23], 0x8F);
      SetupAsmPtr         = Tools.ConcatBytes (b [23], b [25], 0x8F);

      startAddressPC = addressPC;
      return true;
    }


    public override bool WriteToROM (Stream rom, ref int addressPC)
    {
      byte [] b = new byte [DefaultSize];
      Tools.CopyBytes (LevelDataPtr       , b,  0, 3);
      Tools.CopyBytes (TileSet            , b,  3, 1);
      Tools.CopyBytes (SongSet            , b,  4, 1);
      Tools.CopyBytes (PlayIndex          , b,  5, 1);
      Tools.CopyBytes (FxPtr              , b,  6, 2);
      Tools.CopyBytes (EnemySetPtr        , b,  8, 2);
      Tools.CopyBytes (EnemyGfxPtr        , b, 10, 2);
      Tools.CopyBytes (BackgroundScrolling, b, 12, 2);
      Tools.CopyBytes (RoomScrollsPtr     , b, 14, 2);
      Tools.CopyBytes (UnusedPtr          , b, 16, 2);
      Tools.CopyBytes (MainAsmPtr         , b, 18, 2);
      Tools.CopyBytes (PlmSetPtr          , b, 20, 2);
      Tools.CopyBytes (BackgroundPtr      , b, 22, 2);
      Tools.CopyBytes (SetupAsmPtr        , b, 24, 2);
      rom.Write (b, 0, DefaultSize);
      addressPC += Size;
      return true;
    }


    public override void SetDefault ()
    {
      LevelDataPtr = 0;
      TileSet = 0;
      SongSet = 0;
      PlayIndex = 0;
      FxPtr = 0x830000;
      EnemySetPtr = 0xA10000;
      EnemyGfxPtr = 0xB40000;
      BackgroundScrolling = 0xC1C1;
      RoomScrollsPtr = ScrollSet.AllBlue;
      UnusedPtr = 0;
      MainAsmPtr = 0x8F0000;
      PlmSetPtr = 0x8F0000;
      BackgroundPtr = 0x8F0000;
      SetupAsmPtr = 0x8F0000;

      startAddressPC = -1;
    }


    // Log data to text file.
    public override void Log ()
    {
      Logging.WriteLine (
        "ROOM STATE at $" + Tools.IntToHex (startAddressPC) +
        " of size " + Size + " byte");
      Logging.WriteLine ("  LevelDataPtr       : " + Tools.IntToHex (LevelDataPtr, 6));
      Logging.WriteLine ("  TileSet            : " + Tools.IntToHex (TileSet, 2));
      Logging.WriteLine ("  SongSet            : " + Tools.IntToHex (SongSet, 2));
      Logging.WriteLine ("  PlayIndex          : " + Tools.IntToHex (PlayIndex, 2));
      Logging.WriteLine ("  FxPtr              : " + Tools.IntToHex (FxPtr, 4));
      Logging.WriteLine ("  EnemySetPtr        : " + Tools.IntToHex (EnemySetPtr, 4));
      Logging.WriteLine ("  EnemyGfxPtr        : " + Tools.IntToHex (EnemyGfxPtr, 4));
      Logging.WriteLine ("  BackgroundScrolling: " + Tools.IntToHex (BackgroundScrolling, 4));
      Logging.WriteLine ("  RoomScrollsPtr     : " + Tools.IntToHex (RoomScrollsPtr, 4));
      Logging.WriteLine ("  UnusedPtr          : " + Tools.IntToHex (UnusedPtr, 4));
      Logging.WriteLine ("  MainAsmPtr         : " + Tools.IntToHex (MainAsmPtr, 4));
      Logging.WriteLine ("  PlmSetPtr          : " + Tools.IntToHex (PlmSetPtr, 4));
      Logging.WriteLine ("  BackgroundPtr      : " + Tools.IntToHex (BackgroundPtr, 4));
      Logging.WriteLine ("  SetupAsmPtr        : " + Tools.IntToHex (SetupAsmPtr, 4));
      Logging.WriteLine ("");
    }


    public bool Connect (List <Data> PlmSets,
                         List <Data> ScrollSets,
                         List <Data> Backgrounds,
                         List <Data> Fxs,
                         List <Data> LevelDatas,
                         List <Data> EnemySets,
                         List <Data> EnemyGfxs)
    {
      bool success = true;
      if (PlmSetPtr >= 0x8F8000) {
        MyPlmSet = (PlmSet) PlmSets.Find (x => x.StartAddressLR == PlmSetPtr);
        if (MyPlmSet != null)
          MyPlmSet.MyRoomStates.Add (this);
        else
          success = false;
      }
      if (RoomScrollsPtr >= 0x8F8000) {
        MyScrollSet = (ScrollSet) ScrollSets.Find (x => x.StartAddressLR == RoomScrollsPtr);
        if (MyScrollSet != null)
          MyScrollSet.MyRoomStates.Add (this);
        else
          success = false;
      }
      if (BackgroundPtr >= 0x8F8000) {
        MyBackground = (Background) Backgrounds.Find (x => x.StartAddressLR == BackgroundPtr);
        if (MyBackground != null)
          MyBackground.MyRoomStates.Add (this);
        else
          success = false;
      }
      if (FxPtr >= 0x838000) {
        MyFx = (Fx) Fxs.Find (x => x.StartAddressLR == FxPtr);
        if (MyFx != null)
          MyFx.MyRoomStates.Add (this);
        else
          success = false;
      }
      MyLevelData = (LevelData) LevelDatas.Find (x => x.StartAddressLR == LevelDataPtr);
      if (MyLevelData != null)
        MyLevelData.MyRoomStates.Add (this);
      else
        success = false;
      MyEnemySet = (EnemySet) EnemySets.Find (x => x.StartAddressLR == EnemySetPtr);
      if (MyEnemySet != null)
        MyEnemySet.MyRoomStates.Add (this);
      else
        success = false;
      MyEnemyGfx = (EnemyGfx) EnemyGfxs.Find (x => x.StartAddressLR == EnemyGfxPtr);
      if (MyEnemyGfx != null)
        MyEnemyGfx.MyRoomStates.Add (this);
      else
        success = false;
      return success;
    }


    public void Repoint ()
    {
      if (MyPlmSet != null)
        PlmSetPtr = MyPlmSet.StartAddressLR;
      if (MyScrollSet != null)
        RoomScrollsPtr = MyScrollSet.StartAddressLR;
      if (MyBackground != null)
        BackgroundPtr = MyBackground.StartAddressLR;
      if (MyFx != null)
        FxPtr = MyFx.StartAddressLR;
      if (MyLevelData != null)
        LevelDataPtr = MyLevelData.StartAddressLR;
      if (MyEnemySet != null)
        EnemySetPtr = MyEnemySet.StartAddressLR;
      if (MyEnemyGfx != null)
        EnemyGfxPtr = MyEnemyGfx.StartAddressLR;
    }


  } // class RoomState

//========================================================================================
// CLASS ROOM 
//========================================================================================


  class Room: Data, IRepointable
  {
    public const int HeaderSize = 11;

    public byte RoomIndex;
    public byte Area;
    public byte MapX;
    public byte MapY;
    public byte RoomW;
    public byte RoomH;
    public byte UpScroller;
    public byte DownScroller;
    public byte SpecialGfxBitflag;
    public int DoorsPtr; // LoROM address

    public string Name;
    public List <RoomStateHeader> RoomStateHeaders;
    public List <RoomState> RoomStates;

    public DoorSet MyDoorSet;
    public HashSet <Door> MyIncomingDoors;

    public override int Size
    {
      get
      {
        int size = HeaderSize;
        for (int n = 0; n < RoomStateHeaders.Count; n++)
        {
          size += RoomStateHeaders [n].Size + RoomStates [n].Size;
        }
        return size;
      }
    }

    public int DoorsPtrPC // [wip] why does this still exist?
    {
      get {return Tools.LRtoPC (DoorsPtr);}
      set {DoorsPtr = Tools.PCtoLR (value);}
    }


    public Room (): base ()
    {
      RoomIndex         = 0;
      Area          = 0;
      MapX              = 0;
      MapY              = 0;
      RoomW             = 0;
      RoomH             = 0;
      UpScroller        = 0;
      DownScroller      = 0;
      SpecialGfxBitflag = 0;
      DoorsPtr          = 0;

      Name = String.Empty;
      RoomStateHeaders = new List <RoomStateHeader> ();
      RoomStates = new List <RoomState> ();
      MyDoorSet = null;
      MyIncomingDoors = new HashSet <Door> ();
    }


    // Read data from ROM at given PC address.
    public override bool ReadFromROM (Rom rom, int addressPC)
    {
      int sAddressPC = addressPC;
      byte [] b = new byte [HeaderSize];
      rom.Seek (addressPC, SeekOrigin.Begin);
      if (!rom.Read (b, 0, HeaderSize))
        return false;

      RoomIndex         = b [0];
      Area          = b [1];
      MapX              = b [2];
      MapY              = b [3];
      RoomW             = b [4];
      RoomH             = b [5];
      UpScroller        = b [6];
      DownScroller      = b [7];
      SpecialGfxBitflag = b [8];
      DoorsPtr          = Tools.ConcatBytes (b [9], b [10], 0x8F);

      // read room state headers
      addressPC += HeaderSize;
      int i = 0;
      RoomStateHeaders.Add (new RoomStateHeader ());
      if (!RoomStateHeaders [i].ReadFromROM (rom, addressPC))
        return false;
      addressPC += RoomStateHeaders [i].Size;
      while (RoomStateHeaders [i].HeaderType != StateType.Standard)
      {
        i++;
        RoomStateHeaders.Add (new RoomStateHeader ());
        if (!RoomStateHeaders [i].ReadFromROM (rom, addressPC))
          return false;
        addressPC += RoomStateHeaders [i].Size;
      }

      // read room states
      int StateCount = RoomStateHeaders.Count;
      for (int j = 0; j < StateCount; j++)
        RoomStates.Add (null);
      for (int j = 0; j < StateCount; j++)
      {
        RoomStates [j] = new RoomState ();
        addressPC = Tools.LRtoPC (RoomStateHeaders [j].RoomStatePtr);
        if (!RoomStates [j].ReadFromROM (rom, addressPC))
          return false;
      }

      startAddressPC = sAddressPC;
      return true;
    }


    // Write data to ROM at current position (addressPC), which is updated.
    public override bool WriteToROM (Stream rom, ref int addressPC)
    {
      byte [] b = new byte [HeaderSize];
      b [0] = RoomIndex;
      b [1] = Area;
      b [2] = MapX;
      b [3] = MapY;
      b [4] = RoomW;
      b [5] = RoomH;
      b [6] = UpScroller;
      b [7] = DownScroller;
      b [8] = SpecialGfxBitflag;
      Tools.CopyBytes (DoorsPtr, b, 9, 2);

      addressPC += HeaderSize;
      for (int n = 0; n < RoomStateHeaders.Count; n++)
        if (!RoomStateHeaders [n].WriteToROM (rom, ref addressPC))
          return false;
      if (!RoomStates [RoomStates.Count - 1].WriteToROM (rom, ref addressPC))
        return false;
      for (int n = 0; n + 1 < RoomStateHeaders.Count; n++)
        if (!RoomStates [n].WriteToROM (rom, ref addressPC))
          return false;
      return true;
    }


    // Set default values.
    public override void SetDefault ()
    {
      MapX              = 1;
      MapY              = 1;
      RoomW             = 1;
      RoomH             = 1;
      UpScroller        = 0x70;
      DownScroller      = 0xA0;
      SpecialGfxBitflag = 0;
      DoorsPtr          = 0;
      Name              = Tools.IntToHex (RoomIndex);

      startAddressPC = -1;
    }


    // Log data to text file.
    public override void Log ()
    {
      Logging.WriteLine (
        "ROOM at $" + Tools.IntToHex (startAddressPC) +
        " of size " + Size + " byte");
      Logging.WriteLine ("  RoomIndex        : " + Tools.IntToHex (RoomIndex, 2));
      Logging.WriteLine ("  RoomArea         : " + Tools.IntToHex (Area, 2));
      Logging.WriteLine ("  MapX             : " + Tools.IntToHex (MapX, 2));
      Logging.WriteLine ("  MapY             : " + Tools.IntToHex (MapY, 2));
      Logging.WriteLine ("  RoomW            : " + Tools.IntToHex (RoomW, 2));
      Logging.WriteLine ("  RoomH            : " + Tools.IntToHex (RoomH, 2));
      Logging.WriteLine ("  UpScroller       : " + Tools.IntToHex (UpScroller, 2));
      Logging.WriteLine ("  DownScroller     : " + Tools.IntToHex (DownScroller, 2));
      Logging.WriteLine ("  SpecialGfxBitflag: " + Tools.IntToHex (SpecialGfxBitflag, 2));
      Logging.WriteLine ("  DoorsPtr         : " + Tools.IntToHex (DoorsPtr, 4));
      Logging.WriteLine ("");
    }


    // Change the location within the ROM.
    public override void Reallocate (int addressPC)
    {
      startAddressPC = addressPC;
      addressPC += HeaderSize;
      for (int n = 0; n < RoomStateHeaders.Count; n++)
      {
        RoomStateHeaders [n].Reallocate (addressPC);
        addressPC += RoomStateHeaders [n].Size;
      }
      RoomStates [RoomStates.Count - 1].Reallocate (addressPC);
      addressPC += RoomStates [RoomStates.Count - 1].Size;
      for (int n = 0; n + 1 < RoomStateHeaders.Count; n++)
      {
        RoomStates [n].Reallocate (addressPC);
        addressPC += RoomStates [n].Size;
      }
    }


    public bool Connect (List <Data> DoorSets)
    {
      MyDoorSet = (DoorSet) DoorSets.Find (x => x.StartAddressLR == DoorsPtr);
      if (MyDoorSet != null)
        MyDoorSet.MyRoom = this;
      for (int n = 0; n < RoomStates.Count; n++)
        RoomStates [n].MyRoom = this;
      return MyDoorSet != null;
    }


    public void Repoint () {
      if (MyDoorSet != null)
        DoorsPtr = MyDoorSet.StartAddressLR;
      for (int n = 0; n < RoomStateHeaders.Count; n++)
      {
        RoomStateHeaders [n].RoomStatePtr = RoomStates [n].StartAddressLR;
        RoomStates [n].Repoint ();
      }
    }

  } // class Room

}