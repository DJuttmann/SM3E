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

namespace SM3E.UI
{
  /// <summary>
  /// Interaction logic for Editor.xaml
  /// </summary>
  public partial class Editor: UserControl
  {
    private Project MainProject;

    private UITileViewer LevelData;

    private LevelDataRenderer MainRenderer;


    bool QuietSelect = false;
    bool DraggingPlm = false;
    bool DraggingEnemy = false;


    // Constructor.
    public Editor ()
    {
      InitializeComponent();

      SetupLevelData ();
      SetupLayerSelect ();
    }


    // Set the project and subscribe to its events.
    public void SetProject (Project p)
    {
      MainProject = p;

      MainProject.AreaListChanged += LoadAreaListBox;
      MainProject.RoomListChanged += LoadRoomListBox;
      MainProject.RoomStateListChanged += LoadRoomStateListBox;
      MainProject.AreaSelected += AreaSelected;
      MainProject.RoomSelected += RoomSelected;
      MainProject.RoomStateSelected += RoomStateSelected;
      MainProject.DoorSelected += LoadDoorData;
      MainProject.LevelDataSelected += NewLevelData;
      MainProject.LevelDataModified += LevelDataModified;

      NavigateView.ScreenSelected += LevelDataScrollToScreen;

      NavigateView.SetProject (MainProject);
      PropertiesView.SetProject (MainProject);
      TileLayersEditor.SetProject (MainProject);
      PlmLayerEditor.SetProject (MainProject);
      EnemyLayerEditor.SetProject (MainProject);
      ScrollLayerEditor.SetProject (MainProject);
    }


//========================================================================================
// Setup & Updating

    
    private void SetupLayerSelect ()
    {
      LayerSelect.Items.Clear ();
      LayerSelect.Items.Add (CreateLayerItem ("Foreground", LayerForegroundCheckBox_Click));
      LayerSelect.Items.Add (CreateLayerItem ("BTS", LayerBtsCheckBox_Click));
      LayerSelect.Items.Add (CreateLayerItem ("Background", LayerBackgroundCheckBox_Click));
      LayerSelect.Items.Add (CreateLayerItem ("PLMs", LayerPlmsCheckBox_Click));
      LayerSelect.Items.Add (CreateLayerItem ("Enemies", LayerEnemiesCheckBox_Click));
      LayerSelect.Items.Add (CreateLayerItem ("Scrolls", LayerScrollsCheckBox_Click));
      LayerSelect.Items.Add (CreateLayerItem ("Effects", LayerEffectsCheckBox_Click));
      LayerSelect.SelectedIndex = 0;
    }


    private Grid CreateLayerItem (string text, RoutedEventHandler handler)
    {
      Grid g = new Grid ();
      g.HorizontalAlignment = HorizontalAlignment.Stretch;
      g.VerticalAlignment = VerticalAlignment.Stretch;
      CheckBox c = new CheckBox () {VerticalAlignment = VerticalAlignment.Center,
                                    IsChecked = true};
      c.Click += handler;
      g.Children.Add (c);
      g.Children.Add (new Label () {Content = text,
                                    Margin = new Thickness (20, -5, 0, -3)});
      return g;
    }


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


    private void LevelDataScrollToScreen (object sender, RoomSelectEventArgs e)
    {
      LevelData.ScrollToScreen (e.ScreenX, e.ScreenY);
    }


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


    private void LoadDoorData (object sender, EventArgs e)
    {
      // [wip]
    }


//========================================================================================
// Event handlers


    private void AddRoom_Click (object sender, RoutedEventArgs e)
    {
      MainProject.AddRoom ();
    }


    private void DeleteRoom_Click (object sender, RoutedEventArgs e)
    {
      MainProject.DeleteRoom ();
    }


    private void AddRoomState_Click (object sender, RoutedEventArgs e)
    {
      MainProject.AddRoomState ();
    }


    private void MoveRoomStateUp_Click (object sender, RoutedEventArgs e)
    {
      MainProject.MoveRoomStateUp ();
    }


    private void MoveRoomStateDown_Click (object sender, RoutedEventArgs e)
    {
      MainProject.MoveRoomStateDown ();
    }


    private void DeleteRoomState_Click (object sender, RoutedEventArgs e)
    {
      MainProject.DeleteRoomState ();
    }


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
      MainProject.ForegroundVisible = ((CheckBox) sender).IsChecked ?? false;
      MainRenderer.InvalidateAll ();
      LevelData.ReloadVisibleTiles ();
    }


    private void LayerBtsCheckBox_Click (object sender, RoutedEventArgs e)
    {
      MainProject.BtsVisible = ((CheckBox) sender).IsChecked ?? false;
      MainRenderer.InvalidateAll ();
      LevelData.ReloadVisibleTiles ();
    }


    private void LayerBackgroundCheckBox_Click (object sender, RoutedEventArgs e)
    {
      MainProject.BackgroundVisible = ((CheckBox) sender).IsChecked ?? false;
      MainRenderer.InvalidateAll ();
      LevelData.ReloadVisibleTiles ();
    }


    private void LayerPlmsCheckBox_Click (object sender, RoutedEventArgs e)
    {
      MainProject.PlmsVisible = ((CheckBox) sender).IsChecked ?? false;
      MainRenderer.InvalidateAll ();
      LevelData.ReloadVisibleTiles ();
    }


    private void LayerEnemiesCheckBox_Click (object sender, RoutedEventArgs e)
    {
      MainProject.EnemiesVisible = ((CheckBox) sender).IsChecked ?? false;
      MainRenderer.InvalidateAll ();
      LevelData.ReloadVisibleTiles ();
    }


    private void LayerScrollsCheckBox_Click (object sender, RoutedEventArgs e)
    {
      MainProject.ScrollsVisible = ((CheckBox) sender).IsChecked ?? false;
      MainRenderer.InvalidateAll ();
      LevelData.ReloadVisibleTiles ();
    }


    private void LayerEffectsCheckBox_Click (object sender, RoutedEventArgs e)
    {
      MainProject.EffectsVisible = ((CheckBox) sender).IsChecked ?? false;
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
        if (LayerSelect.SelectedIndex == 3) // Plm layer
        {
          DraggingPlm = false;
          if (e.Button == MouseButton.Right &&
              MainProject.SelectPlmAt (e.TileClickX, e.TileClickY))
            UpdateLevelDataMarker ();
          if (e.Button == MouseButton.Left)
          {
            MainProject.GetPlmPosition (out int x, out int y, 
                                        out int width, out int height);
            if (e.TileClickX >= x && e.TileClickX < x + width &&
                e.TileClickY >= y && e.TileClickY < y + height)
              DraggingPlm = true;
          }
        }
        
        else if (LayerSelect.SelectedIndex == 4) // Enemy layer
        {
          DraggingEnemy = false;
          if (e.Button == MouseButton.Right &&
              MainProject.SelectEnemyAt (e.ClickX * 16, e.ClickY * 16))
            UpdateLevelDataMarker ();
          if (e.Button == MouseButton.Left)
          {
            MainProject.GetEnemyPosition (out double x, out double y, 
                                          out double width, out double height);
            width /= 2;
            height /= 2;
            if (e.ClickX >= x - width && e.ClickX < x + width &&
                e.ClickY >= y - height && e.ClickY < y + height)
              DraggingEnemy = true;
          }
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

  } // partial class Editor

}
