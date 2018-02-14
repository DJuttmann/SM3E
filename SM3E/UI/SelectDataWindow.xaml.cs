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
  /// Interaction logic for SelectDataWindow.xaml
  /// </summary>
  public partial class SelectDataWindow: Window
  {
    Project MainProject;
    string Type;


    public SelectDataWindow (Project p, string type)
    {
      InitializeComponent ();

      MainProject = p;
      Type = type;

      ExistingRadio.IsChecked = true;
      if (type == "scroll set")
        ScrollsSelectDefault.Visibility = Visibility.Visible;

      AreaSelect.ItemsSource = MainProject.AreaNames;
      RoomSelect.ItemsSource = MainProject.RoomNames;
      StateSelect.ItemsSource = MainProject.RoomStateNames;

      AreaSelect.SelectedIndex = MainProject.AreaIndex;
      RoomSelect.SelectedIndex = MainProject.RoomIndex;
      StateSelect.SelectedIndex = MainProject.RoomStateIndex;
      Title = "Select " + Type;
      CurrentDataPtr.Text = NewDataPtr.Text;
    }


//----------------------------------------------------------------------------------------

    private void NoneRadio_Checked (object sender, RoutedEventArgs e)
    {
      AreaSelect.IsEnabled = false;
      RoomSelect.IsEnabled = false;
      StateSelect.IsEnabled = false;
      if (Type == "scroll set")
        NewDataPtr.Text = ScrollsSelectDefault.SelectedIndex == 0 ? "All blue" : "All green";
      else
      NewDataPtr.Text = "000000";
    }


    private void ExistingRadio_Checked (object sender, RoutedEventArgs e)
    {
      AreaSelect.IsEnabled = true;
      RoomSelect.IsEnabled = true;
      StateSelect.IsEnabled = true;
      StateSelect_SelectionChanged (this, null);
    }


    private void NewRadio_Checked (object sender, RoutedEventArgs e)
    {
      AreaSelect.IsEnabled = false;
      RoomSelect.IsEnabled = false;
      StateSelect.IsEnabled = false;
      NewDataPtr.Text = "<new>";
    }


    private void CopyRadio_Checked (object sender, RoutedEventArgs e)
    {
      AreaSelect.IsEnabled = true;
      RoomSelect.IsEnabled = true;
      StateSelect.IsEnabled = true;
      StateSelect_SelectionChanged (this, null);
    }
    

    private void ScrollsSelectDefault_SelectionChanged (object sender, SelectionChangedEventArgs e)
    {
      if (NoneRadio.IsChecked == true)
        NewDataPtr.Text = ScrollsSelectDefault.SelectedIndex == 0 ? "All blue" : "All green";
      if (ExistingRadio.IsChecked == true || CopyRadio.IsChecked == true)
        StateSelect_SelectionChanged (this, null);
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
      List <string> names = MainProject.GetRoomStateNames (AreaSelect.SelectedIndex,
                                                           RoomSelect.SelectedIndex);
      StateSelect.ItemsSource = names;
      StateSelect.SelectedIndex = names.Count - 1;
      StateSelect_SelectionChanged (this, null);
    }


    private void StateSelect_SelectionChanged (object sender, SelectionChangedEventArgs e)
    {
      int ptr = 0;
      switch (Type)
      {
      case "level data":
        ptr = MainProject.GetLevelDataPtr (AreaSelect.SelectedIndex,
                                           RoomSelect.SelectedIndex,
                                           StateSelect.SelectedIndex);
        break;
      case "scroll set":
        ptr = MainProject.GetScrollSetPtr (AreaSelect.SelectedIndex,
                                           RoomSelect.SelectedIndex,
                                           StateSelect.SelectedIndex);
        break;
      case "plm set":
        ptr = MainProject.GetPlmSetPtr    (AreaSelect.SelectedIndex,
                                           RoomSelect.SelectedIndex,
                                           StateSelect.SelectedIndex);
        break;
      case "enemy set":
        ptr = MainProject.GetEnemySetPtr  (AreaSelect.SelectedIndex,
                                           RoomSelect.SelectedIndex,
                                           StateSelect.SelectedIndex);
        break;
      case "enemy gfx":
        ptr = MainProject.GetEnemyGfxPtr  (AreaSelect.SelectedIndex,
                                           RoomSelect.SelectedIndex,
                                           StateSelect.SelectedIndex);
        break;
      case "effects":
        ptr = MainProject.GetFxPtr        (AreaSelect.SelectedIndex,
                                           RoomSelect.SelectedIndex,
                                           StateSelect.SelectedIndex);
        break;
      default:
        NewDataPtr.Text = String.Empty;
        return;
      }

      if (Type == "scroll set" && ptr == 0)
        NewDataPtr.Text = ScrollsSelectDefault.SelectedIndex == 0 ? "All blue" : "All green";
      else
      {
        NewDataPtr.Text = Tools.IntToHex (ptr, 6);
        if (CopyRadio.IsChecked == true)
          NewDataPtr.Text += " (copy)";
      }
    }


    private void Cancel_Click (object sender, RoutedEventArgs e)
    {
      DialogResult = false;
      Close ();
    }


    private void Save_Click (object sender, RoutedEventArgs e)
    {
      if (NoneRadio.IsChecked == true)
        SaveNone ();
      if (NewRadio.IsChecked == true)
        SaveNew ();
      if (CopyRadio.IsChecked == true)
        SaveCopy ();
      if (ExistingRadio.IsChecked == true)
        SaveExisting ();

      DialogResult = true;
      Close ();
    }


    private void SaveNone ()
    {
      AreaSelect.SelectedIndex = -1;
      SaveExisting ();
    }


    private void SaveNew ()
    {
    }


    private void SaveCopy ()
    {
    }


    private void SaveExisting ()
    {
      int area = AreaSelect.SelectedIndex;
      int room = RoomSelect.SelectedIndex;
      int state = StateSelect.SelectedIndex;

      switch (Type)
      {
      case "level data":
        MainProject.SetLevelData (area, room, state);
        break;
      case "scroll set":
        int defaultColor = ScrollsSelectDefault.SelectedIndex == 0 ?
                           ScrollSet.AllBlue : ScrollSet.AllGreen;
        MainProject.SetScrollSet (area, room, state, defaultColor);
        break;
      case "plm set":
        MainProject.SetPlmSet (area, room, state);
        break;
      case "enemy set":
        MainProject.SetEnemySet (area, room, state);
        break;
      case "enemy gfx":
        MainProject.SetEnemyGfx (area, room, state);
        break;
      case "effects":
        MainProject.SetFx (area, room, state);
        break;
      }
    }

  } // partial class SelectDataWindow

}
