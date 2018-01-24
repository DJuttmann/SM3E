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
    
    // Active area/room/state/... changes.
    public event EventHandler AreaSelected;
    public event EventHandler RoomSelected;
    public event EventHandler RoomStateSelected;
    public event EventHandler LevelDataSelected;
    public event EventHandler TileSetSelected;
    public event EventHandler TileSelected;
    public event EventHandler BtsSelected;

    // Level data of the current room state is changed.
    public event EventHandler LevelDataModified;

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

}