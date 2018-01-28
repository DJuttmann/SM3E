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
      MainProject.DoorListChanged += LoadDoorListBox;
      MainProject.PlmListChanged += LoadPlmListBox;
      MainProject.PlmTypeListChanged += LoadPlmTypeListBox;
      MainProject.EnemyListChanged += LoadEnemyListBox;
      MainProject.EnemyGfxListChanged += LoadEnemyGfxListBox;
      MainProject.EnemyTypeListChanged += LoadEnemyTypeListBox;

      MainProject.AreaSelected += UpdateMapEditor;
      MainProject.AreaSelected += AreaSelected;
      MainProject.RoomSelected += LoadRoomData;
      MainProject.RoomSelected += RoomSelected;
      MainProject.RoomStateSelected += LoadRoomStateData;
      MainProject.RoomStateSelected += RoomStateSelected;
      MainProject.DoorSelected += LoadDoorData;
      MainProject.DoorSelected += DoorSelected;
      MainProject.PlmSelected += LoadPlmData;
      MainProject.PlmSelected += PlmSelected;
      MainProject.PlmTypeSelected += LoadPlmTypeData;
      MainProject.PlmTypeSelected += PlmTypeSelected;
      MainProject.EnemySelected += LoadEnemyData;
      MainProject.EnemySelected += EnemySelected;
      MainProject.EnemyGfxSelected += LoadEnemyGfxData;
      MainProject.EnemyGfxSelected += EnemyGfxSelected;
      MainProject.EnemyTypeSelected += LoadEnemyTypeData;
      MainProject.EnemyTypeSelected += EnemyTypeSelected;

      MainProject.LevelDataSelected += NewLevelData;
      MainProject.TileSetSelected += UpdateTileSelector;

      MainProject.LevelDataModified += LevelDataModified;
    }

  }
}
