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

//----------------------------------------------------------------------------------------

    // Move selected PLM up in list.
    public void MovePlmUp ()
    {
      if (ActivePlmSet == null || ActivePlm == null)
        return;
      if (PlmIndex > 0)
      {
        Plm temp = ActivePlmSet.Plms [PlmIndex];
        ActivePlmSet.Plms [PlmIndex] =  ActivePlmSet.Plms [PlmIndex - 1];
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
        ActivePlmSet.Plms [PlmIndex] =  ActivePlmSet.Plms [PlmIndex + 1];
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
      if (ForceDeleteScrollPlmData ())
      {
        HandlingSelection = true;
        int newIndex = Math.Min (ScrollDataIndex, ScrollDataNames.Count - 1);
        ForceSelectScrollData (newIndex);
        ScrollDataListChanged?.Invoke (this, new ListLoadEventArgs (newIndex));
        HandlingSelection = false;
      }
    }

//----------------------------------------------------------------------------------------

    public void MoveEnemyUp ()
    {
      if (ActiveEnemySet == null || ActiveEnemy == null)
        return;
      if (EnemyIndex > 0)
      {
        Enemy temp = ActiveEnemySet.Enemies [EnemyIndex];
        ActiveEnemySet.Enemies [EnemyIndex] =  ActiveEnemySet.Enemies [EnemyIndex - 1];
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
        ActiveEnemySet.Enemies [EnemyIndex] =  ActiveEnemySet.Enemies [EnemyIndex + 1];
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
        int newIndex = Math.Min (EnemyGfxIndex,
                                 ActiveRoomState.MyEnemyGfx.EnemyGfxCount - 1);
        ForceSelectEnemyGfx (newIndex);
        EnemyGfxListChanged?.Invoke (this, new ListLoadEventArgs (EnemyGfxIndex));
        HandlingSelection = false;
      }
    }


//========================================================================================
// Object management without UI concerns.


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
      ForceDeleteScrollPlmData ();
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


    private bool ForceDeleteScrollPlmData ()
    {
      if (ActivePlm?.MyScrollPlmData != null)
      {
        ActivePlm.MyScrollPlmData.MyPlms.Remove (ActivePlm);
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


  } // partial class Project

}
