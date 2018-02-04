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
    private string [] Areas = new string [AreaCount];
    private List <Data> [] Rooms = new List <Data> [AreaCount];
    private List <Data> DoorSets;
    private List <Data> Doors;
    private List <Data> RoomStates;
    private List <Data> ScrollSets;
    private List <Data> PlmSets;
    private List <Data> ScrollPlmDatas;
    private List <Data> Backgrounds;
    private List <Data> Fxs;
    private List <Data> SaveRooms;
    private List <Data> LevelDatas;
    private List <Data> EnemySets;
    private List <Data> EnemyGfxs;
    private List <Data> ScrollAsms;

    private List <Data> TileSets;
    private List <Data> TileTables;
    private List <Data> TileSheets;
    private List <Data> Palettes;
    private List <Data> AreaMaps;
    private List <PlmType> PlmTypes;
    private List <EnemyType> EnemyTypes;

    private Dictionary <string, List <Data>> DataLists;


//========================================================================================
// Properties


    // List of all scroll data associated with active room state.
    private List <IScrollData> ScrollDatas
    {
      get
      {
        var data = new List <IScrollData> ();
        if (ActiveRoomState == null)
          return data;
        if (ActiveRoomState.MyScrollSet != null)
          data.Add (ActiveRoomState.MyScrollSet);
        
        if (ActivePlmSet != null)
        {
          foreach (Plm p in ActivePlmSet.Plms)
            if (p.MyScrollPlmData != null && !data.Contains (p.MyScrollPlmData))
              data.Add (p.MyScrollPlmData);
        }

        if (ActiveRoom != null)
        {
          foreach (Door d in ActiveRoom.MyIncomingDoors)
            if (d.MyScrollAsm != null && !data.Contains (d.MyScrollAsm))
              data.Add (d.MyScrollAsm);
        }
        return data;
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
        return ActiveRoom?.Area ?? -1;
      }
      set
      {
        if (ActiveRoom != null && (byte) value != ActiveRoom.Area)
        {
          int newRoomIndex = NewRoomIndex (value);
          if (newRoomIndex != -1)
          {
            HandlingSelection = true;
            var a = new ActiveItems (this);

            Room CurrentRoom = ActiveRoom;
            int CurrentRoomStateIndex = RoomStateIndex;
            CurrentRoom.Area = (byte) value;
            CurrentRoom.RoomIndex = (byte) newRoomIndex;
            Rooms [AreaIndex].Remove (CurrentRoom);
            Rooms [value].Add (CurrentRoom);
            Rooms [value].Sort ((x, y) => ((Room) x).RoomIndex - ((Room) y).RoomIndex);
            ForceSelectArea (value);
            ForceSelectRoom (Rooms [value].FindIndex (x => x == CurrentRoom));
            ForceSelectRoomState (CurrentRoomStateIndex);

            RaiseChangeEvents (a);
            HandlingSelection = false;
          }
        }
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
        if (ActiveRoom != null && ActiveRoom.Name != value)
        {
          ActiveRoom.Name = value;
          RoomListChanged (this, new ListLoadEventArgs (RoomIndex));
        }
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
        if (ActiveRoom != null && ActiveRoom.UpScroller != (byte) value)
        {
          ActiveRoom.UpScroller = (byte) value;
          RoomDataModified?.Invoke (this, null);
        }
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
        if (ActiveRoom != null &&  ActiveRoom.DownScroller != (byte) value)
        {
          ActiveRoom.DownScroller = (byte) value;
          RoomDataModified?.Invoke (this, null);
        }
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
        if (ActiveRoom != null && ActiveRoom.SpecialGfxBitflag != (byte) value)
        {
          ActiveRoom.SpecialGfxBitflag = (byte) value;
          RoomDataModified?.Invoke (this, null);
        }
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
        if (RoomStateIndex != IndexNone &&
            ActiveRoom.RoomStateHeaders [RoomStateIndex].HeaderType != value)
        {
          ActiveRoom.RoomStateHeaders [RoomStateIndex].HeaderType = value;
          RoomStateDataModified?.Invoke (this, null);
        }
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
        if (RoomStateIndex != IndexNone &&
            ActiveRoom.RoomStateHeaders [RoomStateIndex].Value != (byte) value)
        {
          ActiveRoom.RoomStateHeaders [RoomStateIndex].Value = (byte) value;
          RoomStateDataModified?.Invoke (this, null);
        }
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
        if (ActiveRoomState != null && ActiveRoomState.SongSet != (byte) value)
        {
          ActiveRoomState.SongSet = (byte) value;
          RoomStateDataModified?.Invoke (this, null);
        }
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
        if (ActiveRoomState != null && ActiveRoomState.PlayIndex != (byte) value)
        {
          ActiveRoomState.PlayIndex = (byte) value;
          RoomStateDataModified?.Invoke (this, null);
        }
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
        if (ActiveRoomState != null && ActiveRoomState.BackgroundScrolling != value)
        {
          ActiveRoomState.BackgroundScrolling = value;
          RoomStateDataModified?.Invoke (this, null);
        }
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

    // Plm type
    public string PlmTypeName
    {
      get {return ActivePlmType?.Name ?? "<none>";}
    }

    public BlitImage PlmTypeImage
    {
      get {return ActivePlmType?.Graphics;}
    }

    // Enemy type
    public string EnemyTypeName
    {
      get {return ActiveEnemyType?.Name ?? "<none>";}
    }

    public BlitImage EnemyTypeImage
    {
      get {return ActiveEnemyType?.Graphics;}
    }

//---------------------------------------------------------------------------------------------------
// Name lists.

    // List of area names.
    public List <string> AreaNames
    {
      get {return Areas.ToList ();}
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
          foreach (RoomStateHeader r 
                   in ((Room) Rooms [AreaIndex] [RoomIndex]).RoomStateHeaders)
            names.Add (r.HeaderType.ToString ());
        return names;
      }
    }

    // List of outgoing door names for active room.
    public List <string> DoorNames
    {
      get
      {
        var names = new List <string> ();
        if (ActiveRoom != null && ActiveRoom.MyDoorSet != null)
        {
          for (int n = 0; n < ActiveRoom.MyDoorSet.DoorCount; n++)
          {
            Room destRoom = ActiveRoom.MyDoorSet.MyDoors [n].MyTargetRoom;
            names.Add (Tools.IntToHex (n) + " " + (destRoom?.Name ?? ""));
          }
        }
        return names;
      }
    }

    // List of Plm names for active room
    public List <string> PlmNames
    {
      get
      {
        var names = new List <string> ();
        if (ActivePlmSet != null)
        {
          for (int n = 0; n < ActivePlmSet.PlmCount; n++)
          {
            string newName = ActivePlmSet.Plms [n].MyPlmType?.Name ??
                             Tools.IntToHex (ActivePlmSet.Plms [n].PlmID);
            if (names.Contains (newName))
            {
              int i = 1;
              while (names.Contains (newName + " [" + i.ToString () + "]"))
                i++;
              newName += " [" + i.ToString () + "]";
            }
            names.Add (newName);
          }
        }
        return names;
      }
    }

    // List of Plm type names.
    public List <string> PlmTypeNames
    {
      get
      {
        var names = new List <string> ();
        for (int n = 0; n < PlmTypes.Count; n++)
        {
          names.Add (PlmTypes [n].Name);
        }
        return names;
      }
    }

    // List of enemy names for active room
    public List <string> EnemyNames
    {
      get
      {
        var names = new List <string> ();
        if (ActiveEnemySet != null)
        {
          for (int n = 0; n < ActiveEnemySet.EnemyCount; n++)
          {
            string newName = ActiveEnemySet.Enemies [n].MyEnemyType?.Name ??
                             Tools.IntToHex (ActiveEnemySet.Enemies [n].EnemyID);
            if (names.Contains (newName))
            {
              int i = 1;
              while (names.Contains (newName + " [" + i.ToString () + "]"))
                i++;
              newName += " [" + i.ToString () + "]";
            }
            names.Add (newName);
          }
        }
        return names;
      }
    }

    // List of enemy gfx names for active room
    public List <string> EnemyGfxNames
    {
      get
      {
        var names = new List <string> ();
        if (ActiveRoomState != null && ActiveRoomState.MyEnemyGfx != null)
        {
          for (int n = 0; n < ActiveRoomState.MyEnemyGfx.EnemyGfxCount; n++)
          {
            names.Add (ActiveRoomState.MyEnemyGfx.MyEnemyTypes [n]?.Name ??
                       Tools.IntToHex (ActiveRoomState.MyEnemyGfx.EnemyIDs [n]));
          }
        }
        return names;
      }
    }

    // List of enemy type names.
    public List <string> EnemyTypeNames
    {
      get
      {
        var names = new List <string> ();
        for (int n = 0; n < EnemyTypes.Count; n++)
        {
          names.Add (EnemyTypes [n].Name);
        }
        return names;
      }
    }

    // List of scroll data names.
    public List <string> ScrollDataNames
    {
      get
      {
        var names = new List <string> ();
        List <IScrollData> data = ScrollDatas;
        
        for (int n = 0; n < data.Count; n++)
        {
          switch (data [n])
          {
          case ScrollSet s:
            names.Add ("Room Scrolls");
            break;
          case ScrollPlmData s:
            names.Add ("PLM ($" + s.StartAddressPC + ")");
            break;
          case ScrollAsm s:
            names.Add ("ASM ($" + s.StartAddressPC + ")");
            break;
          default:
            break;
          }
        }
        /*
        if (ActivePlmSet != null)
        {
          foreach (Plm p in ActivePlmSet.Plms)
            if (p.MyScrollPlmData != null)
              names.Add ("PLM (" + p.PosX + "," + p.PosY + ")");
        }

        if (ActiveRoom != null)
        {
          foreach (Door d in ActiveRoom.MyIncomingDoors)
            if (d.MyScrollAsm != null)
              names.Add ("ASM (" + d.ScreenX + "," + d.ScreenY + ")");
        }*/
        return names;
      }
    }

    // List of scroll color names
    public List <string> ScrollColorNames
    {
      get
      {
        return new List <string> (new [] {"Red", "Green", "Blue", "Unchanged"});
      }
    }

//---------------------------------------------------------------------------------------------------
// Other.

    // Width of active room in tiles.
    public int RoomWidthInTiles
    {
      get {return ActiveRoom?.RoomW * 16 ?? 0;}
    }

    // Height of active room in tiles.
    public int RoomHeightInTiles
    {
      get {return ActiveRoom?.RoomH * 16 ?? 0;}
    }

    // Width of active room in Screens.
    public int RoomWidthInScreens
    {
      get {return ActiveRoom?.RoomW ?? 0;}
    }

    // Height of active room in Screens.
    public int RoomHeightInScreens
    {
      get {return ActiveRoom?.RoomH ?? 0;}
    }

    // Position of room on map
    public int RoomX
    {
      get {return ActiveRoom?.MapX ?? 0;}
    }
    public int RoomY
    {
      get {return ActiveRoom?.MapY ?? 0;}
    }


//========================================================================================
// Methods


    // Constructor.
    public Project ()
    {
      // Initialize data lists.
      for (int i = 0; i < AreaCount; i++)
        Rooms [i]    = new List <Data> ();
      DoorSets       = new List <Data> ();
      Doors          = new List <Data> ();
      RoomStates     = new List <Data> ();
      ScrollSets     = new List <Data> ();
      PlmSets        = new List <Data> ();
      ScrollPlmDatas = new List <Data> ();
      Backgrounds    = new List <Data> ();
      Fxs            = new List <Data> ();
      SaveRooms      = new List <Data> ();
      LevelDatas     = new List <Data> ();
      EnemySets      = new List <Data> ();
      EnemyGfxs      = new List <Data> ();
      ScrollAsms     = new List <Data> ();
      TileSets       = new List <Data> ();
      TileTables     = new List <Data> ();
      TileSheets     = new List <Data> ();
      Palettes       = new List <Data> ();
      AreaMaps       = new List <Data> ();
      PlmTypes       = new List <PlmType> ();
      EnemyTypes     = new List <EnemyType> ();

      // Add data lists to the DataLists dictionary.
      DataLists = new Dictionary <string, List <Data>> ();
      for (int i = 0; i < AreaCount; i++)
        DataLists.Add ("rooms" + i.ToString (), Rooms [i]);
      DataLists.Add ("doorsets"      , DoorSets      );
      DataLists.Add ("doors"         , Doors         );
      DataLists.Add ("scrollsets"    , ScrollSets    );
      DataLists.Add ("plmsets"       , PlmSets       );
      DataLists.Add ("scrollplmdatas", ScrollPlmDatas);
      DataLists.Add ("backgrounds"   , Backgrounds   );
      DataLists.Add ("fxs"           , Fxs           );
      DataLists.Add ("saverooms"     , SaveRooms     );
      DataLists.Add ("leveldatas"    , LevelDatas    );
      DataLists.Add ("enemysets"     , EnemySets     );
      DataLists.Add ("enemygfxs"     , EnemyGfxs     );
      DataLists.Add ("scrollasms"    , ScrollAsms    );
      DataLists.Add ("tilesets"      , TileSets      );
      DataLists.Add ("tiletables"    , TileTables    );
      DataLists.Add ("tilesheets"    , TileSheets    );
      DataLists.Add ("palettes"      , Palettes      );
      DataLists.Add ("areamaps"      , AreaMaps      );

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
      return ActiveRoomState.MyScrollSet [index];
    }


    public void SetScroll (int xMin, int yMin, int xMax, int yMax)
    {
      if (ActiveRoom == null || ActiveRoomState == null || ActiveScrollData == null)
        return;
      Tools.Order (ref xMin, ref xMax);
      Tools.Order (ref yMin, ref yMax);
      if (yMin < 0)
        yMin = 0;
      if (xMin < 0)
        xMin = 0;
      if (yMax >= RoomHeightInScreens)
        yMax = RoomHeightInScreens - 1;
      if (xMax >= RoomWidthInScreens)
        xMax = RoomWidthInScreens - 1;

      for (int x = xMin; x <= xMax; x++)
      {
        for (int y = yMin; y <= yMax; y++)
        {
          int index = ActiveRoom.RoomW * y + x;
          ActiveScrollData [index] = ActiveScrollColor;
        }
      }
      LevelDataEventArgs e = new LevelDataEventArgs ()
      {
        ScreenXmin = xMin,
        ScreenXmax = xMax,
        ScreenYmin = yMin,
        ScreenYmax = yMax
      };
      LevelDataModified?.Invoke (this, e);
    }


//========================================================================================
// Room State Data


  } // class Project

}
