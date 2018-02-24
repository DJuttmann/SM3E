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
  /// Interaction logic for SelectDoorAsm.xaml
  /// </summary>
  public partial class SelectDoorAsmWindow: Window
  {
    Project MainProject;


    public SelectDoorAsmWindow (Project p)
    {
      InitializeComponent ();


      MainProject = p;

      ExistingRadio.IsChecked = true;

      AreaSelect.ItemsSource = MainProject.AreaNames;
      RoomSelect.ItemsSource = MainProject.RoomNames;
      DoorSelect.ItemsSource = MainProject.DoorNames;
      RegularAsmSelect.ItemsSource = MainProject.DoorAsmNames;

      AreaSelect.SelectedIndex = MainProject.AreaIndex;
      RoomSelect.SelectedIndex = MainProject.RoomIndex;
      DoorSelect.SelectedIndex = MainProject.DoorIndex;
      RegularAsmSelect.SelectedIndex = MainProject.DoorAsmIndex;
      CurrentDataPtr.Text = NewDataPtr.Text;

      switch (MainProject.GetDoorAsmType ())
      {
      case DoorAsmType.None:
        SelectType.SelectedIndex = 0;
        break;
      case DoorAsmType.Regular:
        SelectType.SelectedIndex = 1;
        break;
      case DoorAsmType.Scroll:
        SelectType.SelectedIndex = 2;
        break;
      }
    }


//========================================================================================
// Event handlers.


    private void ExistingRadio_Checked (object sender, RoutedEventArgs e)
    {
      AreaSelect.IsEnabled = true;
      RoomSelect.IsEnabled = true;
      DoorSelect.IsEnabled = true;
      DoorSelect_SelectionChanged (this, null);
    }


    private void NewRadio_Checked (object sender, RoutedEventArgs e)
    {
      AreaSelect.IsEnabled = false;
      RoomSelect.IsEnabled = false;
      DoorSelect.IsEnabled = false;
      NewDataPtr.Text = "<new>";
    }


    private void CopyRadio_Checked (object sender, RoutedEventArgs e)
    {
      AreaSelect.IsEnabled = true;
      RoomSelect.IsEnabled = true;
      DoorSelect.IsEnabled = true;
      DoorSelect_SelectionChanged (this, null);
    }


    private void SelectType_SelectionChanged (object sender, SelectionChangedEventArgs e)
    {
      switch (SelectType.SelectedIndex)
      {
      case 0:
        ScrollArea.Visibility = Visibility.Hidden;
        ScrollRoom.Visibility = Visibility.Hidden;
        ScrollDoor.Visibility = Visibility.Hidden;
        RegularAsm.Visibility = Visibility.Hidden;
        break;
      case 1:
        ScrollArea.Visibility = Visibility.Hidden;
        ScrollRoom.Visibility = Visibility.Hidden;
        ScrollDoor.Visibility = Visibility.Hidden;
        RegularAsm.Visibility = Visibility.Visible;
        break;
      case 2:
        ScrollArea.Visibility = Visibility.Visible;
        ScrollRoom.Visibility = Visibility.Visible;
        ScrollDoor.Visibility = Visibility.Visible;
        RegularAsm.Visibility = Visibility.Hidden;
        break;
      }
    }


    private void AreaSelect_SelectionChanged (object sender, SelectionChangedEventArgs e)
    {
      List <string> names = MainProject.GetRoomNames (AreaSelect.SelectedIndex);
      RoomSelect.ItemsSource = names;
      RoomSelect.SelectedIndex = Math.Min (names.Count - 1, 0);
      RoomSelect_SelectionChanged (this, null);
    }


    private void RoomSelect_SelectionChanged (object sender, SelectionChangedEventArgs e)
    {
      List <string> names = MainProject.GetDoorNames (AreaSelect.SelectedIndex,
                                                      RoomSelect.SelectedIndex);
      DoorSelect.ItemsSource = names;
      DoorSelect.SelectedIndex = Math.Min (names.Count - 1, 0);
      DoorSelect_SelectionChanged (this, null);
    }


    private void DoorSelect_SelectionChanged (object sender, SelectionChangedEventArgs e)
    {
      int ptr = MainProject.GetScrollAsmPtr (AreaSelect.SelectedIndex,
                                             RoomSelect.SelectedIndex,
                                             DoorSelect.SelectedIndex);
      NewDataPtr.Text = Tools.IntToHex (ptr, 6);
      if (CopyRadio.IsChecked == true)
        NewDataPtr.Text += " (copy)";
    }


    private void Cancel_Click (object sender, RoutedEventArgs e)
    {
      DialogResult = false;
      Close ();
    }


    private void Save_Click (object sender, RoutedEventArgs e)
    {
      switch (SelectType.SelectedIndex)
      {
      case 0:
        MainProject.SetRegularDoorAsm (-1);
        break;
      case 1:
        MainProject.SetRegularDoorAsm (RegularAsmSelect.SelectedIndex);
        break;
      case 2:
        bool newData = NewRadio.IsChecked == true || CopyRadio.IsChecked == true;
        MainProject.SetScrollAsm (AreaSelect.SelectedIndex,
                                  RoomSelect.SelectedIndex,
                                  DoorSelect.SelectedIndex, newData);
        break;
      }

      DialogResult = true;
      Close ();
    }
  }
}
