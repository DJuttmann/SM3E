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
  public partial class EnemyLayer : UserControl
  {
    private Project MainProject;

    bool QuietSelect = false;


    // Constructor.
    public EnemyLayer()
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
    }


    private void LoadEnemyData (object sender, EventArgs e)
    {
      // [wip]
    }


    private void LoadEnemyGfxData (object sender, EventArgs e)
    {
      // [wip] maybe do nothing?
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
      MainProject.AddEnemy (64, 64);
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


  }
}
