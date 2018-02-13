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

    bool QuietSelect = false;


    public PropertiesTab()
    {
      InitializeComponent();

      SetupRoomSizeEditor ();
    }


    public void SetProject (Project p)
    {
      MainProject = p;

      MainProject.AreaListChanged += LoadRoomAreaSelect;
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
      StateTypeSelect.SelectedIndex = 0; // [wip]
      StateEventNumberInput.Text = Tools.IntToHex (MainProject.RoomStateEventNumber, 2);
      StateSongeSetInput.Text = Tools.IntToHex (MainProject.SongSet, 2);
      StatePlayIndexInput.Text = Tools.IntToHex (MainProject.PlayIndex, 2);
      StateBgScrollingInput.Text = Tools.IntToHex (MainProject.BackgroundScrolling, 4);

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

      /*
      LavelDataPtrInput.Text = Tools.IntToHex (MainProject.LevelDataPtr, 6);
      ScrollsPtrInput.Text = Tools.IntToHex (MainProject.RoomScrollsPtr, 6);
      PlmSetPtrInput.Text = Tools.IntToHex (MainProject.PlmSetPtr, 6);
      EnemySetPtrInput.Text = Tools.IntToHex (MainProject.EnemySetPtr, 6);
      EnemyGfxPtrInput.Text = Tools.IntToHex (MainProject.EnemyGfxPtr, 6);
      BackgroundInput.Text = Tools.IntToHex (MainProject.BackgroundPtr, 6);
      FxPtrInput.Text = Tools.IntToHex (MainProject.FxPtr, 6);
      SetupAsmPtrInput.Text = Tools.IntToHex (MainProject.SetupAsmPtr, 6);
      MainAsmPtrInput.Text = Tools.IntToHex (MainProject.MainAsmPtr, 6);
      */
    }


    private void LoadRoomAreaSelect (object sender, EventArgs e)
    {
      List <string> names = MainProject.AreaNames;
      for (int n = 0; n < 8; n++)
        RoomAreaSelect.Items [n] = names [n];
    }


//========================================================================================
// Event handlers

    
    private void RoomAreaSelect_Update (object sender, SelectionChangedEventArgs e)
    {
      MainProject.RoomArea = RoomAreaSelect.SelectedIndex;
    }


    private void RoomNameInput_Update (object sender, RoutedEventArgs e)
    {
      MainProject.RoomName = RoomNameInput.Text;
    }


    private void UpScrollerInput_Update (object sender, RoutedEventArgs e)
    {
      MainProject.UpScroller = Tools.HexToInt (UpScrollerInput.Text);
    }


    private void DownScrollerInput_Update (object sender, RoutedEventArgs e)
    {
      MainProject.DownScroller = Tools.HexToInt (DownScrollerInput.Text);
    }


    private void SpecialGfxInput_Update (object sender, RoutedEventArgs e)
    {
      MainProject.SpecialGfx = Tools.HexToInt (SpecialGfxInput.Text);
    }


    private void StateEventNumberInput_Update (object sender, RoutedEventArgs e)
    {
      MainProject.RoomStateEventNumber = Tools.HexToInt (StateEventNumberInput.Text);
    }


    private void StateSongeSetInput_Update (object sender, RoutedEventArgs e)
    {
      MainProject.SongSet = Tools.HexToInt (StateSongeSetInput.Text);
    }


    private void StatePlayIndexInput_Update (object sender, RoutedEventArgs e)
    {
      MainProject.PlayIndex = Tools.HexToInt (StatePlayIndexInput.Text);
    }


    private void StateBgScrollingInput_Update (object sender, RoutedEventArgs e)
    {
      MainProject.BackgroundScrolling = Tools.HexToInt (StateBgScrollingInput.Text);
    }

//----------------------------------------------------------------------------------------

    private void Background_Click (object sender, RoutedEventArgs e)
    {
      var window = new UI.SelectBackgroundWindow (MainProject);
      window.Owner = Window.GetWindow (this);
      window.ShowDialog ();
    }

  }

}
