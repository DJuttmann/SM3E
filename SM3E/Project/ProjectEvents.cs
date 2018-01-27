﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;


namespace SM3E
{

  partial class Project
  {

//========================================================================================
// Events.

    // Area/room/state/... list changes.
    public event ListLoadEventHandler AreaListChanged;
    public event ListLoadEventHandler RoomListChanged;
    public event ListLoadEventHandler RoomStateListChanged;
    public event ListLoadEventHandler DoorListChanged;
    public event ListLoadEventHandler PlmListChanged;
    
    // Active area/room/state/... changes.
    public event EventHandler AreaSelected;
    public event EventHandler RoomSelected;
    public event EventHandler RoomStateSelected;
    public event EventHandler DoorSelected;
    public event EventHandler PlmSelected;
    public event EventHandler LevelDataSelected;
    public event EventHandler TileSetSelected;
    public event EventHandler TileSelected;
    public event EventHandler BtsSelected;

    // Level data of the current room state is changed.
    public event LevelDataEventHandler LevelDataModified;

    // RoomState data changed
    public event EventHandler RoomStateModified;

    // Room data changed
    public event EventHandler RoomModified;

  }


//========================================================================================
// Delegates / EventArgs


  // Delegate for selection events in the project.
  public delegate void ListLoadEventHandler (object sender, ListLoadEventArgs e);

  // Contains suggested items to select in the loaded list;
  public class ListLoadEventArgs: EventArgs
  {
    public int SelectItem;

    public ListLoadEventArgs () {}

    public ListLoadEventArgs (int selectItem)
    {
      SelectItem = selectItem;
    }
  }


  // Delegate for level data changing.
  public delegate void LevelDataEventHandler (object sender, LevelDataEventArgs e);

  // Contains affected screens.
  public class LevelDataEventArgs: EventArgs
  {
    public int ScreenXmin;
    public int ScreenXmax;
    public int ScreenYmin;
    public int ScreenYmax;

    public LevelDataEventArgs () {}
  }

}