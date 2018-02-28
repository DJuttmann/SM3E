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
  /// Interaction logic for PlmLayerTab.xaml
  /// </summary>
  public partial class PlmLayerTab : UserControl
  {
    private Project MainProject;

    private bool QuietSelect = false;

    public event EventHandler AddPlm;


    // Constructor.
    public PlmLayerTab()
    {
      InitializeComponent();
    }


    // Set the project and subscribe to its events.
    public void SetProject (Project p)
    {
      MainProject = p;

      MainProject.PlmListChanged += LoadPlmListBox;
      MainProject.PlmTypeListChanged += LoadPlmTypeListBox;
      MainProject.PlmSelected += LoadPlmData;
      MainProject.PlmSelected += PlmSelected;
      MainProject.PlmTypeSelected += LoadPlmTypeData;
      MainProject.PlmTypeSelected += PlmTypeSelected;
      MainProject.PlmModified += LoadPlmData;
    }


//========================================================================================
// Setup & Updating


    private void LoadPlmListBox (object sender, ListLoadEventArgs e)
    {
      List <string> names = MainProject.PlmNames;
      PlmListBox.Items.Clear ();
      foreach (string name in names)
        PlmListBox.Items.Add (name);
      QuietSelect = true;
      PlmListBox.SelectedIndex = e.SelectItem;
      QuietSelect = false;
    }


    private void PlmSelected (object sender, EventArgs e)
    {
      QuietSelect = true;
      PlmListBox.SelectedIndex = MainProject.PlmIndex;
      QuietSelect = false;
    }


    private void LoadPlmTypeListBox (object sender, ListLoadEventArgs e)
    {
      List <string> names = MainProject.PlmTypeNames;
      PlmTypeListBox.Items.Clear ();
      foreach (string name in names)
        PlmTypeListBox.Items.Add (name);
      QuietSelect = true;
      PlmTypeListBox.SelectedIndex = e.SelectItem;
      QuietSelect = false;
    }


    private void PlmTypeSelected (object sender, EventArgs e)
    {
      QuietSelect = true;
      PlmTypeListBox.SelectedIndex = MainProject.PlmTypeIndex;
      QuietSelect = false;
      IdInput.Text = Tools.IntToHex (MainProject.PlmTypeID, 4);
    }


    private void LoadPlmData (object sender, EventArgs e)
    {
      MainProject.GetPlmPosition (out int x, out int y, out int width, out int height);
      PositionInput.Text = Tools.IntToHex (x, 2) + "," + Tools.IntToHex (y, 2);
    }


    private void LoadPlmTypeData (object sender, EventArgs e)
    {
      PlmName.Content = MainProject.PlmTypeName;
      PlmImage.Source = MainProject.PlmTypeImage?.ToBitmap ();
    }


//========================================================================================
// Event handlers


    private void PlmListBox_SelectionChanged (object sender, SelectionChangedEventArgs e)
    {
      PlmListBox.ScrollIntoView (PlmListBox.SelectedItem);
      if (!QuietSelect)
        MainProject.SelectPlm (PlmListBox.SelectedIndex);
    }


    private void PlmTypeListBox_SelectionChanged (object sender, SelectionChangedEventArgs e)
    {
      PlmTypeListBox.ScrollIntoView (PlmTypeListBox.SelectedItem);
      if (!QuietSelect)
        MainProject.SelectPlmType (PlmTypeListBox.SelectedIndex);
    }


    private void PlmListBox_DoubleClick (object sender, MouseButtonEventArgs e)
    {
      Window window;
      if (MainProject.PlmID == Plm.ScrollID)
        window = new UI.SelectScrollPlmDataWindow (MainProject);
      else
        window = new UI.EditPlmWindow (MainProject);
      window.Owner = Window.GetWindow (this);
      window.ShowDialog ();
    }


    private void AddPlm_Click (object sender, RoutedEventArgs e)
    {
      AddPlm?.Invoke (this, null);
    }


    private void MovePlmUp_Click (object sender, RoutedEventArgs e)
    {
      MainProject.MovePlmUp ();
    }


    private void MovePlmDown_Click (object sender, RoutedEventArgs e)
    {
      MainProject.MovePlmDown ();
    }


    private void DeletePlm_Click (object sender, RoutedEventArgs e)
    {
      MainProject.DeletePlm ();
    }


    private void IdInput_LostFocus (object sender, RoutedEventArgs e)
    {
      MainProject.SelectPlmTypeByID (Tools.HexToInt (IdInput.Text));
    }


    private void IdInput_KeyDown (object sender, KeyEventArgs e)
    {
      if (e.Key == Key.Enter)
        IdInput_LostFocus (sender, null);
      UITools.ValidateHex (ref e);
    }

    
    private void PositionInput_LostFocus (object sender, RoutedEventArgs e)
    {
      string [] values = PositionInput.Text.Split (new char [] {','});
      int x = 0;
      int y = 0;
      if (values.Length > 0)
        x = Tools.HexToInt (values [0]);
      if (values.Length > 1)
        y = Tools.HexToInt (values [1]);
      MainProject.SetPlmPosition (x, y);
    }


    private void PositionInput_KeyDown (object sender, KeyEventArgs e)
    {
      if (e.Key == Key.Enter)
        PositionInput_LostFocus (sender, null);
      UITools.ValidateHexOrComma (ref e);
    }

  } // partial class PlmLayerTab

}
