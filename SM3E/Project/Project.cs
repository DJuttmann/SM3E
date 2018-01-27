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
    private List <Room         > [] Rooms = new List <Room> [AreaCount];
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
    private Room ActiveRoom = null;
    
    // Reference to the active room state.
    private RoomState ActiveRoomState = null;

    // Reference to the active room state's level data.
    private LevelData ActiveLevelData = null;

    // Reference to the active tile set.
    private TileSet ActiveTileSet = null;

    // Reference to the active door.
    private Door ActiveDoor = null;


    // Index of the selected area.
    public int AreaIndex {get; private set;} = IndexNone;

    // Index of the selected room.
    public int RoomIndex {get; private set;} = IndexNone;

    // Index of the selected room state.
    public int RoomStateIndex {get; private set;} = IndexNone;

    // Index of the selected room state's tile set.
    public int TileSetIndex {get; private set;} = 0;

    // Index of the selected room state's tile set.
    public int DoorIndex {get; private set;} = IndexNone;


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
        return ActiveRoom?.Area ?? -1;
      }
      set
      {
        if (ActiveRoom != null)
          ActiveRoom.Area = (byte) value;
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
          foreach (Room r in Rooms [AreaIndex])
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
        if (AreaIndex != IndexNone && RoomIndex != IndexNone)
          foreach (RoomStateHeader r in Rooms [AreaIndex] [RoomIndex].RoomStateHeaders)
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
      get {return ActiveRoom?.RoomW * 16 ?? 0; }
    }

    // Height of active room in tiles.
    public int RoomHeightInTiles
    {
      get {return ActiveRoom?.RoomH * 16 ?? 0; }
    }

    // Width of active room in Screens.
    public int RoomWidthInScreens
    {
      get {return ActiveRoom?.RoomW ?? 0; }
    }

    // Height of active room in Screens.
    public int RoomHeightInScreens
    {
      get {return ActiveRoom?.RoomH ?? 0; }
    }


//========================================================================================
// Methods


    // Constructor.
    public Project ()
    {
      for (int i = 0; i < AreaCount; i++)
        Rooms [i]    = new List <Room         > ();
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
    
    public void SetLayer1 (int rowMin, int colMin, int rowMax, int colMax)
    {
      if (ActiveLevelData == null)
        return;
      Tools.Order (ref rowMin, ref rowMax);
      Tools.Order (ref colMin, ref colMax);
      if (rowMin < 0)
        rowMin = 0;
      if (colMin < 0)
        colMin = 0;
      if (rowMax >= RoomHeightInTiles)
        rowMax = RoomHeightInTiles - 1;
      if (colMax >= RoomWidthInTiles)
        colMax = RoomWidthInTiles - 1;
      if (rowMax < rowMin || colMax < colMin)
        return;
      for (int row = rowMin; row <= rowMax; row++)
        for (int col = colMin; col <= colMax; col++)
          ActiveLevelData.SetLayer1 (row * RoomWidthInTiles + col,
                                     TileIndex, TileHFlip, TileVFlip);
      LevelDataEventArgs e = new LevelDataEventArgs ()
      {
        ScreenXmin = colMin / 16,
        ScreenXmax = colMax / 16,
        ScreenYmin = rowMin / 16,
        ScreenYmax = rowMax / 16
      };
      LevelDataModified?.Invoke (this, e);
    }


    public void SetLayer2 (int rowMin, int colMin, int rowMax, int colMax)
    {
      if (ActiveLevelData == null)
        return;
      Tools.Order (ref rowMin, ref rowMax);
      Tools.Order (ref colMin, ref colMax);
      if (rowMin < 0)
        rowMin = 0;
      if (colMin < 0)
        colMin = 0;
      if (rowMax >= RoomHeightInTiles)
        rowMax = RoomHeightInTiles - 1;
      if (colMax >= RoomWidthInTiles)
        colMax = RoomWidthInTiles - 1;
      for (int row = rowMin; row <= rowMax; row++)
        for (int col = colMin; col <= colMax; col++)
          ActiveLevelData.SetLayer2 (row * RoomWidthInTiles + col,
                                     TileIndex, TileHFlip, TileVFlip);
      LevelDataEventArgs e = new LevelDataEventArgs ()
      {
        ScreenXmin = colMin / 16,
        ScreenXmax = colMax / 16,
        ScreenYmin = rowMin / 16,
        ScreenYmax = rowMax / 16
      };
      LevelDataModified?.Invoke (this, e);
    }


    public void SetBts (int rowMin, int colMin, int rowMax, int colMax)
    {
      if (ActiveLevelData == null)
        return;
      Tools.Order (ref rowMin, ref rowMax);
      Tools.Order (ref colMin, ref colMax);
      if (rowMin < 0)
        rowMin = 0;
      if (colMin < 0)
        colMin = 0;
      if (rowMax >= RoomHeightInTiles)
        rowMax = RoomHeightInTiles - 1;
      if (colMax >= RoomWidthInTiles)
        colMax = RoomWidthInTiles - 1;
      for (int row = rowMin; row <= rowMax; row++)
        for (int col = colMin; col <= colMax; col++)
          ActiveLevelData.SetBts (row * RoomWidthInTiles + col,
                                  BtsType, BtsValue);
      LevelDataEventArgs e = new LevelDataEventArgs ()
      {
        ScreenXmin = colMin / 16,
        ScreenXmax = colMax / 16,
        ScreenYmin = rowMin / 16,
        ScreenYmax = rowMax / 16
      };
      LevelDataModified?.Invoke (this, e);
    }

//========================================================================================
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


//========================================================================================
// Selecting things

    
    private enum ListUpdate
    {
      Area = 0,
      Room = 1,
      RoomState = 2,
      TileSet = 3,
      Door = 4
    }


    // A class tracking which selected items have changed.
    private class ActiveItems
    {
      public int ActiveArea;
      public Room ActiveRoom;
      public RoomState ActiveRoomState;
      public TileSet ActiveTileSet;
      public Door ActiveDoor;


      public ActiveItems (int activeArea, Room activeRoom, RoomState activeRoomState,
                          TileSet activeTileSet, Door activeDoor)
      {
        ActiveArea = activeArea;
        ActiveRoom = activeRoom;
        ActiveRoomState = activeRoomState;
        ActiveTileSet = activeTileSet;
        ActiveDoor = activeDoor;
      }
    }
    

    public void SelectArea (int index)
    {
      var a = new ActiveItems (AreaIndex, ActiveRoom, ActiveRoomState,
                               ActiveTileSet, ActiveDoor);
      if (index == AreaIndex || index < -1 || index >= AreaCount)
        return;
      var update = new List <ListUpdate> ();
      ForceSelectArea (index, update);
      RaiseChangeEvents (a);
    }
    

    public void SelectRoom (int index)
    {
      var a = new ActiveItems (AreaIndex, ActiveRoom, ActiveRoomState,
                               ActiveTileSet, ActiveDoor);
      if (AreaIndex == IndexNone || index == RoomIndex || index < -1 ||
          index >= Rooms [AreaIndex].Count)
        return;
      var update = new List <ListUpdate> ();
      ForceSelectRoom (index, update);
      RaiseChangeEvents (a);
    }
    

    public void SelectRoomState (int index)
    {
      var a = new ActiveItems (AreaIndex, ActiveRoom, ActiveRoomState,
                               ActiveTileSet, ActiveDoor);
      if (ActiveRoom == null || index == RoomStateIndex || index < -1 ||
          index >= ActiveRoom.RoomStates.Count)
        return;
      var update = new List <ListUpdate> ();
      ForceSelectRoomState (index, update);
      RaiseChangeEvents (a);
    }
    

    public void SelectDoor (int index)
    {
      var a = new ActiveItems (AreaIndex, ActiveRoom, ActiveRoomState,
                               ActiveTileSet, ActiveDoor);
      if (ActiveRoom == null || index == DoorIndex || index < -1 ||
          index >= ActiveRoom.MyDoorSet.DoorCount)
        return;
      var update = new List <ListUpdate> ();
      ForceSelectDoor (index, update);
      RaiseChangeEvents (a);
    }


    private void RaiseChangeEvents (ActiveItems a)
    {
      if (a.ActiveTileSet != ActiveTileSet)
      {
        TileSetSelected?.Invoke (this, null);
      }
      if (a.ActiveDoor != ActiveDoor)
      {
        DoorSelected?.Invoke (this, null);
      }
      if (a.ActiveRoomState != ActiveRoomState)
      {
        RoomStateSelected?.Invoke (this, null);
      }
      if (a.ActiveRoom != ActiveRoom)
      {
        RoomSelected?.Invoke (this, null);
        RoomStateListChanged?.Invoke (this, new ListLoadEventArgs (RoomStateIndex));
        DoorListChanged?.Invoke (this, new ListLoadEventArgs (DoorIndex));
      }
      if (a.ActiveArea != AreaIndex)
      {
        AreaSelected?.Invoke (this, null);
        RoomListChanged?.Invoke (this, new ListLoadEventArgs (RoomIndex));
      }
    }
    

    private void ForceSelectArea (int index, List <ListUpdate> update)
    {
      if (index >= 0 && index < AreaCount)
      {
        AreaIndex = index;
        ForceSelectRoom (0, update);
      }
      else
      {
        AreaIndex = IndexNone;
        ForceSelectRoom (IndexNone, update);
      }
    }


    private void ForceSelectRoom (int index, List <ListUpdate> update)
    {
      if (AreaIndex != IndexNone && index >= 0 && index < Rooms [AreaIndex].Count)
      {
        RoomIndex = index;
        ActiveRoom = Rooms [AreaIndex] [RoomIndex];
        ForceSelectRoomState (0, update);
        ForceSelectDoor (0, update);
      }
      else
      {
        RoomIndex = IndexNone;
        ActiveRoom = null;
        ForceSelectRoomState (IndexNone, update);
        ForceSelectDoor (IndexNone, update);
      }
    }


    private void ForceSelectRoomState (int index, List <ListUpdate> update)
    {
      if (ActiveRoom != null && index >= 0 && index < ActiveRoom.RoomStates.Count)
      {
        RoomStateIndex = index;
        ActiveRoomState = ActiveRoom.RoomStates [RoomStateIndex];
        ForceSelectTileSet (ActiveRoom.RoomStates [RoomStateIndex].TileSet, update);
      }
      else
      {
        RoomStateIndex = IndexNone;
        ActiveRoomState = null;
        ForceSelectTileSet (0, update);
      }
    }


    private void ForceSelectTileSet (int index, List <ListUpdate> update)
    {
      
      if (index < 0 || index > TileSets.Count)
        index = 0;
      if (index != TileSetIndex)
      {
        LoadRoomTiles (TileSetIndex);
        TileSetIndex = index;
        ActiveTileSet = TileSets [TileSetIndex];
      }
    }


    private void ForceSelectDoor (int index, List <ListUpdate> update)
    {
      if (ActiveRoom != null && index >= 0 && index < ActiveRoom.MyDoorSet.DoorCount)
      {
        DoorIndex = index;
        ActiveDoor = ActiveRoom.MyDoorSet.MyDoors [DoorIndex];
      }
      else
      {
        DoorIndex = IndexNone;
        ActiveDoor = null;
      }
    }


    public void NavigateThroughDoor (int row, int col)
    {
      if (ActiveRoom?.MyDoorSet == null || ActiveLevelData == null)
        return;
      List <Door> doors = ActiveRoom.MyDoorSet.MyDoors;
      if (GetBtsType (row, col) == 0x9) // if door
      {
        int index = GetBtsValue (row, col);
        if (index < doors.Count)
        {
          //doors [index].MyTargetRoom;
        }
      }
    }

  } // class Project

}
