﻿using System;
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
      // [wip]
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
      MainProject.AddPlm (0, 0);
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

  } // partial class PlmLayerTab

}
