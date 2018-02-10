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

      MainProject.Load ("SuperMetroid.xml");

    }


    private void MainWindow_Close (object sender, EventArgs e)
    {
      MainProject.Save ();
      Logging.Close ();
    }

  } // partial class MainWindow

}
