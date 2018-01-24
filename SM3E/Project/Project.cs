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
