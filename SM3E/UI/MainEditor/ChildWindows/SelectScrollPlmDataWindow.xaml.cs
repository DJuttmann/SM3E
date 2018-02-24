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
  /// Interaction logic for SelectScrollPlmDataWindow.xaml
  /// </summary>
  public partial class SelectScrollPlmDataWindow: Window
  {
    Project MainProject;

    
    public SelectScrollPlmDataWindow (Project p)
    {
      InitializeComponent ();

      MainProject = p;

      ExistingRadio.IsChecked = true;

      AreaSelect.ItemsSource = MainProject.AreaNames;
      RoomSelect.ItemsSource = MainProject.RoomNames;
      StateSelect.ItemsSource = MainProject.RoomStateNames;
      ScrollPlmSelect.ItemsSource = MainProject.PlmNames;

      AreaSelect.SelectedIndex = MainProject.AreaIndex;
      RoomSelect.SelectedIndex = MainProject.RoomIndex;
      StateSelect.SelectedIndex = MainProject.RoomStateIndex;
      ScrollPlmSelect.SelectedIndex = MainProject.ScrollPlmIndex;
      CurrentDataPtr.Text = NewDataPtr.Text;
    }


//========================================================================================
// Event handlers.


    private void NoneRadio_Checked (object sender, RoutedEventArgs e)
    {
      AreaSelect.IsEnabled = false;
      RoomSelect.IsEnabled = false;
      StateSelect.IsEnabled = false;
      ScrollPlmSelect.IsEnabled = false;
      NewDataPtr.Text = "000000";
    }


    private void ExistingRadio_Checked (object sender, RoutedEventArgs e)
    {
      AreaSelect.IsEnabled = true;
      RoomSelect.IsEnabled = true;
      StateSelect.IsEnabled = true;
      ScrollPlmSelect.IsEnabled = true;
      ScrollPlmSelect_SelectionChanged (this, null);
    }


    private void NewRadio_Checked (object sender, RoutedEventArgs e)
    {
      AreaSelect.IsEnabled = false;
      RoomSelect.IsEnabled = false;
      StateSelect.IsEnabled = false;
      ScrollPlmSelect.IsEnabled = false;
      NewDataPtr.Text = "<new>";
    }


    private void CopyRadio_Checked (object sender, RoutedEventArgs e)
    {
      AreaSelect.IsEnabled = true;
      RoomSelect.IsEnabled = true;
      StateSelect.IsEnabled = true;
      ScrollPlmSelect.IsEnabled = true;
      ScrollPlmSelect_SelectionChanged (this, null);
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
      List <string> names = MainProject.GetScrollPlmNames (AreaSelect.SelectedIndex,
                                                           RoomSelect.SelectedIndex,
                                                           StateSelect.SelectedIndex);
      ScrollPlmSelect.ItemsSource = names;
      ScrollPlmSelect.SelectedIndex = Math.Min (names.Count - 1, 0);
      ScrollPlmSelect_SelectionChanged (this, null);
    }


    private void ScrollPlmSelect_SelectionChanged (object sender, SelectionChangedEventArgs e)
    {
      int ptr = MainProject.GetScrollPlmDataPtr (AreaSelect.SelectedIndex,
                                                 RoomSelect.SelectedIndex,
                                                 StateSelect.SelectedIndex,
                                                 ScrollPlmSelect.SelectedIndex);
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
      int area = AreaSelect.SelectedIndex;
      int room = RoomSelect.SelectedIndex;
      int state = StateSelect.SelectedIndex;
      int scrollPlm = ScrollPlmSelect.SelectedIndex;

      DialogResult = true;
      if (NoneRadio.IsChecked == true)
        MainProject.SetScrollPlmData (-1, -1, -1, scrollPlm, false);
      else if (ExistingRadio.IsChecked == true)
        MainProject.SetScrollPlmData (area, room, state, scrollPlm, false);
      else if (NewRadio.IsChecked == true)
        MainProject.SetScrollPlmData (-1, -1, -1, scrollPlm, true);
      else if (CopyRadio.IsChecked == true)
        MainProject.SetScrollPlmData (area, room, state, scrollPlm, true);
      else
        DialogResult = false;

      Close ();
    }

  } // partial class SelectScrollPlmDataWindow

}
