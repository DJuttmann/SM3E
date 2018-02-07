using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Media;

using System.Runtime.InteropServices;

namespace SM3E
{

//========================================================================================
// CLASS PROJECT -- RENDERING METHODS
//========================================================================================


  partial class Project
  {
    // Consts
    public const string BtsTilesFile = "Data/BTS.png";
    public const string BtsTilesSmallFile = "Data/BTS_numbers.png";
    public const int TileSetCount = 29;
    private const int MapTileSheetAddress = 0x1B0000; // PC 1B0000 = LR B68000 (uncomp)
    // private const int MapTileSheetAddress = 0x0D3200; // PC 0D3200 = LR 9AB200 (GB)

    // Fields
    public List <BlitImage> RoomTiles = new List <BlitImage> (); 
    public List <BlitImage> BtsTiles = new List <BlitImage> ();
    public List <BlitImage> BtsTilesSmall = new List <BlitImage> ();
    public List <BlitImage> MapTiles = new List <BlitImage> ();
    public BlitImage MapTileSheet;

    public bool ForegroundVisible = true;
    public bool BtsVisible        = true;
    public bool BackgroundVisible = true;
    public bool PlmsVisible       = true;
    public bool EnemiesVisible    = true;
    public bool ScrollsVisible    = true;
    public bool EffectsVisible    = true;
    

    // Properties

//----------------------------------------------------------------------------------------

    // Loads an 8x8 tile from a tilesheet into a bitmap image.
    public BlitImage Tile8ToImage (int tilesheetIndex, int paletteIndex,
                                      int tileIndex)
    {
      TileSheet t = (TileSheet) TileSheets [tilesheetIndex];
      Palette p = (Palette) Palettes [paletteIndex];
      int index = 256;
      byte [] data = t.RenderTile (index, p, 0);
      return new BlitImage (data, 8);
    }


    // Loads full tilesheet into an image.
    public BlitImage TilesheetToImage (int tilesheetIndex, int paletteIndex)
    {
      TileSheet t = (TileSheet) TileSheets [tilesheetIndex];
      Palette p = (Palette) Palettes [paletteIndex];

      byte [] data = new byte [16 * 64 * 64 * 4];
      for (int row = 0; row < 64; row++)
      {
        for (int col = 0; col < 16; col++)
        {
          t.DrawTile (data, 16 * 8, 64 * 8, p, 0, 8 * col, 8 * row,
                      16 * row + col, false, false);
        }
      }
      return new BlitImage (data, 16 * 8);
    }


    // Loads a 16x16 tile from a tileset into a bitmap image.
    private BlitImage Tile16ToImage (int tilesetIndex, int index)
    {
      TileSet t = (TileSet) TileSets [tilesetIndex];
      byte [] data = t.RenderTile (index);
      return new BlitImage (data, 16);
    }


    // Loads full tileset into a bitmap image.
    public BlitImage RenderTileset ()
    {
      if (ActiveTileSet == null)
        return new BlitImage (512, 512);
      byte [] data = ActiveTileSet.Render ();
      return new BlitImage (data, 512);
    }


    // Load all tiles used for rendering the room's Layer 1 and 2.
    public void LoadRoomTiles (int TileSetIndex)
    {
      RoomTiles.Clear ();
      for (int index = 0; index < 1024; index++)
      {
        RoomTiles.Add (Tile16ToImage (TileSetIndex, index));
      }
    }


    // Load all bts tiles.
    private void LoadBtsTiles ()
    {
      BlitImage tiles = new BlitImage (GraphicsIO.LoadBitmap (BtsTilesFile));
      BlitImage small = new BlitImage (GraphicsIO.LoadBitmap (BtsTilesSmallFile));
      
      // Load 16x16 tiles;
      BtsTiles.Clear ();
      for (int y = 0; y > -tiles.Height; y -= 16)
      {
        for (int x = 0; x > -tiles.Width; x -= 16)
        {
          BlitImage newTile = new BlitImage (16, 16);
          newTile.Clear ();
          newTile.Blit (tiles, x, y, false, false);
          BtsTiles.Add (newTile);
        }
      }

      // Load 8x8 tiles;
      BtsTilesSmall.Clear ();
      for (int y = 0; y > -tiles.Height; y -= 8)
      {
        for (int x = 0; x > -tiles.Width; x -= 8)
        {
          BlitImage newTile = new BlitImage (8, 8);
          newTile.Clear ();
          newTile.Blit (small, x, y, false, false);
          BtsTilesSmall.Add (newTile);
        }
      }
    }


    // Load Map tiles
    private void LoadMapTiles (Rom rom)
    {
      TileSheet t = new UncompressedTileSheet ();
      t.SetSize (256 * 64);
      t.ReadFromROM (rom, MapTileSheetAddress);

      // Palette p = new Palette ();
      // p.ReadFromROM (rom, 0x1B7012);
      
      for (int n = 0; n < 256; n++)
      {
        byte [] b = t.RenderTile (n, (Palette) Palettes [0], 0);
        MapTiles.Add (new BlitImage (b, 8));
      }

      MapTileSheet = new BlitImage (128, 128);
      MapTileSheet.Black ();
      for (int y = 0; y < 16; y++)
      {
        for (int x = 0; x < 16; x++)
        {
          MapTileSheet.Blit (MapTiles [16 * y + x], 8 * x, 8 * y, false, false);
        }
      }
    }


    // Render Area map
    public BlitImage RenderAreaMap ()
    {
      BlitImage image = new BlitImage (512, 256);
      image.Black ();
      AreaMap map;

      if (AreaIndex != IndexNone)
      {
        map = (AreaMap) AreaMaps [AreaIndex];
        for (int y = 0; y < 32; y++)
        {
          for (int x = 0; x < 64; x++)
          {
            int index = (64 * y + x);
            int tileIndex = map.GetTile (index);
            bool hFlip = map.GetHFlip (index);
            bool vFlip = map.GetVFlip (index);
            image.Blit (MapTiles [tileIndex], 8 * x, 8 * y, hFlip, vFlip);
          }
        }
      }
      return image;
    }


//----------------------------------------------------------------------------------------

    
    // Renders a screen from a room.
    public void RenderScreen (int areaIndex, int roomIndex, BlitImage screenImage,
                              int x, int y)
    {
      if (areaIndex >= 0 && areaIndex < AreaCount &&
          roomIndex >= 0 && roomIndex < Rooms [areaIndex].Count)
      {
        ActiveItems a = new ActiveItems (this);
        ActiveRoom = (Room) Rooms [areaIndex] [roomIndex];
        ActiveRoomState = ActiveRoom.RoomStates.Last ();
        LoadRoomTiles (ActiveRoomState.TileSet);
        RenderScreen (screenImage, x, y);
        ActiveRoom = a.ActiveRoom;
        ActiveRoomState = a.ActiveRoomState;
        LoadRoomTiles (ActiveRoomState.TileSet);
      }
      else
      {
        screenImage.Black ();
      }
    }


    // Renders a screen from the active room.
    public bool RenderScreen (BlitImage screenImage, int x, int y)
    {
      int rowMin = y * 16;
      int colMin = x * 16;
      bool HasLayer2 = ActiveLevelData?.HasLayer2 ?? false;

      screenImage.Black ();

      if (BackgroundVisible && HasLayer2)
        RenderSceenLayer2 (screenImage, rowMin, colMin);
      if (ForegroundVisible)
        RenderSceenLayer1 (screenImage, rowMin, colMin);
      if (PlmsVisible)
        RenderScreenPlms (screenImage, rowMin, colMin);
      if (EnemiesVisible)
        RenderScreenEnemies (screenImage, rowMin, colMin);
      if (BtsVisible)
        RenderScreenBts (screenImage, rowMin, colMin);
      if (ScrollsVisible)
      {
        RenderRoomScroll (screenImage, x, y);
        RenderScrollModification (screenImage, x, y);
      }
      return true;
    }


    // Renders Layer 2 of a screen.
    private void RenderSceenLayer2 (BlitImage screenImage, int rowMin, int colMin)
    {
      for (int row = 0; row < 16; row++)
      {
        for (int col = 0; col < 16; col++)
        {
          int r = rowMin + row;
          int c = colMin + col;
          int tile = GetLayer2Tile (r, c);
          bool hFlip = GetLayer2HFlip (r, c);
          bool vFlip = GetLayer2VFlip (r, c);
          screenImage.Blit (RoomTiles [tile], 16 * col, 16 * row, hFlip, vFlip);
        }
      }
    }


    // Renders Layer 1 of a screen.
    private void RenderSceenLayer1 (BlitImage screenImage, int rowMin, int colMin)
    {
      for (int row = 0; row < 16; row++)
      {
        for (int col = 0; col < 16; col++)
        {
          int r = rowMin + row;
          int c = colMin + col;
          int tile = GetLayer1Tile (r, c);
          bool hFlip = GetLayer1HFlip (r, c);
          bool vFlip = GetLayer1VFlip (r, c);
          screenImage.Blit (RoomTiles [tile], 16 * col, 16 * row, hFlip, vFlip);
        }
      }
    }


    // Renders PLMs on a screen.
    private void RenderScreenPlms (BlitImage screenImage, int rowMin, int colMin)
    {
      PlmSet plms = ActiveRoomState?.MyPlmSet;
      if (plms != null)
        foreach (Plm p in plms.Plms)
        {
          BlitImage graphics = p.MyPlmType?.Graphics ?? PlmTypes [0].Graphics;
          screenImage.Blit (graphics,
                            16 * (p.PosX - colMin),
                            16 * (p.PosY - rowMin),
                            false, false);
        }
    }


    // Renders Enemies on a screen.
    private void RenderScreenEnemies (BlitImage screenImage, int rowMin, int colMin)
    {
      EnemySet enemies = ActiveRoomState?.MyEnemySet;
      if (enemies != null)
        foreach (Enemy e in enemies.Enemies)
        {
          BlitImage graphics = e.MyEnemyType?.Graphics ?? EnemyTypes [0].Graphics;
          screenImage.Blit (graphics,
                            e.PosX - 16 * colMin - graphics.Width / 2,
                            e.PosY - 16 * rowMin - graphics.Height / 2,
                            false, false);
        }
    }


    // Renders BTS layer of a screen.
    private void RenderScreenBts (BlitImage screenImage, int rowMin, int colMin)
    {
      for (int row = 0; row < 16; row++)
      {
        for (int col = 0; col < 16; col++)
        {
          int r = rowMin + row;
          int c = colMin + col;
          int btsType = GetBtsType (r, c);
          int btsValue = GetBtsValue (r, c);
          RenderBts (screenImage, 16 * col, 16 * row, btsType, btsValue);
        }
      }
    }


    // Renders a screen scroll.
    private void RenderRoomScroll (BlitImage screenImage, int x, int y)
    {
      ScrollColorToRGBA (GetScroll (x, y), out byte red,  out byte green,
                                           out byte blue, out byte alpha);
      screenImage.DrawRectangle (  0,   0, 256,   3, red, green, blue, alpha);
      screenImage.DrawRectangle (0,   253, 256,   3, red, green, blue, alpha);
      screenImage.DrawRectangle (  0,   3,   3, 250, red, green, blue, alpha);
      screenImage.DrawRectangle (253,   3,   3, 250, red, green, blue, alpha);
    }


    // Renders a PLM or ASM scroll
    private void RenderScrollModification (BlitImage screenImage, int x, int y)
    {
      if (ActiveScrollData is ScrollSet || ActiveScrollData == null)
        return;
      ScrollColor color = ActiveScrollData [RoomWidthInScreens * y + x];
      ScrollColorToRGBA (color, out byte red,  out byte green,
                                out byte blue, out byte alpha);
      screenImage.DrawRectangle (96, 96, 64, 64, red, green, blue, alpha);
    }


    // Convert a scroll color to RGBA values.
    private void ScrollColorToRGBA (ScrollColor color, 
                                    out byte R, out byte G, out byte B, out byte A)
    {
      R = 0x00;
      G = 0x00;
      B = 0x00;
      A = 0x80;
      switch (color)
      {
      case ScrollColor.Red:
        R = 0xFF;
        break;
      case ScrollColor.Green:
        G = 0xFF;
        break;
      case ScrollColor.Blue:
        R = 0x40;
        G = 0x40;
        B = 0xFF;
        break;
      default:
        A = 0x00;
        break;
      }
    }


//----------------------------------------------------------------------------------------
// BTS Rendering

    
    // Render bts graphics onto an image.
    public void RenderBts (BlitImage image, int posX, int posY, int bts_type, int bts_value)
    {
      bool h_flip = false;
      bool v_flip = false;
      int index = -1;
      int temp = 0;
      switch (bts_type) {

      case 0x0: // Air
        if (bts_value == 0x00)
          index = 71;
        break;

      case 0x1: // Slope
        h_flip = ((bts_value >> 6) & 1) > 0;
        v_flip = ((bts_value >> 7) & 1) > 0;
        index = bts_value & 63;
        break;

      case 0x2: // X-ray air, Spike air
        if (bts_value == 0x00)
          index = 88;
        if (bts_value == 0x02)
          index = 90;
        break;

      case 0x3: // Treadmill, Sand
        if (bts_value == 0x08 || bts_value == 0x09) {
          h_flip = (bts_value & 1) > 0;
          index = 93;
        }
        if (bts_value == 0x82)
          index = 94;
        if (bts_value == 0x85)
          index = 95;
        break;

      case 0x4: // Air (shot)
        if (bts_value < 0x08)
          index = 72 + bts_value;
        break;

      case 0x5: // H-copy
        if (bts_value > 0x8F) {
          temp = 1;
          bts_value = 0x100 - bts_value;
        }
        RenderSpecialBts (image, posX, posY,
          50 + temp, 50 + temp, (bts_value >> 4 & 15) + 16, (bts_value & 15) + 16);
        return;

      case 0x6: // Unused
        if (bts_value == 0x00)
          index = 89;
        break;

      case 0x7: // Air (bomb)
        if (bts_value < 0x08)
          index = 80 + bts_value;
        break;

      case 0x8: // Solid
        if (bts_value == 0x00)
          index = 64;
        break;

      case 0x9: // Door
        RenderSpecialBts (image, posX, posY,
          48, 49, bts_value >> 4 & 15, bts_value & 15);
        return;

      case 0xA: // Spike, X-ray wall, Enemy breakable
        if (bts_value < 0x02)
          index = 68 + bts_value;
        if (bts_value == 0x03)
          index = 70;
        if (bts_value == 0x0E)
          index = 91;
        if (bts_value == 0x0F)
          index = 130;
        break;

      case 0xB: // Crumble, Enemy solid, Speed
        if (bts_value < 0x08)
          index = 104 + bts_value;
        if (bts_value == 0x0B)
          index = 92;
        if (bts_value == 0x0E)
          index = 120;
        if (bts_value == 0x0F)
          index = 128;
        break;

      case 0xC: // Shot, Power bomb, Super, Door cap
        if (bts_value < 0x08)
          index = 96 + bts_value;
        if (bts_value == 0x08)
          index = 121;
        if (bts_value == 0x09)
          index = 129;
        if (bts_value == 0x0A)
          index = 124;
        if (bts_value == 0x0B)
          index = 132;
        if (bts_value == 0x40 || bts_value == 0x41) {
          h_flip = (bts_value & 1) > 0;
          index = 66;
        }
        if (bts_value == 0x42 || bts_value == 0x43) {
          v_flip = (bts_value & 1) > 0;
          index = 67;
        }
        break;

      case 0xD: // V-copy
        if (bts_value > 0x8F) {
          temp = 1;
          bts_value = 0x100 - bts_value;
        }
        RenderSpecialBts (image, posX, posY,
          52 + temp, (bts_value >> 4 & 15) + 16, 52 + temp, (bts_value & 15) + 16);
        return;

      case 0xE: // Grapple
        if (bts_value < 0x02)
          index = 122 + bts_value;
        if (bts_value == 0x02)
          index = 131;
        break;

      case 0xF: // Bomb
        if (bts_value < 0x08)
          index = 112 + bts_value;
        break;

      default:
        break;
      }

      if (index == -1)
      {
        RenderSpecialBts (image, posX, posY,
          54, bts_type + 32, (bts_value >> 4 & 15) + 32, (bts_value & 15) + 32);
      }
      else
      {
        image.Blit (BtsTiles [index], posX, posY, h_flip, v_flip);
      }
    }


    // Render a Bts image made up of 4 quadrants.
    private void RenderSpecialBts (BlitImage image, int posX, int posY,
                                   int q1, int q2, int q3, int q4)
    {
      image.Blit (BtsTilesSmall [q1], posX    , posY    , false, false);
      image.Blit (BtsTilesSmall [q2], posX + 8, posY    , false, false);
      image.Blit (BtsTilesSmall [q3], posX    , posY + 8, false, false);
      image.Blit (BtsTilesSmall [q4], posX + 8, posY + 8, false, false);
    }


  } // class Project


//========================================================================================
// CLASS LEVEL DATA RENDERER
//========================================================================================


  class LevelDataRenderer
  {
    private const int ScreenImageSize = 256 * 256 * 4;
    private static readonly byte [] ClearImage = new byte [ScreenImageSize];

    private Project MyProject;
    public int Width {get; private set;}
    public int Height {get; private set;}
    private BlitImage [,] BlitImages;
    private BitmapSource [,] Bitmaps;


    public BlitImage this [int x, int y]
    {
      get {return BlitImages [x, y];}
    }


    public LevelDataRenderer (Project myProject, int width, int height)
    {
      MyProject = myProject;
      Width = width;
      Height = height;
      BlitImages = new BlitImage [width, height];
      Bitmaps = new BitmapSource [width, height];
      for (int x = 0; x < width; x++)
      {
        for (int y = 0; y < height; y++)
        {
          BlitImages [x, y] = new BlitImage (ClearImage, 256);
          Bitmaps [x, y] = null;
        }
      }
    }


    // Unload a screen.
    public void InvalidateScreen (int x, int y)
    {
      Bitmaps [x, y] = null;
    }


    // Unload all screens.
    public void InvalidateAll ()
    {
      for (int x = 0; x < Width; x++)
        for (int y = 0; y < Height; y++)
          Bitmaps [x, y] = null;
    }


    public BitmapSource GetScreen (int x, int y)
    {
      bool success;
      if (Bitmaps [x, y] != null)
        return Bitmaps [x, y];
      success = MyProject.RenderScreen (BlitImages [x, y], x, y);
      if (success)
      {
        Bitmaps [x, y] = BlitImages [x, y].ToBitmap ();
        return Bitmaps [x, y];
      }
      return null;
    }

  }


//========================================================================================
// CLASS BLITIMAGE
//========================================================================================

  
  // BGRA image that can be blitted onto other blitimages.
  public unsafe class BlitImage
  {

    byte* Bytes;
    public int Width {get; private set;}
    public int Height {get; private set;}

    public byte this [int x, int y, int channel]
    {
      get
      {
        if (x >= 0 && x < Width && 
            y >= 0 && y < Height &&
            channel >=0 && channel < 4)
          return Bytes [4 * (Width * y + x) + channel];
        return 0;
      }
      set
      {
        if (x >= 0 && x < Width && 
            y >= 0 && y < Height &&
            channel >=0 && channel < 4)
          Bytes [4 * (Width * y + x) + channel] = value;
      }
    }


    // Constructor.
    public BlitImage (int width, int height)
    {
      Width = width;
      Height = height;
      int size = Width * Height * 4;
      Bytes = (byte*) Marshal.AllocHGlobal (size);
    }


    // Constructor: byte array and width given.
    public BlitImage (byte [] data, int width)
    {
      Width = width;
      Height = data.Length / (width * 4);
      int size = Width * Height * 4;
      Bytes = (byte*) Marshal.AllocHGlobal (size);
      Marshal.Copy (data, 0, (IntPtr) Bytes, size);
    }


    // Constructor: BitmapSource given.
    public BlitImage (BitmapSource bitmap)
    {
      FromBitmap (bitmap);
    }


    ~BlitImage ()
    {
      Marshal.FreeHGlobal ((IntPtr) Bytes);
    }


    // Sets all pixels of the image to transparent.
    public void Clear ()
    {
      int size = Width * Height * 4;
      for (int n = 0; n < size; n++)
        Bytes [n] = 0x00;
    }


    // Sets all pixels of the image to black.
    public void Black ()
    {
      int size = Width * Height * 4;
      for (int n = 0; n < size; n += 4)
      {
        Bytes [n    ] = 0x00;
        Bytes [n + 1] = 0x00;
        Bytes [n + 2] = 0x00;
        Bytes [n + 3] = 0xFF;
      }
    }


    // Blit another image onto this image. High performance version.
    public void Blit (BlitImage overlay, int posX, int posY, bool hFlip, bool vFlip)
    {
      // values for base image.
      int xBase = Math.Max (0, posX);
      int yBase = Math.Max (0, posY);
      int width = Math.Min (posX + overlay.Width, Width) - xBase;
      int height = Math.Min (posY + overlay.Height, Height) - yBase;
      int dxBase = 4;
      int dyBase = (Width - width) * 4;
      if (width <= 0 || Height <= 0)  // overlay does not overlap base.
        return;

      // values for overlay image
      int xOver = Math.Max (-posX, 0);
      int yOver = Math.Max (-posY, 0);
      int dxOver = 4;
      int dyOver = overlay.Width * 4;
      if (hFlip)
      {
        xOver = overlay.Width - xOver - 1;
        dxOver *= -1;
      }
      if (vFlip)
      {
        yOver = overlay.Height - yOver - 1;
        dyOver *= -1;
      }
      dyOver -= width * dxOver;

      int iBase = (Width * yBase + xBase) * 4;
      int iOver = (overlay.Width * yOver + xOver) * 4;
      for (int y = 0; y < height; y++)
      {
        int iBaseMax = iBase + width * dxBase;
        while (iBase != iBaseMax)
        {
          if (overlay.Bytes [iOver + 3] == 0xFF)
          {
            Bytes [iBase    ] = overlay.Bytes [iOver    ];
            Bytes [iBase + 1] = overlay.Bytes [iOver + 1];
            Bytes [iBase + 2] = overlay.Bytes [iOver + 2];
            Bytes [iBase + 3] = 0xFF;
          }
          iBase += dxBase;
          iOver += dxOver;
        }
        iBase += dyBase;
        iOver += dyOver;
      }
    }


    // Draw Rectangle on image
    public void DrawRectangle (int posX, int posY, int width, int height,
                               byte red, byte green, byte blue, byte alpha)
    {
      int beta = 255 - alpha;
      int R = alpha * red   + 0x7F;
      int G = alpha * green + 0x7F;
      int B = alpha * blue  + 0x7F;
      int xMin = Math.Max (0, posX);
      int yMin = Math.Max (0, posY);
      int xMax = Math.Min (Width, posX + width);
      int yMax = Math.Min (Height, posY + height);
      width = xMax - xMin; // adjust width to cropped rectangle
      int dy = 4 * (Width - width);

      int i = 4 * (yMin * Width + xMin);
      for (int y = yMin; y < yMax; y++)
      {
        int iMax = i + width * 4;
        while (i != iMax)
        {
          Bytes [i    ] = (byte) ((beta * Bytes [i    ] + B) / 0xFF);
          Bytes [i + 1] = (byte) ((beta * Bytes [i + 1] + G) / 0xFF);
          Bytes [i + 2] = (byte) ((beta * Bytes [i + 2] + R) / 0xFF);
          i += 4;
        }
        i += dy;
      }
    }

    
    // Convert to BitmapSource.
    public BitmapSource ToBitmap ()
    {
      if (Width == 0 || Height == 0)
        return null;
      int size = Width * Height * 4;
      byte [] b = new byte [size];
      Marshal.Copy ((IntPtr) Bytes, b, 0, size);
      return BitmapSource.Create (Width, Height, 92, 92, PixelFormats.Bgra32, 
                                  null, b, Width * 4);
    }


    // Load from BitmapSource.
    public void FromBitmap (BitmapSource Source)
    {
      Width = Source.PixelWidth;
      Height = Source.PixelHeight;
      byte [] b = new byte [Width * Height * 4];
      Source.CopyPixels (b, 4 * Width, 0);
      int size = Width * Height * 4;
      Bytes = (byte*) Marshal.AllocHGlobal (size);
      Marshal.Copy (b, 0, (IntPtr) Bytes, size);
    }

  }


//========================================================================================
// CLASS GRAPHICS IO
//========================================================================================


  class GraphicsIO
  {

    // [wip] MOVE THIS TO BETTER PLACE
    public static BitmapSource LoadBitmap (string fileName)
    {
      Uri uri = new Uri (fileName, UriKind.RelativeOrAbsolute);
      // Uri uri = new Uri (Environment.CurrentDirectory + "\\" + fileName);
      BitmapSource b = new BitmapImage (uri);
      if (b.Format != PixelFormats.Bgra32)
        b = new FormatConvertedBitmap (b, PixelFormats.Bgra32, null, 0);
      return b;
    }

  }

}