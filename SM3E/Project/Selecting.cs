using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;


namespace SM3E
{


  partial class Project
  {

    // References to selected items
    private Room ActiveRoom = null;
    private RoomState ActiveRoomState = null;
    private Door ActiveDoor = null;
    private Plm ActivePlm = null;
    private PlmType ActivePlmType = null;
    private Enemy ActiveEnemy = null;
    private EnemyType ActiveEnemyType = null;
    private EnemyType ActiveEnemyGfx = null;
    private IScrollData ActiveScrollData = null;
    private ScrollColor ActiveScrollColor = ScrollColor.None;

    private TileSet ActiveTileSet
    {
      get {return TileSetIndex != IndexNone ? (TileSet) TileSets [TileSetIndex]
                                            : null;}
    }

    private LevelData ActiveLevelData
    {
      get {return ActiveRoomState?.MyLevelData;}
    }

    private PlmSet ActivePlmSet
    {
      get {return ActiveRoomState?.MyPlmSet;}
    }

    private EnemySet ActiveEnemySet
    {
      get {return ActiveRoomState?.MyEnemySet;}
    }

    private AreaMap ActiveAreaMap
    {
      get {return AreaIndex != IndexNone ? (AreaMap) AreaMaps [AreaIndex] : null;}
    }

    private Background ActiveBackground
    {
      get {return ActiveRoomState?.MyBackground;}
    }


    // Indices of selected items.
    public int AreaIndex {get; private set;} = IndexNone;
    public int RoomIndex {get; private set;} = IndexNone;
    public int RoomStateIndex {get; private set;} = IndexNone;
    public int DoorIndex {get; private set;} = IndexNone;
    public int PlmIndex {get; private set;} = IndexNone;
    public int PlmTypeIndex {get; private set;} = IndexNone;
    public int EnemyIndex {get; private set;} = IndexNone;
    public int EnemyGfxIndex {get; private set;} = IndexNone;
    public int EnemyTypeIndex {get; private set;} = IndexNone;
    public int ScrollDataIndex {get; private set;} = IndexNone;
    public int ScrollColorIndex {get; private set;} = IndexNone;
    public int TileSetIndex
    {
      get {return ActiveRoomState?.TileSet ?? -1;}
    }
    public int BackgroundIndex
    {
      get {return Backgrounds.FindIndex (x => x == ActiveRoomState?.MyBackground);}
    }
    public int SetupAsmIndex
    {
      get {return SetupAsms.FindIndex (x => x == ActiveRoomState?.MySetupAsm);}
    }
    public int MainAsmIndex
    {
      get {return MainAsms.FindIndex (x => x == ActiveRoomState?.MyMainAsm);}
    }


//========================================================================================
// Selecting things


    // A class tracking which selected items have changed.
    private class ActiveItems
    {
      public int ActiveArea;
      public Room ActiveRoom;
      public RoomState ActiveRoomState;
      public TileSet ActiveTileSet;
      public Door ActiveDoor;
      public LevelData ActiveLevelData;
      public Plm ActivePlm;
      public PlmType ActivePlmType;
      public Enemy ActiveEnemy;
      public EnemyType ActiveEnemyGfx;
      public EnemyType ActiveEnemyType;
      public IScrollData ActiveScrollData;
      public ScrollColor ActiveScrollColor;
      public Background ActiveBackground;

      public ActiveItems (Project p)
      {
        ActiveArea = p.AreaIndex;
        ActiveRoom = p.ActiveRoom;
        ActiveRoomState = p.ActiveRoomState;
        ActiveTileSet = p.ActiveTileSet;
        ActiveDoor = p.ActiveDoor;
        ActiveLevelData = p.ActiveLevelData;
        ActivePlm = p.ActivePlm;
        ActivePlmType = p.ActivePlmType;
        ActiveEnemy = p.ActiveEnemy;
        ActiveEnemyGfx = p.ActiveEnemyGfx;
        ActiveEnemyType = p.ActiveEnemyType;
        ActiveScrollData = p.ActiveScrollData;
        ActiveScrollColor = p.ActiveScrollColor;
        ActiveBackground = p.ActiveBackground;
      }
    }

    // Flag for detecting if any selection is being handled.
    private bool HandlingSelection = false;
    

    public void SelectArea (int index)
    {
      if (HandlingSelection)
        return;
      if (index == AreaIndex || index < -1 || index >= AreaCount)
        return;
      HandlingSelection = true;
      var a = new ActiveItems (this);
      ForceSelectArea (index);
      RaiseChangeEvents (a);
      HandlingSelection = false;
    }
    

    public void SelectRoom (int index)
    {
      if (HandlingSelection)
        return;
      if (AreaIndex == IndexNone || index == RoomIndex || index < -1 ||
          index >= Rooms [AreaIndex].Count)
        return;
      HandlingSelection = true;
      var a = new ActiveItems (this);
      ForceSelectRoom (index);
      RaiseChangeEvents (a);
      HandlingSelection = false;
    }
    

    public void SelectRoomState (int index)
    {
      if (HandlingSelection)
        return;
      if (ActiveRoom == null || index == RoomStateIndex || index < -1 ||
          index >= ActiveRoom.RoomStates.Count)
        return;
      HandlingSelection = true;
      var a = new ActiveItems (this);
      ForceSelectRoomState (index);
      RaiseChangeEvents (a);
      HandlingSelection = false;
    }
    

    public void SelectDoor (int index)
    {
      if (HandlingSelection)
        return;
      if (ActiveRoom == null || index == DoorIndex || index < -1 ||
          index >= ActiveRoom.MyDoorSet.DoorCount)
        return;
      HandlingSelection = true;
      var a = new ActiveItems (this);
      ForceSelectDoor (index);
      RaiseChangeEvents (a);
      HandlingSelection = false;
    }


    public void SelectPlm (int index)
    {
      if (HandlingSelection)
        return;
      if (ActiveRoomState == null || index == PlmIndex || index < -1 ||
          index >= ActiveRoomState.MyPlmSet.PlmCount)
        return;
      HandlingSelection = true;
      var a = new ActiveItems (this);
      ForceSelectPlm (index);
      RaiseChangeEvents (a);
      HandlingSelection = false;
    }


    public void SelectPlmType (int index)
    {
      if (HandlingSelection)
        return;
      if (index == PlmTypeIndex || index < -1 || index >= PlmTypes.Count)
        return;
      HandlingSelection = true;
      var a = new ActiveItems (this);
      ForceSelectPlmType (index);
      RaiseChangeEvents (a);
      HandlingSelection = false;
    }


    public void SelectEnemy (int index)
    {
      if (HandlingSelection)
        return;
      if (ActiveRoomState == null || index == EnemyIndex || index < -1 ||
          index >= ActiveRoomState.MyEnemySet.EnemyCount)
        return;
      HandlingSelection = true;
      var a = new ActiveItems (this);
      ForceSelectEnemy (index);
      RaiseChangeEvents (a);
      HandlingSelection = false;
    }


    public void SelectEnemyGfx (int index)
    {
      if (HandlingSelection)
        return;
      if (ActiveRoomState == null || index == EnemyGfxIndex || index < -1 ||
          index >= ActiveRoomState.MyEnemyGfx.EnemyGfxCount)
        return;
      HandlingSelection = true;
      var a = new ActiveItems (this);
      ForceSelectEnemyGfx (index);
      RaiseChangeEvents (a);
      HandlingSelection = false;
    }


    public void SelectEnemyType (int index)
    {
      if (HandlingSelection)
        return;
      if (index == EnemyTypeIndex || index < -1 || index >= EnemyTypes.Count)
        return;
      HandlingSelection = true;
      var a = new ActiveItems (this);
      ForceSelectEnemyType (index);
      RaiseChangeEvents (a);
      HandlingSelection = false;
    }


    public void SelectScrollData (int index)
    {
      if (HandlingSelection)
        return;
      if (index == ScrollDataIndex || index < -1 || index >= ScrollDatas.Count)
        return;
      HandlingSelection = true;
      var a = new ActiveItems (this);
      ForceSelectScrollData (index);
      RaiseChangeEvents (a);
      HandlingSelection = false;
    }


    public void SelectScrollColor (int index)
    {
      if (HandlingSelection)
        return;
      if (index == ScrollColorIndex || index < -1 || index >= 4)
        return;
      HandlingSelection = true;
      var a = new ActiveItems (this);
      ForceSelectScrollColor (index);
      RaiseChangeEvents (a);
      HandlingSelection = false;
    }


    // Checks which selected items have changed and raises the corresponding events.
    // [wip] Order the statements according to call hierarchy?
    private void RaiseChangeEvents (ActiveItems a)
    {
      if (a.ActiveTileSet != ActiveTileSet || a.ActiveBackground != ActiveBackground)
        LoadBackground ();
      if (a.ActiveTileSet != ActiveTileSet)
      {
        LoadRoomTiles (TileSetIndex);
        TileSetSelected?.Invoke (this, null);
      }
      if (a.ActiveScrollColor != ActiveScrollColor)
        ScrollColorSelected?.Invoke (this, null);
      if (a.ActiveEnemyType != ActiveEnemyType)
        EnemyTypeSelected (this, null);
      if (a.ActiveEnemyGfx != ActiveEnemyGfx)
        EnemyGfxSelected?.Invoke (this, null);
      if (a.ActiveEnemy != ActiveEnemy)
        EnemySelected?.Invoke (this, null);
      if (a.ActivePlmType != ActivePlmType)
        PlmTypeSelected?.Invoke (this, null);
      if (a.ActivePlm != ActivePlm)
        PlmSelected?.Invoke (this, null);
      if (a.ActiveDoor != ActiveDoor)
        DoorSelected?.Invoke (this, null);
      if (a.ActiveRoomState != ActiveRoomState)
      {
        RoomStateSelected?.Invoke (this, null);
        LevelDataSelected?.Invoke (this, null);
        PlmListChanged?.Invoke (this, new ListLoadEventArgs (PlmIndex));
        EnemyListChanged?.Invoke (this, new ListLoadEventArgs (EnemyIndex));
        EnemyGfxListChanged?.Invoke (this, new ListLoadEventArgs (EnemyGfxIndex));
        ScrollDataListChanged?.Invoke (this, new ListLoadEventArgs (ScrollDataIndex));
      }
      else if (a.ActiveLevelData != ActiveLevelData)
      {
        LevelDataSelected.Invoke (this, null);
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
      if (a.ActiveScrollData != ActiveScrollData)
      {
        ScrollDataSelected?.Invoke (this, null);
        var e = new LevelDataEventArgs () {AllScreens = true};
        LevelDataModified?.Invoke (this, e);
      }
    }
    

//========================================================================================
// Forced selection - always sets the item index to some valid value (may be IndexNone).


    private void ForceSelectArea (int index)
    {
      if (index >= 0 && index < AreaCount)
      {
        AreaIndex = index;
        ForceSelectRoom (0);
      }
      else
      {
        AreaIndex = IndexNone;
        ForceSelectRoom (IndexNone);
      }
    }


    private void ForceSelectRoom (int index)
    {
      if (AreaIndex != IndexNone && index >= 0 && index < Rooms [AreaIndex].Count)
      {
        RoomIndex = index;
        ActiveRoom = (Room) Rooms [AreaIndex] [RoomIndex];
        ForceSelectRoomState (ActiveRoom.RoomStates.Count - 1);
        ForceSelectDoor (0);
      }
      else
      {
        RoomIndex = IndexNone;
        ActiveRoom = null;
        ForceSelectRoomState (IndexNone);
        ForceSelectDoor (IndexNone);
      }
    }


    private void ForceSelectRoomState (int index)
    {
      if (ActiveRoom != null && index >= 0 && index < ActiveRoom.RoomStates.Count)
      {
        RoomStateIndex = index;
        ActiveRoomState = ActiveRoom.RoomStates [RoomStateIndex];
        ForceSelectPlm (0);
        ForceSelectEnemy (0);
        ForceSelectScrollData (0);
      }
      else
      {
        RoomStateIndex = IndexNone;
        ActiveRoomState = null;
        ForceSelectPlm (IndexNone);
        ForceSelectEnemy (IndexNone);
        ForceSelectScrollData (IndexNone);
      }
    }


    private void ForceSelectDoor (int index)
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


    private void ForceSelectPlm (int index)
    {
      if (ActiveRoomState?.MyPlmSet != null && index >= 0 &&
          index < ActiveRoomState.MyPlmSet.PlmCount)
      {
        PlmIndex = index;
        ActivePlm = ActiveRoomState.MyPlmSet.Plms [PlmIndex];
        ForceSelectPlmType (ActivePlm.MyPlmType?.Index ?? IndexNone);
      }
      else
      {
        PlmIndex = IndexNone;
        ActivePlm = null;
        ForceSelectPlmType (IndexNone);
      }
    }


    private void ForceSelectPlmType (int index)
    {
      if (index >= 0 && index < PlmTypes.Count)
      {
        PlmTypeIndex = index;
        ActivePlmType = PlmTypes [PlmTypeIndex];
      }
      else
      {
        PlmTypeIndex = IndexNone;
        ActivePlmType = null;
      }
    }


    private void ForceSelectEnemy (int index)
    {
      if (ActiveRoomState?.MyEnemySet != null && index >= 0 &&
          index < ActiveRoomState.MyEnemySet.EnemyCount)
      {
        EnemyIndex = index;
        ActiveEnemy = ActiveRoomState.MyEnemySet.Enemies [EnemyIndex];
        ForceSelectEnemyType (ActiveEnemy.MyEnemyType?.Index ?? IndexNone);
      }
      else
      {
        EnemyIndex = IndexNone;
        ActiveEnemy = null;
        ForceSelectEnemyType (IndexNone);
      }
    }


    private void ForceSelectEnemyGfx (int index)
    {
      if (ActiveRoomState?.MyEnemyGfx != null && index >= 0 &&
          index < ActiveRoomState.MyEnemyGfx.EnemyGfxCount)
      {
        EnemyGfxIndex = index;
        ActiveEnemyGfx = ActiveRoomState?.MyEnemyGfx?.MyEnemyTypes [EnemyGfxIndex];
        ForceSelectEnemyType (ActiveEnemyGfx?.Index ?? IndexNone);
      }
      else
      {
        EnemyGfxIndex = IndexNone;
        ActiveEnemyGfx = null;
        ForceSelectEnemyType (IndexNone);
      }
    }


    private void ForceSelectEnemyType (int index)
    {
      if (index >= 0 && index < EnemyTypes.Count)
      {
        EnemyTypeIndex = index;
        ActiveEnemyType = EnemyTypes [EnemyTypeIndex];
      }
      else
      {
        EnemyTypeIndex = IndexNone;
        ActiveEnemyType = null;
      }
    }


    private void ForceSelectScrollData (int index)
    {
      List <IScrollData> data = ScrollDatas;
      if (index >= 0 && index < data.Count)
      {
        ScrollDataIndex = index;
        ActiveScrollData = data [ScrollDataIndex];
      }
      else
      {
        ScrollDataIndex = IndexNone;
        ActiveScrollData = null;
      }
    }


    private void ForceSelectScrollColor (int index)
    {
      if (index >= 0 && index < 4)
      {
        ScrollColorIndex = index;
        ActiveScrollColor = (ScrollColor) ScrollColorIndex;
      }
      else
      {
        ScrollDataIndex = IndexNone;
        ActiveScrollColor = ScrollColor.None;
      }
    }

//----------------------------------------------------------------------------------------

    // Check if tile in room is door tile; if it is, select the destination room.
    public bool NavigateThroughDoor (int row, int col, out int screenX, out int screenY)
    {
      screenX = 0;
      screenY = 0;
      if (ActiveRoom?.MyDoorSet == null || ActiveLevelData == null)
        return false;
      List <Door> doors = ActiveRoom.MyDoorSet.MyDoors;
      if (GetBtsType (row, col) != 0x9) // return if not door
        return false;
      int index = GetBtsValue (row, col);
      if (index >= doors.Count)
        return false;
      Room targetRoom = doors [index].MyTargetRoom;
      if (targetRoom == null)
        return false;
      int targetArea = targetRoom.Area;
      screenX = doors [index].ScreenX;
      screenY = doors [index].ScreenY;

      HandlingSelection = true;
      var a = new ActiveItems (this);
      ForceSelectArea (targetArea);
      ForceSelectRoom (Rooms [targetArea].FindIndex (x => x == targetRoom));
      RaiseChangeEvents (a);
      HandlingSelection = false;
      return true;
    }


    // Try to move to a room at position on the map.
    public bool NavigateToMapPosition (int x, int y, out int screenX, out int screenY)
    {
      screenX = 0;
      screenY = 0;
      if (AreaIndex == IndexNone)
        return false;
      int startIndex = RoomIndex;
      if (startIndex == IndexNone)
        startIndex = Rooms [AreaIndex].Count - 1;
      int i = startIndex;
      do
      {
        i = (i + 1) % Rooms [AreaIndex].Count;
        Room r = (Room) Rooms [AreaIndex] [i];
        if (x >= r.MapX && x < r.MapX + r.Width &&
            y >= r.MapY && y < r.MapY + r.Height)
        {
          HandlingSelection = true;
          var a = new ActiveItems (this);
          ForceSelectRoom (i);
          RaiseChangeEvents (a);
          HandlingSelection = false;
          screenX = x - ActiveRoom.MapX;
          screenY = y - ActiveRoom.MapY;
          return true;
        }
      } while (i != startIndex);
      return false;
    }


    // Select door at given square.
    public void SelectDoorAt (int row, int col)
    {
      if (ActiveRoom?.MyDoorSet == null || ActiveLevelData == null)
        return;
      List <Door> doors = ActiveRoom.MyDoorSet.MyDoors;
      if (GetBtsType (row, col) != 0x9) // return if not door
        return;
      int index = GetBtsValue (row, col);
      if (index >= doors.Count)
        return;

      HandlingSelection = true;
      var a = new ActiveItems (this);
      ForceSelectDoor (index);
      RaiseChangeEvents (a);
      HandlingSelection = false;
    }


    // Select PLM at given tile position (col, row), updates row, col, width & height to
    // exact position and size of PLM.
    public bool SelectPlmAt (int col, int row)
    {
      if (ActivePlmSet == null)
        return false;
      for (int index = 0; index < ActivePlmSet.PlmCount; index++)
      {
        Plm p = ActivePlmSet.Plms [index];
        int width = p.MyPlmType?.Graphics.Width / 16 ?? 0;
        int height = p.MyPlmType?.Graphics.Height / 16 ?? 0;
        if (col >= p.PosX && col < p.PosX + width &&
            row >= p.PosY && row < p.PosY + height)
        {
          HandlingSelection = true;
          var a = new ActiveItems (this);
          ForceSelectPlm (index);
          RaiseChangeEvents (a);
          HandlingSelection = false;
          return true;
        }
      }
      return false;
    }


    // Select enemy at given pixel position (x, y), updates x, y, width & height to
    // exact position and size of enemy.
    public bool SelectEnemyAt (double x, double y)
    {
      if (ActiveEnemySet == null)
        return false;
      for (int index = 0; index < ActiveEnemySet.EnemyCount; index++)
      {
        Enemy e = ActiveEnemySet.Enemies [index];
        double width = e.MyEnemyType?.Graphics.Width ?? 1.0;
        double height = e.MyEnemyType?.Graphics.Height ?? 1.0;
        if (x >= e.PosX - width  / 2 && x < e.PosX + width  / 2 &&
            y >= e.PosY - height / 2 && y < e.PosY + height / 2)
        {
          HandlingSelection = true;
          var a = new ActiveItems (this);
          ForceSelectEnemy (index);
          RaiseChangeEvents (a);
          HandlingSelection = false;
          return true;
        }
      }
      return false;
    }

  } // partial class Project

}