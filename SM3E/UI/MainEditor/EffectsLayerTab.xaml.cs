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
  /// Interaction logic for EffectsLayerTab.xaml
  /// </summary>
  public partial class EffectsLayerTab: UserControl
  {

    Project MainProject;


    public EffectsLayerTab ()
    {
      InitializeComponent ();
    }


    // Set the project and subscribe to its events.
    public void SetProject (Project p)
    {
      MainProject = p;

      MainProject.FxSelected += LoadEffectsState;
      MainProject.FxSelected += LoadEffectsData;
      MainProject.FxDataListChanged += LoadFxDataList;
      MainProject.FxDataSelected += LoadEffectsData;
      MainProject.FxDataSelected += FxDataSelected;
    }


//========================================================================================
// Setup & Updating


    private void LoadEffectsState (object sender, EventArgs e)
    {
      NotNullCheck.IsChecked = MainProject.GetFxState ();
    }


    private void LoadEffectsData (object sender, EventArgs e)
    {
      MainProject.GetFxData (out int doorIndex,
                             out int surfaceStart,
                             out int surfaceNew,
                             out int surfaceSpeed,
                             out int surfaceDelay,
                             out FxType fxType,
                             out int fxBitA,
                             out int fxBitB,
                             out int fxBitC,
                             out int paletteFxBitflags,
                             out int tileAnimationBitflags,
                             out int paletteBlend);

      LoadFxType (fxType);
      PaletteBlendInput.Text = Tools.IntToHex (paletteBlend);
      FxBitAInput.Text = Tools.IntToHex (fxBitA);
      FxBitBInput.Text = Tools.IntToHex (fxBitB);
      SurfaceStartInput.Text = Tools.IntToHex (surfaceStart);
      SurfaceNewInput.Text = Tools.IntToHex (surfaceNew);
      SurfaceSpeedInput.Text = Tools.IntToHex (surfaceSpeed);
      SurfaceDelayInput.Text = Tools.IntToHex (surfaceDelay);
      PaletteOptionsInput.Text = Tools.IntToHex (paletteFxBitflags);
      AnimationOptionsInput.Text = Tools.IntToHex (tileAnimationBitflags);
      LiquidOptionsInput.Text = Tools.IntToHex (fxBitC);
    }


    private void FxDataSelected (object sender, EventArgs e)
    {
      FxDataSelect.SelectedIndex = MainProject.FxDataIndex;
    }


    private void LoadFxType (FxType fxType)
    {
      switch (fxType)
      {
      default:
      case FxType.Unknown:
        TypeSelect.SelectedIndex = -1;
        break;
      case FxType.None:
        TypeSelect.SelectedIndex = 0;
        break;
      case FxType.Lava:
        TypeSelect.SelectedIndex = 1;
        break;
      case FxType.Acid:
        TypeSelect.SelectedIndex = 2;
        break;
      case FxType.Water:
        TypeSelect.SelectedIndex = 3;
        break;
      case FxType.Spores:
        TypeSelect.SelectedIndex = 4;
        break;
      case FxType.Rain:
        TypeSelect.SelectedIndex = 5;
        break;
      case FxType.Fog:
        TypeSelect.SelectedIndex = 6;
        break;
      case FxType.BgScroll:
        TypeSelect.SelectedIndex = 7;
        break;
      case FxType.BgGlow:
        TypeSelect.SelectedIndex = 8;
        break;
      case FxType.Statues:
        TypeSelect.SelectedIndex = 9;
        break;
      case FxType.CeresRidley:
        TypeSelect.SelectedIndex = 10;
        break;
      case FxType.CeresMode7:
        TypeSelect.SelectedIndex = 11;
        break;
      case FxType.Haze:
        TypeSelect.SelectedIndex = 12;
        break;
      }
    }


    private void LoadFxDataList (object sender, ListLoadEventArgs e)
    {
      List <string> names = MainProject.FxDataNames;
      FxDataSelect.ItemsSource = names;
      FxDataSelect.SelectedIndex = e.SelectItem;
      FxDataSelect.ScrollIntoView (FxDataSelect.SelectedItem);
    }


//========================================================================================
// Event handlers


    private void NotNull_Checked (object sender, RoutedEventArgs e)
    {
      EffectsGrid.IsEnabled = true;
      LiquidGrid.IsEnabled = true;
      MiscGrid.IsEnabled = true;
    }


    private void NotNull_UnChecked (object sender, RoutedEventArgs e)
    {
      EffectsGrid.IsEnabled = false;
      LiquidGrid.IsEnabled = false;
      MiscGrid.IsEnabled = false;
    }


    private void FxDataSelect_SelectionChanged (object sender, SelectionChangedEventArgs e)
    {
      MainProject.SelectFxData (FxDataSelect.SelectedIndex);
    }


    private void AnimationOptions_TextChanged (object sender, TextChangedEventArgs e)
    {
      int value = Tools.HexToInt (AnimationOptionsInput.Text);
      A0Check.IsChecked = (value & 0x01) > 0;
      A1Check.IsChecked = (value & 0x02) > 0;
      A2Check.IsChecked = (value & 0x04) > 0;
      A3Check.IsChecked = (value & 0x08) > 0;
      A4Check.IsChecked = (value & 0x10) > 0;
      A5Check.IsChecked = (value & 0x20) > 0;
      A6Check.IsChecked = (value & 0x40) > 0;
      A7Check.IsChecked = (value & 0x80) > 0;
    }


    private void PaletteOptions_TextChanged (object sender, TextChangedEventArgs e)
    {
      int value = Tools.HexToInt (PaletteOptionsInput.Text);
      P0Check.IsChecked = (value & 0x01) > 0;
      P1Check.IsChecked = (value & 0x02) > 0;
      P2Check.IsChecked = (value & 0x04) > 0;
      P3Check.IsChecked = (value & 0x08) > 0;
      P4Check.IsChecked = (value & 0x10) > 0;
      P5Check.IsChecked = (value & 0x20) > 0;
      P6Check.IsChecked = (value & 0x40) > 0;
      P7Check.IsChecked = (value & 0x80) > 0;
    }


    private void LiquidOptions_TextChanged (object sender, TextChangedEventArgs e)
    {
      int value = Tools.HexToInt (LiquidOptionsInput.Text);
      L0Check.IsChecked = (value & 0x01) > 0;
      L1Check.IsChecked = (value & 0x02) > 0;
      L2Check.IsChecked = (value & 0x04) > 0;
      L3Check.IsChecked = (value & 0x08) > 0;
      L4Check.IsChecked = (value & 0x10) > 0;
      L5Check.IsChecked = (value & 0x20) > 0;
      L6Check.IsChecked = (value & 0x40) > 0;
      L7Check.IsChecked = (value & 0x80) > 0;
    }

  } // partial class EffectsLayerTab

}
