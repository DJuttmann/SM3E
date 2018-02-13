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
  /// Interaction logic for SelectBackgroundWindow.xaml
  /// </summary>
  public partial class SelectBackgroundWindow: Window
  {
    Project MyProject;


    // Constructor.
    public SelectBackgroundWindow (Project p)
    {
      InitializeComponent ();

      MyProject = p;

      MyProject.GetBackgroundStatus (out bool HasBackground, out bool HasLayer2);
      DefaultBgCheck.IsChecked = HasBackground;
      Layer2Check.IsChecked = HasLayer2;
      BackgroundSelect.ItemsSource = MyProject.BackgroundNames;
      if (!HasBackground)
      {
        BackgroundSelect.IsEnabled = false;
        BackgroundImage.Visibility = Visibility.Hidden;
      }
      else
      {
        BackgroundSelect.SelectedIndex = MyProject.BackgroundIndex;
      }
    }


    private void BackgroundSelect_SelectionChanged (object sender, SelectionChangedEventArgs e)
    {
      BlitImage img = MyProject.RenderBackground (BackgroundSelect.SelectedIndex);
      BackgroundImage.Source = img.ToBitmap ();
    }


    private void DefaultBgCheck_Click (object sender, RoutedEventArgs e)
    {
      if (DefaultBgCheck.IsChecked == true)
      {
        BackgroundSelect.IsEnabled = true;
        BackgroundImage.Visibility = Visibility.Visible;
      }
      else
      {
        BackgroundSelect.IsEnabled = false;
        BackgroundImage.Visibility = Visibility.Hidden;
      }
    }

    private void Cancel_Click (object sender, RoutedEventArgs e)
    {
      DialogResult = false;
      Close ();
    }


    private void Save_Click (object sender, RoutedEventArgs e)
    {
      int index = DefaultBgCheck.IsChecked == true ? BackgroundSelect.SelectedIndex : -1;
      MyProject.SetBackgroundStatus (index, Layer2Check.IsChecked == true);

      DialogResult = true;
      Close ();
    }

  }

}
