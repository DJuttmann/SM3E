using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Reflection;
//using MBuild.Properties;
using System.Collections.Generic;


namespace SM3E
{


//========================================================================================
// CLASS TILE SHEET 
//========================================================================================


  abstract class TileSheet: RawData
  {
    public const int TileAreaInPixels = 64;
    public const int CreAddressPC = 0x1C8000;

    public override int Size
    {
      get {return 0;} // [wip] UNFINISHED!
    }

    public int TileCount
    {
      get {return Bytes.Count / TileAreaInPixels;}
      set
      {
        int v = value * TileAreaInPixels;
        if (v < Bytes.Count)
          Bytes.RemoveRange (v, Bytes.Count - v);
        else
          for (int i = Bytes.Count; i < v; i++)
            Bytes.Add (0);
      }
    }


    // Constructor.
    public TileSheet (): base () {}


    // Read data from ROM at given PC address.
    public abstract override bool ReadFromROM (Rom rom, int addressPC);
    // Write data to ROM at current position (addressPC), which is updated.
    public abstract override bool WriteToROM (Stream rom, ref int addressPC);

//----------------------------------------------------------------------------------------

    // Draw a pixel on an image saved in a byte array.
    private void DrawPixel (byte [] image, int width, int height,
                            int posX, int posY, int channel, byte value)
    {
      if (posX < 0 || posX >= width || posY < 0 || posY >= height)
        return;
      image [ 4 * ((width * posY) + posX) + channel] = value;
    }


    // Draw a tile from the tilemap on a tile sheet.
    public void DrawTile (byte [] image, int width, int height,
                          Palette p, int paletteRow,
                          int posX, int posY,
                          int tileIndex, bool hFlip, bool vFlip)
    {
      int xStart, yStart;
      byte colour_index;

      if (!hFlip)
        xStart = posX + 7;
      else
        xStart = posX;
      if (vFlip)
        yStart = posY + 7;
      else
        yStart = posY;
      tileIndex *= 64;
      if (tileIndex >= Bytes.Count) // [wip] this happens a lot because CRE not integrated yet.
        return;                     // std::cout << "invalid index" << std::endl;

      int x = xStart;
      for (int i = 0; i < 8; i++)
      {
        int y = yStart;
        for (int j = 0; j < 8; j++)
        {
          colour_index = Bytes [tileIndex + 8 * j + i];
          DrawPixel (image, width, height, x, y, 0,
                     p.B [16 * paletteRow + colour_index]);
          DrawPixel (image, width, height, x, y, 1,
                     p.G [16 * paletteRow + colour_index]);
          DrawPixel (image, width, height, x, y, 2,
                     p.R [16 * paletteRow + colour_index]);
          if (colour_index == 0)
            DrawPixel (image, width, height, x, y, 3, 0x00);
          else
            DrawPixel (image, width, height, x, y, 3, 0xFF);
          if (vFlip)
            y--;
          else
            y++;
        }
        if (!hFlip)
          x--;
        else
          x++;
      }
    }


    // [wip] THIS IS A NEW METHOD!!
    public byte [] RenderTile (int tileIndex, Palette p, int paletteRow)
    {
      byte [] image = new byte [256];
      tileIndex *= 64;
      if (tileIndex < 0 || tileIndex >= Bytes.Count)
        return image;
      for (int i = 0; i < 8; i++)
      {
        for (int j = 0; j < 8; j++)
        {
          byte colour_index = Bytes [tileIndex + 8 * j + 7 - i];
          DrawPixel (image, 8, 8, i, j, 0,
                     p.B [16 * paletteRow + colour_index]);
          DrawPixel (image, 8, 8, i, j, 1,
                     p.G [16 * paletteRow + colour_index]);
          DrawPixel (image, 8, 8, i, j, 2,
                     p.R [16 * paletteRow + colour_index]);
          if (colour_index == 0)
            DrawPixel (image, 8, 8, i, j, 3, 0x00);
          else
            DrawPixel (image, 8, 8, i, j, 3, 0xFF);
        }
      }
      return image;
    }

  } // class TileSheet


//========================================================================================


  class CompressedTileSheet: TileSheet, ICompressed
  {
    protected List <byte> CompressedData;
    protected bool CompressionUpToDate = false;


    public override int Size
    {
      get
      {
        if (!CompressionUpToDate)
          Compress ();
        return CompressedData.Count;
      }
    }


    // Constructor.
    public CompressedTileSheet (): base ()
    {
      CompressedData = new List <byte> ();
    }


    // Read data from ROM at given PC address.
    public override bool ReadFromROM (Rom rom, int addressPC)
    {
      //byte [] buffer;
      //int decompressedSize = 0;
      int pixelCount = 0;
      byte bite;
      byte bit;

      //decompressedSize = Compression.ReadCompressedData (out b, addressPC);
      rom.Seek (addressPC);
      int compressedSize = rom.Decompress (out List <byte> buffer);
      CompressedData.Clear ();
      rom.Seek (addressPC);
      rom.Read (CompressedData, compressedSize);
      CompressionUpToDate = true;
      int decompressedSize = buffer.Count;
      // if (decompressedSize > 100000)
      //   decompressedSize = 100000;  // Dunno how important these lines are (some safety mechanism?)

      pixelCount = decompressedSize << 1; // 2x decompressed size (1 pixel uses 4 bits)
      TileCount = pixelCount >> 6;        // this resizes the Bytes list

      for (int t = 0; t < TileCount; t++) {
        for (int n = 0; n < 8; n++) {       // bitplane 0
          bite = buffer [t << 5 | n << 1];
          for (int i = 0; i < 8; i++) {
            bit = (byte) (bite & 1);
            Bytes [t << 6 | n << 3 | i] = bit;
            bite >>= 1;
          }
        }
        for (int n = 0; n < 8; n++) {       // bitplane 1
          bite = buffer [t << 5 | n << 1 | 1];
          for (int i = 0; i < 8; i++) {
            bit = (byte) (bite & 1);
            Bytes [t << 6 | n << 3 | i] = (byte) (Bytes [t << 6 | n << 3 | i] | bit << 1);
            bite >>= 1;
          }
        }
        for (int n = 0; n < 8; n++) {       // bitplane 2
          bite = buffer [t << 5 | n << 1 | 16];
          for (int i = 0; i < 8; i++) {
            bit = (byte) (bite & 1);
            Bytes [t << 6 | n << 3 | i] = (byte) (Bytes [t << 6 | n << 3 | i] | bit << 2);
            bite >>= 1;
          }
        }
        for (int n = 0; n < 8; n++) {       // bitplane 3
          bite = buffer [t << 5 | n << 1 | 17];
          for (int i = 0; i < 8; i++) {
            bit = (byte) (bite & 1);
            Bytes [t << 6 | n << 3 | i] = (byte) (Bytes [t << 6 | n << 3 | i] | bit << 3);
            bite >>= 1;
          }
        }
      }

      startAddressPC = addressPC;
      return true;
    }


    // Write data to ROM at current position (addressPC), which is updated.
    public override bool WriteToROM (Stream rom, ref int addressPC)
    {
      // [wip]
      return false;
    }


    public bool Compress ()
    {
      // [wip]
      return false;
    }

  } // class CompressedTileSheet


//========================================================================================


  class UncompressedTileSheet: TileSheet
  {

    // Constructor.
    public UncompressedTileSheet (): base () {}


    // Read data from ROM at given PC address.
    public override bool ReadFromROM (Rom rom, int addressPC)
    {
      byte bite;
      byte bit;
      int size = TileCount * 32;
      byte [] b = new byte [size];
      rom.Seek (addressPC);
      if (!rom.Read (b, 0, size))
        return false;

      for (int t = 0; t < TileCount; t++) {
        for (int n = 0; n < 8; n++) {       // bitplane 0
          bite = b [t << 5 | n << 1];
          for (int i = 0; i < 8; i++) {
            bit = (byte) (bite & 1);
            Bytes [t << 6 | n << 3 | i] = bit;
            bite >>= 1;
          }
        }
        for (int n = 0; n < 8; n++) {       // bitplane 1
          bite = b [t << 5 | n << 1 | 1];
          for (int i = 0; i < 8; i++) {
            bit = (byte) (bite & 1);
            Bytes [t << 6 | n << 3 | i] = (byte) (Bytes [t << 6 | n << 3 | i] | bit << 1);
            bite >>= 1;
          }
        }
        for (int n = 0; n < 8; n++) {       // bitplane 2
          bite = b [t << 5 | n << 1 | 16];
          for (int i = 0; i < 8; i++) {
            bit = (byte) (bite & 1);
            Bytes [t << 6 | n << 3 | i] = (byte) (Bytes [t << 6 | n << 3 | i] | bit << 2);
            bite >>= 1;
          }
        }
        for (int n = 0; n < 8; n++) {       // bitplane 3
          bite = b [t << 5 | n << 1 | 17];
          for (int i = 0; i < 8; i++) {
            bit = (byte) (bite & 1);
            Bytes [t << 6 | n << 3 | i] = (byte) (Bytes [t << 6 | n << 3 | i] | bit << 3);
            bite >>= 1;
          }
        }
      }

      startAddressPC = addressPC;
      return true;
    }


    // Write data to ROM at current position (addressPC), which is updated.
    public override bool WriteToROM (Stream rom, ref int addressPC)
    {
      // [wip]
      return false;
    }

  } // class UncompressedTileSheet


//========================================================================================


  class GbTileSheet: TileSheet
  {
    
    // Constructor.
    public GbTileSheet (): base () {}


    // Read data from ROM at given PC address.
    public override bool ReadFromROM (Rom rom, int addressPC)
    {
      int size = TileCount * 16;
      byte [] b = new byte [size];
      rom.Seek (addressPC);
      if (!rom.Read (b, 0, size))
        return false;

      int index = 0;
      for (int t = 0; t < TileCount; t++)
      {
        for (int tileRow = 0; tileRow < 8; tileRow++)
        {
          for (int bit = 0; bit < 8; bit++)
          {
            int value = ((b [16 * t + 2 * tileRow] >> bit) & 1) +
                        ((b [16 * t + 2 * tileRow + 1] >> bit) & 1) * 2;
            Bytes [index] = (byte) value;
            index++;
          }
        }
      }

      startAddressPC = addressPC;
      return true;
    }

    
    // Write data to ROM at current position (addressPC), which is updated.
    public override bool WriteToROM (Stream rom, ref int addressPC)
    {
      // [wip]
      return false;
    }

  }


//========================================================================================
// CLASS PALETTE 
//========================================================================================


  class Palette: Data
  {
    public List <byte> R;
    public List <byte> G;
    public List <byte> B;

    public override int Size
    {
      get {return 0;} // [wip] NOT YET FINISHED
    }


    // Constructor.
    public Palette ()
    {
      R = new List <byte> ();
      G = new List <byte> ();
      B = new List <byte> ();
    }


    // Read data from ROM at given PC address.
    public override bool ReadFromROM (Rom rom, int addressPC)
    {
      //byte [] buffer;
      //int DecompressedSize = Compression.ReadCompressedData (out buffer, addressPC);
      rom.Seek (addressPC);
      rom.Decompress (out List <byte> buffer);
      int decompressedSize = buffer.Count;
      int colour_count = decompressedSize / 2;

      R.Clear ();
      G.Clear ();
      B.Clear ();
      for (int n = 0; n < colour_count; n++)
      {
        int colour = Tools.ConcatBytes (buffer [n << 1], buffer [n << 1 | 1]);
        R.Add ((byte) ((colour & 0x001F) << 3));
        G.Add ((byte) ((colour & 0x03E0) >> 2));
        B.Add ((byte) ((colour & 0x7C00) >> 7));
      }
      startAddressPC = addressPC;
      return true;
    }


    // Write data to ROM at current position (addressPC), which is updated.
    public override bool WriteToROM (Stream rom, ref int addressPC)
    {
      // [wip]
      return false;
    }


    // Set default values.
    public override void SetDefault ()
    {
      // Do nothing.
    }



  }


//========================================================================================
// CLASS TILE TABLE 
//========================================================================================


  class TileTable: Data
  {
    public const int CreAddressPC = 0x1CA09D;

    List <int> Tiles8; // list of 8x8 tiles, groups of 4 form 16x16 tiles

    public override int Size
    {
      get {return 0;} // [wip] NOT YET FINISHED
    }


    // Constructor.
    public TileTable (): base ()
    {
      Tiles8 = new List <int> ();
    }


    // Read data from ROM at given PC address.
    public override bool ReadFromROM (Rom rom, int addressPC)
    {
      //byte [] buffer;
      //int DecompressedSize = Compression.ReadCompressedData (out buffer, addressPC);
      rom.Seek (addressPC);
      rom.Decompress (out List <byte> buffer);
      int decompressedSize = buffer.Count;

      int tile8Count = decompressedSize >> 1;
      for (int n = 0; n < tile8Count; n++)
        Tiles8.Add (Tools.ConcatBytes (buffer [2 * n], buffer [2 * n + 1]));

      startAddressPC = addressPC;
      return true;
    }


    // Write data to ROM at current position (addressPC), which is updated.
    public override bool WriteToROM (Stream rom, ref int addressPC)
    {
      // [wip]
      return false;
    }

    
    // Set default values.
    public override void SetDefault ()
    {
      // Do noting.
    }
    
//----------------------------------------------------------------------------------------


    public bool GetVFlip (int tileIndex, int quadrant) {
      int i = 4 * tileIndex + quadrant;
      if (i >= 0 && i < Tiles8.Count)
        return ((Tiles8 [i] >> 15) & 1) > 0;
    //  std::cout << "invalid tile index " << i << std::endl;
      return false;
    }


    public bool GetHFlip (int tileIndex, int quadrant) {
      int i = 4 * tileIndex + quadrant;
      if (i >= 0 && i < Tiles8.Count)
        return ((Tiles8 [i] >> 14) & 1) > 0;
    //  std::cout << "invalid tile index " << i << std::endl;
      return false;
    }


    bool GetPriority (int tileIndex, int quadrant) {
      int i = 4 * tileIndex + quadrant;
      if (i >= 0 && i < Tiles8.Count)
        return ((Tiles8 [i] >> 13) & 1) > 0;
    //  std::cout << "invalid tile index " << i << std::endl;
      return false;
    }


    public int GetPaletteRow (int tileIndex, int quadrant) {
      int i = 4 * tileIndex + quadrant;
      if (i >= 0 && i < Tiles8.Count)
        return (Tiles8 [i] >> 10) & 7;
    //  std::cout << "invalid tile index " << i << std::endl;
      return 0;
    }


    public int GetTile8Index (int tileIndex, int quadrant) {
      int i = 4 * tileIndex + quadrant;
      if (i >= 0 && i < Tiles8.Count)
        return Tiles8 [i] & 1023;
    //  std::cout << "invalid tile index " << i << std::endl;
      return 0;
    }

  } // class TileTable


//========================================================================================
// CLASS TILE SET 
//========================================================================================


  class TileSet: Data
  {
    public const int DefaultSize = 9;
    public const int TileSetsAddresPC = 0x7E6A2; // Location of tile set pointers
    public const int Count = 29;
    public const int RowCount = 32;
    public const int ColCount = 32;

    public int SceTablePtr;
    public int SceSheetPtr;
    public int PalettePtr;

    public TileTable MyCreTable;
    public TileSheet MyCreSheet;
    public TileTable MySceTable;
    public TileSheet MySceSheet;
    public Palette MyPalette;

    public override int Size
    {
      get {return DefaultSize;}
    }


    public TileSet (): base ()
    {
      SceTablePtr = 0;
      SceSheetPtr = 0;
      PalettePtr   = 0;

      MyCreTable = null;
      MyCreSheet = null;
      MySceTable = null;
      MySceSheet = null;
      MyPalette  = null;
    }


    // Read data from ROM at given PC address.
    public override bool ReadFromROM (Rom rom, int addressPC)
    {
      byte [] data = new byte [DefaultSize];  
      rom.Seek (addressPC);
      if (!rom.Read (data, 0, DefaultSize))
        return false;
      SceTablePtr = Tools.ConcatBytes (data [0], data [1], data [2]);
      SceSheetPtr = Tools.ConcatBytes (data [3], data [4], data [5]);
      PalettePtr = Tools.ConcatBytes (data [6], data [7], data [8]);

      startAddressPC = addressPC;
      return true;
    }


    // Write data to ROM at current position (addressPC), which is updated.
    public override bool WriteToROM (Stream rom, ref int addressPC)
    {
      // [wip]
      return false;
    }


    // Set default values.
    public override void SetDefault ()
    {
      // Do nothing
    }


    public bool Connect (List <Data> TileTables, List <Data> TileSheets,
                         List <Data> Palettes)
    {
      bool success = true;
      if (TileTables.Count == 0 || TileSheets.Count == 0)
        return false;
      MyCreTable = (TileTable) TileTables [0];
      MyCreSheet = (TileSheet) TileSheets [0];
      MySceTable = (TileTable) TileTables.Find (x => x.StartAddressLR == SceTablePtr);
      if (MySceTable == null)
        success = false;
      MySceSheet = (TileSheet) TileSheets.Find (x => x.StartAddressLR == SceSheetPtr);
      if (MySceSheet == null)
        success = false;
      MyPalette = (Palette) Palettes.Find (x => x.StartAddressLR == PalettePtr);
      if (MyPalette == null)
        success = false;
      return success;
    }

//----------------------------------------------------------------------------------------

    // Render a 16x16 tile from the tile map to a byte array (BGRA).
    public byte [] RenderTile (int index)
    {
      if (index < 0 || index > RowCount * ColCount)
        return null;
      int row = index / ColCount;
      int col = index % ColCount;
      int FirstSceTileRow = 8; // [wip] make static field?
      int firstCreTileIndex = 0x280;
      int width = 16; // Tile is 16 pixels wide
      int height = 16; // and 16 pixels high.

      TileTable table = MyCreTable;
      if (row >= FirstSceTileRow)
      {
        table = MySceTable;
        row -= FirstSceTileRow;
      }

      byte [] image = new byte [width * height * 4];
      for (int q1 = 0; q1 < 2; q1++) {
        for (int q0 = 0; q0 < 2; q0++) {
          int quadrant = 2 * q1 + q0;
          int tile8Index = table.GetTile8Index (ColCount * row + col, quadrant);
          int paletteRow = table.GetPaletteRow (ColCount * row + col, quadrant);
          bool hFlip = table.GetHFlip (ColCount * row + col, quadrant);
          bool vFlip = table.GetVFlip (ColCount * row + col, quadrant);
          if (tile8Index < firstCreTileIndex)
            MySceSheet.DrawTile (image, width, height, MyPalette, paletteRow,
                                  8 * q0, 8 * q1,
                                  tile8Index, hFlip, vFlip);
          else
            MyCreSheet.DrawTile (image, width, height, MyPalette, paletteRow,
                                  8 * q0, 8 * q1,
                                  tile8Index - firstCreTileIndex, hFlip, vFlip);
        }
      }
      return image;
    }


    // Renders tile set onto an image (uint8_t array in BGRA format).
    public byte [] Render ()
    {
      byte [] image;                    // array of 8 bit RGBA values
      int tile_columns = 32;    // image is always 32 tiles wide
      int tile_rows = 32;       // and 32 tiles high
      int first_SCE_tile_row = 8;        // row in table where SCE tiles start
      int image_w = tile_columns * 16;   // image width
      int image_h = tile_rows * 16;      // image height
      int quadrant;     // 8x8 part of a 16x16 tile, ranges 0-3 top left to bottom right
      int tile_8_index; // index of 8x8 tile in the tile sheet
      int palette_row;  // 8 colour row of the palette, ranges 0-7
      bool h_flip, v_flip;       // flip 8x8 tile horizontally or vertically
      TileTable current_table; // the tile table to read, either CRE or SCE
      int first_CRE_tile_index = 0x280; // index of first 8x8 tile in CRE tile sheet
      int j2 = 0;

      current_table = MyCreTable;
      image = new byte [image_w * image_h * 4]; // 4 channels per pixel
      for (int j = 0; j < tile_rows; j++) {
        if (j == first_SCE_tile_row) {
          current_table = MySceTable;
          j2 = 0;
        }
        for (int i = 0; i < tile_columns; i++)
        {
          for (int q1 = 0; q1 < 2; q1++)
          {
            for (int q0 = 0; q0 < 2; q0++)
            {
              quadrant = 2 * q1 + q0;
              tile_8_index = current_table.GetTile8Index (tile_columns * j2 + i, quadrant);
              palette_row = current_table.GetPaletteRow (tile_columns * j2 + i, quadrant);
              h_flip = current_table.GetHFlip (tile_columns * j2 + i, quadrant);
              v_flip = current_table.GetVFlip (tile_columns * j2 + i, quadrant);
              if (tile_8_index < first_CRE_tile_index)
                MySceSheet.DrawTile (image, image_w, image_h, MyPalette, palette_row,
                                     16 * i + 8 * q0, 16 * j + 8 * q1,
                                     tile_8_index, h_flip, v_flip);
              else
                MyCreSheet.DrawTile (image, image_w, image_h, MyPalette, palette_row,
                                     16 * i + 8 * q0, 16 * j + 8 * q1,
                                     tile_8_index - first_CRE_tile_index, h_flip, v_flip);
            }
          }
        }
        j2++;
      }

      return image;
    }


  } // class TileSet

}