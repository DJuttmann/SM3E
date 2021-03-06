﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;


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


  [AttributeUsage (AttributeTargets.Field, AllowMultiple = false)]
  public class RegisteredData: Attribute
  {
    public readonly string Name;

    public RegisteredData (string name)
    {
      Name = name;
    }
  }


//========================================================================================
// CLASS PROJECT
//========================================================================================


  public partial class Project
  {
    // Consts
    public const int IndexNone = -1;
    public const int AreaCount = 8;


    // Fields
    private string [] Areas = new string [AreaCount];

    [RegisteredData ("rooms")]
    private ListArray <Data> Rooms;

    [RegisteredData ("doorsets")]
    private List <Data> DoorSets;

    [RegisteredData ("doors")]
    private List <Data> Doors;
    
    [RegisteredData ("scrollsets")]
    private List <Data> ScrollSets;

    [RegisteredData ("plmsets")]
    private List <Data> PlmSets;

    [RegisteredData ("scrollplmdatas")]
    private List <Data> ScrollPlmDatas;

    [RegisteredData ("backgrounds")]
    private List <Data> Backgrounds;

    [RegisteredData ("fxs")]
    private List <Data> Fxs;

    [RegisteredData ("savestations")]
    private List <Data> SaveStations;

    [RegisteredData ("leveldatas")]
    private List <Data> LevelDatas;

    [RegisteredData ("enemysets")]
    private List <Data> EnemySets;

    [RegisteredData ("enemygfxs")]
    private List <Data> EnemyGfxs;

    [RegisteredData ("scrollasms")]
    private List <Data> ScrollAsms;

    [RegisteredData ("doorasms")]
    private List <Data> DoorAsms;

    [RegisteredData ("setupasms")]
    private List <Data> SetupAsms;

    [RegisteredData ("mainasms")]
    private List <Data> MainAsms;

    [RegisteredData ("tilesets")]
    private List <Data> TileSets;

    [RegisteredData ("tiletables")]
    private List <Data> TileTables;

    [RegisteredData ("tilesheets")]
    private List <Data> TileSheets;

    [RegisteredData ("palettes")]
    private List <Data> Palettes;

    [RegisteredData ("areamaps")]
    private List <Data> AreaMaps;

    private List <PlmType> PlmTypes;

    private List <EnemyType> EnemyTypes;

    private Dictionary <string, IEnumerable <Data>> DataLists;

//----------------------------------------------------------------------------------------

    // Constructor.
    public Project ()
    {
      Rooms          = new ListArray <Data> (AreaCount);
      DoorSets       = new List <Data> ();
      Doors          = new List <Data> ();
      ScrollSets     = new List <Data> ();
      PlmSets        = new List <Data> ();
      ScrollPlmDatas = new List <Data> ();
      Backgrounds    = new List <Data> ();
      Fxs            = new List <Data> ();
      SaveStations      = new List <Data> ();
      LevelDatas     = new List <Data> ();
      EnemySets      = new List <Data> ();
      EnemyGfxs      = new List <Data> ();
      ScrollAsms     = new List <Data> ();
      DoorAsms       = new List <Data> ();
      SetupAsms      = new List <Data> ();
      MainAsms       = new List <Data> ();
      TileSets       = new List <Data> ();
      TileTables     = new List <Data> ();
      TileSheets     = new List <Data> ();
      Palettes       = new List <Data> ();
      AreaMaps       = new List <Data> ();
      PlmTypes       = new List <PlmType> ();
      EnemyTypes     = new List <EnemyType> ();

      // Add data lists to the DataLists dictionary.
      DataLists = new Dictionary <string, IEnumerable <Data>> ();
      FieldInfo [] fields = typeof (Project).GetFields (BindingFlags.NonPublic | 
                                                        BindingFlags.Instance);
      foreach (FieldInfo f in fields)
      {
        var attribute = f.GetCustomAttribute (typeof (RegisteredData)) as RegisteredData;
        if (attribute != null)
          DataLists.Add (attribute.Name, (IEnumerable <Data>) f.GetValue (this));
      }

      // Load Resources.
      LoadBtsTiles ();
    }

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

    // Type/h-flip/v-flip/palette/... of selected map tile
    private int mapTileType = 0;
    public int MapTileType
    {
      get {return mapTileType;}
      set
      {
        if (value >= 0 && value < 256 && value != mapTileType)
        {
          mapTileType = value;
          MapTileSelected?.Invoke (this, null);
        }
      }
    }
    private bool mapTileHFlip = false;
    public bool MapTileHFlip
    {
      get {return mapTileHFlip;}
      set
      {
        if (value != mapTileHFlip)
        {
          mapTileHFlip = value;
          MapTileSelected?.Invoke (this, null);
        }
      }
    }
    private bool mapTileVFlip = false;
    public bool MapTileVFlip
    {
      get {return mapTileVFlip;}
      set
      {
        if (value != mapTileVFlip)
        {
          mapTileVFlip = value;
          MapTileSelected?.Invoke (this, null);
        }
      }
    }
    private int mapTilePalette = 0;
    public int MapTilePalette
    {
      get {return mapTilePalette;}
      set
      {
        if (value >= 0 && value < 8 && value != mapTilePalette)
        {
          mapTilePalette = value;
          MapPaletteSelected?.Invoke (this, null);
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
            ChangesMade = true;
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
          HandlingSelection = true;
          RoomListChanged (this, new ListLoadEventArgs (RoomIndex));
          HandlingSelection = false;
          ChangesMade = true;
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
          ChangesMade = true;
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
          ChangesMade = true;
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
          ChangesMade = true;
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
        if (HandlingSelection || ActiveRoom == null || RoomStateIndex == IndexNone)
          return;
        StateType currentType = ActiveRoom.RoomStateHeaders [RoomStateIndex].HeaderType;
        if (currentType != StateType.Standard && currentType != value)
        {
          if (value == StateType.Standard)
            MakeRoomStateStandard ();
          else
          {
            ActiveRoom.RoomStateHeaders [RoomStateIndex].HeaderType = value;
            HandlingSelection = true;
            RoomStateDataModified?.Invoke (this, null);
            RoomStateListChanged?.Invoke (this, new ListLoadEventArgs (RoomStateIndex));
            HandlingSelection = false;
          }
          ChangesMade = true;
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
          ChangesMade = true;
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
          ChangesMade = true;
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
          ChangesMade = true;
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
          ChangesMade = true;
        }
      }
    }

    public int RoomStateTileSet
    {
      set
      {
        if (ActiveRoomState != null && value >= 0 && value < TileSets.Count &&
            value != TileSetIndex)
        {
          ActiveRoomState.TileSet = (byte) value;
          LoadRoomTiles (value);
          LoadBackground ();
          RoomStateDataModified?.Invoke (this, null);
          var e = new LevelDataEventArgs ()
          {
            ScreenXmin = 0,
            ScreenXmax = RoomWidthInScreens - 1,
            ScreenYmin = 0,
            ScreenYmax = RoomHeightInScreens - 1
          };
          LevelDataModified?.Invoke (this, e);
          ChangesMade = true;
        }
      }
    }

    // Room state pointers
    public int LevelDataPtr
    {
      get {return ActiveRoomState?.MyLevelData?.StartAddressLR ?? 0;}
    }

    public int ScrollSetPtr
    {
      get {return ActiveRoomState?.MyScrollSet?.StartAddressLR ?? 0;}
    }

    public int PlmSetPtr
    {
      get {return ActiveRoomState?.MyPlmSet?.StartAddressLR ?? 0;}
    }

    public int EnemySetPtr
    {
      get {return ActiveRoomState?.MyEnemySet?.StartAddressLR ?? 0;}
    }

    public int EnemyGfxPtr
    {
      get {return ActiveRoomState?.MyEnemyGfx?.StartAddressLR ?? 0;}
    }

    public int BackgroundPtr
    {
      get {return ActiveRoomState?.MyBackground?.StartAddressLR ?? 0;}
    }

    public int FxPtr
    {
      get {return ActiveRoomState?.MyFx?.StartAddressLR ?? 0;}
    }

    public int SetupAsmPtr
    {
      get {return ActiveRoomState?.MySetupAsm?.StartAddressLR ?? 0;}
    }

    public int MainAsmPtr
    {
      get {return ActiveRoomState?.MyMainAsm?.StartAddressLR ?? 0;}
    }
    
    // PLM
    public int PlmID
    {
      get {return ActivePlm?.PlmID ?? 0;}
    }

    // PLM type
    public string PlmTypeName
    {
      get {return ActivePlmType?.Name ?? "<none>";}
    }

    public int PlmTypeID
    {
      get {return ActivePlmType?.PlmID ?? 0;}
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

    public int EnemyTypeID
    {
      get {return ActiveEnemyType?.EnemyID ?? 0;}
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
            names.Add (r.Name);
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
            string name = Tools.IntToHex (n) + " ";
            Door d = ActiveRoom.MyDoorSet.MyDoors [n];
            if (d.ElevatorPad)
              name += "[Elevator pad]";
            else
              name += d.MyTargetRoom?.Name ?? "";
            names.Add (name);
          }
        }
        return names;
      }
    }

    // List of incoming door names for active room.
    public List <string> IncomingDoorNames
    {
      get
      {
        var names = new List <string> ();
        if (ActiveRoom != null)
        {
          foreach (Door d in ActiveRoom.MyIncomingDoors)
          {
            if (d.MyDoorSets.Count > 0)
            {
              DoorSet doorset =  d.MyDoorSets.First <DoorSet> ();
              string name = "Door ";
              name += Tools.IntToHex (doorset.MyDoors.FindIndex (x => x == d));
              name += " " + d.MyDoorSets.First <DoorSet> ().MyRoom.Name;
              names.Add (name);
            }
            else
              names.Add ("[unused door]");
          }
        }
        return names;
      }
    }
    
    // List of Plm names for active room state.
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
            names.Add ("PLM ($" + Tools.IntToHex (s.StartAddressLR) + ")");
            break;
          case ScrollAsm s:
            names.Add ("ASM ($" + Tools.IntToHex (s.StartAddressLR) + ")");
            break;
          default:
            break;
          }
        }
        return names;
      }
    }


    // List of scroll color names
    public List <string> ScrollColorNames
    {
      get
      {
        return new List <string> (new [] {"Red", "Blue", "Green", "Unchanged"});
      }
    }


    // List of background names.
    public List <string> BackgroundNames
    {
      get
      {
        var names = new List <string> ();
        foreach (Background b in Backgrounds)
          names.Add (b.Name);
        return names;
      }
    }


    // List of Setup ASM names.
    public List <string> SetupAsmNames
    {
      get
      {
        var names = new List <string> ();
        foreach (Asm a in SetupAsms)
          names.Add (a.Name);
        return names;
      }
    }


    // List of Main ASM names.
    public List <string> MainAsmNames
    {
      get
      {
        var names = new List <string> ();
        foreach (Asm a in MainAsms)
          names.Add (a.Name);
        return names;
      }
    }


    // List of tileset names.
    public List <string> TileSetNames
    {
      get
      {
        var names = new List <string> ();
        for (int i = 0; i < TileSets.Count; i++)
          names.Add (Tools.IntToHex (i, 2));
        return names;
      }
    }


    // List of Room state pointer names
    public List <string> PointerNames
    {
      get
      {
        List <string> names = new List <string> ();

        if (ActiveRoomState?.MyLevelData != null)
          names.Add ("Data $" + Tools.IntToHex (LevelDataPtr));
        else
          names.Add (String.Empty);

        if (ActiveRoomState?.MyScrollSet != null)
          names.Add ("Data $" + Tools.IntToHex (ScrollSetPtr));
        else if (ActiveRoomState?.ScrollSetPtr == ScrollSet.AllBlue)
          names.Add ("All blue");
        else if (ActiveRoomState?.ScrollSetPtr == ScrollSet.AllGreen)
          names.Add ("All green");
        else
          names.Add ("[unknown]");

        if (ActiveRoomState?.MyPlmSet != null)
          names.Add ("Data $" + Tools.IntToHex (PlmSetPtr));
        else
          names.Add (String.Empty);

        if (ActiveRoomState?.MyEnemySet != null)
          names.Add ("Data $" + Tools.IntToHex (EnemySetPtr));
        else
          names.Add (String.Empty);

        if (ActiveRoomState?.MyEnemyGfx != null)
          names.Add ("Data $" + Tools.IntToHex (EnemyGfxPtr));
        else
          names.Add (String.Empty);

        if (ActiveRoomState?.MyBackground != null)
          names.Add (ActiveRoomState.MyBackground.Name);
        else if (ActiveRoomState?.MyLevelData?.HasLayer2 ?? false)
          names.Add ("Layer 2");
        else
          names.Add (String.Empty);

        if (ActiveRoomState?.MyFx != null)
          names.Add ("Data $" + Tools.IntToHex (FxPtr));
        else
          names.Add (String.Empty);

        if (ActiveRoomState?.MySetupAsm != null)
          names.Add (ActiveRoomState.MySetupAsm.Name);
        else
          names.Add (String.Empty);

        if (ActiveRoomState?.MyMainAsm != null)
          names.Add (ActiveRoomState.MyMainAsm.Name);
        else
          names.Add (String.Empty);

        return names;
      }
    }


    // List of Room state pointer reference counts
    public int [] PointerReferenceCounts
    {
      get
      {
        return new int [] { 
          ActiveLevelData?.ReferenceCount ?? 0,
          ActiveRoomState?.MyScrollSet?.ReferenceCount ?? 0,
          ActivePlmSet?.ReferenceCount ?? 0,
          ActiveEnemySet?.ReferenceCount ?? 0,
          ActiveRoomState?.MyEnemyGfx?.ReferenceCount ?? 0,
          ActiveRoomState?.MyBackground?.ReferenceCount ?? 0,
          ActiveRoomState?.MyFx?.ReferenceCount ?? 0,
          ActiveRoomState?.MySetupAsm?.ReferenceCount ?? 0,
          ActiveRoomState?.MyMainAsm?.ReferenceCount ?? 0,
          };
      }
    }


    // List of regular door ASM names
    public List <string> DoorAsmNames
    {
      get
      {
        var names = new List <string> ();
        foreach (Asm a in DoorAsms)
          names.Add (a.Name);
        return names;
      }
    }


    // Door ASM name
    public string DoorAsmName
    {
      get
      {
        if (ActiveDoor == null)
          return String.Empty;
        if (ActiveDoor.MyScrollAsm != null)
          return "Scroll $" + Tools.IntToHex (ActiveDoor.MyScrollAsm.StartAddressLR);
        if (ActiveDoor.MyDoorAsm != null)
          return ActiveDoor.MyDoorAsm.Name;
        return String.Empty;
      }
    }


    // Fx data names
    public List <string> FxDataNames
    {
      get
      {
        var names = new List <string> ();
        if (ActiveFx != null)
        {
          foreach (Door d in ActiveFx.FxDoors)
          {
            if (d == null)
              names.Add ("[Default]");
            else if (d.MyDoorSets.Count > 0)
            {
              DoorSet doorset =  d.MyDoorSets.First <DoorSet> ();
              string name = "Door ";
              name += Tools.IntToHex (doorset.MyDoors.FindIndex (x => x == d));
              name += " " + d.MyDoorSets.First <DoorSet> ().MyRoom.Name;
              names.Add (name);
            }
            else
              names.Add ("[Unused door]");
          }
        }
        return names;
      }
    }

//---------------------------------------------------------------------------------------------------
// Other.

    // Width of active room in tiles.
    public int RoomWidthInTiles
    {
      get {return ActiveRoom?.Width * 16 ?? 0;}
    }

    // Height of active room in tiles.
    public int RoomHeightInTiles
    {
      get {return ActiveRoom?.Height * 16 ?? 0;}
    }

    // Width of active room in Screens.
    public int RoomWidthInScreens
    {
      get {return ActiveRoom?.Width ?? 0;}
    }

    // Height of active room in Screens.
    public int RoomHeightInScreens
    {
      get {return ActiveRoom?.Height ?? 0;}
    }

    // Position of room on map
    public int RoomX
    {
      get {return ActiveRoom?.MapX ?? 0;}
      set
      {
        if (ActiveRoom != null && value >= 0 && value < 64)
        {
          ActiveRoom.MapX = (byte) value;
          RoomPositionChanged?.Invoke (this, null);
          ChangesMade = true;
        }
      }
    }
    public int RoomY
    {
      get {return ActiveRoom?.MapY ?? 0;}
      set
      {
        if (ActiveRoom != null && value >= 0 && value < 32)
        {
          ActiveRoom.MapY = (byte) value;
          RoomPositionChanged?.Invoke (this, null);
          ChangesMade = true;
        }
      }
    }

  } // class Project

}
