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
using System.Windows.Shapes;

namespace SM3E.UI
{
  /// <summary>
  /// Interaction logic for EditRoomPositionWindow.xaml
  /// </summary>
  public partial class EditRoomPositionWindow: Window
  {

    private Project MainProject;
    private UITileViewer RoomSizeEditor;
    bool SetSize;

    int NewPositionX;
    int NewPositionY;
    int NewWidth;
    int NewHeight;


    public EditRoomPositionWindow (Project p, bool setSize)
    {
      InitializeComponent ();

      MainProject = p;
      SetSize = setSize;

      NewPositionX = MainProject.RoomX;
      NewPositionY = MainProject.RoomY + 1;
      NewWidth = MainProject.RoomWidthInScreens;
      NewHeight = MainProject.RoomHeightInScreens;

      RoomSizeEditor = new UITileViewer (16.0, 64, 32, 64, 32, MapViewer);
      RoomSizeEditor.MarkerVisible = true;
      RoomSizeEditor.Screens [0, 0].SetValue (RenderOptions.BitmapScalingModeProperty,
                                              BitmapScalingMode.NearestNeighbor);
      MapViewer.Content = RoomSizeEditor.Element;
      BlitImage areaMap = MainProject.RenderAreaMap ();
      areaMap.DrawRectangle (NewPositionX * 8, NewPositionY * 8, NewWidth * 8, NewHeight * 8,
                             0x00, 0xFF, 0x00, 0x40);
      ImageSource source = areaMap.ToBitmap ();
      RoomSizeEditor.Screens [0, 0].Source = source;

      if (SetSize)
      {
        RoomSizeEditor.MouseUp += RoomSizeEditor_MouseUp;
        Title = "Crop/expand room to new size";
      }
      else
      {
        RoomSizeEditor.MouseDown += RoomSizeEditor_MouseDown;
        Title = "Set room position on map";
      }

      UpdateSelection ();
      ContentRendered += ScrollToMarker;
    }

//========================================================================================
// Updating


    private void UpdateSelection ()
    {
      RoomSizeEditor.SetMarker (NewPositionX, NewPositionY, NewWidth, NewHeight);
    }


    private void ScrollToMarker (object sender, EventArgs e)
    {
      RoomSizeEditor.ScrollToMarker ();
    }


//========================================================================================
// Event handlers


    private void RoomSizeEditor_MouseDown (object sender, TileViewerMouseEventArgs e)
    {
      if (e.Button == MouseButton.Left && e.TileClickY > 0)
      {
        NewPositionX = e.TileClickX;
        NewPositionY = e.TileClickY;
      }
      UpdateSelection ();
    }


    private void RoomSizeEditor_MouseUp (object sender, TileViewerMouseEventArgs e)
    {
      if (e.Button == MouseButton.Left && e.TileClickY > 0)
      {
        NewPositionX = Math.Min (e.TileClickX, e.PosTileX);
        NewPositionY = Math.Min (e.TileClickY, e.PosTileY);
        NewWidth = Math.Min (Math.Abs (e.PosTileX - e.TileClickX) + 1, 15);
        NewHeight = Math.Min (Math.Abs (e.PosTileY - e.TileClickY) + 1, 15);
        double ratio = (double) NewWidth / (double) NewHeight;
        while (NewWidth * NewHeight > 50)
        {
          if (Math.Abs (ratio * (NewHeight - 1) - NewWidth) < 
              Math.Abs (ratio * (NewHeight) - NewWidth + 1))
            NewHeight--;
          else
            NewWidth--;
        }
      }
      UpdateSelection ();
    }


    private void Save_Click (object sender, RoutedEventArgs e)
    {
      if (SetSize)
      {
        MainProject.SetRoomSize (NewPositionX, NewPositionY - 1, NewWidth, NewHeight);
      }
      else
      {
        MainProject.RoomX = NewPositionX;
        MainProject.RoomY = NewPositionY - 1;
      }

      DialogResult = true;
      Close ();
    }


    private void Cancel_Click (object sender, RoutedEventArgs e)
    {
      DialogResult = false;
      Close ();
    }

  }

}
