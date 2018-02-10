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
  /// Interaction logic for TileLayersTab.xaml
  /// </summary>
  public partial class TileLayersTab : UserControl
  {
    private Project MainProject;

    private UITileViewer TileSelector;
    private UITileViewer BtsSelector;

    bool QuietSelect = false;


    // Constructor.
    public TileLayersTab()
    {
      InitializeComponent();

      SetupTileSelector ();
      SetupBtsSelector ();
    }


    // Set the project and subscribe to its events.
    public void SetProject (Project p)
    {
      MainProject = p;

      MainProject.TileSetSelected += UpdateTileSelector;
      MainProject.TileSelected += UpdateActiveTile;
      MainProject.BtsSelected += UpdateActiveBts;
    }


//========================================================================================
// Setup & Updating


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
// events


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

  } // partial class TileLayersTab

}
