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
  /// Interaction logic for SaveStationEditor.xaml
  /// </summary>
  public partial class SaveStationEditor: Window
  {
    Project MainProject;


    public SaveStationEditor (Project p)
    {
      InitializeComponent ();

      MainProject = p;

      AreaSelect.ItemsSource = MainProject.AreaNames;
      AreaSelect.SelectedIndex = 0;
    }


    private void Area_SelectionChanged (object sender, SelectionChangedEventArgs e)
    {
      SaveStationListBox.ItemsSource = 
        MainProject.GetSaveStationNames (AreaSelect.SelectedIndex);
      SaveStationListBox.ScrollIntoView (SaveStationListBox.SelectedItem);
      SaveStationListBox.SelectedIndex = 0;
    }


    private void SaveStation_SelectionChanged (object sender, SelectionChangedEventArgs e)
    {
      RoomListBox.ItemsSource = MainProject.GetRoomNames (AreaSelect.SelectedIndex);
      MainProject.GetSaveStationRoomDoor (AreaSelect.SelectedIndex,
                                          SaveStationListBox.SelectedIndex,
                                          out int roomIndex, out int doorIndex);
      RoomListBox.SelectedIndex = roomIndex;
      RoomListBox.ScrollIntoView (RoomListBox.SelectedItem);
      DoorListBox.ItemsSource =
        MainProject.GetIncomingDoorNames (AreaSelect.SelectedIndex, 
                                          RoomListBox.SelectedIndex);
      DoorListBox.SelectedIndex = doorIndex;
      DoorListBox.ScrollIntoView (DoorListBox.SelectedItem);
    }


    private void Room_SelectionChanged (object sender, SelectionChangedEventArgs e)
    {
    }


    private void Door_SelectionChanged (object sender, SelectionChangedEventArgs e)
    {
    }


    private void ExitClick (object sender, RoutedEventArgs e)
    {
      DialogResult = true;
      Close ();
    }

  } // partial class SaveStationEditor

}
