﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;


namespace SM3E
{

  public partial class Project
  {

//========================================================================================
// Getters & Setters

    
    // List of room names for an area.
    public List <string> GetRoomNames (int areaIndex)
    {
      var names = new List <string> ();
      if (areaIndex >= 0 && areaIndex < AreaCount)
        foreach (Room r in Rooms [areaIndex])
          names.Add (r.Name);
      return names;
    }


    // List of room state names for given room in area.
    public List <string> GetRoomStateNames (int areaIndex, int roomIndex)
    {
      var names = new List <string> ();
      if (areaIndex >= 0 && areaIndex < AreaCount &&
          roomIndex >= 0 && roomIndex < Rooms [areaIndex].Count)
        foreach (RoomStateHeader h in ((Room) Rooms [areaIndex] [roomIndex]).RoomStateHeaders)
          names.Add (h.Name);
      return names;
    }


    // List of room state names for given room in area.
    public List <string> GetDoorNames (int areaIndex, int roomIndex)
    {
      var names = new List <string> ();
      if (areaIndex >= 0 && areaIndex < AreaCount &&
          roomIndex >= 0 && roomIndex < Rooms [areaIndex].Count)
      {
        DoorSet s = ((Room) Rooms [areaIndex] [roomIndex]).MyDoorSet;
        for (int n = 0; n < s.DoorCount; n++)
        {
          string name = Tools.IntToHex (n) + " ";
          Door d = s.MyDoors [n];
          if (d.ElevatorPad)
            name += "[Elevator pad]";
          else
            name += d.MyTargetRoom?.Name ?? "";
          names.Add (name);
        }
      }
      return names;
    }


    // List of PLM names for ginen room state.
    public List <string> GetScrollPlmNames (int areaIndex, int roomIndex, int stateIndex)
    {
      var names = new List <string> ();
      RoomState s = IndexToRoomState (areaIndex, roomIndex, stateIndex);
      if (s != null && s.MyPlmSet != null)
      {
        foreach (Plm p in s.MyPlmSet.Plms)
        {
          if (p.PlmID == Plm.ScrollID)
          names.Add (String.Format ("Scroll PLM at ({0},{1})",
                                    Tools.IntToHex (p.PosX), Tools.IntToHex (p.PosY)));
        }
      }
      return names;
    }


    public void GetRoomSizeInScreens (int areaIndex, int roomIndex,
                                      out int width, out int height)
    {
      width = 0;
      height = 0;
      if (areaIndex >= 0 && areaIndex < AreaCount &&
          roomIndex >= 0 && roomIndex < Rooms [areaIndex].Count)
      {
        Room r = (Room) Rooms [areaIndex] [roomIndex];
        width = r.Width;
        height = r.Height;
      } 
    }


    public void SetRoomSize (int x, int y, int width, int height)
    {
      if (ActiveRoom != null &&
          x >= 0 && x < 64 && y >= 0 && y < 32 &&
          width > 0 && width < 16 && height > 0 && height < 16 && 
          width * height <= 50)
      {
        var resizedData = new HashSet <Data> ();
        int dx = x - ActiveRoom.MapX;
        int dy = y - ActiveRoom.MapY;
        foreach (RoomState s in ActiveRoom.RoomStates)
        {
          if (!resizedData.Contains (s.MyLevelData))
          {
            s.MyLevelData?.Resize (ActiveRoom.Width, dx, dy, width, height);
            resizedData.Add (s.MyLevelData);
          }
          if (!resizedData.Contains (s.MyPlmSet))
          {
            s.MyPlmSet?.Shift (dx * -16, dy * -16);
            resizedData.Add (s.MyPlmSet);
          }
          if (!resizedData.Contains (s.MyEnemySet))
          {
            s.MyEnemySet?.Shift (dx * -256, dy * -256);
            resizedData.Add (s.MyEnemySet);
          }
          if (!resizedData.Contains (s.MyScrollSet))
          {
            s.MyScrollSet?.Resize (ActiveRoom.Width, dx, dy, width, height);
            resizedData.Add (s.MyScrollSet);
          }
          foreach (Plm p in s.MyPlmSet.Plms)
          {
            if (!resizedData.Contains (p.MyScrollPlmData))
            {
              p.MyScrollPlmData?.Resize (ActiveRoom.Width, ActiveRoom.Height,
                                         dx, dy, width, height);
              resizedData.Add (p.MyScrollPlmData);
            }
          }
        }
        foreach (Door d in ActiveRoom.MyIncomingDoors)
        {
          if (!resizedData.Contains (d.MyScrollAsm))
          {
            d.MyScrollAsm?.Resize (ActiveRoom.Width, ActiveRoom.Height,
                                   dx, dy, width, height);
            resizedData.Add (d.MyScrollAsm);
          }
        }
        ActiveRoom.MapX = (byte) x;
        ActiveRoom.MapY = (byte) y;
        ActiveRoom.Width = (byte) width;
        ActiveRoom.Height = (byte) height;
        RoomPositionChanged?.Invoke (this, null);
        LevelDataSelected (this, null); // stronger than modified because of size change
        ChangesMade = true;
      }
    }


//========================================================================================
// Level data

    // Check if room and room state are active.
    private bool ActiveLevelDataInfo (out LevelData data, out int width)
    {
      //if (roomIndex != IndexNone && roomStateIndex != IndexNone && 
      //    ActiveRoomState.MyLevelData != null)
      if (ActiveLevelData != null)
      {
        data = ActiveLevelData; // ActiveRoomState.MyLevelData;
        width = ActiveRoom.Width * 16;
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


    public void SelectLayer1 (int col, int row)
    {
      if (ActiveLevelData != null)
      {
        TileIndex = GetLayer1Tile (row, col);
        TileHFlip = GetLayer1HFlip (row, col);
        TileVFlip = GetLayer1VFlip (row, col);
        TileSelected?.Invoke (this, null);
      }
    }


    public void SelectLayer2 (int col, int row)
    {
      if (ActiveLevelData?.HasLayer2 == true)
      {
        TileIndex = GetLayer2Tile (row, col);
        TileHFlip = GetLayer2HFlip (row, col);
        TileVFlip = GetLayer2VFlip (row, col);
        TileSelected?.Invoke (this, null);
      }
    }


    public void SelectBts (int col, int row)
    {
      if (ActiveLevelData != null)
      {
        BtsType = GetBtsType (row, col);
        BtsValue = GetBtsValue (row, col);
        BtsSelected?.Invoke (this, null);
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
      ChangesMade = true;
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
      ChangesMade = true;
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
      ChangesMade = true;
    }


//========================================================================================
// Scrolls


    public ScrollColor GetScroll (int x, int y)
    {
      if (ActiveRoom == null || ActiveRoomState == null)
        return ScrollColor.None;
      if (ActiveRoomState.MyScrollSet == null)
      {
        switch (ActiveRoomState.ScrollSetPtr)
        {
        case ScrollSet.AllBlue:
          return ScrollColor.Blue;
        case ScrollSet.AllGreen:
          return ScrollColor.Green;
        default:
          return ScrollColor.None;
        }
      }
      int index = ActiveRoom.Width * y + x;
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
          int index = ActiveRoom.Width * y + x;
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
      ChangesMade = true;
    }


//========================================================================================
// PLMs and Enemies

    
    // Return position and size of selected PLM
    public void GetPlmPosition (out int x, out int y, out int width, out int height)
    {
      x = 0;
      y = 0;
      width = 0;
      height = 0;
      if (ActivePlm != null)
      {
        x = ActivePlm.PosX;
        y = ActivePlm.PosY;
        width  = ActivePlm.MyPlmType?.Graphics.Width  / 16 ?? 1;
        height = ActivePlm.MyPlmType?.Graphics.Height / 16 ?? 1;
      }
    }


    public void SetPlmPosition (int x, int y)
    {
      if (ActivePlm == null)
        return;
      ActivePlm.PosX = (byte) x;
      ActivePlm.PosY = (byte) y;
      LevelDataModified?.Invoke (this, new LevelDataEventArgs () {AllScreens = true});
      PlmModified?.Invoke (this, null);
      ChangesMade = true;
    }


    public void SetPlmPositionRelative (int dx, int dy)
    {
      SetPlmPosition (ActivePlm.PosX + dx, ActivePlm.PosY + dy);
    }


    // Get properties if PLM is not a scroll PLM.
    public void GetPlmProperties (out int properties, out int index)
    {
      properties = 0;
      index = 0;
      if (ActivePlm != null && ActivePlm.PlmID != Plm.ScrollID)
      {
        properties = (ActivePlm.MainVariable >> 8) & 0xFF;
        index = ActivePlm.MainVariable & 0xFF;;
      }
    }


    // Set properties if PLM is not a scroll PLM.
    public void SetPlmProperties (int properties, int index)
    {
      if (ActivePlm != null && ActivePlm.PlmID != Plm.ScrollID)
      {
        byte b0 = (byte) index;
        byte b1 = (byte) properties;
        ActivePlm.MainVariable = Tools.ConcatBytes (b0, b1);
        ChangesMade = true;
      }
    }


    // Return position and size of selected Enemy (in tiles, output is double)
    public void GetEnemyPosition (out double x, out double y,
                                  out double width, out double height)
    {
      x = 0.0;
      y = 0.0;
      width = 0.0;
      height = 0.0;
      if (ActiveEnemy != null)
      {
        width  = ActiveEnemy.MyEnemyType?.Graphics.Width  / 16.0 ?? 1.0;
        height = ActiveEnemy.MyEnemyType?.Graphics.Height / 16.0 ?? 1.0;
        x = ActiveEnemy.PosX / 16.0;
        y = ActiveEnemy.PosY / 16.0;
      }
    }


    public void SetEnemyPosition (double x, double y)
    {
      if (ActiveEnemy == null)
        return;
      ActiveEnemy.PosX = (int) Math.Round (x * 16.0);
      ActiveEnemy.PosY = (int) Math.Round (y * 16.0);
      LevelDataModified?.Invoke (this, new LevelDataEventArgs () {AllScreens = true});
      EnemyModified?.Invoke (this, null);
      ChangesMade = true;
    }


    public void SetEnemyPositionRelative (double dx, double dy)
    {
      if (ActiveEnemy == null)
        return;
      ActiveEnemy.PosX += (int) Math.Round (dx * 16.0);
      ActiveEnemy.PosY += (int) Math.Round (dy * 16.0);
      LevelDataModified?.Invoke (this, new LevelDataEventArgs () {AllScreens = true});
      EnemyModified?.Invoke (this, null);
      ChangesMade = true;
    }


    // Return size of selected Enemy in pixels.
    public void GetEnemyPixelPosition (out int x, out int y)
    {
      x = ActiveEnemy?.PosX ?? 0;
      y = ActiveEnemy?.PosY ?? 0;
    }


    // Return size of selected Enemy in pixels.
    public void SetEnemyPixelPosition (int x, int y)
    {
      if (ActiveEnemy != null)
      {
        ActiveEnemy.PosX = x;
        ActiveEnemy.PosY = y;
        LevelDataModified?.Invoke (this, new LevelDataEventArgs () {AllScreens = true});
        EnemyModified?.Invoke (this, null);
        ChangesMade = true;
      }
    }


    public void GetEnemyProperties (out int special, out int graphics, out int tilemaps,
                                    out int speed, out int speed2)
    {
      special = ActiveEnemy?.Special ?? 0;
      graphics = ActiveEnemy?.Graphics ?? 0;
      tilemaps = ActiveEnemy?.Tilemaps ?? 0;
      speed = ActiveEnemy?.Speed ?? 0;
      speed2 = ActiveEnemy?.Speed2 ?? 0;
    }


    public void SetEnemyProperties (int special, int graphics, int tilemaps,
                                    int speed, int speed2)
    {
      if (ActiveEnemy == null)
        return;
      ActiveEnemy.Special = special;
      ActiveEnemy.Graphics = graphics;
      ActiveEnemy.Tilemaps = tilemaps;
      ActiveEnemy.Speed = speed;
      ActiveEnemy.Speed2 = speed2;
      ChangesMade = true;
    }


    public EnemyGfxPalette GetEnemyGfxPalette ()
    {
      EnemyGfx e = ActiveRoomState?.MyEnemyGfx;
      if (e != null && EnemyGfxIndex >= 0 && EnemyGfxIndex < e.EnemyGfxCount)
      {
        return ((EnemyGfxPalette) e.Palettes [EnemyGfxIndex]);
      }
      return EnemyGfxPalette.None;
    }


    public void SetEnemyGfxPalette (EnemyGfxPalette palette)
    {
      EnemyGfx e = ActiveRoomState?.MyEnemyGfx;
      if (e != null && EnemyGfxIndex >= 0 && EnemyGfxIndex < e.EnemyGfxCount)
      {
        e.Palettes [EnemyGfxIndex] = (int) palette;
        ChangesMade = true;
      }
    }


//========================================================================================
// Effects


    public void GetFxData (out int surfaceStart,
                           out int surfaceNew,
                           out int surfaceSpeed,
                           out int surfaceDelay,
                           out FxType fxType,
                           out int fxBitA,
                           out int fxBitB,
                           out int liquidOptions,
                           out int paletteOptions,
                           out int animationOptions,
                           out int paletteBlend)
    {
      if (ActiveFxData != null)
      {
        surfaceStart          = ActiveFxData.LiquidSurfaceStart;
        surfaceNew            = ActiveFxData.LiquidSurfaceNew;
        surfaceSpeed          = ActiveFxData.LiquidSurfaceSpeed;
        surfaceDelay          = ActiveFxData.LiquidSurfaceDelay;
        fxType                = (FxType) ActiveFxData.FxType;
        fxBitA                = ActiveFxData.FxBitA;
        fxBitB                = ActiveFxData.FxBitB;
        liquidOptions         = ActiveFxData.FxBitC;
        paletteOptions        = ActiveFxData.PaletteFxBitflags;
        animationOptions      = ActiveFxData.TileAnimationBitflags;
        paletteBlend          = ActiveFxData.PaletteBlend;
      }
      else
      {
        surfaceStart          = 0;
        surfaceNew            = 0;
        surfaceSpeed          = 0;
        surfaceDelay          = 0;
        fxType                = 0;
        fxBitA                = 0;
        fxBitB                = 0;
        liquidOptions         = 0;
        paletteOptions        = 0;
        animationOptions      = 0;
        paletteBlend          = 0;
      }
    }

    
    public void SetFxSurfaceStart (int surfaceStart)
    {
      if (HandlingSelection || ActiveFxData == null)
        return;
      ActiveFxData.LiquidSurfaceStart = surfaceStart;
      FxDataModified?.Invoke (this, null);
      ChangesMade = true;
    }


    public void SetFxSurfaceNew (int surfaceNew)
    {
      if (HandlingSelection || ActiveFxData == null)
        return;
      ActiveFxData.LiquidSurfaceNew = surfaceNew;
      FxDataModified?.Invoke (this, null);
      ChangesMade = true;
    }


    public void SetFxSurfaceSpeed (int surfaceSpeed)
    {
      if (HandlingSelection || ActiveFxData == null)
        return;
      ActiveFxData.LiquidSurfaceSpeed = surfaceSpeed;
      FxDataModified?.Invoke (this, null);
      ChangesMade = true;
    }


    public void SetFxSurfaceDelay (int surfaceDelay)
    {
      if (HandlingSelection || ActiveFxData == null)
        return;
      ActiveFxData.LiquidSurfaceDelay = (byte) surfaceDelay;
      FxDataModified?.Invoke (this, null);
      ChangesMade = true;
    }


    public void SetFxType (FxType fxType)
    {
      if (HandlingSelection || ActiveFxData == null)
        return;
      ActiveFxData.FxType = (byte) fxType;
      FxDataModified?.Invoke (this, null);
      ChangesMade = true;
    }


    public void SetFxBitA (int fxBitA)
    {
      if (HandlingSelection || ActiveFxData == null)
        return;
      ActiveFxData.FxBitA = (byte) fxBitA;
      FxDataModified?.Invoke (this, null);
      ChangesMade = true;
    }


    public void SetFxBitB (int fxBitB)
    {
      if (HandlingSelection || ActiveFxData == null)
        return;
      ActiveFxData.FxBitB = (byte) fxBitB;
      FxDataModified?.Invoke (this, null);
      ChangesMade = true;
    }


    public void SetFxLiquidOptions (int liquidOptions)
    {
      if (HandlingSelection || ActiveFxData == null)
        return;
      ActiveFxData.FxBitC = (byte) liquidOptions;
      FxDataModified?.Invoke (this, null);
      ChangesMade = true;
    }


    public void SetFxPaletteOptions (int paletteOptions)
    {
      if (HandlingSelection || ActiveFxData == null)
        return;
      ActiveFxData.PaletteFxBitflags = (byte) paletteOptions;
      FxDataModified?.Invoke (this, null);
      ChangesMade = true;
    }


    public void SetFxAnimationOptions (int animationOptions)
    {
      if (HandlingSelection || ActiveFxData == null)
        return;
      ActiveFxData.TileAnimationBitflags = (byte) animationOptions;
      FxDataModified?.Invoke (this, null);
      ChangesMade = true;
    }


    public void SetFxPaletteBlend (int paletteBlend)
    {
      if (HandlingSelection || ActiveFxData == null)
        return;
      ActiveFxData.PaletteBlend = (byte) paletteBlend;
      FxDataModified?.Invoke (this, null);
      ChangesMade = true;
    }


//========================================================================================
// Doors


    public void GetDoorDestination (out int areaIndex, out int roomIndex,
                                    out int screenX, out int screenY,
                                    out int doorCapX, out int doorCapY,
                                    out int distanceToSpawn)
    {
      Room r = ActiveDoor?.MyTargetRoom;
      areaIndex = r?.Area ?? IndexNone;
      if (areaIndex != IndexNone)
        roomIndex = Rooms [areaIndex].FindIndex (x => x == r);
      else
        roomIndex = IndexNone;
      screenX = ActiveDoor?.ScreenX ?? 0;
      screenY = ActiveDoor?.ScreenY ?? 0;
      doorCapX = ActiveDoor?.DoorCapX ?? 0;
      doorCapY = ActiveDoor?.DoorCapY ?? 0;
      distanceToSpawn = ActiveDoor?.DistanceToSpawn ?? 0;
    }


    public void GetDoorProperties (out bool isElevator, out bool isElevatorPad,
                                   out int direction, out bool closes)
    {
      isElevator = ActiveDoor?.GetElevatorBit () ?? false;
      isElevatorPad = ActiveDoor?.ElevatorPad ?? false;
      direction = ActiveDoor?.GetDirection () ?? 0;
      closes = ActiveDoor?.GetDoorCloses () ?? false;
    }


    public void SetDoorDestination (int areaIndex, int roomIndex,
                                    int screenX, int screenY,
                                    int doorCapX, int doorCapY,
                                    int distanceToSpawn)
    {
      if (ActiveDoor == null)
        return;
      if (areaIndex >= 0 && areaIndex < AreaCount &&
          roomIndex >= 0 && roomIndex < Rooms [areaIndex].Count)
        ActiveDoor.MyTargetRoom = (Room) Rooms [areaIndex] [roomIndex];
      else
        ActiveDoor.MyTargetRoom = null;
      ActiveDoor.SetAreaTransitionBit (areaIndex != AreaIndex);
      ActiveDoor.ScreenX = (byte) screenX;
      ActiveDoor.ScreenY = (byte) screenY;
      ActiveDoor.DoorCapX = (byte) doorCapX;
      ActiveDoor.DoorCapY = (byte) doorCapY;
      ActiveDoor.DistanceToSpawn = distanceToSpawn;
      HandlingSelection = true;
      DoorListChanged?.Invoke (this, new ListLoadEventArgs (DoorIndex));
      HandlingSelection = false;
      ChangesMade = true;
    }


    public void SetDoorProperties (bool isElevator, bool isElevatorPad,
                                   int direction, bool closes)
    {
      if (ActiveDoor == null)
        return;
      ActiveDoor.SetElevatorBit (isElevator);
      ActiveDoor.ElevatorPad = isElevatorPad;
      ActiveDoor.SetDirection (direction);
      ActiveDoor.SetDoorCloses (closes);
      ChangesMade = true;
    }


//========================================================================================
// Map


    // Get the active map tile.
    public BlitImage GetMapTile ()
    {
      return MapTiles [256 * MapTilePalette + MapTileType];
    }


    // Set the map tile at position (x, y) to the currently selected map tile.
    public void SetMapTile (int xMin, int yMin, int xMax, int yMax)
    {
      if (ActiveAreaMap == null)
        return;
      Tools.Order (ref xMin, ref xMax);
      Tools.Order (ref yMin, ref yMax);
      for (int y = yMin; y <= yMax; y++)
      {
        for (int x = xMin; x <= xMax; x++)
        {
          int index = 64 * y + x; 
          ActiveAreaMap.SetTile (index, MapTileType);
          ActiveAreaMap.SetHFlip (index, MapTileHFlip);
          ActiveAreaMap.SetVFlip (index, MapTileVFlip);
          ActiveAreaMap.SetPalette (index, MapTilePalette);
        }
      }
      MapDataModified?.Invoke (this, null);
      ChangesMade = true;
    }


    public void SelectMapTile (int x, int y)
    {
      if (AreaIndex != IndexNone)
      {
        var map = (AreaMap) AreaMaps [AreaIndex];
        int index = 64 * y + x;
        mapTileType = map.GetTile (index);
        mapTileHFlip = map.GetHFlip (index);
        mapTileVFlip = map.GetVFlip (index);
        mapTilePalette = map.GetPalette (index);
        MapTileSelected?.Invoke (this, null);
        MapPaletteSelected?.Invoke (this, null);
      }
    }


//========================================================================================
// Room Pointers


    public void GetBackgroundStatus (out bool HasBackground, out bool HasLayer2)
    {
      HasBackground = ActiveRoomState?.MyBackground != null;
      HasLayer2 = ActiveLevelData?.HasLayer2 ?? false;
    }


    public void SetBackgroundStatus (int index, bool HasLayer2)
    {
      if (ActiveRoomState == null)
        return;

      bool backgroundChanged = false;
      if (BackgroundIndex != index)
      {
        Background target = index >= 0 && index < SetupAsms.Count ? 
                            (Background) Backgrounds [index] : null;
        ActiveRoomState.SetBackground (target, out var deleteBackground);
        backgroundChanged = true;
      }
      
      if (ActiveLevelData != null && HasLayer2 != ActiveLevelData.HasLayer2)
      {
        ActiveLevelData.HasLayer2 = HasLayer2;
        backgroundChanged = true;
      }

      if (backgroundChanged)
      {
        LoadBackground ();
        var e = new LevelDataEventArgs ()
        {
          ScreenXmin = 0,
          ScreenXmax = RoomWidthInScreens - 1,
          ScreenYmin = 0,
          ScreenYmax = RoomHeightInScreens - 1
        };
        LevelDataModified?.Invoke (this, e);
        RoomStateDataModified?.Invoke (this, null);
        ChangesMade = true;
      }
    }


    public void SetSetupAsm (int index)
    {
      if (ActiveRoomState == null || index < -1 || index >= SetupAsms.Count ||
          index == SetupAsmIndex)
        return;
      Asm target = index >= 0 && index < SetupAsms.Count ? (Asm) MainAsms [index] : null;
      ActiveRoomState.SetSetupAsm (target, out Asm deleteAsm);
      RoomStateDataModified?.Invoke (this, null);
      ChangesMade = true;
    }


    public void SetMainAsm (int index)
    {
      if (ActiveRoomState == null || index < -1 || index >= MainAsms.Count ||
          index == MainAsmIndex)
        return;
      Asm target = index >= 0 && index < MainAsms.Count ? (Asm) MainAsms [index] : null;
      ActiveRoomState.SetMainAsm (target, out Asm deleteAsm);
      RoomStateDataModified?.Invoke (this, null);
      ChangesMade = true;
    }

//----------------------------------------------------------------------------------------

    private RoomState IndexToRoomState (int areaIndex, int roomIndex, int roomStateIndex)
    {
      if (areaIndex < 0 || areaIndex >= AreaCount ||
          roomIndex < 0 || roomIndex >= Rooms [areaIndex].Count)
        return null;
      Room r = (Room) Rooms [areaIndex] [roomIndex];
      if (roomStateIndex < 0 || roomStateIndex >= r.RoomStates.Count)
        return null;
      return r.RoomStates [roomStateIndex];
    }


    // Get pointer to level data for given room state.
    public int GetLevelDataPtr (int areaIndex, int roomIndex, int roomStateIndex)
    {
      RoomState s = IndexToRoomState (areaIndex, roomIndex, roomStateIndex);
      return s?.MyLevelData?.StartAddressLR ?? 0;
    }


    // Set level data of active room state to level data of given roomstate.
    public void SetLevelData (int areaIndex, int roomIndex, int roomStateIndex,
                              bool newData)
    {
      if (ActiveRoomState == null)
        return;
      RoomState s = IndexToRoomState (areaIndex, roomIndex, roomStateIndex);
      LevelData d = s?.MyLevelData;
      if (newData)
      {
        d = d != null ? new LevelData (d) : new LevelData (RoomWidthInScreens,
                                                           RoomHeightInScreens);
        LevelDatas.Add (d);
      }
      ActiveRoomState.SetLevelData (d, out LevelData deleteData);
      LevelDatas.Remove (deleteData);

      RoomStateDataModified?.Invoke (this, null);
      LevelDataSelected?.Invoke (this, null);
      ChangesMade = true;
    }


    // Get pointer to scroll set for given room state.
    public int GetScrollSetPtr (int areaIndex, int roomIndex, int roomStateIndex)
    {
      RoomState s = IndexToRoomState (areaIndex, roomIndex, roomStateIndex);
      return s?.MyScrollSet?.StartAddressLR ?? 0;
    }


    // Set scroll set of active room state to scroll set of given roomstate.
    public void SetScrollSet (int areaIndex, int roomIndex, int roomStateIndex,
                              bool newData, int defaultColor)
    {
      if (ActiveRoomState == null)
        return;
      RoomState s = IndexToRoomState (areaIndex, roomIndex, roomStateIndex);
      ScrollSet d = s?.MyScrollSet;
      if (newData)
      {
        d = d != null ? new ScrollSet (d) : new ScrollSet ();
        ScrollSets.Add (d);
      }
      ActiveRoomState.SetScrollSet (d, out ScrollSet deleteData);
      ScrollSets.Remove (deleteData);
      if (ActiveRoomState.MyScrollSet == null)
        ActiveRoomState.ScrollSetPtr = defaultColor;

      RoomStateDataModified?.Invoke (this, null);
      LevelDataModified?.Invoke (this, new LevelDataEventArgs () {AllScreens = true});
      HandlingSelection = true;
      ForceSelectScrollData (0);
      ScrollDataListChanged?.Invoke (this, new ListLoadEventArgs (-1));
      HandlingSelection = false;
      ChangesMade = true;
    }


    // Get pointer to PLM set for given room state.
    public int GetPlmSetPtr (int areaIndex, int roomIndex, int roomStateIndex)
    {
      RoomState s = IndexToRoomState (areaIndex, roomIndex, roomStateIndex);
      return s?.MyPlmSet?.StartAddressLR ?? 0;
    }


    // Set PLM set of active room state to PLM set of given roomstate.
    public void SetPlmSet (int areaIndex, int roomIndex, int roomStateIndex,
                           bool newData)
    {
      if (ActiveRoomState == null)
        return;
      RoomState s = IndexToRoomState (areaIndex, roomIndex, roomStateIndex);
      PlmSet d = s?.MyPlmSet;
      if (newData)
      {
        d = d != null ? new PlmSet (d) : new PlmSet ();
        PlmSets.Add (d);
      }
      ActiveRoomState.SetPlmSet (d, out PlmSet deleteData);
      PlmSets.Remove (deleteData);

      RoomStateDataModified?.Invoke (this, null);
      LevelDataModified?.Invoke (this, new LevelDataEventArgs () {AllScreens = true});
      HandlingSelection = true;
      ForceSelectPlm (0);
      PlmListChanged?.Invoke (this, new ListLoadEventArgs (-1));
      HandlingSelection = false;
      ChangesMade = true;
    }


    // Get pointer to enemy set for given room state.
    public int GetEnemySetPtr (int areaIndex, int roomIndex, int roomStateIndex)
    {
      RoomState s = IndexToRoomState (areaIndex, roomIndex, roomStateIndex);
      return s?.MyEnemySet?.StartAddressLR ?? 0;
    }


    // Set enemy set of active room state to enemy set of given roomstate.
    public void SetEnemySet (int areaIndex, int roomIndex, int roomStateIndex,
                             bool newData)
    {
      if (ActiveRoomState == null)
        return;
      RoomState s = IndexToRoomState (areaIndex, roomIndex, roomStateIndex);
      EnemySet d = s?.MyEnemySet;
      if (newData)
      {
        d = d != null ? new EnemySet (d) : new EnemySet ();
        EnemySets.Add (d);
      }
      ActiveRoomState.SetEnemySet (d, out EnemySet deleteData);
      EnemySets.Remove (deleteData);

      RoomStateDataModified?.Invoke (this, null);
      LevelDataModified?.Invoke (this, new LevelDataEventArgs () {AllScreens = true});
      HandlingSelection = true;
      ForceSelectEnemy (0);
      EnemyListChanged?.Invoke (this, new ListLoadEventArgs (-1));
      HandlingSelection = false;
      ChangesMade = true;
    }


    // Get pointer to enemy gfx for given room state.
    public int GetEnemyGfxPtr (int areaIndex, int roomIndex, int roomStateIndex)
    {
      RoomState s = IndexToRoomState (areaIndex, roomIndex, roomStateIndex);
      return s?.MyEnemyGfx?.StartAddressLR ?? 0;
    }


    // Set enemy gfx of active room state to enemy gfx of given roomstate.
    public void SetEnemyGfx (int areaIndex, int roomIndex, int roomStateIndex,
                             bool newData)
    {
      if (ActiveRoomState == null)
        return;
      RoomState s = IndexToRoomState (areaIndex, roomIndex, roomStateIndex);
      EnemyGfx d = s?.MyEnemyGfx;
      if (newData)
      {
        d = d != null ? new EnemyGfx (d) : new EnemyGfx ();
        EnemyGfxs.Add (d);
      }
      ActiveRoomState.SetEnemyGfx (d, out EnemyGfx deleteData);
      EnemyGfxs.Remove (deleteData);

      RoomStateDataModified?.Invoke (this, null);
      HandlingSelection = true;
      ForceSelectEnemyGfx (0);
      EnemyGfxListChanged?.Invoke (this, new ListLoadEventArgs (-1));
      HandlingSelection = false;
      ChangesMade = true;
    }


    // Get pointer to fx for given room state.
    public int GetFxPtr (int areaIndex, int roomIndex, int roomStateIndex)
    {
      RoomState s = IndexToRoomState (areaIndex, roomIndex, roomStateIndex);
      return s?.MyFx?.StartAddressLR ?? 0;
    }


    // Set fx of active room state to fx of given roomstate.
    public void SetFx (int areaIndex, int roomIndex, int roomStateIndex,
                       bool newData)
    {
      if (ActiveRoomState == null)
        return;
      RoomState s = IndexToRoomState (areaIndex, roomIndex, roomStateIndex);
      Fx d = s?.MyFx;
      if (newData)
      {
        d = d != null ? new Fx (d) : new Fx ();
        Fxs.Add (d);
      }
      ActiveRoomState.SetFx (d, out Fx deleteData);
      Fxs.Remove (deleteData);

      RoomStateDataModified?.Invoke (this, null);
      LevelDataModified?.Invoke (this, new LevelDataEventArgs () {AllScreens = true});
      ChangesMade = true;
    }


    // Get Door Asm type
    public DoorAsmType GetDoorAsmType ()
    {
      if (ActiveDoor.MyDoorAsm != null)
        return DoorAsmType.Regular;
      if (ActiveDoor.MyScrollAsm != null)
        return DoorAsmType.Scroll;
      return DoorAsmType.None;
    }


    private Door IndexToDoor (int areaIndex, int roomIndex, int doorIndex)
    {
      if (areaIndex < 0 || areaIndex >= AreaCount ||
          roomIndex < 0 || roomIndex >= Rooms [areaIndex].Count)
        return null;
      Room r = (Room) Rooms [areaIndex] [roomIndex];
      if (doorIndex < 0 || doorIndex >= r.MyDoorSet.DoorCount)
        return null;
      return r.MyDoorSet.MyDoors [doorIndex];
    }


    // Get pointer to Asm for given Door.
    public int GetScrollAsmPtr (int areaIndex, int roomIndex, int doorIndex)
    {
      Door d = IndexToDoor (areaIndex, roomIndex, doorIndex);
      return d?.MyScrollAsm?.StartAddressLR ?? 0;
    }


    public void SetRegularDoorAsm (int index)
    {
      if (ActiveDoor == null)
        return;
      Asm target = null;
      if (index >= 0 && index < DoorAsms.Count)
        target = (Asm) DoorAsms [index];
      ActiveDoor.SetDoorAsm (target, out ScrollAsm deleteData);
      ScrollAsms.Remove (deleteData);

      DoorDataModified?.Invoke (this, null);
      ChangesMade = true;
    }


    public void SetScrollAsm (int areaIndex, int roomIndex, int doorIndex,
                       bool newData)
    {
      if (ActiveDoor == null)
        return;
      Door d = IndexToDoor (areaIndex, roomIndex, doorIndex);
      ScrollAsm a = d?.MyScrollAsm;
      if (newData)
      {
        a = a != null ? new ScrollAsm (a) : new ScrollAsm ();
        ScrollAsms.Add (a);
      }
      ActiveDoor.SetScrollAsm (a, out ScrollAsm deleteData);
      ScrollAsms.Remove (deleteData);

      DoorDataModified?.Invoke (this, null);
      ChangesMade = true;
    }


    // List of all scroll PLMs for give room state
    private List <Plm> GetScrollPlms (int areaIndex, int roomIndex, int roomStateIndex)
    {
      RoomState s = IndexToRoomState (areaIndex, roomIndex, roomStateIndex);
      List <Plm> plms = s?.MyPlmSet?.Plms;
      if (plms != null)
        return (from Plm p in plms
                where p.PlmID == Plm.ScrollID
                select p).ToList ();
      return new List <Plm> ();
    }


    public int GetScrollPlmDataPtr (int areaIndex, int roomIndex, int roomStateIndex,
                                    int scrollPlmIndex)
    {
      List <Plm> scrollPlms = GetScrollPlms (areaIndex, roomIndex, roomStateIndex);
      if (scrollPlms == null || scrollPlmIndex < 0 || scrollPlmIndex >= scrollPlms.Count)
        return 0;
      return scrollPlms [scrollPlmIndex].MyScrollPlmData?.StartAddressLR ?? 0;
    }


    public void SetScrollPlmData (int areaIndex, int roomIndex, int roomStateIndex,
                                  int scrollPlmIndex, bool newData)
    {
      if (ActivePlm == null || ActivePlm.PlmID != Plm.ScrollID)
        return;
      List <Plm> scrollPlms = GetScrollPlms (areaIndex, roomIndex, roomStateIndex);
      ScrollPlmData d = null;
      if (scrollPlmIndex >= 0 && scrollPlmIndex < scrollPlms.Count)
        d = scrollPlms [scrollPlmIndex].MyScrollPlmData;

      if (newData)
      {
        d = d != null ? new ScrollPlmData (d) : new ScrollPlmData ();
        ScrollPlmDatas.Add (d);
      }
      ActivePlm.SetScrollPlmData (d, out ScrollPlmData deleteData);
      ScrollPlmDatas.Remove (deleteData);

      if (ActivePlm.MyScrollPlmData != null)
      {
        int i = ScrollDatas.FindIndex (x => x == ActivePlm.MyScrollPlmData);
        ForceSelectScrollData (i);
      }
      else
        ForceSelectScrollData (-1);
      ScrollDataListChanged (this, new ListLoadEventArgs (ScrollDataIndex));
      RoomStateDataModified?.Invoke (this, null);
      LevelDataModified?.Invoke (this, new LevelDataEventArgs () {AllScreens = true});
      ChangesMade = true;
    }


//========================================================================================
// Save stations


    // Save station names
    public List <string> GetSaveStationNames (int areaIndex)
    {
      var names = new List <string> ();
      if (areaIndex >= 0 && areaIndex < AreaCount)
      {
        int offset = SaveStation.AreaOffsets [areaIndex];
        int max = SaveStation.AreaOffsets [areaIndex + 1];
        for (int i = 0; offset + i < max; i++)
        {
          SaveStation s = (SaveStation) SaveStations [offset + i];
          string name = Tools.IntToHex (i, 2) + " ";
          name += s.MyRoom?.Name ?? String.Empty;
          names.Add (name);
        }
      }
      return names;
    }


    SaveStation IndexToSaveStation (int areaIndex, int saveIndex)
    {
      if (areaIndex < 0 || areaIndex >= AreaCount)
        return null;
      int offset = SaveStation.AreaOffsets [areaIndex];
      int max = SaveStation.AreaOffsets [areaIndex + 1];
      if (saveIndex < 0 || offset + saveIndex >= max)
        return null;
      return (SaveStation) SaveStations [offset + saveIndex];
    }


    public void GetSaveStationRoomDoor (int areaIndex, int saveIndex,
                                        out int roomIndex, out int doorIndex)
    {
      roomIndex = -1;
      doorIndex = -1;
      SaveStation s = IndexToSaveStation (areaIndex, saveIndex);
      if (s != null)
      {
        roomIndex = Rooms [areaIndex].FindIndex (x => x == s.MyRoom);
        if (roomIndex == -1)
          return;
        Room r = (Room) Rooms [areaIndex] [roomIndex];
        doorIndex = r.MyIncomingDoors.ToList ().FindIndex (x => x == s.MyDoor);
      }
    }


    public List <string> GetIncomingDoorNames (int areaIndex, int roomIndex)
    {
      var names = new List <string> ();
      if (areaIndex < 0 || areaIndex >= AreaCount ||
          roomIndex < 0 || roomIndex >= Rooms [areaIndex].Count)
        return names;

      Room room = (Room) Rooms [areaIndex] [roomIndex];
      foreach (Door d in room.MyIncomingDoors)
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
      return names;
    }


    public void SetSaveRoomReferences (int areaIndex, int saveIndex,
                                       int roomIndex, int doorIndex)
    {
      SaveStation s = IndexToSaveStation (areaIndex, saveIndex);
      if (s == null || roomIndex < -1 || roomIndex > Rooms [areaIndex].Count)
        return;
      ChangesMade = true;
      if (roomIndex == -1)
      {
        s.SetRoom (null);
        s.SetDoor (null);
        return;
      }
      Room r = (Room) Rooms [areaIndex] [roomIndex];
      s.SetRoom (r);
      if (doorIndex < -1 || doorIndex > r.MyIncomingDoors.Count)
        return;
      if (doorIndex == -1)
      {
        s.SetDoor (null);
        return;
      }
      s.SetDoor (r.MyIncomingDoors.ToArray () [doorIndex]);
    }


    public void GetSaveStationValues (int areaIndex, int saveIndex,
                                      out int doorBts, out int screenX, out int screenY,
                                      out int samusX, out int samusY)
    {
      doorBts = 0;
      screenX = 0;
      screenY = 0;
      samusX = 0;
      samusY = 0;
      SaveStation s = IndexToSaveStation (areaIndex, saveIndex);
      if (s == null)
        return;
      doorBts = s.DoorBts;
      screenX = s.ScreenX;
      screenY = s.ScreenY;
      samusX = s.SamusX;
      samusY = s.SamusY;
      ChangesMade = true;
    }


    public void SetSaveStationValues (int areaIndex, int saveIndex,
                                      int doorBts, int screenX, int screenY,
                                      int samusX, int samusY)
    {
      SaveStation s = IndexToSaveStation (areaIndex, saveIndex);
      if (s == null)
        return;
      s.DoorBts = doorBts;
      s.ScreenX = screenX;
      s.ScreenY = screenY;
      s.SamusX = samusX;
      s.SamusY = samusY;
      ChangesMade = true;
    }

  } // partial class Project

}