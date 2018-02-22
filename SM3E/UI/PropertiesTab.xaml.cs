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
  /// Interaction logic for PropertiesTab.xaml
  /// </summary>
  public partial class PropertiesTab : UserControl
  {

    private Project MainProject;

    private UITileViewer RoomSizeEditor;


    public PropertiesTab()
    {
      InitializeComponent();

      SetupRoomSizeEditor ();
    }


    public void SetProject (Project p)
    {
      MainProject = p;

      MainProject.AreaListChanged += LoadRoomAreaSelect;
      MainProject.TileSetListChanged += LoadTileSetSelect;
      MainProject.AreaSelected += UpdateRoomSizeEditor;
      MainProject.RoomSelected += LoadRoomData;
      MainProject.RoomStateSelected += LoadRoomStateData;
      MainProject.MapDataModified += UpdateRoomSizeEditor;
      MainProject.RoomDataModified += LoadRoomData;
      MainProject.RoomStateDataModified += LoadRoomStateData;
    }


//========================================================================================
// Setup & Updating


    private void SetupRoomSizeEditor ()
    {
      RoomSizeEditor = new UITileViewer (16.0, 64, 32, 64, 32, RoomSizeViewer);
      RoomSizeEditor.MarkerVisible = true;
      RoomSizeEditor.Screens [0, 0].SetValue (RenderOptions.BitmapScalingModeProperty,
                                              BitmapScalingMode.NearestNeighbor);
      RoomSizeViewer.Content = RoomSizeEditor.Element;
    }


    private void UpdateRoomSizeEditor (object sender, EventArgs e)
    {
      // [wip] this line does the same as UpdateMapEditor () in NavigateTab.xaml.cs
      // Streamline this to avoid redundant work?
      ImageSource source = MainProject.RenderAreaMap ().ToBitmap ();
      RoomSizeEditor.Screens [0, 0].Source = source;
    }


    private void LoadRoomData (object sender, EventArgs e)
    {
      RoomAreaSelect.SelectedIndex = MainProject.RoomArea;
      RoomNameInput.Text = MainProject.RoomName;
      UpScrollerInput.Text = Tools.IntToHex (MainProject.UpScroller, 2);
      DownScrollerInput.Text = Tools.IntToHex (MainProject.DownScroller, 2);
      SpecialGfxInput.Text = Tools.IntToHex (MainProject.SpecialGfx, 2);
      RoomSizeEditor.SetMarker (MainProject.RoomX, MainProject.RoomY + 1,
                                MainProject.RoomWidthInScreens, 
                                MainProject.RoomHeightInScreens);
      RoomSizeEditor.ScrollToMarker ();
    }


    private void LoadRoomStateData (object sender, EventArgs e)
    {
      UpdateStateTypeSelect ();
      StateEventNumberInput.Text = Tools.IntToHex (MainProject.RoomStateEventNumber, 2);
      StateSongeSetInput.Text = Tools.IntToHex (MainProject.SongSet, 2);
      StatePlayIndexInput.Text = Tools.IntToHex (MainProject.PlayIndex, 2);
      StateBgScrollingInput.Text = Tools.IntToHex (MainProject.BackgroundScrolling, 4);
      StateTileSetSelect.SelectedIndex = MainProject.TileSetIndex;

      List <string> names = MainProject.PointerNames;
      LavelDataPtrInput.Text = names [0];
      ScrollsPtrInput.Text = names [1];
      PlmSetPtrInput.Text = names [2];
      EnemySetPtrInput.Text = names [3];
      EnemyGfxPtrInput.Text = names [4];
      BackgroundInput.Text = names [5];
      FxPtrInput.Text = names [6];
      SetupAsmPtrInput.Text = names [7];
      MainAsmPtrInput.Text = names [8];

      int [] refCounts = MainProject.PointerReferenceCounts;
      LavelDataRefCount.Content = refCounts [0];
      ScrollsRefCount.Content = refCounts [1];
      PlmSetRefCount.Content = refCounts [2];
      EnemySetRefCount.Content = refCounts [3];
      EnemyGfxRefCount.Content = refCounts [4];
      BackgroundRefCount.Content = refCounts [5];
      FxRefCount.Content = refCounts [6];
      SetupAsmRefCount.Content = refCounts [7];
      MainAsmRefCount.Content = refCounts [8];
    }


    private void UpdateStateTypeSelect ()
    {
      StateTypeSelect.IsEnabled = true;
      StateEventNumberInput.IsEnabled = false;
      switch (MainProject.RoomStateType)
      {
      case StateType.Standard:
        StateTypeSelect.SelectedIndex = 7;
        StateTypeSelect.IsEnabled = false;
        break;
      case StateType.Events:
        StateTypeSelect.SelectedIndex = 0;
        StateEventNumberInput.IsEnabled = true;
        break;
      case StateType.Bosses:
        StateTypeSelect.SelectedIndex = 1;
        StateEventNumberInput.IsEnabled = true;
        break;
      case StateType.TourianBoss:
        StateTypeSelect.SelectedIndex = 2;
        break;
      case StateType.Morph:
        StateTypeSelect.SelectedIndex = 3;
        break;
      case StateType.MorphMissiles:
        StateTypeSelect.SelectedIndex = 4;
        break;
      case StateType.PowerBombs:
        StateTypeSelect.SelectedIndex = 5;
        break;
      case StateType.SpeedBooster:
        StateTypeSelect.SelectedIndex = 6;
        break;
      default:
        StateTypeSelect.SelectedIndex = -1;
        break;
      }
    }


    private void LoadRoomAreaSelect (object sender, EventArgs e)
    {
      List <string> names = MainProject.AreaNames;
      for (int n = 0; n < 8; n++)
        RoomAreaSelect.Items [n] = names [n];
    }



    private void LoadTileSetSelect (object sender, ListLoadEventArgs e)
    {
      List <string> names = MainProject.TileSetNames;
      StateTileSetSelect.ItemsSource = names;
    }



//========================================================================================
// Event handlers

    
    private void RoomAreaSelect_SelectionChanged (object sender, SelectionChangedEventArgs e)
    {
      MainProject.RoomArea = RoomAreaSelect.SelectedIndex;
    }


    private void RoomNameInput_LostFocus (object sender, RoutedEventArgs e)
    {
      MainProject.RoomName = RoomNameInput.Text;
    }


    private void RoomNameInput_KeyDown (object sender, KeyEventArgs e)
    {
      if (e.Key == Key.Enter)
        RoomNameInput_LostFocus (sender, null);
    }


    private void UpScrollerInput_LostFocus (object sender, RoutedEventArgs e)
    {
      MainProject.UpScroller = Tools.HexToInt (UpScrollerInput.Text);
    }

    
    private void UpScrollerInput_KeyDown (object sender, KeyEventArgs e)
    {
      if (e.Key == Key.Enter)
        UpScrollerInput_LostFocus (sender, null);
      UITools.ValidateHex (ref e);
    }


    private void DownScrollerInput_LostFocus (object sender, RoutedEventArgs e)
    {
      MainProject.DownScroller = Tools.HexToInt (DownScrollerInput.Text);
    }


    private void DownScrollerInput_KeyDown (object sender, KeyEventArgs e)
    {
      if (e.Key == Key.Enter)
        DownScrollerInput_LostFocus (sender, null);
      UITools.ValidateHex (ref e);
    }


    private void SpecialGfxInput_LostFocus (object sender, RoutedEventArgs e)
    {
      MainProject.SpecialGfx = Tools.HexToInt (SpecialGfxInput.Text);
    }


    private void SpecialGfxInput_KeyDown (object sender, KeyEventArgs e)
    {
      if (e.Key == Key.Enter)
        SpecialGfxInput_LostFocus (sender, null);
      UITools.ValidateHex (ref e);
    }

    
    private void StateTypeSelect_SelectionChanged (object sender, SelectionChangedEventArgs e)
    {
      switch (StateTypeSelect.SelectedIndex)
      {
      case 0:
        MainProject.RoomStateType = StateType.Events;
        break;
      case 1:
        MainProject.RoomStateType = StateType.Bosses;
        break;
      case 2:
        MainProject.RoomStateType = StateType.TourianBoss;
        break;
      case 3:
        MainProject.RoomStateType = StateType.Morph;
        break;
      case 4:
        MainProject.RoomStateType = StateType.MorphMissiles;
        break;
      case 5:
        MainProject.RoomStateType = StateType.PowerBombs;
        break;
      case 6:
        MainProject.RoomStateType = StateType.SpeedBooster;
        break;
      case 7:
        MainProject.RoomStateType = StateType.Standard;
        break;
      }
    }


    private void StateEventNumberInput_LostFocus (object sender, RoutedEventArgs e)
    {
      MainProject.RoomStateEventNumber = Tools.HexToInt (StateEventNumberInput.Text);
    }


    private void StateEventNumberInput_KeyDown (object sender, KeyEventArgs e)
    {
      if (e.Key == Key.Enter)
        StateEventNumberInput_LostFocus (sender, null);
      UITools.ValidateHex (ref e);
    }


    private void StateSongeSetInput_LostFocus (object sender, RoutedEventArgs e)
    {
      MainProject.SongSet = Tools.HexToInt (StateSongeSetInput.Text);
    }


    private void StateSongeSetInput_KeyDown (object sender, KeyEventArgs e)
    {
      if (e.Key == Key.Enter)
        StateSongeSetInput_LostFocus (sender, null);
      UITools.ValidateHex (ref e);
    }


    private void StatePlayIndexInput_LostFocus (object sender, RoutedEventArgs e)
    {
      MainProject.PlayIndex = Tools.HexToInt (StatePlayIndexInput.Text);
    }


    private void StatePlayIndexInput_KeyDown (object sender, KeyEventArgs e)
    {
      if (e.Key == Key.Enter)
        StatePlayIndexInput_LostFocus (sender, null);
      UITools.ValidateHex (ref e);
    }


    private void StateBgScrollingInput_LostFocus (object sender, RoutedEventArgs e)
    {
      MainProject.BackgroundScrolling = Tools.HexToInt (StateBgScrollingInput.Text);
    }


    private void StateBgScrollingInput_KeyDown (object sender, KeyEventArgs e)
    {
      if (e.Key == Key.Enter)
        StateBgScrollingInput_LostFocus (sender, null);
      UITools.ValidateHex (ref e);
    }


    private void StateTileSetSelect_SelectionChanged (object sender, SelectionChangedEventArgs e)
    {
      MainProject.RoomStateTileSet = StateTileSetSelect.SelectedIndex;
    }

//----------------------------------------------------------------------------------------

    private void LevelData_Click (object sender, RoutedEventArgs e)
    {
      var window = new UI.SelectDataWindow (MainProject, "level data");
      window.Owner = Window.GetWindow (this);
      window.ShowDialog ();
    }


    private void ScrollSet_Click (object sender, RoutedEventArgs e)
    {
      var window = new UI.SelectDataWindow (MainProject, "scroll set");
      window.Owner = Window.GetWindow (this);
      window.ShowDialog ();
    }


    private void PlmSet_Click (object sender, RoutedEventArgs e)
    {
      var window = new UI.SelectDataWindow (MainProject, "plm set");
      window.Owner = Window.GetWindow (this);
      window.ShowDialog ();
    }


    private void EnemySet_Click (object sender, RoutedEventArgs e)
    {
      var window = new UI.SelectDataWindow (MainProject, "enemy set");
      window.Owner = Window.GetWindow (this);
      window.ShowDialog ();
    }


    private void EnemyGfx_Click (object sender, RoutedEventArgs e)
    {
      var window = new UI.SelectDataWindow (MainProject, "enemy gfx");
      window.Owner = Window.GetWindow (this);
      window.ShowDialog ();
    }


    private void Fx_Click (object sender, RoutedEventArgs e)
    {
      var window = new UI.SelectDataWindow (MainProject, "effects");
      window.Owner = Window.GetWindow (this);
      window.ShowDialog ();
    }


    private void Background_Click (object sender, RoutedEventArgs e)
    {
      var window = new UI.SelectBackgroundWindow (MainProject);
      window.Owner = Window.GetWindow (this);
      window.ShowDialog ();
    }


    private void SetupAsm_Click (object sender, RoutedEventArgs e)
    {
      var window = new UI.SelectAsmWindow (MainProject, "setup");
      window.Owner = Window.GetWindow (this);
      window.ShowDialog ();
    }


    private void MainAsm_Click (object sender, RoutedEventArgs e)
    {
      var window = new UI.SelectAsmWindow (MainProject, "main");
      window.Owner = Window.GetWindow (this);
      window.ShowDialog ();
    }

  } // class PropertiesTab

}
