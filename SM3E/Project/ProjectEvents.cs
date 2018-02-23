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

//========================================================================================
// Events.

    // Area/room/state/... list changes.
    public event ListLoadEventHandler AreaListChanged;
    public event ListLoadEventHandler RoomListChanged;
    public event ListLoadEventHandler RoomStateListChanged;
    public event ListLoadEventHandler DoorListChanged;
    public event ListLoadEventHandler PlmListChanged;
    public event ListLoadEventHandler PlmTypeListChanged;
    public event ListLoadEventHandler EnemyListChanged;
    public event ListLoadEventHandler EnemyGfxListChanged;
    public event ListLoadEventHandler EnemyTypeListChanged;
    public event ListLoadEventHandler ScrollDataListChanged;
    public event ListLoadEventHandler ScrollColorListChanged;
    public event ListLoadEventHandler TileSetListChanged;
    
    // New active area/room/state/... selected.
    public event EventHandler AreaSelected;
    public event EventHandler RoomSelected;
    public event EventHandler RoomStateSelected;
    public event EventHandler DoorSelected;
    public event EventHandler PlmSelected;
    public event EventHandler PlmTypeSelected;
    public event EventHandler EnemySelected;
    public event EventHandler EnemyGfxSelected;
    public event EventHandler EnemyTypeSelected;
    public event EventHandler ScrollDataSelected;
    public event EventHandler ScrollColorSelected;

    public event EventHandler LevelDataSelected;
    public event EventHandler TileSetSelected;
    public event EventHandler TileSelected;
    public event EventHandler BtsSelected;
    public event EventHandler MapTileSelected;
    public event EventHandler MapPaletteSelected;

    // Level data of the current room state is changed.
    public event LevelDataEventHandler LevelDataModified;
    public event EventHandler MapDataModified;
    public event EventHandler RoomDataModified;
    public event EventHandler RoomStateDataModified;
    public event EventHandler DoorDataModified;
    public event EventHandler PlmModified;
    public event EventHandler EnemyModified;
    public event EventHandler RoomPositionChanged;

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
    public bool AllScreens = false;
    public int ScreenXmin;
    public int ScreenXmax;
    public int ScreenYmin;
    public int ScreenYmax;

    public LevelDataEventArgs () {}
  }

}