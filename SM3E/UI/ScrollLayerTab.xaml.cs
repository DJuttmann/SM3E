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

namespace SM3E.UI
{
  /// <summary>
  /// Interaction logic for ScrollLayerTab.xaml
  /// </summary>
  public partial class ScrollLayerTab: UserControl
  {
    private Project MainProject;

    bool QuietSelect = false;


    // Constructor.
    public ScrollLayerTab()
    {
      InitializeComponent();
    }


    // Set the project and subscribe to its events.
    public void SetProject (Project p)
    {
      MainProject = p;

      MainProject.ScrollDataListChanged += LoadScrollDataListBox;
      MainProject.ScrollColorListChanged += LoadScrollColorListBox;
      MainProject.ScrollDataSelected += ScrollDataSelected;
      MainProject.ScrollColorSelected += ScrollColorSelected;
    }


//========================================================================================
// Setup & Updating


    private void LoadScrollDataListBox (object sender, ListLoadEventArgs e)
    {
      List <string> names = MainProject.ScrollDataNames;
      ScrollDataListBox.Items.Clear ();
      foreach (string name in names)
        ScrollDataListBox.Items.Add (name);
      QuietSelect = true;
      ScrollDataListBox.SelectedIndex = e.SelectItem;
      QuietSelect = false;
    }


    private void ScrollDataSelected (object sender, EventArgs e)
    {
      QuietSelect = true;
      ScrollDataListBox.SelectedIndex = MainProject.ScrollDataIndex;
      QuietSelect = false;
    }


    private void LoadScrollColorListBox (object sender, ListLoadEventArgs e)
    {
      List <string> names = MainProject.ScrollColorNames;
      ScrollColorListBox.Items.Clear ();
      foreach (string name in names)
        ScrollColorListBox.Items.Add (name);
      QuietSelect = true;
      ScrollColorListBox.SelectedIndex = e.SelectItem;
      QuietSelect = false;
    }


    private void ScrollColorSelected (object sender, EventArgs e)
    {
      QuietSelect = true;
      ScrollColorListBox.SelectedIndex = MainProject.ScrollColorIndex;
      QuietSelect = false;
    }


//========================================================================================
// Event handlers


    private void ScrollDataListBox_SelectionChanged (object sender, SelectionChangedEventArgs e)
    {
      ScrollDataListBox.ScrollIntoView (ScrollDataListBox.SelectedItem);
      if (!QuietSelect)
        MainProject.SelectScrollData (ScrollDataListBox.SelectedIndex);
    }


    private void ScrollColorListBox_SelectionChanged (object sender, SelectionChangedEventArgs e)
    {
      ScrollColorListBox.ScrollIntoView (ScrollColorListBox.SelectedItem);
      if (!QuietSelect)
        MainProject.SelectScrollColor (ScrollColorListBox.SelectedIndex);
    }

  } // partial class ScrollLayerTab

}
