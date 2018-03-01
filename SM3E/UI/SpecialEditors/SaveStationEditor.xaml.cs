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

    int RoomIndex = -1;
    int DoorIndex = -1;
    bool changesMade = false;
    bool ChangesMade
    {
      get {return changesMade;}
      set
      {
        changesMade = value;
        SaveButton.IsEnabled = changesMade;
      }
    }
    

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
      SaveStationListBox.SelectedIndex = 0;
    }


    private void SaveStation_SelectionChanged (object sender, SelectionChangedEventArgs e)
    {
      SaveStationListBox.ScrollIntoView (SaveStationListBox.SelectedItem);
      if (SaveStationListBox.SelectedIndex == -1)
      {
        RoomGrid.IsEnabled = false;
        DoorGrid.IsEnabled = false;
      }
      else
      {
        RoomGrid.IsEnabled = true;
        DoorGrid.IsEnabled = true;
      }

      RoomListBox.ItemsSource = MainProject.GetRoomNames (AreaSelect.SelectedIndex);
      MainProject.GetSaveStationRoomDoor (AreaSelect.SelectedIndex,
                                          SaveStationListBox.SelectedIndex,
                                          out RoomIndex, out DoorIndex);
      RoomListBox.SelectedIndex = RoomIndex;
      DoorListBox.ItemsSource =
        MainProject.GetIncomingDoorNames (AreaSelect.SelectedIndex, 
                                          RoomListBox.SelectedIndex);
      DoorListBox.SelectedIndex = DoorIndex;

      MainProject.GetSaveStationValues (AreaSelect.SelectedIndex,
                                        SaveStationListBox.SelectedIndex,
                                        out int doorBts, out int screenX, out int screenY,
                                        out int samusX, out int samusY);
      PositionInput.Text = Tools.IntToHex (doorBts, 4);
      SaveXInput.Text    = Tools.IntToHex (screenX, 4);
      SaveYInput.Text    = Tools.IntToHex (screenY, 4);
      SamusXInput.Text   = Tools.IntToHex (samusX , 4);
      SamusYInput.Text   = Tools.IntToHex (samusY , 4);

      ChangesMade = false;
    }


    private void Room_SelectionChanged (object sender, SelectionChangedEventArgs e)
    {
      RoomListBox.ScrollIntoView (RoomListBox.SelectedItem);
      if (RoomListBox.SelectedIndex != RoomIndex)
        ChangesMade = true;
      DoorListBox.ItemsSource =
        MainProject.GetIncomingDoorNames (AreaSelect.SelectedIndex, 
                                          RoomListBox.SelectedIndex);
      DoorListBox.SelectedIndex = -1;
    }


    private void Door_SelectionChanged (object sender, SelectionChangedEventArgs e)
    {
      DoorListBox.ScrollIntoView (DoorListBox.SelectedItem);
      if (DoorListBox.SelectedIndex != DoorIndex)
        ChangesMade = true;
    }
    

    private void Submit (object sender, RoutedEventArgs e)
    {
      TextBox t = (TextBox) sender;
      int i = Tools.HexToInt (t.Text);
      t.Text = Tools.IntToHex (i, 4);
      ChangesMade = true;
    }


    private void Validate (object sender, KeyEventArgs e)
    {
      if (e.Key == Key.Enter)
        Submit (sender, null);
      UITools.ValidateHex (ref e);
    }


    private void SaveClick (object sender, RoutedEventArgs e)
    {
      MainProject.SetSaveRoomReferences (AreaSelect.SelectedIndex,
                                         SaveStationListBox.SelectedIndex,
                                         RoomListBox.SelectedIndex,
                                         DoorListBox.SelectedIndex);
      MainProject.SetSaveStationValues (AreaSelect.SelectedIndex,
                                        SaveStationListBox.SelectedIndex,
                                        Tools.HexToInt (PositionInput.Text),
                                        Tools.HexToInt (SaveXInput.Text),
                                        Tools.HexToInt (SaveYInput.Text),
                                        Tools.HexToInt (SamusXInput.Text),
                                        Tools.HexToInt (SamusYInput.Text));
      int temp = SaveStationListBox.SelectedIndex;
      SaveStationListBox.ItemsSource = 
        MainProject.GetSaveStationNames (AreaSelect.SelectedIndex);
      SaveStationListBox.SelectedIndex = temp;
      SaveStationListBox.ScrollIntoView (SaveStationListBox.SelectedItem);
      ChangesMade = false;
    }

  } // partial class SaveStationEditor

}
