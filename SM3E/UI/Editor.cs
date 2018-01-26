using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SM3E
{

//========================================================================================
// CLASS MAINWINDOW -- LEVEL EDITOR METHODS
//========================================================================================


  public partial class MainWindow: Window
  {
    UITileViewer MapTileSelector;
    UITileViewer MapEditor;

    UITileViewer LevelData;
    UITileViewer TileSelector;
    UITileViewer BtsSelector;

    UITileViewer RoomSizeEditor;

    LevelDataRenderer MainRenderer;

//========================================================================================
// Navigate


    private void SetupMapTileSelector ()
    {
      MapTileSelector = new UITileViewer (8.0, 16, 16, 16, 16, null);
      MapTileSelector.Screens [0, 0].Source = MainProject.MapTileSheet.ToBitmap ();
      MapTileViewer.Children.Add (MapTileSelector.Element);
    }


    private void SetupMapEditor ()
    {
      MapEditor = new UITileViewer (8.0, 64, 32, 64, 32, null);
      MapViewer.Children.Add (MapEditor.Element);

      RoomSizeEditor = new UITileViewer (16.0, 64, 32, 64, 32, null);
      RoomSizeEditor.Screens [0, 0].SetValue (RenderOptions.BitmapScalingModeProperty,
                                              BitmapScalingMode.NearestNeighbor);
      RoomSizeViewer.Content = RoomSizeEditor.Element;
    }


    private void UpdateMapEditor (object sender, EventArgs e)
    {
      ImageSource source = MainProject.RenderAreaMap ().ToBitmap ();
      MapEditor.Screens [0, 0].Source = source;
      RoomSizeEditor.Screens [0, 0].Source = source;
    }
  

//========================================================================================
// Edit

    private void SetupLevelData ()
    {
      LevelData = new UITileViewer (16.0, 16, 16, 16, 16, LevelDataViewer);
      LevelData.ViewportChanged += LevelDataViewportChanged;
      LevelData.MouseDown += LevelViewer_MouseDown;
      LevelData.MouseUp += LevelViewer_MouseUp;
      LevelData.BackgroundColor = Color.FromRgb (0x00, 0x00, 0x00);
      LevelDataViewer.Content = LevelData.Element;
      NewLevelData (null, null);
    }


    private void NewLevelData (object sender, EventArgs e)
    {
      LevelData.SetSize (MainProject.RoomHeightInTiles, 
                         MainProject.RoomWidthInTiles, 16.0);
      MainRenderer = new LevelDataRenderer (MainProject, MainProject.RoomWidthInScreens,
                                                         MainProject.RoomHeightInScreens);
      LevelData.ReloadVisibleTiles ();
    }


    private void SetupTileSelector ()
    {
      TileSelector = new UITileViewer (16.0, 32, 32, 32, 32, TileSelectorViewer);
      TileSelector.MouseDown += TileSelector_MouseDown;
      TileSelector.BackgroundColor = Color.FromRgb (0xFF, 0x00, 0xFF);
      TileSelectorViewer.Content = TileSelector.Element;
      MainProject.TileSelected += UpdateActiveTile;
      SelectedTileImage.RenderTransformOrigin = new Point (0.5, 0.5);
    }


    private void UpdateTileSelector (object sender, EventArgs e)
    {
      TileSelector.Screens [0, 0].Source = MainProject.RenderTileset ().ToBitmap ();
    }


    private void UpdateActiveTile (object sender, EventArgs e)
    {
      int index = MainProject.TileIndex;
      double hFlip = MainProject.TileHFlip ? -1.0 : 1.0;
      double vFlip = MainProject.TileVFlip ? -1.0 : 1.0;
      if (index != -1)
      SelectedTileImage.Source = MainProject.RoomTiles [index].ToBitmap ();
      SelectedTileImage.RenderTransform = new ScaleTransform (hFlip, vFlip);
    }


    private void SetupBtsSelector ()
    {
      BtsSelector = new UITileViewer (16.0, 8, 17, 8, 17, BtsSelectorViewer);
      BtsSelector.MouseDown += BtsSelector_MouseDown;
      BtsSelector.BackgroundColor = Color.FromRgb (0x00, 0x00, 0x00);
      BtsSelectorViewer.Content = BtsSelector.Element;
      BtsSelector.Screens [0, 0].Source = GraphicsIO.LoadBitmap ("BTS.png");
      MainProject.BtsSelected += UpdateActiveBts;
    }


    private void UpdateActiveBts (object sender, EventArgs e)
    {
      int type = MainProject.BtsType;
      int value = MainProject.BtsValue;
      BlitImage image = new BlitImage (16, 16);
      image.Clear ();
      MainProject.RenderBts (image, 0, 0, type, value);
      SelectedBtsImage.Source = image.ToBitmap ();
    }


//========================================================================================


    private void LoadAreaListBox (object sender, ListLoadEventArgs e)
    {
      List <string> names = MainProject.AreaNames;
      AreaListBox.Items.Clear ();
      foreach (string name in names)
        AreaListBox.Items.Add (name);
      AreaListBox.SelectedItem = e.SelectItem;
    }


    private void LoadRoomListBox (object sender, ListLoadEventArgs e)
    {
      List <string> names = MainProject.RoomNames;
      RoomListBox.Items.Clear ();
      foreach (string name in names)
        RoomListBox.Items.Add (name);
      RoomListBox.SelectedIndex = e.SelectItem;
    }


    private void LoadRoomStateListBox (object sender, ListLoadEventArgs e)
    {
      List <string> names = MainProject.RoomStateNames;
      RoomStateListBox.Items.Clear ();
      foreach (string name in names)
        RoomStateListBox.Items.Add (name);
      RoomStateListBox.SelectedIndex = e.SelectItem;
    }


    private void LoadDoorListBox (object sender, ListLoadEventArgs e)
    {
      List <string> names = MainProject.DoorNames;
      DoorListBox.Items.Clear ();
      foreach (string name in names)
        DoorListBox.Items.Add (name);
      DoorListBox.SelectedIndex = e.SelectItem;
    }


    private void LoadRoomData (object sender, EventArgs e)
    {
      RoomAreaSelect.SelectedIndex = MainProject.RoomArea;
      RoomNameInput.Text = MainProject.RoomName;
      UpScrollerInput.Text = Tools.IntToHex (MainProject.UpScroller, 2);
      DownScrollerInput.Text = Tools.IntToHex (MainProject.DownScroller, 2);
      SpecialGfxInput.Text = Tools.IntToHex (MainProject.SpecialGfx, 2);
    }


    private void LoadRoomStateData (object sender, EventArgs e)
    {
      StateTypeSelect.SelectedIndex = 0; // [wip]
      StateEventNumberInput.Text = Tools.IntToHex (MainProject.RoomStateEventNumber, 2);
      StateSongeSetInput.Text = Tools.IntToHex (MainProject.SongSet, 2);
      StateSongIndexInput.Text = Tools.IntToHex (MainProject.PlayIndex, 2);
      StateBgScrollingInput.Text = Tools.IntToHex (MainProject.BackgroundScrolling, 4);

      LavelDataPtrInput.Text = Tools.IntToHex (MainProject.LevelDataPtr, 6);
      ScrollsPtrInput.Text = Tools.IntToHex (MainProject.RoomScrollsPtr, 6);
      PlmSetPtrInput.Text = Tools.IntToHex (MainProject.PlmSetPtr, 6);
      EnemySetPtrInput.Text = Tools.IntToHex (MainProject.EnemySetPtr, 6);
      EnemyGfxPtrInput.Text = Tools.IntToHex (MainProject.EnemyGfxPtr, 6);
      FxPtrInput.Text = Tools.IntToHex (MainProject.FxPtr, 6);
      SetupAsmPtrInput.Text = Tools.IntToHex (MainProject.SetupAsmPtr, 6);
      MainAsmPtrInput.Text = Tools.IntToHex (MainProject.MainAsmPtr, 6);
    }


//========================================================================================
// Event handlers


    private void AreaListBox_SelectionChanged (object sender, SelectionChangedEventArgs e)
    {
      MainProject.AreaIndex = AreaListBox.SelectedIndex;
    }


    private void RoomListBox_SelectionChanged (object sender, SelectionChangedEventArgs e)
    {
      MainProject.RoomIndex = RoomListBox.SelectedIndex;
    }


    private void RoomStateListBox_SelectionChanged (object sender, SelectionChangedEventArgs e)
    {
      MainProject.RoomStateIndex = RoomStateListBox.SelectedIndex;
    }


    // Loads the screens that need to be visible in the level viewer.
    private void LevelDataViewportChanged (object sender, ViewportEventArgs e)
    {
      if (sender is UITileViewer viewer)
      {
        for (int x = e.StartScreenX; x <= e.EndScreenX; x++)
          for (int y = e.StartScreenY; y <= e.EndScreenY; y++)
            viewer.Screens [x, y].Source = MainRenderer.GetScreen (x, y);
      }
    }


    private void LevelDataModified (object sender, LevelDataEventArgs e)
    {
      for (int x = e.ScreenXmin; x <= e.ScreenXmax; x++)
        for (int y = e.ScreenYmin; y <= e.ScreenYmax; y++)
          MainRenderer.InvalidateScreen (x, y);
      LevelData.ReloadVisibleTiles ();
    }


    private void LayerForegroundCheckBox_Click (object sender, RoutedEventArgs e)
    {
      MainProject.ForegroundVisible = LayerForegroundCheckBox.IsChecked ?? false;
      MainRenderer.InvalidateAll ();
      LevelData.ReloadVisibleTiles ();
    }


    private void LayerBtsCheckBox_Click (object sender, RoutedEventArgs e)
    {
      MainProject.BtsVisible = LayerBtsCheckBox.IsChecked ?? false;
      MainRenderer.InvalidateAll ();
      LevelData.ReloadVisibleTiles ();
    }


    private void LayerBackgroundCheckBox_Click (object sender, RoutedEventArgs e)
    {
      MainProject.BackgroundVisible = LayerBackgroundCheckBox.IsChecked ?? false;
      MainRenderer.InvalidateAll ();
      LevelData.ReloadVisibleTiles ();
    }


    private void LayerPlmsCheckBox_Click (object sender, RoutedEventArgs e)
    {
      MainProject.PlmsVisible = LayerPlmsCheckBox.IsChecked ?? false;
      MainRenderer.InvalidateAll ();
      LevelData.ReloadVisibleTiles ();
    }


    private void LayerEnemiesCheckBox_Click (object sender, RoutedEventArgs e)
    {
      MainProject.EnemiesVisible = LayerEnemiesCheckBox.IsChecked ?? false;
      MainRenderer.InvalidateAll ();
      LevelData.ReloadVisibleTiles ();
    }


    private void LayerScrollsCheckBox_Click (object sender, RoutedEventArgs e)
    {
      MainProject.ScrollsVisible = LayerScrollsCheckBox.IsChecked ?? false;
      MainRenderer.InvalidateAll ();
      LevelData.ReloadVisibleTiles ();
    }


    private void LayerEffectsCheckBox_Click (object sender, RoutedEventArgs e)
    {
      MainProject.EffectsVisible = LayerEffectsCheckBox.IsChecked ?? false;
      MainRenderer.InvalidateAll ();
      LevelData.ReloadVisibleTiles ();
    }

//----------------------------------------------------------------------------------------
// Level viewer events

    // Mouse down.
    private void LevelViewer_MouseDown (object sender, TileViewerMouseEventArgs e)
    {
      switch (EditorTabs.SelectedIndex)
      {
      case 0: // Navigate
        // LevelViewerNavigate_MouseDown (e);
        break;

      case 1: // Edit
        // LevelViewerEdit_MouseDown (e)
        break;

      case 2: // Properties
        break;

      default:
        break;
      }
    }


    private void LevelViewerNavigate_MouseDown (TileViewerMouseEventArgs e)
    {
      switch (e.Button)
      {
      case MouseButton.Left:
        // Check if door and navigate through there.
        break;

      case MouseButton.Right:
        // Check if door and select it in the editor.
        break;

      default:
        break;
      }
    }




    // Mouse Up.
    private void LevelViewer_MouseUp (object sender, TileViewerMouseEventArgs e)
    {
      switch (EditorTabs.SelectedIndex)
      {
      case 0: // Navigate
        LevelViewerNavigate_MouseDown (e);
        break;

      case 1: // Edit
        LevelViewerEdit_MouseUp (e);
        break;

      case 2: // Properties
        break;

      default:
        break;
      }
    }


    private void LevelViewerEdit_MouseUp (TileViewerMouseEventArgs e)
    {
      switch (e.Button)
      {
      case MouseButton.Left:
        // Place tiles in selected area.
        switch (LayerSelect.SelectedIndex)
        {
        case 0: // Layer 1
          MainProject.SetLayer1 (e.PosTileY, e.PosTileX, e.ClickTileY, e.ClickTileX);
          break;
        case 1: // Bts
          MainProject.SetBts (e.PosTileY, e.PosTileX, e.ClickTileY, e.ClickTileX);
          break;
        case 2: // Layer 2
          MainProject.SetLayer2 (e.PosTileY, e.PosTileX, e.ClickTileY, e.ClickTileX);
          break;
        default:
          break;
        }
        break;

      case MouseButton.Right:
        // Copy selected area.
        break;

      default:
        break;
      }
    }

//----------------------------------------------------------------------------------------
// Tile/Bts selector events

    // TileSelector Mouse down.
    private void TileSelector_MouseDown (object sender, TileViewerMouseEventArgs e)
    {
      if (e.Button != MouseButton.Left && e.Button != MouseButton.Left)
        return;
      MainProject.TileIndex = e.ClickTileY * 32 + e.ClickTileX;
    }


    // BtsSelector Mouse down.
    private void BtsSelector_MouseDown (object sender, TileViewerMouseEventArgs e)
    {
      if (e.Button != MouseButton.Left && e.Button != MouseButton.Left)
        return;
      BtsConvert.TextureIndexToBts (e.ClickTileX, e.ClickTileY,
                                    out int btsType, out int btsValue);
      MainProject.BtsValue = btsValue;
      MainProject.BtsType = btsType;
    }

    // H/V-flip buttons
    private void TileVFlipButton_Click (object sender, RoutedEventArgs e)
    {
      MainProject.TileVFlip = !MainProject.TileVFlip;
    }

    private void TileHFlipButton_Click (object sender, RoutedEventArgs e)
    {
      MainProject.TileHFlip = !MainProject.TileHFlip;
    }

    private void BtsVFlipButton_Click (object sender, RoutedEventArgs e)
    {
      MainProject.VFlipBts ();
    }

    private void BtsHFlipButton_Click (object sender, RoutedEventArgs e)
    {
      MainProject.HFlipBts ();
    }

  } // class MainWindow


//========================================================================================
// CLASS BTS CONVERT
//========================================================================================


  static class BtsConvert
  {

    public static void TextureIndexToBts (int col, int row, out int btsType, out int btsValue)
    {
      btsType = 0;
      btsValue = 0;
      int index = 8 * row + col;
      if (row < 8) {
        btsType = 0x1;
        btsValue = index;
        return;
      }
      switch (row) {
      case 8: // Wall, door, door caps, spikes, air
        switch (col) {
        case 0:
          btsType = 0x8;
          btsValue = 0x00;
          break;
        case 1:
          btsType = 0x9;
          btsValue = 0x00;
          break;
        case 2:
        case 3:
          btsType = 0xC;
          btsValue = (col == 3 ? 0x42 : 0x40); // 0x40 + 2 * (row == 3);
          break;
        case 4:
        case 5:
        case 6:
          btsType = 0xA;
          btsValue = (col == 6 ? col - 3 : col - 4); // col - 4 + (col == 6);
          break;
        default:
          btsType = 0x0;
          btsValue = 0x00;
          break;
        }
        break;
      case 9: // Air (shot)
        btsType = 0x4;
        btsValue = col;
        break;
      case 10: // Air (bomb)
        btsType = 0x7;
        btsValue = col;
        break;
      case 11:
        switch (col) {
        case 0:
          btsType = 0x2;
          btsValue = 0x00;
          break;
        case 1:
          btsType = 0x6;
          btsValue = 0x00;
          break;
        case 2:
          btsType = 0x2;
          btsValue = 0x02;
          break;
        case 3:
          btsType = 0xA;
          btsValue = 0x0E;
          break;
        case 4:
          btsType = 0xB;
          btsValue = 0x0B;
          break;
        case 5:
          btsType = 0x3;
          btsValue = 0x08;
          break;
        case 6:
          btsType = 0x3;
          btsValue = 0x82;
          break;
        case 7:
          btsType = 0x3;
          btsValue = 0x85;
          break;
        }
        break;
      case 12: // shot
        btsType = 0xC;
        btsValue = col;
        break;
      case 13: // crumble
        btsType = 0xB;
        btsValue = col;
        break;
      case 14: // bomb
        btsType = 0xF;
        btsValue = col;
        break;
      case 15:
      case 16:
        switch (col) {
        case 0:
          btsType = 0xB;
          btsValue = (row == 16 ? 0x0F : 0x0E); // 0x0E + (row == 16);
          break;
        case 1:
          btsType = 0xC;
          btsValue = (row == 16 ? 0x09 : 0x08); // 0x08 + (row == 16);
          break;
        case 2:
          if (row == 15) {
            btsType = 0xE;
            btsValue = 0x00;
          }
          else {
            btsType = 0xA;
            btsValue = 0x0F;
          }
          break;
        case 3:
          btsType = 0xE;
          btsValue = (row == 16 ? 0x02 : 0x01); // 0x01 + (row == 16);
          break;
        case 4:
        case 5:
        case 6:
          btsType = 0xC;
          btsValue = (row == 16 ? 0x0B : 0x0A); // 0x0A + (row == 16);
          break;
        case 7:
          if (row == 15)
            btsType = 0x5;
          else
            btsType = 0xD;
          btsValue = 0x01;
          break;
        }
        break;
      default:
        break;
      }
    }


  }
}