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
  /// Interaction logic for EnemyLayer.xaml
  /// </summary>
  public partial class EnemyLayerTab: UserControl
  {
    private Project MainProject;

    bool QuietSelect = false;

    public event EventHandler AddEnemy;


    // Constructor.
    public EnemyLayerTab ()
    {
      InitializeComponent();
    }


    // Set the project and subscribe to its events.
    public void SetProject (Project p)
    {
      MainProject = p;

      MainProject.EnemyListChanged += LoadEnemyListBox;
      MainProject.EnemyGfxListChanged += LoadEnemyGfxListBox;
      MainProject.EnemyTypeListChanged += LoadEnemyTypeListBox;
      MainProject.EnemySelected += LoadEnemyData;
      MainProject.EnemySelected += EnemySelected;
      MainProject.EnemyGfxSelected += LoadEnemyGfxData;
      MainProject.EnemyGfxSelected += EnemyGfxSelected;
      MainProject.EnemyTypeSelected += LoadEnemyTypeData;
      MainProject.EnemyTypeSelected += EnemyTypeSelected;
      MainProject.EnemyModified += LoadEnemyData;
    }


//========================================================================================
// Setup & Updating


    private void LoadEnemyListBox (object sender, ListLoadEventArgs e)
    {
      List <string> names = MainProject.EnemyNames;
      EnemyListBox.Items.Clear ();
      foreach (string name in names)
        EnemyListBox.Items.Add (name);
      QuietSelect = true;
      EnemyListBox.SelectedIndex = e.SelectItem;
      QuietSelect = false;
    }


    private void EnemySelected (object sender, EventArgs e)
    {
      QuietSelect = true;
      EnemyListBox.SelectedIndex = MainProject.EnemyIndex;
      QuietSelect = false;
    }


    private void LoadEnemyGfxListBox (object sender, ListLoadEventArgs e)
    {
      List <string> names = MainProject.EnemyGfxNames;
      EnemyGfxListBox.Items.Clear ();
      foreach (string name in names)
        EnemyGfxListBox.Items.Add (name);
      QuietSelect = true;
      EnemyGfxListBox.SelectedIndex = e.SelectItem;
      QuietSelect = false;
    }


    private void EnemyGfxSelected (object sender, EventArgs e)
    {
      QuietSelect = true;
      EnemyGfxListBox.SelectedIndex = MainProject.EnemyGfxIndex;
      QuietSelect = false;
    }


    private void LoadEnemyTypeListBox (object sender, ListLoadEventArgs e)
    {
      List <string> names = MainProject.EnemyTypeNames;
      EnemyTypeListBox.Items.Clear ();
      foreach (string name in names)
        EnemyTypeListBox.Items.Add (name);
      QuietSelect = true;
      EnemyTypeListBox.SelectedIndex = e.SelectItem;
      QuietSelect = false;
    }


    private void EnemyTypeSelected (object sender, EventArgs e)
    {
      QuietSelect = true;
      EnemyTypeListBox.SelectedIndex = MainProject.EnemyTypeIndex;
      QuietSelect = false;
      IdInput.Text = Tools.IntToHex (MainProject.EnemyTypeID, 4);
    }


    private void LoadEnemyData (object sender, EventArgs e)
    {
      MainProject.GetEnemyPixelPosition (out int x, out int y);
      PositionInput.Text = Tools.IntToHex (x, 4) + "," + Tools.IntToHex (y, 4);
    }


    private void LoadEnemyGfxData (object sender, EventArgs e)
    {
      switch (MainProject.GetEnemyGfxPalette ())
      {
      default:
        GfxPaletteInput.SelectedIndex = -1;
        break;
      case EnemyGfxPalette.P1:
        GfxPaletteInput.SelectedIndex = 0;
        break;
      case EnemyGfxPalette.P2:
        GfxPaletteInput.SelectedIndex = 1;
        break;
      case EnemyGfxPalette.P3:
        GfxPaletteInput.SelectedIndex = 2;
        break;
      case EnemyGfxPalette.P4:
        GfxPaletteInput.SelectedIndex = 3;
        break;
      }
    }


    private void LoadEnemyTypeData (object sender, EventArgs e)
    {
      EnemyName.Content = MainProject.EnemyTypeName;
      EnemyImage.Source = MainProject.EnemyTypeImage?.ToBitmap ();
    }


//========================================================================================
// Event handlers


    private void EnemyListBox_SelectionChanged (object sender, SelectionChangedEventArgs e)
    {
      EnemyListBox.ScrollIntoView (EnemyListBox.SelectedItem);
      if (!QuietSelect)
        MainProject.SelectEnemy (EnemyListBox.SelectedIndex);
    }


    private void EnemyListBox_DoubleClick (object sender, MouseButtonEventArgs e)
    {
      var window = new UI.EditEnemyWindow (MainProject, false);
      window.Owner = Window.GetWindow (this);
      window.ShowDialog ();
    }


    private void EnemyGfxListBox_SelectionChanged (object sender, SelectionChangedEventArgs e)
    {
      EnemyGfxListBox.ScrollIntoView (EnemyGfxListBox.SelectedItem);
      if (!QuietSelect)
        MainProject.SelectEnemyGfx (EnemyGfxListBox.SelectedIndex);
    }


    private void EnemyTypeListBox_SelectionChanged (object sender, SelectionChangedEventArgs e)
    {
      EnemyTypeListBox.ScrollIntoView (EnemyTypeListBox.SelectedItem);
      if (!QuietSelect)
        MainProject.SelectEnemyType (EnemyTypeListBox.SelectedIndex);
    }


    private void AddEnemy_Click (object sender, RoutedEventArgs e)
    {
      AddEnemy?.Invoke (this, null);
    }


    private void MoveEnemyUp_Click (object sender, RoutedEventArgs e)
    {
      MainProject.MoveEnemyUp ();
    }


    private void MoveEnemyDown_Click (object sender, RoutedEventArgs e)
    {
      MainProject.MoveEnemyDown ();
    }


    private void DeleteEnemy_Click (object sender, RoutedEventArgs e)
    {
      MainProject.DeleteEnemy ();
    }


    private void AddEnemyGfx_Click (object sender, RoutedEventArgs e)
    {
      MainProject.AddEnemyGfx ();
    }


    private void MoveEnemyGfxUp_Click (object sender, RoutedEventArgs e)
    {
      MainProject.MoveEnemyGfxUp ();
    }


    private void MoveEnemyGfxDown_Click (object sender, RoutedEventArgs e)
    {
      MainProject.MoveEnemyGfxDown ();
    }


    private void DeleteEnemyGfx_Click (object sender, RoutedEventArgs e)
    {
      MainProject.DeleteEnemyGfx ();
    }


    private void IdInput_LostFocus (object sender, RoutedEventArgs e)
    {
      MainProject.SelectEnemyTypeByID (Tools.HexToInt (IdInput.Text));
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
      MainProject.SetEnemyPixelPosition (x, y);
    }


    private void PositionInput_KeyDown (object sender, KeyEventArgs e)
    {
      if (e.Key == Key.Enter)
        PositionInput_LostFocus (sender, null);
      UITools.ValidateHexOrComma (ref e);
    }

  }

}
