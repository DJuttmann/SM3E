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
  /// Interaction logic for AsmSelectWindow.xaml
  /// </summary>
  public partial class SelectAsmWindow: Window
  {
    private Project MainProject;
    private string Type;
    List <string> AsmNames;


    // Constructor.
    public SelectAsmWindow (Project p, string type)
    {
      InitializeComponent ();

      MainProject = p;
      Type = type;

      int index = -1;
      switch (type)
      {
      case "setup":
        AsmNames = MainProject.SetupAsmNames;
        index = MainProject.SetupAsmIndex;
        break;
      case "main":
        AsmNames = MainProject.MainAsmNames;
        index = MainProject.MainAsmIndex;
        break;
      default:
        AsmNames = new List <string> ();
        break;
      }
      AsmSelect.ItemsSource = AsmNames;
      AsmSelect.SelectedIndex = index;
      AsmSelect.ScrollIntoView (AsmSelect.SelectedItem);
      AsmCheck.IsChecked = (index >= 0);
      AsmCheck_Click (this, null);
      Title = "Select " + type + " ASM";
      AsmCheck.Content = "Use " + type + " ASM";
    }


    private void AsmCheck_Click (object sender, RoutedEventArgs e)
    {
      if (AsmCheck.IsChecked == true)
        AsmSelect.IsEnabled = true;
      else
        AsmSelect.IsEnabled = false;
    }


    private void Cancel_Click (object sender, RoutedEventArgs e)
    {
      DialogResult = false;
      Close ();
    }


    private void Save_Click (object sender, RoutedEventArgs e)
    {
      int index = AsmSelect.IsEnabled == true ? AsmSelect.SelectedIndex : -1;
      switch (Type)
      {
      case "setup":
        MainProject.SetSetupAsm (index);
        break;
      case "main":
        MainProject.SetMainAsm (index);
        break;
      default:
        DialogResult = false;
        Close ();
        return;
      }

      DialogResult = true;
      Close ();
    }
  }
}
