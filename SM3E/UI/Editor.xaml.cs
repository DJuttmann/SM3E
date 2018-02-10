﻿using System;
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

namespace SM3E.UI
{
  /// <summary>
  /// Interaction logic for Editor.xaml
  /// </summary>
  public partial class Editor: UserControl
  {
    private Project MainProject;

    private UITileViewer LevelData;
    private UITileViewer TileSelector;
    private UITileViewer BtsSelector;

    private LevelDataRenderer MainRenderer;


    bool QuietSelect = false;
    bool DraggingPlm = false;
    bool DraggingEnemy = false;


    public Editor ()
    {
      InitializeComponent();

      SetupLevelData ();
      SetupTileSelector ();
      SetupBtsSelector ();
    }


    // Set the project and subscribe to its events.
    public void SetProject (Project p)
    {
      MainProject = p;

      MainProject.AreaListChanged += LoadAreaListBox;
      MainProject.RoomListChanged += LoadRoomListBox;
      MainProject.RoomStateListChanged += LoadRoomStateListBox;
      MainProject.PlmListChanged += LoadPlmListBox;
      MainProject.PlmTypeListChanged += LoadPlmTypeListBox;
      MainProject.EnemyListChanged += LoadEnemyListBox;
      MainProject.EnemyGfxListChanged += LoadEnemyGfxListBox;
      MainProject.EnemyTypeListChanged += LoadEnemyTypeListBox;
      MainProject.ScrollDataListChanged += LoadScrollDataListBox;
      MainProject.ScrollColorListChanged += LoadScrollColorListBox;

      MainProject.AreaSelected += AreaSelected;
      MainProject.RoomSelected += RoomSelected;
      MainProject.RoomStateSelected += RoomStateSelected;
      MainProject.DoorSelected += LoadDoorData;
      MainProject.PlmSelected += LoadPlmData;
      MainProject.PlmSelected += PlmSelected;
      MainProject.PlmTypeSelected += LoadPlmTypeData;
      MainProject.PlmTypeSelected += PlmTypeSelected;
      MainProject.EnemySelected += LoadEnemyData;
      MainProject.EnemySelected += EnemySelected;
      MainProject.EnemyGfxSelected += LoadEnemyGfxData;
      MainProject.EnemyGfxSelected += EnemyGfxSelected;
      MainProject.EnemyTypeSelected += LoadEnemyTypeData;
      MainProject.EnemyTypeSelected += EnemyTypeSelected;
      MainProject.ScrollDataSelected += ScrollDataSelected;
      MainProject.ScrollColorSelected += ScrollColorSelected;

      MainProject.LevelDataSelected += NewLevelData;
      MainProject.TileSetSelected += UpdateTileSelector;
      MainProject.TileSelected += UpdateActiveTile;
      MainProject.BtsSelected += UpdateActiveBts;

      MainProject.LevelDataModified += LevelDataModified;

      NavigateView.SetProject (MainProject, LevelData);
      PropertiesView.SetProject (MainProject);
    }

//========================================================================================
// Properties


//========================================================================================
// Edit

    private void SetupLevelData ()
    {
      LevelData = new UITileViewer (16.0, 16, 16, 16, 16, LevelDataViewer);
      LevelData.ViewportChanged += LevelDataViewportChanged;
      LevelData.MouseDown += LevelViewer_MouseDown;
      LevelData.MouseUp += LevelViewer_MouseUp;
      LevelData.MouseDrag += LevelViewer_MouseDrag;
      LevelData.BackgroundColor = Color.FromRgb (0x00, 0x00, 0x00);
      LevelDataViewer.Content = LevelData.Element;
    }


    private void NewLevelData (object sender, EventArgs e)
    {
      LevelData.SetSize (MainProject.RoomHeightInTiles, 
                         MainProject.RoomWidthInTiles, 16.0);
      MainRenderer = new LevelDataRenderer (MainProject, MainProject.RoomWidthInScreens,
                                                         MainProject.RoomHeightInScreens);
      LevelData.ReloadVisibleTiles ();
    }


    private void UpdateLevelDataMarker ()
    {
      if (LevelData == null)
        return;
      bool editing = (EditorTabs.SelectedIndex == 1);
      if (editing && LayerSelect.SelectedIndex == 3) // Plm layer
      {
        MainProject.GetPlmPosition (out int x, out int y,
                                    out int width, out int height);
        LevelData.SetMarker (x, y, width, height);
        LevelData.MarkerVisible = true;
      }
      else if (editing && LayerSelect.SelectedIndex == 4) // Enemy layer
      {
        MainProject.GetEnemyPosition (out double x, out double y,
                                      out double width, out double height);
        LevelData.SetMarker (x - width / 2, y - height / 2, width, height);
        LevelData.MarkerVisible = true;
      }
      else
      {
        LevelData.MarkerVisible = false;
      }
    }


    private void SetupTileSelector ()
    {
      TileSelector = new UITileViewer (16.0, 32, 32, 32, 32, TileSelectorViewer);
      TileSelector.MouseDown += TileSelector_MouseDown;
      TileSelector.BackgroundColor = Color.FromRgb (0xFF, 0x00, 0xFF);
      TileSelectorViewer.Content = TileSelector.Element;
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
      // [wip] perhaps Bts tiles should be obtained from MainProject.
      BtsSelector.Screens [0, 0].Source = GraphicsIO.LoadBitmap (Project.BtsTilesFile);
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
      QuietSelect = true;
      AreaListBox.SelectedIndex = e.SelectItem;
      QuietSelect = false;
    }


    private void AreaSelected (object sender, EventArgs e)
    {
      QuietSelect = true;
      AreaListBox.SelectedIndex = MainProject.AreaIndex;
      QuietSelect = false;
    }


    private void LoadRoomListBox (object sender, ListLoadEventArgs e)
    {
      List <string> names = MainProject.RoomNames;
      RoomListBox.Items.Clear ();
      foreach (string name in names)
        RoomListBox.Items.Add (name);
      QuietSelect = true;
      RoomListBox.SelectedIndex = e.SelectItem;
      QuietSelect = false;
    }


    private void RoomSelected (object sender, EventArgs e)
    {
      QuietSelect = true;
      RoomListBox.SelectedIndex = MainProject.RoomIndex;
      QuietSelect = false;
    }


    private void LoadRoomStateListBox (object sender, ListLoadEventArgs e)
    {
      List <string> names = MainProject.RoomStateNames;
      RoomStateListBox.Items.Clear ();
      foreach (string name in names)
        RoomStateListBox.Items.Add (name);
      QuietSelect = true;
      RoomStateListBox.SelectedIndex = e.SelectItem;
      QuietSelect = false;
    }


    private void RoomStateSelected (object sender, EventArgs e)
    {
      QuietSelect = true;
      RoomStateListBox.SelectedIndex = MainProject.RoomStateIndex;
      QuietSelect = false;
    }


    private void LoadPlmListBox (object sender, ListLoadEventArgs e)
    {
      List <string> names = MainProject.PlmNames;
      PlmListBox.Items.Clear ();
      foreach (string name in names)
        PlmListBox.Items.Add (name);
      QuietSelect = true;
      PlmListBox.SelectedIndex = e.SelectItem;
      QuietSelect = false;
    }


    private void PlmSelected (object sender, EventArgs e)
    {
      QuietSelect = true;
      PlmListBox.SelectedIndex = MainProject.PlmIndex;
      QuietSelect = false;
    }


    private void LoadPlmTypeListBox (object sender, ListLoadEventArgs e)
    {
      List <string> names = MainProject.PlmTypeNames;
      PlmTypeListBox.Items.Clear ();
      foreach (string name in names)
        PlmTypeListBox.Items.Add (name);
      QuietSelect = true;
      PlmTypeListBox.SelectedIndex = e.SelectItem;
      QuietSelect = false;
    }


    private void PlmTypeSelected (object sender, EventArgs e)
    {
      QuietSelect = true;
      PlmTypeListBox.SelectedIndex = MainProject.PlmTypeIndex;
      QuietSelect = false;
    }


    private void LoadEnemyListBox (object sender, ListLoadEventArgs e)
    {
      List <string> names = MainProject.EnemyNames;
      EnemyListBox.Items.Clear ();
      foreach (string name in names)
        EnemyListBox.Items.Add (name);
      QuietSelect = true;
      EnemyListBox.SelectedIndex = e.SelectItem;
      QuietSelect = false;
    }


    private void EnemySelected (object sender, EventArgs e)
    {
      QuietSelect = true;
      EnemyListBox.SelectedIndex = MainProject.EnemyIndex;
      QuietSelect = false;
    }


    private void LoadEnemyGfxListBox (object sender, ListLoadEventArgs e)
    {
      List <string> names = MainProject.EnemyGfxNames;
      EnemyGfxListBox.Items.Clear ();
      foreach (string name in names)
        EnemyGfxListBox.Items.Add (name);
      QuietSelect = true;
      EnemyGfxListBox.SelectedIndex = e.SelectItem;
      QuietSelect = false;
    }


    private void EnemyGfxSelected (object sender, EventArgs e)
    {
      QuietSelect = true;
      EnemyGfxListBox.SelectedIndex = MainProject.EnemyGfxIndex;
      QuietSelect = false;
    }


    private void LoadEnemyTypeListBox (object sender, ListLoadEventArgs e)
    {
      List <string> names = MainProject.EnemyTypeNames;
      EnemyTypeListBox.Items.Clear ();
      foreach (string name in names)
        EnemyTypeListBox.Items.Add (name);
      QuietSelect = true;
      EnemyTypeListBox.SelectedIndex = e.SelectItem;
      QuietSelect = false;
    }


    private void EnemyTypeSelected (object sender, EventArgs e)
    {
      QuietSelect = true;
      EnemyTypeListBox.SelectedIndex = MainProject.EnemyTypeIndex;
      QuietSelect = false;
    }


    private void LoadScrollDataListBox (object sender, ListLoadEventArgs e)
    {
      List <string> names = MainProject.ScrollDataNames;
      ScrollDataListBox.Items.Clear ();
      foreach (string name in names)
        ScrollDataListBox.Items.Add (name);
      QuietSelect = true;
      ScrollDataListBox.SelectedIndex = e.SelectItem;
      QuietSelect = false;
    }


    private void ScrollDataSelected (object sender, EventArgs e)
    {
      QuietSelect = true;
      ScrollDataListBox.SelectedIndex = MainProject.ScrollDataIndex;
      QuietSelect = false;
    }


    private void LoadScrollColorListBox (object sender, ListLoadEventArgs e)
    {
      List <string> names = MainProject.ScrollColorNames;
      ScrollColorListBox.Items.Clear ();
      foreach (string name in names)
        ScrollColorListBox.Items.Add (name);
      QuietSelect = true;
      ScrollColorListBox.SelectedIndex = e.SelectItem;
      QuietSelect = false;
    }


    private void ScrollColorSelected (object sender, EventArgs e)
    {
      QuietSelect = true;
      ScrollColorListBox.SelectedIndex = MainProject.ScrollColorIndex;
      QuietSelect = false;
    }


    private void LoadDoorData (object sender, EventArgs e)
    {
      // [wip]
    }


    private void LoadPlmData (object sender, EventArgs e)
    {
      // [wip]
    }


    private void LoadPlmTypeData (object sender, EventArgs e)
    {
      PlmName.Content = MainProject.PlmTypeName;
      PlmImage.Source = MainProject.PlmTypeImage?.ToBitmap ();
    }


    private void LoadEnemyData (object sender, EventArgs e)
    {
      // [wip]
    }


    private void LoadEnemyGfxData (object sender, EventArgs e)
    {
      // [wip] maybe do nothing?
    }


    private void LoadEnemyTypeData (object sender, EventArgs e)
    {
      EnemyName.Content = MainProject.EnemyTypeName;
      EnemyImage.Source = MainProject.EnemyTypeImage?.ToBitmap ();
    }


//========================================================================================
// Event handlers


    private void EditorTabs_SelectionChanged (object sender, SelectionChangedEventArgs e)
    {
      UpdateLevelDataMarker ();
    }


    private void LayerSelect_SelectionChanged (object sender, SelectionChangedEventArgs e)
    {
      if (LayerSelect.SelectedItem == null)
      {
        var removedItems = e.RemovedItems;
        if (removedItems.Count > 0)
          LayerSelect.SelectedItem = removedItems [0];
        else
          LayerSelect.SelectedIndex = 0;
        return;
      }

      UpdateLevelDataMarker ();

      TileLayersEditor.Visibility = Visibility.Hidden;
      PlmLayerEditor.Visibility = Visibility.Hidden;
      EnemyLayerEditor.Visibility = Visibility.Hidden;
      ScrollLayerEditor.Visibility = Visibility.Hidden;
      GfxLayerEditor.Visibility = Visibility.Hidden;
      switch (LayerSelect.SelectedIndex)
      {
      case 0:
      case 1:
      case 2:
        TileLayersEditor.Visibility = Visibility.Visible;
        break;
      case 3:
        PlmLayerEditor.Visibility = Visibility.Visible;
        break;
      case 4:
        EnemyLayerEditor.Visibility = Visibility.Visible;
        break;
      case 5:
        ScrollLayerEditor.Visibility = Visibility.Visible;
        break;
      case 6:
        GfxLayerEditor.Visibility = Visibility.Visible;
        break;
      }
    }


    private void AreaListBox_SelectionChanged (object sender, SelectionChangedEventArgs e)
    {
      AreaListBox.ScrollIntoView (AreaListBox.SelectedItem);
      if (!QuietSelect)
        MainProject.SelectArea (AreaListBox.SelectedIndex);
    }


    private void RoomListBox_SelectionChanged (object sender, SelectionChangedEventArgs e)
    {
      RoomListBox.ScrollIntoView (RoomListBox.SelectedItem);
      if (!QuietSelect)
        MainProject.SelectRoom (RoomListBox.SelectedIndex);
    }


    private void RoomStateListBox_SelectionChanged (object sender, SelectionChangedEventArgs e)
    {
      RoomStateListBox.ScrollIntoView (RoomStateListBox.SelectedItem);
      if (!QuietSelect)
        MainProject.SelectRoomState (RoomStateListBox.SelectedIndex);
    }


    private void PlmListBox_SelectionChanged (object sender, SelectionChangedEventArgs e)
    {
      PlmListBox.ScrollIntoView (PlmListBox.SelectedItem);
      if (!QuietSelect)
        MainProject.SelectPlm (PlmListBox.SelectedIndex);
    }


    private void PlmTypeListBox_SelectionChanged (object sender, SelectionChangedEventArgs e)
    {
      PlmTypeListBox.ScrollIntoView (PlmTypeListBox.SelectedItem);
      if (!QuietSelect)
        MainProject.SelectPlmType (PlmTypeListBox.SelectedIndex);
    }


    private void EnemyListBox_SelectionChanged (object sender, SelectionChangedEventArgs e)
    {
      EnemyListBox.ScrollIntoView (EnemyListBox.SelectedItem);
      if (!QuietSelect)
        MainProject.SelectEnemy (EnemyListBox.SelectedIndex);
    }


    private void EnemyListBox_DoubleClick (object sender, MouseButtonEventArgs e)
    {
      var window = new UI.EditEnemyWindow (MainProject, false);
      window.Owner = Window.GetWindow (this);
      window.ShowDialog ();
    }


    private void EnemyGfxListBox_SelectionChanged (object sender, SelectionChangedEventArgs e)
    {
      EnemyGfxListBox.ScrollIntoView (EnemyGfxListBox.SelectedItem);
      if (!QuietSelect)
        MainProject.SelectEnemyGfx (EnemyGfxListBox.SelectedIndex);
    }


    private void EnemyTypeListBox_SelectionChanged (object sender, SelectionChangedEventArgs e)
    {
      EnemyTypeListBox.ScrollIntoView (EnemyTypeListBox.SelectedItem);
      if (!QuietSelect)
        MainProject.SelectEnemyType (EnemyTypeListBox.SelectedIndex);
    }


    private void ScrollDataListBox_SelectionChanged (object sender, SelectionChangedEventArgs e)
    {
      ScrollDataListBox.ScrollIntoView (ScrollDataListBox.SelectedItem);
      if (!QuietSelect)
        MainProject.SelectScrollData (ScrollDataListBox.SelectedIndex);
    }


    private void ScrollColorListBox_SelectionChanged (object sender, SelectionChangedEventArgs e)
    {
      ScrollColorListBox.ScrollIntoView (ScrollColorListBox.SelectedItem);
      if (!QuietSelect)
        MainProject.SelectScrollColor (ScrollColorListBox.SelectedIndex);
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


    private void AddPlm_Click (object sender, RoutedEventArgs e)
    {
      MainProject.AddPlm (0, 0);
    }


    private void MovePlmUp_Click (object sender, RoutedEventArgs e)
    {
      MainProject.MovePlmUp ();
    }


    private void MovePlmDown_Click (object sender, RoutedEventArgs e)
    {
      MainProject.MovePlmDown ();
    }


    private void DeletePlm_Click (object sender, RoutedEventArgs e)
    {
      MainProject.DeletePlm ();
    }


    private void AddEnemy_Click (object sender, RoutedEventArgs e)
    {
      MainProject.AddEnemy (64, 64);
    }


    private void MoveEnemyUp_Click (object sender, RoutedEventArgs e)
    {
      MainProject.MoveEnemyUp ();
    }


    private void MoveEnemyDown_Click (object sender, RoutedEventArgs e)
    {
      MainProject.MoveEnemyDown ();
    }


    private void DeleteEnemy_Click (object sender, RoutedEventArgs e)
    {
      MainProject.DeleteEnemy ();
    }


    private void AddEnemyGfx_Click (object sender, RoutedEventArgs e)
    {
      MainProject.AddEnemyGfx ();
    }


    private void MoveEnemyGfxUp_Click (object sender, RoutedEventArgs e)
    {
      MainProject.MoveEnemyGfxUp ();
    }


    private void MoveEnemyGfxDown_Click (object sender, RoutedEventArgs e)
    {
      MainProject.MoveEnemyGfxDown ();
    }


    private void DeleteEnemyGfx_Click (object sender, RoutedEventArgs e)
    {
      MainProject.DeleteEnemyGfx ();
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
        if (LayerSelect.SelectedIndex == 3) // Plm layer
        {
          if (MainProject.SelectPlmAt (e.TileClickX, e.TileClickY) &&
              e.Button == MouseButton.Left)
          {
            DraggingPlm = true;
            UpdateLevelDataMarker ();
          }
          else
            DraggingPlm = false;
        }
        
        else if (LayerSelect.SelectedIndex == 4) // Enemy layer
        {
          if (MainProject.SelectEnemyAt (e.ClickX * 16, e.ClickY * 16) &&
              e.Button == MouseButton.Left)
          {
            DraggingEnemy = true;
            UpdateLevelDataMarker ();
          }
          else
            DraggingEnemy = false;
        }

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
      case MouseButton.Left: // Check if door and navigate through there.
        if (MainProject.NavigateThroughDoor (e.TileClickY, e.TileClickX,
                                             out int screenX, out int screenY))
          LevelData.ScrollToScreen (screenX, screenY); 
        break;

      case MouseButton.Right: // Check if door and select it in the editor.
        MainProject.SelectDoorAt (e.TileClickY, e.TileClickX);
        break;

      default:
        break;
      }
    }


    // Mouse drag.
    private void LevelViewer_MouseDrag (object sender, TileViewerMouseEventArgs e)
    {
      switch (EditorTabs.SelectedIndex)
      {
      case 0: // Navigate
        // LevelViewerNavigate_MouseDown (e);
        break;

      case 1: // Edit
        if (LayerSelect.SelectedIndex == 3) // Plm layer
        {
          if (LevelData.MarkerVisible == true && DraggingPlm)
          {
            MainProject.GetPlmPosition (out int x, out int y,
                                        out int width, out int height);
            LevelData.SetMarker (x + e.PosTileX - e.TileClickX,
                                 y + e.PosTileY - e.TileClickY,
                                 width, height);
          }
        }
        else if (LayerSelect.SelectedIndex == 4) // Enemy layer
        {
          if (LevelData.MarkerVisible == true && DraggingEnemy)
          {
            MainProject.GetEnemyPosition (out double x, out double y,
                                          out double width, out double height);
            LevelData.SetMarker (x + e.PosX - e.ClickX - width / 2,
                                 y + e.PosY - e.ClickY - height / 2,
                                 width, height);
          }
        }
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
      DraggingPlm = false;
      DraggingEnemy = false;
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
          MainProject.SetLayer1 (e.PosTileY, e.PosTileX, e.TileClickY, e.TileClickX);
          break;
        case 1: // Bts
          MainProject.SetBts (e.PosTileY, e.PosTileX, e.TileClickY, e.TileClickX);
          break;
        case 2: // Layer 2
          MainProject.SetLayer2 (e.PosTileY, e.PosTileX, e.TileClickY, e.TileClickX);
          break;
        case 3: // PLMs
          if (DraggingPlm)
            MainProject.SetPlmPositionRelative (e.PosTileX - e.TileClickX,
                                                e.PosTileY - e.TileClickY);
          break;
        case 4: // Enemies
          if (DraggingEnemy)
            MainProject.SetEnemyPositionRelative (e.PosX - e.ClickX, e.PosY - e.ClickY);
          break;
        case 5: // Scrolls
          MainProject.SetScroll (e.PosTileX / 16, e.PosTileY / 16,
                                 e.TileClickX / 16, e.TileClickY / 16);
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
      MainProject.TileIndex = e.TileClickY * 32 + e.TileClickX;
    }


    // BtsSelector Mouse down.
    private void BtsSelector_MouseDown (object sender, TileViewerMouseEventArgs e)
    {
      if (e.Button != MouseButton.Left && e.Button != MouseButton.Left)
        return;
      BtsConvert.TextureIndexToBts (e.TileClickX, e.TileClickY,
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


//========================================================================================
// Room state data edit events


  }

}