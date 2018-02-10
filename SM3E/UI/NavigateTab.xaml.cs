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
  /// Interaction logic for NavigateTab.xaml
  /// </summary>
  public partial class NavigateTab : UserControl
  {
    private Project MainProject;
    private UITileViewer LevelData; // [wip] Make this unnecessary through events?

    public UITileViewer MapTileSelector;
    public UITileViewer MapEditor;

    private bool QuietSelect = false;


    // Constructor.
    public NavigateTab()
    {
      InitializeComponent();

      SetupMapEditor ();
      SetupMapTileSelector ();
    }


    public void Init (Project p, UITileViewer levelData)
    {
      MainProject = p;
      LevelData = levelData;

      MainProject.DoorListChanged += LoadDoorListBox;
      MainProject.AreaSelected += UpdateMapEditor;
      MainProject.DoorSelected += DoorSelected;
      MainProject.MapTileSelected += UpdateActiveMapTile;
      MainProject.MapPaletteSelected += UpdateMapTileSelector;
      MainProject.MapDataModified += UpdateMapEditor;
    }

//========================================================================================


    private void SetupMapTileSelector ()
    {
      MapTileSelector = new UITileViewer (8.0, 16, 16, 16, 16, null);
      MapTileSelector.MouseDown += MapTileSelector_MouseDown;
      MapTileViewer.Children.Add (MapTileSelector.Element);
    }


    private void SetupMapEditor ()
    {
      MapEditor = new UITileViewer (8.0, 64, 32, 64, 32, null);
      MapEditor.MarkerVisible = true;
      MapEditor.MouseDown += MapEditor_MouseDown;
      MapEditor.MouseUp += MapEditor_MouseUp;
      MapViewer.Children.Add (MapEditor.Element);
      SelectedMapTileImage.RenderTransformOrigin = new Point (0.5, 0.5);
    }


    private void UpdateMapTileSelector (object sender, EventArgs e)
    {
      ImageSource source = MainProject.MapTileSheet.ToBitmap ();
      MapTileSelector.Screens [0, 0].Source = source;
    }


    private void UpdateMapEditor (object sender, EventArgs e)
    {
      ImageSource source = MainProject.RenderAreaMap ().ToBitmap ();
      MapEditor.Screens [0, 0].Source = source;
    }


    private void UpdateActiveMapTile (object sender, EventArgs e)
    {
      int index = MainProject.MapTileType;
      double hFlip = MainProject.MapTileHFlip ? -1.0 : 1.0;
      double vFlip = MainProject.MapTileVFlip ? -1.0 : 1.0;
      if (index != -1)
        SelectedMapTileImage.Source = MainProject.MapTiles [index].ToBitmap ();
      SelectedMapTileImage.RenderTransform = new ScaleTransform (hFlip, vFlip);
    }


    private void LoadDoorListBox (object sender, ListLoadEventArgs e)
    {
      List <string> names = MainProject.DoorNames;
      DoorListBox.Items.Clear ();
      foreach (string name in names)
        DoorListBox.Items.Add (name);
      QuietSelect = true;
      DoorListBox.SelectedIndex = e.SelectItem;
      QuietSelect = false;
    }


    private void DoorSelected (object sender, EventArgs e)
    {
      QuietSelect = true;
      DoorListBox.SelectedIndex = MainProject.DoorIndex;
      QuietSelect = false;
    }


//========================================================================================
// Events

    
    private void DoorListBox_SelectionChanged (object sender, SelectionChangedEventArgs e)
    {
      DoorListBox.ScrollIntoView (DoorListBox.SelectedItem);
      if (!QuietSelect)
        MainProject.SelectDoor (DoorListBox.SelectedIndex);
    }


    private void DoorListBox_DoubleClick (object sender, MouseButtonEventArgs e)
    {
      var window = new UI.EditDoorWindow (MainProject, false);
      window.Owner = Window.GetWindow (this);
      window.ShowDialog ();
    }

//----------------------------------------------------------------------------------------
// Map viewer & tile selector

    private void MapNavigateRadio_Click (object sender, RoutedEventArgs e)
    {
      MapEditor.MarkerVisible = true;
      MapEditor.Element.SetValue (Grid.CursorProperty, Cursors.Hand);
    }


    private void MapEditRadio_Click (object sender, RoutedEventArgs e)
    {
      MapEditor.MarkerVisible = false;
      MapEditor.Element.SetValue (Grid.CursorProperty, Cursors.Arrow);
    }


    private void MapEditVFlip_Click (object sender, RoutedEventArgs e)
    {
      MainProject.MapTileVFlip = !MainProject.MapTileVFlip;
    }


    private void MapEditHFlip_Click (object sender, RoutedEventArgs e)
    {
      MainProject.MapTileHFlip = !MainProject.MapTileHFlip;
    }


    private void MapEditor_MouseDown (object sender, TileViewerMouseEventArgs e)
    {
      if (MapNavigateRadio.IsChecked == true)
      {
        if (MainProject.NavigateToMapPosition (e.TileClickX, e.TileClickY - 1,
                                               out int screenX, out int screenY))
          LevelData.ScrollToScreen (screenX, screenY);
      }
    }


    private void MapEditor_MouseUp (object sender, TileViewerMouseEventArgs e)
    {
      if (MapEditRadio.IsChecked == true)
      {
        MainProject.SetMapTile (e.TileClickX, e.TileClickY, e.PosTileX, e.PosTileY);
      }
    }


    private void MapTileSelector_MouseDown (object sender, TileViewerMouseEventArgs e)
    {
      MainProject.MapTilePalette = MapPaletteSelect.SelectedIndex;
      MainProject.MapTileType = 16 * e.TileClickY + e.TileClickX;
    }


    private void MapPaletteSelect_SelectionChanged (object sender, SelectionChangedEventArgs e)
    {
      if (MainProject != null)
        MainProject.MapTilePalette = MapPaletteSelect.SelectedIndex;
    }

  } // class NavigateTab

}
