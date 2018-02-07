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
  /// Interaction logic for EditDoorWindow.xaml
  /// </summary>
  public partial class EditDoorWindow : Window
  {
    private Project MyProject;
    private UITileViewer ScreenPreview;
    private BlitImage ScreenImage;
    private int RoomWidth;
    private int RoomHeight;
    private int ScreenX;
    private int ScreenY;
    private int DoorCapX;
    private int DoorCapY;



    // Constructor.
    public EditDoorWindow (Project p, bool createNew)
    {
      InitializeComponent ();

      MyProject = p;
      ScreenPreview = new UITileViewer (16.0, 16, 16, 16, 16, ScreenPreviewGrid);
      ScreenPreviewGrid.Children.Add (ScreenPreview.Element);
      ScreenPreview.MouseDown += Tile_Click;
      ScreenPreview.MarkerVisible = true;
      ScreenImage = new BlitImage (256, 256);
      foreach (string name in MyProject.AreaNames)
        AreaListBox.Items.Add (name);

      if (createNew)
      {
        AreaListBox.SelectedIndex = MyProject.AreaIndex;
      }
      else
      {
        MyProject.GetDoorDestination (out int areaIndex, out int roomIndex,
                                      out int screenX, out int screenY,
                                      out int doorCapX, out int doorCapY,
                                      out int distanceToSpawn);
        AreaListBox.SelectedIndex = areaIndex;
        RoomListBox.SelectedIndex = roomIndex;
        ScreenX = screenX;
        ScreenY = screenY;
        DoorCapX = doorCapX;
        DoorCapY = doorCapY;
        SpawnDistanceInput.Text = Tools.IntToHex (distanceToSpawn, 4);
        RenderScreen ();
        UpdateDoorCap ();
        UpdateButtons ();
      }
    }


    private void RenderScreen ()
    {
      MyProject.RenderScreen (AreaListBox.SelectedIndex,
                              RoomListBox.SelectedIndex,
                              ScreenImage, ScreenX, ScreenY);
      ScreenPreview.Screens [0, 0].Source = ScreenImage.ToBitmap ();
    }


    private void UpdateDoorCap ()
    {
      ScreenPreview.SetMarker (DoorCapX & 0xF, DoorCapY & 0xF, 1, 1);
    }


    private void UpdateButtons ()
    {
      if (ScreenX == 0)
        ButtonLeft.IsEnabled = false;
      else
        ButtonLeft.IsEnabled = true;

      if (ScreenX == RoomWidth - 1)
        ButtonRight.IsEnabled = false;
      else
        ButtonRight.IsEnabled = true;

      if (ScreenY == 0)
        ButtonUp.IsEnabled = false;
      else
        ButtonUp.IsEnabled = true;

      if (ScreenY == RoomHeight - 1)
        ButtonDown.IsEnabled = false;
      else
        ButtonDown.IsEnabled = true;
    }


    private void AreaListBox_SelectionChanged (object sender, SelectionChangedEventArgs e)
    {
      RoomListBox.Items.Clear ();
      foreach (string name in MyProject.GetRoomNames (AreaListBox.SelectedIndex))
        RoomListBox.Items.Add (name);
      RoomListBox.SelectedIndex = 0;
    }


    private void RoomListBox_SelectionChanged (object sender, SelectionChangedEventArgs e)
    {
      MyProject.GetRoomSizeInScreens (AreaListBox.SelectedIndex,
                                      RoomListBox.SelectedIndex,
                                      out RoomWidth, out RoomHeight);
      ScreenX = 0;
      ScreenY = 0;
      RenderScreen ();
      UpdateButtons ();
    }


    private void ButtonUp_Click (object sender, RoutedEventArgs e)
    {
      if (ScreenY > 0)
      {
        ScreenY--;
        RenderScreen ();
        UpdateButtons ();
      }
    }


    private void ButtonDown_Click (object sender, RoutedEventArgs e)
    {
      if (ScreenY < RoomHeight - 1)
      {
        ScreenY++;
        RenderScreen ();
        UpdateButtons ();
      }
    }


    private void ButtonLeft_Click (object sender, RoutedEventArgs e)
    {
      if (ScreenX > 0)
      {
        ScreenX--;
        RenderScreen ();
        UpdateButtons ();
      }
    }


    private void ButtonRight_Click (object sender, RoutedEventArgs e)
    {
      if (ScreenX < RoomWidth - 1)
      {
        ScreenX++;
        RenderScreen ();
        UpdateButtons ();
      }
    }


    private void Tile_Click (object sender, TileViewerMouseEventArgs e)
    {
      DoorCapX = e.TileClickX;
      DoorCapY = e.TileClickY;
      UpdateDoorCap ();
    }


    private void Save_Click (object sender, RoutedEventArgs e)
    {
      // [wip] save the changes.

      DialogResult = true;
      Close ();
    }


    private void Cancel_Click (object sender, RoutedEventArgs e)
    {
      DialogResult = false;
      Close ();
    }

  } // partial class EditDoorWindow

}
