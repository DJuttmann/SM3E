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
  /// Interaction logic for EditPlmWindow.xaml
  /// </summary>
  public partial class EditPlmWindow: Window
  {
    Project MainProject;


    public EditPlmWindow (Project p)
    {
      InitializeComponent ();

      MainProject = p;

      MainProject.GetPlmProperties (out int properties, out int index);
      PropertiesInput.Text = Tools.IntToHex (properties, 2);
      IndexInput.Text = Tools.IntToHex (index, 2);
    }


    private void Save_Click (object sender, RoutedEventArgs e)
    {
      MainProject.SetPlmProperties (Tools.HexToInt (PropertiesInput.Text),
                                    Tools.HexToInt (IndexInput.Text));
      DialogResult = true;
      return;
    }


    private void Cancel_Click (object sender, RoutedEventArgs e)
    {
      DialogResult = false;
      return;
    }
  }
}
