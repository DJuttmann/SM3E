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
using System.IO;

namespace SM3E
{
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow: Window
  {
    // Fields
    Project MainProject;

    // Constructor
    public MainWindow ()
    {
      InitializeComponent ();

      if (!Logging.Open ())
      {
        System.Windows.MessageBox.Show ("log failed");
        return;
      }
      MainProject = new Project ();

      EditorView.SetProject (MainProject);


    }


    private void MainWindow_Close (object sender, EventArgs e)
    {
      MainProject.Save ();
      Logging.Close ();
    }

   
    private void NewProject_Click (object sender, RoutedEventArgs e)
    {
      var window = new UI.NewProjectWindow (MainProject);
      window.Owner = Window.GetWindow (this);
      window.ShowDialog ();
    }


    private void OpenProject_Click (object sender, RoutedEventArgs e)
    {
      var Open = new Microsoft.Win32.OpenFileDialog ()
      {
        Filter = "Project files (*.sm3p)|*.sm3p"
        // InitialDirectory = Directory.GetCurrentDirectory ()
      };
      if (Open.ShowDialog (Window.GetWindow (this)) ?? false)
      {
        MainProject.Load (Open.FileName);
      }
    }


    private void SaveProject_Click (object sender, RoutedEventArgs e)
    {

    }

  } // partial class MainWindow

}
