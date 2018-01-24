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

namespace SM3E
{
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow: Window
  {
    // Fields
    Project MainProject = new Project ();

    // Constructor
    public MainWindow ()
    {
      InitializeComponent ();

      if (!Logging.Open ())
        {
          System.Windows.MessageBox.Show ("log failed");
          return;
        }

      SetupLevelData ();
      SetupTileSelector ();
      SetupBtsSelector ();
      SetupMapEditor ();

      SetProjectHandlers ();
      MainProject.Load ("SuperMetroid.txt");

      SetupMapTileSelector ();
    }


    private void SetProjectHandlers ()
    {
      MainProject.AreaListChanged += LoadAreaListBox;
      MainProject.RoomListChanged += LoadRoomListBox;
      MainProject.RoomStateListChanged += LoadRoomStateListBox;

      MainProject.AreaSelected += UpdateMapEditor;
      // MainProject.RoomSelected += DoNothing;
      // MainProject.RoomStateSelected += DoNothing;
      MainProject.LevelDataSelected += NewLevelData;
      MainProject.TileSetSelected += UpdateTileSelector;
    }

  }
}
