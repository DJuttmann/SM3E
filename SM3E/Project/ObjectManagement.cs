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

    // Returns an unused room index for given area, or -1 if area is full.
    private int NewRoomIndex (int areaIndex)
    {
      List <int> roomIndices = new List <int> ();
      if (Rooms [areaIndex].Count >= 256)
        return -1;
      foreach (Room r in Rooms [areaIndex])
        roomIndices.Add (r.RoomIndex);
      roomIndices.Sort ();
      int index = 0;
      while (index < roomIndices.Count && roomIndices [index] == index)
        index++;
      return index;
    }


//========================================================================================
// Object management - exposed methods.

    
    public void AddRoom ()
    {
      if (AreaIndex == IndexNone)
        return;
      var a = new ActiveItems (this);
      if (ForceAddRoom (1, 1, 1, 1, "(new room)"))
      {
        HandlingSelection = true;
        ForceSelectRoom (Rooms [AreaIndex].Count - 1);
        RoomListChanged?.Invoke (this, new ListLoadEventArgs (RoomIndex));
        //RoomStateListChanged?.Invoke (this, new ListLoadEventArgs (RoomStateIndex));
        RaiseChangeEvents (a);
        HandlingSelection = false;
      }
    }


    public void DeleteRoom ()
    {
      if (AreaIndex == IndexNone)
        return;
      var a = new ActiveItems (this);
      if (ForceDeleteRoom ())
      {
        HandlingSelection = true;
        ForceSelectRoom (Math.Min (RoomIndex, Rooms [AreaIndex].Count - 1));
        RoomListChanged?.Invoke (this, new ListLoadEventArgs (RoomIndex));
        RaiseChangeEvents (a);
        HandlingSelection = false;
      }
    }


    public void AddDoor ()
    {
      if (ActiveRoom?.MyDoorSet == null)
        return;
      var a = new ActiveItems (this);
      if (ForceAddDoor ())
      {
        HandlingSelection = true;
        ForceSelectDoor (ActiveRoom.MyDoorSet.DoorCount - 1);
        DoorListChanged?.Invoke (this, new ListLoadEventArgs (DoorIndex));
        RaiseChangeEvents (a);
        HandlingSelection = false;
      }
    }


    public void DeleteDoor ()
    {
      if (ActiveRoom?.MyDoorSet == null || ActiveDoor == null)
        return;
      var a = new ActiveItems (this);
      if (ForceRemoveDoor ())
      {
        HandlingSelection = true;
        ForceSelectDoor (Math.Max (DoorIndex, ActiveRoom.MyDoorSet.DoorCount - 1));
        DoorListChanged?.Invoke (this, new ListLoadEventArgs (DoorIndex));
        RaiseChangeEvents (a);
        HandlingSelection = false;
      }
    }


    // Move selected room state up in list.
    public void MoveRoomStateUp ()
    {
      if (ActiveRoom == null || ActiveRoomState == null)
        return;
      if (RoomStateIndex > 0 && RoomStateIndex + 1 < ActiveRoom.RoomStates.Count)
      {
        var temp1 = ActiveRoom.RoomStates [RoomStateIndex];
        ActiveRoom.RoomStates [RoomStateIndex] =
          ActiveRoom.RoomStates [RoomStateIndex - 1];
        ActiveRoom.RoomStates [RoomStateIndex - 1] = temp1;

        var temp2 = ActiveRoom.RoomStateHeaders [RoomStateIndex];
        ActiveRoom.RoomStateHeaders [RoomStateIndex] =
          ActiveRoom.RoomStateHeaders [RoomStateIndex - 1];
        ActiveRoom.RoomStateHeaders [RoomStateIndex - 1] = temp2;

        RoomStateIndex--;
        HandlingSelection = true;
        RoomStateListChanged?.Invoke (this, new ListLoadEventArgs (RoomStateIndex));
        HandlingSelection = false;
      }
    }


    // Move selected room state down in list.
    public void MoveRoomStateDown ()
    {
      if (ActiveRoom == null || ActiveRoomState == null)
        return;
      if (RoomStateIndex + 2 < ActiveRoom.RoomStates.Count)
      {
        var temp1 = ActiveRoom.RoomStates [RoomStateIndex];
        ActiveRoom.RoomStates [RoomStateIndex] =
          ActiveRoom.RoomStates [RoomStateIndex + 1];
        ActiveRoom.RoomStates [RoomStateIndex + 1] = temp1;

        var temp2 = ActiveRoom.RoomStateHeaders [RoomStateIndex];
        ActiveRoom.RoomStateHeaders [RoomStateIndex] =
          ActiveRoom.RoomStateHeaders [RoomStateIndex + 1];
        ActiveRoom.RoomStateHeaders [RoomStateIndex + 1] = temp2;

        RoomStateIndex++;
        HandlingSelection = true;
        RoomStateListChanged?.Invoke (this, new ListLoadEventArgs (RoomStateIndex));
        HandlingSelection = false;
      }
    }


    // Make selected room state the standard room state.
    public void MakeRoomStateStandard ()
    {
      if (ActiveRoom == null || ActiveRoomState == null)
        return;
      int standardIndex = ActiveRoom.RoomStateCount - 1;
      if (RoomStateIndex == standardIndex)
        return;

      var temp = ActiveRoom.RoomStates [RoomStateIndex];
      ActiveRoom.RoomStates [RoomStateIndex] =
        ActiveRoom.RoomStates [standardIndex];
      ActiveRoom.RoomStates [standardIndex] = temp;

      RoomStateIndex = standardIndex;
      HandlingSelection = true;
      RoomStateListChanged?.Invoke (this, new ListLoadEventArgs (RoomStateIndex));
      RoomStateDataModified?.Invoke (this, null);
      HandlingSelection = false;
    }


    // Add new room state.
    public void AddRoomState ()
    {
      if (ActiveRoom == null)
        return;
      var a = new ActiveItems (this);
      if (ForceAddRoomState (StateType.Events))
      {
        HandlingSelection = true;
        ForceSelectRoomState (0);
        RoomStateListChanged?.Invoke (this, new ListLoadEventArgs (RoomStateIndex));
        var e = new LevelDataEventArgs ()
        {
          ScreenXmin = 0,
          ScreenXmax = RoomWidthInScreens - 1,
          ScreenYmin = 0,
          ScreenYmax = RoomHeightInScreens - 1
        };
        LevelDataModified?.Invoke (this, e);
        RaiseChangeEvents (a);
        HandlingSelection = false;
      }
    }


    // Delete active room state.
    public void DeleteRoomState ()
    {
      if (ActiveRoom == null || ActiveRoomState == null ||
          RoomStateIndex == ActiveRoom.RoomStates.Count - 1)
        return;
      var a = new ActiveItems (this);
      if (ForceDeleteRoomState ())
      {
        HandlingSelection = true;
        ForceSelectRoomState (RoomStateIndex);
        RoomStateListChanged?.Invoke (this, new ListLoadEventArgs (RoomStateIndex));
        var e = new LevelDataEventArgs ()
        {
          ScreenXmin = 0,
          ScreenXmax = RoomWidthInScreens - 1,
          ScreenYmin = 0,
          ScreenYmax = RoomHeightInScreens - 1
        };
        LevelDataModified?.Invoke (this, e);
        RaiseChangeEvents (a);
        HandlingSelection = false;
      }
    }


//----------------------------------------------------------------------------------------
// PLMs

    // Move selected PLM up in list.
    public void MovePlmUp ()
    {
      if (ActivePlmSet == null || ActivePlm == null)
        return;
      if (PlmIndex > 0)
      {
        Plm temp = ActivePlmSet.Plms [PlmIndex];
        ActivePlmSet.Plms [PlmIndex] = ActivePlmSet.Plms [PlmIndex - 1];
        ActivePlmSet.Plms [PlmIndex - 1] = temp;
        PlmIndex--;
        HandlingSelection = true;
        PlmListChanged?.Invoke (this, new ListLoadEventArgs (PlmIndex));
        HandlingSelection = false;
      }
    }


    // Move selected PLM down in list.
    public void MovePlmDown ()
    {
      if (ActivePlmSet == null || ActivePlm == null)
        return;
      if (PlmIndex + 1 < ActivePlmSet.PlmCount)
      {
        Plm temp = ActivePlmSet.Plms [PlmIndex];
        ActivePlmSet.Plms [PlmIndex] = ActivePlmSet.Plms [PlmIndex + 1];
        ActivePlmSet.Plms [PlmIndex + 1] = temp;
        PlmIndex++;
        HandlingSelection = true;
        PlmListChanged?.Invoke (this, new ListLoadEventArgs (PlmIndex));
        HandlingSelection = false;
      }
    }


    // Add a new PLM.
    public void AddPlm (int col, int row)
    {
      if (ActivePlmSet == null || ActivePlmType == null)
        return;
      if (ForceAddPlm (col, row))
      {
        HandlingSelection = true;
        var a = new ActiveItems (this);
        ForceSelectPlm (ActivePlmSet.PlmCount - 1);
        PlmListChanged?.Invoke (this, new ListLoadEventArgs (PlmIndex));
        var e = new LevelDataEventArgs ()
        {
          ScreenXmin = 0,
          ScreenXmax = RoomWidthInScreens - 1,
          ScreenYmin = 0,
          ScreenYmax = RoomHeightInScreens - 1
        };
        LevelDataModified?.Invoke (this, e);
        RaiseChangeEvents (a);
        HandlingSelection = false;
      }
    }


    // Delete the active PLM.
    public void DeletePlm ()
    {
      if (ActivePlmSet == null || ActivePlm == null)
        return;
      if (ForceDeletePlm ())
      {
        HandlingSelection = true;
        var a = new ActiveItems (this);
        int newIndex = Math.Min (PlmIndex, ActivePlmSet.PlmCount - 1);
        ForceSelectPlm (newIndex);
        PlmListChanged?.Invoke (this, new ListLoadEventArgs (PlmIndex));
        var e = new LevelDataEventArgs ()
        {
          ScreenXmin = 0,
          ScreenXmax = RoomWidthInScreens - 1,
          ScreenYmin = 0,
          ScreenYmax = RoomHeightInScreens - 1
        };
        LevelDataModified?.Invoke (this, e);
        RaiseChangeEvents (a);
        HandlingSelection = false;
      }
    }


    // Add scroll PLM data to active plm (if it is a scroll plm).
    public void AddScrollPlmData ()
    {
      if (ActivePlm.MyScrollPlmData == null)
        return;
      if (ForceAddScrollPlmData ())
      {
        HandlingSelection = true;
        int newIndex = ScrollDatas.FindIndex (x => x == ActivePlm.MyScrollPlmData);
        ForceSelectScrollData (newIndex);
        ScrollDataListChanged?.Invoke (this, new ListLoadEventArgs (newIndex));
        HandlingSelection = false;
      }
    }


    // Delete scroll PLM data for active plm (if it is a scroll plm).
    public void DeleteScrollPlmData ()
    {
      if (ActivePlm.MyScrollPlmData == null)
        return;
      if (ForceRemoveScrollPlmData ())
      {
        HandlingSelection = true;
        int newIndex = Math.Min (ScrollDataIndex, ScrollDataNames.Count - 1);
        ForceSelectScrollData (newIndex);
        ScrollDataListChanged?.Invoke (this, new ListLoadEventArgs (newIndex));
        HandlingSelection = false;
      }
    }

//----------------------------------------------------------------------------------------
// Enemies

    public void MoveEnemyUp ()
    {
      if (ActiveEnemySet == null || ActiveEnemy == null)
        return;
      if (EnemyIndex > 0)
      {
        Enemy temp = ActiveEnemySet.Enemies [EnemyIndex];
        ActiveEnemySet.Enemies [EnemyIndex] = ActiveEnemySet.Enemies [EnemyIndex - 1];
        ActiveEnemySet.Enemies [EnemyIndex - 1] = temp;
        EnemyIndex--;
        HandlingSelection = true;
        EnemyListChanged?.Invoke (this, new ListLoadEventArgs (EnemyIndex));
        HandlingSelection = false;
      }
    }


    public void MoveEnemyDown ()
    {
      if (ActiveEnemySet == null || ActiveEnemy == null)
        return;
      if (EnemyIndex + 1 < ActiveEnemySet.EnemyCount)
      {
        Enemy temp = ActiveEnemySet.Enemies [EnemyIndex];
        ActiveEnemySet.Enemies [EnemyIndex] = ActiveEnemySet.Enemies [EnemyIndex + 1];
        ActiveEnemySet.Enemies [EnemyIndex + 1] = temp;
        EnemyIndex++;
        HandlingSelection = true;
        EnemyListChanged?.Invoke (this, new ListLoadEventArgs (EnemyIndex));
        HandlingSelection = false;
      }
    }


    public void AddEnemy (int x, int y)
    {
      if (ActiveEnemySet == null)
        return;
      if (ForceAddEnemy (x, y))
      {
        HandlingSelection = true;
        var a = new ActiveItems (this);
        ForceSelectEnemy (ActiveEnemySet.EnemyCount - 1);
        EnemyListChanged?.Invoke (this, new ListLoadEventArgs (EnemyIndex));
        var e = new LevelDataEventArgs ()
        {
          ScreenXmin = 0,
          ScreenXmax = RoomWidthInScreens - 1,
          ScreenYmin = 0,
          ScreenYmax = RoomHeightInScreens - 1
        };
        LevelDataModified?.Invoke (this, e);
        RaiseChangeEvents (a);
        HandlingSelection = false;
      }
    }


    public void DeleteEnemy ()
    {
      if (ActiveEnemySet == null || ActiveEnemy == null)
        return;
      if (ForceDeleteEnemy ())
      {
        HandlingSelection = true;
        var a = new ActiveItems (this);
        int newIndex = Math.Min (EnemyIndex, ActiveEnemySet.EnemyCount - 1);
        ForceSelectEnemy (newIndex);
        EnemyListChanged?.Invoke (this, new ListLoadEventArgs (EnemyIndex));
        var e = new LevelDataEventArgs ()
        {
          ScreenXmin = 0,
          ScreenXmax = RoomWidthInScreens - 1,
          ScreenYmin = 0,
          ScreenYmax = RoomHeightInScreens - 1
        };
        LevelDataModified?.Invoke (this, e);
        RaiseChangeEvents (a);
        HandlingSelection = false;
      }
    }

//----------------------------------------------------------------------------------------

    public void MoveEnemyGfxUp ()
    {
      if (ActiveRoomState?.MyEnemyGfx == null || EnemyGfxIndex == IndexNone)
        return;
      if (EnemyGfxIndex > 0)
      {
        var temp1 = ActiveRoomState.MyEnemyGfx.EnemyIDs [EnemyGfxIndex];
        ActiveRoomState.MyEnemyGfx.EnemyIDs [EnemyGfxIndex] =
          ActiveRoomState.MyEnemyGfx.EnemyIDs [EnemyGfxIndex - 1];
        ActiveRoomState.MyEnemyGfx.EnemyIDs [EnemyGfxIndex - 1] = temp1;

        var temp2 = ActiveRoomState.MyEnemyGfx.Palettes [EnemyGfxIndex];
        ActiveRoomState.MyEnemyGfx.Palettes [EnemyGfxIndex] =
          ActiveRoomState.MyEnemyGfx.Palettes [EnemyGfxIndex - 1];
        ActiveRoomState.MyEnemyGfx.Palettes [EnemyGfxIndex - 1] = temp2;

        var temp3 = ActiveRoomState.MyEnemyGfx.MyEnemyTypes [EnemyGfxIndex];
        ActiveRoomState.MyEnemyGfx.MyEnemyTypes [EnemyGfxIndex] =
          ActiveRoomState.MyEnemyGfx.MyEnemyTypes [EnemyGfxIndex - 1];
        ActiveRoomState.MyEnemyGfx.MyEnemyTypes [EnemyGfxIndex - 1] = temp3;

        EnemyGfxIndex--;
        HandlingSelection = true;
        EnemyGfxListChanged?.Invoke (this, new ListLoadEventArgs (EnemyGfxIndex));
        HandlingSelection = false;
      }
    }


    public void MoveEnemyGfxDown ()
    {
      if (ActiveRoomState?.MyEnemyGfx == null || EnemyGfxIndex == IndexNone)
        return;
      if (EnemyGfxIndex + 1 < ActiveRoomState.MyEnemyGfx.EnemyGfxCount)
      {
        var temp1 = ActiveRoomState.MyEnemyGfx.EnemyIDs [EnemyGfxIndex];
        ActiveRoomState.MyEnemyGfx.EnemyIDs [EnemyGfxIndex] =
          ActiveRoomState.MyEnemyGfx.EnemyIDs [EnemyGfxIndex + 1];
        ActiveRoomState.MyEnemyGfx.EnemyIDs [EnemyGfxIndex + 1] = temp1;

        var temp2 = ActiveRoomState.MyEnemyGfx.Palettes [EnemyGfxIndex];
        ActiveRoomState.MyEnemyGfx.Palettes [EnemyGfxIndex] =
          ActiveRoomState.MyEnemyGfx.Palettes [EnemyGfxIndex + 1];
        ActiveRoomState.MyEnemyGfx.Palettes [EnemyGfxIndex + 1] = temp2;

        var temp3 = ActiveRoomState.MyEnemyGfx.MyEnemyTypes [EnemyGfxIndex];
        ActiveRoomState.MyEnemyGfx.MyEnemyTypes [EnemyGfxIndex] =
          ActiveRoomState.MyEnemyGfx.MyEnemyTypes [EnemyGfxIndex + 1];
        ActiveRoomState.MyEnemyGfx.MyEnemyTypes [EnemyGfxIndex + 1] = temp3;

        EnemyGfxIndex++;
        HandlingSelection = true;
        EnemyGfxListChanged?.Invoke (this, new ListLoadEventArgs (EnemyGfxIndex));
        HandlingSelection = false;
      }
    }


    public void AddEnemyGfx ()
    {
      if (ActiveRoomState?.MyEnemyGfx == null)
        return;
      if (ForceAddEnemyGfx ())
      {
        HandlingSelection = true;
        ForceSelectEnemyGfx (ActiveRoomState.MyEnemyGfx.EnemyGfxCount - 1);
        EnemyGfxListChanged?.Invoke (this, new ListLoadEventArgs (EnemyIndex));
        HandlingSelection = false;
      }
    }


    public void DeleteEnemyGfx ()
    {
      if (ActiveRoomState?.MyEnemyGfx == null || EnemyGfxIndex == IndexNone)
        return;
      if (ForceDeleteEnemyGfx ())
      {
        HandlingSelection = true;
        var a = new ActiveItems (this);
        int newIndex = Math.Min (EnemyGfxIndex,
                                 ActiveRoomState.MyEnemyGfx.EnemyGfxCount - 1);
        ForceSelectEnemyGfx (newIndex);
        EnemyGfxListChanged?.Invoke (this, new ListLoadEventArgs (EnemyGfxIndex));
        RaiseChangeEvents (a);
        HandlingSelection = false;
      }
    }


//========================================================================================
// Object management - internal methods.


    // Add room to active area.
    private bool ForceAddRoom (int mapX, int mapY, int width, int height, string name)
    {
      if (AreaIndex == IndexNone)
        return false;
      int newRoomIndex = NewRoomIndex (AreaIndex);
      if (newRoomIndex == -1)
        return false;
      DoorSet newDoorSet = new DoorSet ();
      newDoorSet.SetDefault ();

      Room newRoom = new Room ();
      newRoom.SetDefault ();
      newRoom.MapX = (byte) mapX;
      newRoom.MapY = (byte) mapY;
      newRoom.Width = (byte) width;
      newRoom.Height = (byte) height;
      newRoom.Name = name;
      newRoom.MyDoorSet = newDoorSet;

      Rooms [AreaIndex].Add (newRoom);
      DoorSets.Add (newDoorSet);
      ForceSelectRoom (Rooms [AreaIndex].Count - 1);
      ForceAddRoomState (StateType.Standard);
      return true;
    }


    // Delete selected room from active area.
    private bool ForceDeleteRoom ()
    {
      if (AreaIndex == IndexNone || ActiveRoom == null)
        return false;
      ForceDeleteDoorSet ();
      foreach (Door d in ActiveRoom.MyIncomingDoors)
        d.MyTargetRoom = null;
      while (ActiveRoom.RoomStates.Count > 0)
      {
        ForceSelectRoomState (ActiveRoom.RoomStates.Count - 1);
        ForceDeleteRoomState ();
      }
      Rooms [AreaIndex].Remove (ActiveRoom);
      return true;
    }


    // Delete doorset from currently active room.
    private bool ForceDeleteDoorSet ()
    {
      if (ActiveRoom?.MyDoorSet == null)
        return false;
      ActiveRoom.MyDoorSet.MyRoom = null;
      while (ActiveRoom.MyDoorSet.MyDoors.Count > 0)
      {
        ForceSelectDoor (ActiveRoom.MyDoorSet.MyDoors.Count - 1);
        ForceRemoveDoor ();
      }
      DoorSets.Remove (ActiveRoom.MyDoorSet);
      ActiveRoom.MyDoorSet = null;
      return true;
    }


    // Add door to currently active room's doorset.
    private bool ForceAddDoor ()
    {
      if (ActiveRoom?.MyDoorSet == null)
        return false;
      Door newDoor = new Door ();
      newDoor.SetDefault ();
      newDoor.MyDoorSets.Add (ActiveRoom.MyDoorSet);
      ActiveRoom.MyDoorSet.MyDoors.Add (newDoor);
      ActiveRoom.MyDoorSet.DoorPtrs.Add (0);
      return true;
    }


    // Remove door from currently active room's doorset.
    // Delete alltogether if it is not referenced by any other doorset.
    private bool ForceRemoveDoor ()
    {
      if (ActiveRoom?.MyDoorSet == null || ActiveDoor == null)
        return false;
      ActiveDoor.MyDoorSets.Remove (ActiveRoom.MyDoorSet);
      if (ActiveDoor.MyDoorSets.Count == 0)
        DeleteData (ActiveDoor);
      ActiveRoom.MyDoorSet.MyDoors.Remove (ActiveDoor);
      ActiveRoom.MyDoorSet.DoorPtrs.RemoveAt (DoorIndex);
      return true;
    }

    
    // Add room state to active room.
    private bool ForceAddRoomState (StateType type)
    {
      if (ActiveRoom == null)
        return false;
      RoomStateHeader newHeader = new RoomStateHeader ();
      newHeader.SetDefault ();
      newHeader.HeaderType = type;

      var newState = new RoomState ();
      var newLevelData = new LevelData (RoomWidthInScreens, RoomHeightInScreens);
      var newPlmSet = new PlmSet ();
      var newEnemySet = new EnemySet ();
      var newEnemyGfx = new EnemyGfx ();
      var newFx = new Fx ();

      newState.SetDefault ();
      newLevelData.SetDefault ();
      newPlmSet.SetDefault ();
      newEnemySet.SetDefault (); 
      newEnemyGfx.SetDefault (); 
      newFx.SetDefault ();

      newState.SetLevelData (newLevelData, out var ignore1);
      newState.SetScrollSet (null, out var ignore2);
      newState.SetPlmSet (newPlmSet, out var ignore3);
      newState.SetEnemySet (newEnemySet, out var ignore4);
      newState.SetEnemyGfx (newEnemyGfx, out var ignore5);
      newState.SetFx (newFx, out var ignore6);
      newState.SetBackground (null, out var ignore7);
      newState.SetSetupAsm (null, out var ignore8);
      newState.SetMainAsm (null, out var ignore9);

      ActiveRoom.RoomStateHeaders.Insert (0, newHeader);
      ActiveRoom.RoomStates.Insert (0, newState);
      LevelDatas.Add (newState.MyLevelData);
      PlmSets.Add (newState.MyPlmSet);
      EnemySets.Add (newState.MyEnemySet);
      EnemyGfxs.Add (newState.MyEnemyGfx);
      Fxs.Add (newState.MyFx);
      return true;
    }


    // Delete selected room state from active room.
    private bool ForceDeleteRoomState ()
    {
      if (ActiveRoom == null || ActiveRoomState == null)
        return false;
      ForceRemoveLevelData ();
      ForceRemoveScrollSet ();
      ForceRemovePlmSet ();
      ForceRemoveEnemySet ();
      ForceRemoveEnemyGfx ();
      ForceRemoveFx ();
      ForceRemoveBackground ();
      
      ActiveRoomState.MyRoom = null;
      ActiveRoom.RoomStateHeaders.RemoveAt (RoomStateIndex);
      ActiveRoom.RoomStates.RemoveAt (RoomStateIndex);
      return true;
    }


    // Remove level data from currently active room state.
    // Delete alltogether if it is not referenced by any other room state.
    private bool ForceRemoveLevelData ()
    {
      if (ActiveRoomState == null || ActiveLevelData == null)
        return false;
      ActiveLevelData.MyRoomStates.Remove (ActiveRoomState);
      if (ActiveLevelData.MyRoomStates.Count == 0)
        LevelDatas.Remove (ActiveLevelData);
      ActiveRoomState.MyLevelData = null;
      return true;
    }


    // Remove scroll set from currently active room state.
    // Delete alltogether if it is not referenced by any other room state.
    private bool ForceRemoveScrollSet ()
    {
      if (ActiveRoomState == null || ActiveRoomState.MyScrollSet == null)
        return false;
      ActiveRoomState.MyScrollSet.MyRoomStates.Remove (ActiveRoomState);
      if (ActiveRoomState.MyScrollSet.MyRoomStates.Count == 0)
        ScrollSets.Remove (ActiveRoomState.MyScrollSet);
      ActiveRoomState.MyScrollSet = null;
      return true;
    }


    // Remove PLM set from currently active room state.
    // Delete alltogether if it is not referenced by any other room state.
    private bool ForceRemovePlmSet ()
    {
      if (ActiveRoomState == null || ActivePlmSet == null)
        return false;
      ActivePlmSet.MyRoomStates.Remove (ActiveRoomState);
      if (ActivePlmSet.MyRoomStates.Count == 0)
      {
        while (ActivePlmSet.PlmCount > 0)
        {
          ForceSelectPlm (ActivePlmSet.PlmCount - 1);
          ForceDeletePlm ();
        }
        PlmSets.Remove (ActivePlmSet);
      }
      ActiveRoomState.MyPlmSet = null;
      return true;
    }


    // Remove enemy set from currently active room state.
    // Delete alltogether if it is not referenced by any other room state.
    private bool ForceRemoveEnemySet ()
    {
      if (ActiveRoomState == null || ActiveEnemySet == null)
        return false;
      ActiveEnemySet.MyRoomStates.Remove (ActiveRoomState);
      if (ActiveEnemySet.MyRoomStates.Count == 0)
      {
        while (ActiveEnemySet.EnemyCount > 0)
        {
          ForceSelectEnemy (ActiveEnemySet.EnemyCount - 1);
          ForceDeleteEnemy ();
        }
        EnemySets.Remove (ActiveEnemySet);
      }
      ActiveRoomState.MyEnemySet = null;
      return true;
    }


    // Remove enemy gfx from currently active room state.
    // Delete alltogether if it is not referenced by any other room state.
    private bool ForceRemoveEnemyGfx ()
    {
      if (ActiveRoomState == null || ActiveRoomState.MyEnemyGfx == null)
        return false;
      ActiveRoomState.MyEnemyGfx.MyRoomStates.Remove (ActiveRoomState);
      if (ActiveRoomState.MyEnemyGfx.MyRoomStates.Count == 0)
        EnemyGfxs.Remove (ActiveRoomState.MyEnemyGfx);
      ActiveRoomState.MyEnemyGfx = null;
      return true;
    }


    // Remove fx from currently active room state.
    // Delete alltogether if it is not referenced by any other room state.
    private bool ForceRemoveFx ()
    {
      // [wip] implement ActiveFx and replace in this method?
      if (ActiveRoomState == null || ActiveRoomState.MyFx == null)
        return false;
      ActiveRoomState.MyFx.MyRoomStates.Remove (ActiveRoomState);
      if (ActiveRoomState.MyFx.MyRoomStates.Count == 0)
        Fxs.Remove (ActiveRoomState.MyFx);
      ActiveRoomState.MyFx = null;
      return true;
    }


    // Remove background from currently active room state.
    private bool ForceRemoveBackground ()
    {
      // [wip] implement ActiveBackground and replace in this method?
      if (ActiveRoomState == null || ActiveRoomState.MyBackground == null)
        return false;
      ActiveRoomState.MyBackground.MyRoomStates.Remove (ActiveRoomState);
      // Do not delete background if unreferenced
      ActiveRoomState.MyBackground = null;
      return true;
    }

//----------------------------------------------------------------------------------------

    private bool ForceAddPlm (int col, int row)
    {
      if (ActivePlmSet == null || ActivePlmType == null)
        return false;
      Plm newPlm = new Plm ();
      newPlm.SetDefault ();
      newPlm.PlmID = ActivePlmType.PlmID;
      newPlm.MyPlmType = ActivePlmType;
      newPlm.PosX = (byte) col;
      newPlm.PosY = (byte) row;
      ActivePlmSet.Plms.Add (newPlm);
      return true;
    }


    private bool ForceDeletePlm ()
    {
      if (ActivePlmSet == null || ActivePlm == null)
        return false;
      ForceRemoveScrollPlmData ();
      ActivePlmSet.Plms.RemoveAt (PlmIndex);
      return true;
    }


    private bool ForceAddScrollPlmData ()
    {
      if (ActivePlm.PlmID == Plm.ScrollID && ActivePlm.MyScrollPlmData == null)
      {
        ScrollPlmData newData = new ScrollPlmData ();
        newData.MyPlms.Add (ActivePlm);
        ScrollPlmDatas.Add (newData);
        ActivePlm.MyScrollPlmData = null;
        return true;
      }
      return false;
    }


    // Remove scroll PLM data from currently active PLM (if it is scroll PLM).
    // Delete alltogether if it is not referenced by any other PLM.
    private bool ForceRemoveScrollPlmData ()
    {
      if (ActivePlm?.MyScrollPlmData != null)
      {
        ActivePlm.MyScrollPlmData.MyPlms.Remove (ActivePlm);
        if (ActivePlm.MyScrollPlmData.MyPlms.Count == 0)
          ScrollPlmDatas.Remove (ActivePlm.MyScrollPlmData);
        ActivePlm.MyScrollPlmData = null;
        return true;
      }
      return false;
    }

    
    private bool ForceAddEnemy (int x, int y)
    {
      if (ActiveEnemySet == null || ActiveEnemyType == null)
        return false;
      Enemy newEnemy = new Enemy ();
      newEnemy.SetDefault ();
      newEnemy.EnemyID = ActiveEnemyType.EnemyID;
      newEnemy.MyEnemyType = ActiveEnemyType;
      newEnemy.PosX = x;
      newEnemy.PosY = y;
      ActiveEnemySet.Enemies.Add (newEnemy);
      return true;
    }


    private bool ForceDeleteEnemy ()
    {
      if (ActiveEnemySet == null || ActiveEnemy == null)
        return false;
      ActiveEnemySet.Enemies.RemoveAt (EnemyIndex);
      return true;
    }


    private bool ForceAddEnemyGfx ()
    {
      if (ActiveRoomState?.MyEnemyGfx == null || ActiveEnemyType == null ||
          ActiveRoomState.MyEnemyGfx.EnemyIDs.Contains (ActiveEnemyType.EnemyID) ||
          ActiveRoomState.MyEnemyGfx.EnemyIDs.Count >= 4)
        return false;
      ActiveRoomState.MyEnemyGfx.EnemyIDs.Add (ActiveEnemyType.EnemyID);
      ActiveRoomState.MyEnemyGfx.Palettes.Add (0x0000);
      ActiveRoomState.MyEnemyGfx.MyEnemyTypes.Add (ActiveEnemyType);
      return true;
    }


    private bool ForceDeleteEnemyGfx ()
    {
      if (ActiveRoomState?.MyEnemyGfx == null || EnemyGfxIndex == IndexNone)
        return false;
      ActiveRoomState.MyEnemyGfx.EnemyIDs.RemoveAt (EnemyGfxIndex);
      ActiveRoomState.MyEnemyGfx.Palettes.RemoveAt (EnemyGfxIndex);
      ActiveRoomState.MyEnemyGfx.MyEnemyTypes.RemoveAt (EnemyGfxIndex);
      return true;
    }


//========================================================================================
// Object management - tools.


    private void DeleteData (Data data)
    {
      if (data is IReferenceable refdata)
        refdata.DetachAllReferences ();
      switch (data)
      {
      case Room d:
        // Rooms         .Remove (d); [wip] // implement delete method for ListArray <T>
        break;
      case DoorSet d:
        DoorSets      .Remove (d);
        break;
      case Door d:
        Doors         .Remove (d);
        break;
      case ScrollSet d:
        ScrollSets    .Remove (d);
        break;
      case PlmSet d:
        PlmSets       .Remove (d);
        break;
      case ScrollPlmData d:
        ScrollPlmDatas.Remove (d);
        break;
      case Background d:
        Backgrounds   .Remove (d);
        break;
      case Fx d:
        Fxs           .Remove (d);
        break;
      case SaveRoom d:
        SaveRooms     .Remove (d);
        break;
      case LevelData d:
        LevelDatas    .Remove (d);
        break;
      case EnemySet d:
        EnemySets     .Remove (d);
        break;
      case EnemyGfx d:
        EnemyGfxs     .Remove (d);
        break;
      case ScrollAsm d:
        ScrollAsms    .Remove (d);
        break;
      case Asm d:
        DoorAsms      .Remove (d);
        SetupAsms     .Remove (d);
        MainAsms      .Remove (d);
        break;
      case TileSet d:
        TileSets      .Remove (d);
        break;
      case TileTable d:
        TileTables    .Remove (d);
        break;
      case TileSheet d:
        TileSheets    .Remove (d);
        break;
      case Palette d:
        Palettes      .Remove (d);
        break;
      case AreaMap d:
        AreaMaps      .Remove (d);
        break;
      default:
        break;
      }
    }

  } // partial class Project

}
