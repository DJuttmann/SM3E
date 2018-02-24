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
  /// Interaction logic for EditEnemyWindow.xaml
  /// </summary>
  public partial class EditEnemyWindow: Window
  {
    private Project MyProject;
    private bool CreateNew;


    public EditEnemyWindow (Project p, bool createNew)
    {
      InitializeComponent ();

      MyProject = p;
      CreateNew = createNew;
      if (CreateNew)
      {
        SpecialInput.Text = "0000";
        GraphicsInput.Text = "0000";
        TilemapsInput.Text = "0000";
        SpeedInput.Text = "0000";
        Speed2Input.Text = "0000";
      }
      else
      {
        MyProject.GetEnemyProperties (out int special, out int graphics, out int tilemaps,
                                      out int speed, out int speed2);
        SpecialInput.Text = Tools.IntToHex (special, 4);
        GraphicsInput.Text = Tools.IntToHex (graphics, 4);
        TilemapsInput.Text = Tools.IntToHex (tilemaps, 4);
        SpeedInput.Text = Tools.IntToHex (speed, 4);
        Speed2Input.Text = Tools.IntToHex (speed2, 4);
      }
    }


    private void Save_Click (object sender, RoutedEventArgs e)
    {
      MyProject.SetEnemyProperties (Tools.HexToInt (SpecialInput.Text),
                                    Tools.HexToInt (GraphicsInput.Text),
                                    Tools.HexToInt (TilemapsInput.Text),
                                    Tools.HexToInt (SpeedInput.Text),
                                    Tools.HexToInt (Speed2Input.Text));
      DialogResult = true;
      Close ();
    }


    private void Cancel_Click (object sender, RoutedEventArgs e)
    {
      DialogResult = false;
      Close ();
    }


    private void ValidateHexInput (object sender, KeyEventArgs e)
    {
      if (e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9)
        return;
      if (System.Text.RegularExpressions.Regex.IsMatch (e.Key.ToString (), "[^0-9^A-F^a-f]"))
        e.Handled = true;
    }

  } // partial class EditEnemyWindow

}
