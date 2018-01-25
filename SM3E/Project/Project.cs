using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;


namespace SM3E
{

  public enum Layer
  {
    Layer1,
    BTS,
    Layer2,
    Plms,
    Enemies,
    Scrolls,
    Fx
  }


//========================================================================================
// CLASS PROJECT
//========================================================================================


  partial class Project
  {
    // Consts
    public const int IndexNone = -1;
    public const int AreaCount = 8;


    // Fields
    private List <Room         > Rooms;
    private List <DoorSet      > DoorSets;
    private List <Door         > Doors;
    private List <RoomState    > RoomStates;
    private List <ScrollSet    > ScrollSets;
    private List <PlmSet       > PlmSets;
    private List <ScrollPlmData> ScrollPlmDatas;
    private List <Background   > Backgrounds;
    private List <Fx           > Fxs;
    private List <SaveRoom     > SaveRooms;
    private List <LevelData    > LevelDatas;
    private List <EnemySet     > EnemySets;
    private List <EnemyGfx     > EnemyGfxs;
    private List <ScrollAsm    > ScrollAsms;

    private List <TileSet      > TileSets;
    private List <TileTable    > TileTables;
    private List <TileSheet    > TileSheets;
    private List <Palette      > Palettes;
    private List <AreaMap      > AreaMaps;
    private List <PlmType      > PlmTypes;
    private List <EnemyType    > EnemyTypes;

//========================================================================================
// Properties

// Active items (private reference to object)

    // Reference to the active room.
    private Room ActiveRoom
    {
      get {return roomIndex != IndexNone ? Rooms [roomIndex] : null;}
    }
    
    // Reference to the active room state.
    private RoomState ActiveRoomState
    {
      get {return roomStateIndex != IndexNone ? ActiveRoom.RoomStates [roomStateIndex] : null;}
    }

    // Reference to the active room state's level data.
    private LevelData ActiveLevelData
    {
      get {return ActiveRoomState?.MyLevelData;}
    }

    // Reference to the active tile set.
    private TileSet ActiveTileSet
    {
      get {return tileSetIndex != IndexNone ? TileSets [tileSetIndex] : null;}
    }


    // Index of the selected area.
    private int areaIndex = IndexNone;
    public int AreaIndex
    {
      get {return areaIndex;}
      set
      {
        if (value >= -1 && value < AreaCount && value != areaIndex)
        {
          areaIndex = value;
          roomIndex = IndexNone;
          roomStateIndex = IndexNone;
          AreaSelected?.Invoke (this, null);
          RoomListChanged?.Invoke (this, new ListLoadEventArgs (0));
        }
      }
    }

    // Index of the selected room.
    private int roomIndex = IndexNone;
    public int RoomIndex
    {
      get
      {
        int index = -1;
        for (int n = 0; n <= roomIndex; n++)
          if (Rooms [n].RoomArea == AreaIndex)
            index++;
        return index;
      }
      set
      {
        if (areaIndex == IndexNone)
        {
          roomIndex = IndexNone;
          roomStateIndex = IndexNone;
        }
        else if (value >= -1 && value < Rooms.Count && value != RoomIndex)
        {
          int index = -1;
          for (int n = 0; n <= value; n++)
          {
            index = Rooms.FindIndex (index + 1, x => x.RoomArea == AreaIndex);
            if (index == -1)
              break;
          }
          roomIndex = index;
          roomStateIndex = IndexNone;
          RoomStateIndex = 0;
          RoomSelected?.Invoke (this, null);
          RoomStateListChanged?.Invoke (this, new ListLoadEventArgs (0));
          DoorListChanged?.Invoke (this, new ListLoadEventArgs (0));
        }
      }
    }

    // Index of the selected room state.
    private int roomStateIndex = IndexNone;
    public int RoomStateIndex
    {
      get {return roomStateIndex;}
      set
      {
        LevelData oldLevelData = ActiveLevelData;
        int oldTileSetIndex = TileSetIndex;
        if (roomIndex == IndexNone)
        {
          roomStateIndex = IndexNone;
        }
        else if (value >= -1 && value < ActiveRoom.RoomStates.Count
                             && value != roomStateIndex)
        {
          roomStateIndex = value;
          TileSetIndex = ActiveRoomState != null ? ActiveRoomState.TileSet : IndexNone;
          RoomStateSelected?.Invoke (this, null);
          if (oldLevelData != ActiveLevelData || oldTileSetIndex != TileSetIndex)
            LevelDataSelected?.Invoke (this, null);
        }
      }
    }

    // Index of the selected room state's tile set.
    private int tileSetIndex = IndexNone;
    public int TileSetIndex
    {
      get {return tileSetIndex;}
      private set
      {
        if (value >= 0 && value < TileSetCount && value != tileSetIndex)
        {
          tileSetIndex = value;
          LoadRoomTiles (tileSetIndex);
          TileSetSelected?.Invoke (this, null);
        }
      }
    }

    // Index/H-flip/V-Flip of the selected room tile.
    private int tileIndex = IndexNone;
    public int TileIndex
    {
      get {return tileIndex;}
      set
      {
        if (value >= 0 && value < 1024 && value != tileIndex)
        {
          tileIndex = value;
          TileSelected?.Invoke (this, null);
        }
      }
    }
    private bool tileHFlip = false;
    public bool TileHFlip
    {
      get {return tileHFlip;}
      set
      {
        if (tileHFlip != value)
        {
          tileHFlip = value;
          TileSelected?.Invoke (this, null);
        }
      }
    }
    private bool tileVFlip = false;
    public bool TileVFlip
    {
      get {return tileVFlip;}
      set
      {
        if (tileVFlip != value)
        {
          tileVFlip = value;
          TileSelected?.Invoke (this, null);
        }
      }
    }

    // The selected BTS Type/value.
    private int btsType = 0;
    public int BtsType
    {
      get {return btsType;}
      set
      {
        if (value >= 0 && value < 16 && value != btsType)
        {
          btsType = value;
          BtsSelected?.Invoke (this, null);
        }
      }
    }
    private int btsValue = 0;
    public int BtsValue
    {
      get {return btsValue;}
      set
      {
        if (value >= 0 && value < 1024 && value != btsValue)
        {
          btsValue = value;
          BtsSelected?.Invoke (this, null);
        }
      }
    }

    // Area of the selected room.
    public int RoomArea
    {
      get
      {
        return ActiveRoom?.RoomArea ?? -1;
      }
      set
      {
        if (ActiveRoom != null)
          ActiveRoom.RoomArea = (byte) value;
      }
    }

    // Name of the selected room.
    public string RoomName
    {
      get
      {
        return ActiveRoom?.Name ?? "";
      }
      set
      {
        if (ActiveRoom != null)
          ActiveRoom.Name = value;
      }
    }

    // Up scroller value for the selected room.
    public int UpScroller
    {
      get
      {
        return ActiveRoom?.UpScroller ?? 0;
      }
      set
      {
        if (ActiveRoom != null)
          ActiveRoom.UpScroller = (byte) value;
      }
    }

    // Down scroller value for the selected room.
    public int DownScroller
    {
      get
      {
        return ActiveRoom?.DownScroller ?? 0;
      }
      set
      {
        if (ActiveRoom != null)
          ActiveRoom.DownScroller = (byte) value;
      }
    }

    // Special graphics bitflag for the selected room.
    public int SpecialGfx
    {
      get
      {
        return ActiveRoom?.SpecialGfxBitflag ?? 0;
      }
      set
      {
        if (ActiveRoom != null)
          ActiveRoom.SpecialGfxBitflag = (byte) value;
      }
    }

    // Type of the selected room state.
    public StateType RoomStateType
    {
      get
      {
        if (RoomStateIndex != IndexNone)
          return ActiveRoom.RoomStateHeaders [RoomStateIndex].HeaderType;
        return StateType.None;
      }
      set
      {
        if (RoomStateIndex != IndexNone)
          ActiveRoom.RoomStateHeaders [RoomStateIndex].HeaderType = value;
      }
    }

    // Event number of the selected room state.
    public int RoomStateEventNumber
    {
      get
      {
        if (RoomStateIndex != IndexNone)
          return ActiveRoom.RoomStateHeaders [RoomStateIndex].Value;
        return 0x00;
      }
      set
      {
        if (RoomStateIndex != IndexNone)
          ActiveRoom.RoomStateHeaders [RoomStateIndex].Value = (byte) value;
      }
    }

    // Song set of the selected room state.
    public int SongSet
    {
      get
      {
        return ActiveRoomState?.SongSet ?? 0;
      }
      set
      {
        if (ActiveRoomState != null)
          ActiveRoomState.SongSet = (byte) value;
      }
    }

    // Song index of the selected rooms state.
    public int PlayIndex
    {
      get
      {
        return ActiveRoomState?.PlayIndex ?? 0;
      }
      set
      {
        if (ActiveRoomState != null)
          ActiveRoomState.PlayIndex = (byte) value;
      }
    }

    // Background scrolling value of the selected rooms state.
    public int BackgroundScrolling
    {
      get
      {
        return ActiveRoomState?.BackgroundScrolling ?? 0;
      }
      set
      {
        if (ActiveRoomState != null)
          ActiveRoomState.BackgroundScrolling = value;
      }
    }

    // Pointers
    public int LevelDataPtr
    {
      get {return ActiveRoomState?.LevelDataPtr ?? 0;}
    }

    public int RoomScrollsPtr
    {
      get {return ActiveRoomState?.RoomScrollsPtr ?? 0;}
    }

    public int PlmSetPtr
    {
      get {return ActiveRoomState?.PlmSetPtr ?? 0;}
    }

    public int EnemySetPtr
    {
      get {return ActiveRoomState?.EnemySetPtr ?? 0;}
    }

    public int EnemyGfxPtr
    {
      get {return ActiveRoomState?.EnemyGfxPtr ?? 0;}
    }

    public int FxPtr
    {
      get {return ActiveRoomState?.FxPtr ?? 0;}
    }

    public int SetupAsmPtr
    {
      get {return ActiveRoomState?.SetupAsmPtr ?? 0;}
    }

    public int MainAsmPtr
    {
      get {return ActiveRoomState?.MainAsmPtr ?? 0;}
    }



//---------------------------------------------------------------------------------------------------
// Other

    // List of area names.
    public List <string> AreaNames
    {
      get
      {
        var names = new List <string> ();
        for (int i = 0; i < 8; i++)
          names.Add (i.ToString ());
        return names;
      }
    }

    // List of room names for active area.
    public List <string> RoomNames
    {
      get
      {
        var names = new List <string> ();
        if (AreaIndex != IndexNone)
          foreach (Room r in Rooms)
            if (r.RoomArea == AreaIndex)
              names.Add (r.Name);
        return names;
      }
    }

    // List of room state names for active room.
    public List <string> RoomStateNames
    {
      get
      {
        var names = new List <string> ();
        if (RoomIndex != IndexNone)
          foreach (RoomStateHeader r in Rooms [RoomIndex].RoomStateHeaders)
            names.Add (r.HeaderType.ToString ());
        return names;
      }
    }

    // List of room state names for active room.
    public List <string> DoorNames
    {
      get
      {
        List <string> doorNames = new List <string> ();
        if (ActiveRoom != null && ActiveRoom.MyDoorSet != null)
        {
          for (int n = 0; n < ActiveRoom.MyDoorSet.DoorCount; n++)
          {
            Room destRoom = ActiveRoom.MyDoorSet.MyDoors [n].MyTargetRoom;
            doorNames.Add (Tools.IntToHex (n) + " " + (destRoom?.Name ?? ""));
          }
        }
        return doorNames;
      }
    }

    // Width of active room in tiles.
    public int RoomWidthInTiles
    {
      get
      {
        if (roomIndex != IndexNone)
          return Rooms [roomIndex].RoomW * 16;
        return 0;
      }
    }

    // Height of active room in tiles.
    public int RoomHeightInTiles
    {
      get
      {
        if (roomIndex != IndexNone)
          return Rooms [roomIndex].RoomH * 16;
        return 0;
      }
    }

    // Width of active room in Screens.
    public int RoomWidthInScreens
    {
      get
      {
        if (roomIndex != IndexNone)
          return Rooms [roomIndex].RoomW;
        return 0;
      }
    }

    // Height of active room in Screens.
    public int RoomHeightInScreens
    {
      get
      {
        if (roomIndex != IndexNone)
          return Rooms [roomIndex].RoomH;
        return 0;
      }
    }


//========================================================================================
// Methods


    // Constructor.
    public Project ()
    {
      Rooms          = new List <Room         > ();
      DoorSets       = new List <DoorSet      > ();
      Doors          = new List <Door         > ();
      RoomStates     = new List <RoomState    > ();
      ScrollSets     = new List <ScrollSet    > ();
      PlmSets        = new List <PlmSet       > ();
      ScrollPlmDatas = new List <ScrollPlmData> ();
      Backgrounds    = new List <Background   > ();
      Fxs            = new List <Fx           > ();
      SaveRooms      = new List <SaveRoom     > ();
      LevelDatas     = new List <LevelData    > ();
      EnemySets      = new List <EnemySet     > ();
      EnemyGfxs      = new List <EnemyGfx     > ();
      ScrollAsms     = new List <ScrollAsm    > ();

      TileSets       = new List <TileSet      > ();
      TileTables     = new List <TileTable    > ();
      TileSheets     = new List <TileSheet    > ();
      Palettes       = new List <Palette      > ();
      AreaMaps       = new List <AreaMap      > ();
      PlmTypes       = new List <PlmType      > ();
      EnemyTypes     = new List <EnemyType    > ();

      // Load Resources.
      LoadBtsTiles ();
    }


//========================================================================================
// Getters & Setters

// Level data

    // Check if room and room state are active.
    private bool ActiveLevelDataInfo (out LevelData data, out int width)
    {
      //if (roomIndex != IndexNone && roomStateIndex != IndexNone && 
      //    ActiveRoomState.MyLevelData != null)
      if (ActiveLevelData != null)
      {
        data = ActiveLevelData; // ActiveRoomState.MyLevelData;
        width = ActiveRoom.RoomW * 16;
        return true;
      }
      data = null;
      width = -1;
      return false;
    }


    public int GetLayer1Tile (int row, int col)
    {
      if (ActiveLevelDataInfo (out LevelData data, out int width))
        return data.GetLayer1Tile (row * width + col);
      return 0;
    }


    public bool GetLayer1HFlip (int row, int col)
    {
      if (ActiveLevelDataInfo (out LevelData data, out int width))
        return data.GetLayer1HFlip (row * width + col);
      return false;
    }


    public bool GetLayer1VFlip (int row, int col)
    {
      if (ActiveLevelDataInfo (out LevelData data, out int width))
        return data.GetLayer1VFlip (row * width + col);
      return false;
    }


    public int GetBtsType (int row, int col)
    {
      if (ActiveLevelDataInfo (out LevelData data, out int width))
        return data.GetBtsType (row * width + col);
      return 0;
    }


    public int GetBtsValue (int row, int col)
    {
      if (ActiveLevelDataInfo (out LevelData data, out int width))
        return data.GetBtsValue (row * width + col);
      return 0;
    }


    public int GetLayer2Tile (int row, int col)
    {
      if (ActiveLevelDataInfo (out LevelData data, out int width))
        return data.GetLayer2Tile (row * width + col);
      return 0;
    }


    public bool GetLayer2HFlip (int row, int col)
    {
      if (ActiveLevelDataInfo (out LevelData data, out int width))
        return data.GetLayer2HFlip (row * width + col);
      return false;
    }


    public bool GetLayer2VFlip (int row, int col)
    {
      if (ActiveLevelDataInfo (out LevelData data, out int width))
        return data.GetLayer2VFlip (row * width + col);
      return false;
    }


    // Flip the active BTS horizontally.
    public void HFlipBts ()
    {
      switch (BtsType) {
      case 0x1: // Slope
        BtsValue ^= 0x40;
        break;
      case 0x3: // Treadmill
        if (BtsValue == 0x08 || BtsValue == 0x09)
          BtsValue ^= 1;
        break;
      case 0x5: // H-copy
        if (BtsValue != 0)
          BtsValue = 0x100 - BtsValue;
        break;
      case 0xC: // Door cap
        if (BtsValue == 0x40 || BtsValue == 0x41)
          BtsValue ^= 1;
        break;
      default:
        break;
      }
    }


    // Flip the active BTS vertically.
    public void VFlipBts ()
    {
      switch (BtsType) {
      case 0x1: // Slope
        BtsValue ^= 0x80;
        break;
      case 0xC: // Door cap
        if (BtsValue == 0x42 || BtsValue == 0x43)
          BtsValue ^= 1;
        break;
      case 0xD: // V-copy
        if (BtsValue != 0)
          BtsValue = 0x100 - BtsValue;
        break;
      default:
        break;
      }
    }

//----------------------------------------------------------------------------------------
// Scrolls

    public ScrollColor GetScroll (int x, int y)
    {
      if (ActiveRoom == null || ActiveRoomState == null)
        return ScrollColor.None;
      if (ActiveRoomState.MyScrollSet == null)
      {
        switch (ActiveRoomState.RoomScrollsPtr)
        {
        case ScrollSet.AllBlue:
          return ScrollColor.Blue;
        case ScrollSet.AllGreen:
          return ScrollColor.Green;
        default:
          return ScrollColor.None;
        }
      }
      int index = ActiveRoom.RoomW * y + x;
      return ActiveRoomState.MyScrollSet.GetScroll (index);
    }



  } // class Project

}
