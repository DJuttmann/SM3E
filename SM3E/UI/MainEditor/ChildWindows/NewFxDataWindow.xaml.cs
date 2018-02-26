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
  /// Interaction logic for NewFxDataWindow.xaml
  /// </summary>
  public partial class NewFxDataWindow: Window
  {
    Project MainProject;

    public NewFxDataWindow (Project p)
    {
      InitializeComponent ();

      MainProject = p;

      DoorSelect.ItemsSource = MainProject.IncomingDoorNames;
      DoorSelect.SelectedIndex = 0;
    }


    private void Cancel_Click (object sender, RoutedEventArgs e)
    {
      DialogResult = false;
      Close ();
    }


    private void Create_Click (object sender, RoutedEventArgs e)
    {
      MainProject.AddFxData (DoorSelect.SelectedIndex);

      DialogResult = true;
      Close ();
    }


    private void DoorSelect_SelectionChanged (object sender, SelectionChangedEventArgs e)
    {
      if (DoorSelect.SelectedIndex == -1)
        CreateButton.IsEnabled = false;
      else
        CreateButton.IsEnabled = true;
    }

  }

}
