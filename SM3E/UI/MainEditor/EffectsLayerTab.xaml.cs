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
    private static readonly List <FxType> FxTypeList = new List <FxType>
    {
      FxType.None,
      FxType.Lava,
      FxType.Acid,
      FxType.Water,
      FxType.Spores,
      FxType.Rain,
      FxType.Fog,
      FxType.BgScroll,
      FxType.BgGlow,
      FxType.Statues,
      FxType.CeresRidley,
      FxType.CeresMode7,
      FxType.Haze,
    };

    Project MainProject;

    public EffectsLayerTab ()
    {
      InitializeComponent ();
    }


    // Set the project and subscribe to its events.
    public void SetProject (Project p)
    {
      MainProject = p;

      MainProject.FxSelected += LoadFxData;
      MainProject.FxDataListChanged += LoadFxDataList;
      MainProject.FxDataSelected += LoadFxData;
      MainProject.FxDataSelected += FxDataSelected;
      MainProject.FxDataModified += LoadFxData;
    }


//========================================================================================
// Setup & Updating


    private void LoadFxData (object sender, EventArgs e)
    {
      MainProject.GetFxData (out int surfaceStart,
                             out int surfaceNew,
                             out int surfaceSpeed,
                             out int surfaceDelay,
                             out FxType fxType,
                             out int fxBitA,
                             out int fxBitB,
                             out int liquidOptions,
                             out int paletteOptions,
                             out int animationOptions,
                             out int paletteBlend);

      TypeSelect.SelectedIndex = FxTypeList.FindIndex (x => x == fxType);
      PaletteBlendInput.Text = Tools.IntToHex (paletteBlend, 2);
      FxBitAInput.Text = Tools.IntToHex (fxBitA, 2);
      FxBitBInput.Text = Tools.IntToHex (fxBitB, 2);
      SurfaceStartInput.Text = Tools.IntToHex (surfaceStart, 4);
      SurfaceNewInput.Text = Tools.IntToHex (surfaceNew, 4);
      SurfaceSpeedInput.Text = Tools.IntToHex (surfaceSpeed, 4);
      SurfaceDelayInput.Text = Tools.IntToHex (surfaceDelay, 2);
      PaletteOptionsInput.Text = Tools.IntToHex (paletteOptions, 2);
      AnimationOptionsInput.Text = Tools.IntToHex (animationOptions, 2);
      LiquidOptionsInput.Text = Tools.IntToHex (liquidOptions, 2);
      SetCheckBoxes (liquidOptions, paletteOptions, animationOptions);

      switch (MainProject.AreaIndex)
      {
      case 0:
        A2Check.Content = "Ocean";
        A3Check.Content = "Lava";
        break;
      case 3:
        A2Check.Content = "Conveyor (L)";
        A3Check.Content = "Conveyor (R)";
        break;
      case 4:
        A2Check.Content = "Sand source";
        A3Check.Content = "Sand fall";
        break;
      default:
        A2Check.Content = "-";
        A3Check.Content = "-";
        break;
      }
    }


    private void SetCheckBoxes (int liquidOptions, int paletteOptions,
                                int animationOptions)
    {
      A0Check.IsChecked = (animationOptions & 0x01) > 0;
      A1Check.IsChecked = (animationOptions & 0x02) > 0;
      A2Check.IsChecked = (animationOptions & 0x04) > 0;
      A3Check.IsChecked = (animationOptions & 0x08) > 0;
      A4Check.IsChecked = (animationOptions & 0x10) > 0;
      A5Check.IsChecked = (animationOptions & 0x20) > 0;
      A6Check.IsChecked = (animationOptions & 0x40) > 0;
      A7Check.IsChecked = (animationOptions & 0x80) > 0;

      P0Check.IsChecked = (paletteOptions & 0x01) > 0;
      P1Check.IsChecked = (paletteOptions & 0x02) > 0;
      P2Check.IsChecked = (paletteOptions & 0x04) > 0;
      P3Check.IsChecked = (paletteOptions & 0x08) > 0;
      P4Check.IsChecked = (paletteOptions & 0x10) > 0;
      P5Check.IsChecked = (paletteOptions & 0x20) > 0;
      P6Check.IsChecked = (paletteOptions & 0x40) > 0;
      P7Check.IsChecked = (paletteOptions & 0x80) > 0;

      L0Check.IsChecked = (liquidOptions & 0x01) > 0;
      L1Check.IsChecked = (liquidOptions & 0x02) > 0;
      L2Check.IsChecked = (liquidOptions & 0x04) > 0;
      L3Check.IsChecked = (liquidOptions & 0x08) > 0;
      L4Check.IsChecked = (liquidOptions & 0x10) > 0;
      L5Check.IsChecked = (liquidOptions & 0x20) > 0;
      L6Check.IsChecked = (liquidOptions & 0x40) > 0;
      L7Check.IsChecked = (liquidOptions & 0x80) > 0;
    }


    private void FxDataSelected (object sender, EventArgs e)
    {
      FxDataSelect.SelectedIndex = MainProject.FxDataIndex;
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


    private void AddFxData_Click (object sender, RoutedEventArgs e)
    {
      if (FxDataSelect.Items.Count == 0)
      {
        MainProject.AddFxData (-1);
      }
      else
      {
        var window = new NewFxDataWindow (MainProject);
        window.Owner = Window.GetWindow (this);
        window.ShowDialog ();
      }
    }


    private void DeleteFxData_Click (object sender, RoutedEventArgs e)
    {
      MainProject.DeleteFxData ();
    }


    private void FxDataSelect_SelectionChanged (object sender, SelectionChangedEventArgs e)
    {
      MainProject.SelectFxData (FxDataSelect.SelectedIndex);
    }


    private void Type_SelectionChanged (object sender, SelectionChangedEventArgs e)
    {
      if (TypeSelect.SelectedIndex >= 0)
        MainProject.SetFxType (FxTypeList [TypeSelect.SelectedIndex]);
      else
        MainProject.SetFxType (FxType.None);
    }


    private void PaletteBlend_LostFocus (object sender, RoutedEventArgs e)
    {
      MainProject.SetFxPaletteBlend (Tools.HexToInt (PaletteBlendInput.Text));
    }


    private void PaletteBlend_KeyDown (object sender, KeyEventArgs e)
    {
      if (e.Key == Key.Enter)
        PaletteBlend_LostFocus (this, null);
      UITools.ValidateHex (ref e);
    }


    private void FxBitA_LostFocus (object sender, RoutedEventArgs e)
    {
      MainProject.SetFxBitA (Tools.HexToInt (FxBitAInput.Text));
    }


    private void FxBitA_KeyDown (object sender, KeyEventArgs e)
    {
      if (e.Key == Key.Enter)
        FxBitA_LostFocus (this, null);
      UITools.ValidateHex (ref e);
    }


    private void FxBitB_LostFocus (object sender, RoutedEventArgs e)
    {
      MainProject.SetFxBitB (Tools.HexToInt (FxBitBInput.Text));
    }


    private void FxBitB_KeyDown (object sender, KeyEventArgs e)
    {
      if (e.Key == Key.Enter)
        FxBitB_LostFocus (this, null);
      UITools.ValidateHex (ref e);
    }


    private void SurfaceStart_LostFocus (object sender, RoutedEventArgs e)
    {
      MainProject.SetFxSurfaceStart (Tools.HexToInt (SurfaceStartInput.Text));
    }


    private void SurfaceStart_KeyDown (object sender, KeyEventArgs e)
    {
      if (e.Key == Key.Enter)
        SurfaceStart_LostFocus (this, null);
      UITools.ValidateHex (ref e);
    }


    private void SurfaceNew_LostFocus (object sender, RoutedEventArgs e)
    {
      MainProject.SetFxSurfaceNew (Tools.HexToInt (SurfaceNewInput.Text));
    }


    private void SurfaceNew_KeyDown (object sender, KeyEventArgs e)
    {
      if (e.Key == Key.Enter)
        SurfaceNew_LostFocus (this, null);
      UITools.ValidateHex (ref e);
    }


    private void SurfaceSpeed_LostFocus (object sender, RoutedEventArgs e)
    {
      MainProject.SetFxSurfaceSpeed (Tools.HexToInt (SurfaceSpeedInput.Text));
    }


    private void SurfaceSpeed_KeyDown (object sender, KeyEventArgs e)
    {
      if (e.Key == Key.Enter)
        SurfaceSpeed_LostFocus (this, null);
      UITools.ValidateHex (ref e);
    }


    private void SurfaceDelay_LostFocus (object sender, RoutedEventArgs e)
    {
      MainProject.SetFxSurfaceDelay (Tools.HexToInt (SurfaceDelayInput.Text));
    }


    private void SurfaceDelay_KeyDown (object sender, KeyEventArgs e)
    {
      if (e.Key == Key.Enter)
        SurfaceDelay_LostFocus (this, null);
      UITools.ValidateHex (ref e);
    }

    private void LiquidOptions_LostFocus (object sender, RoutedEventArgs e)
    {
      MainProject.SetFxLiquidOptions (Tools.HexToInt (LiquidOptionsInput.Text));
    }

    private void LiquidOptions_KeyDown (object sender, KeyEventArgs e)
    {
      if (e.Key == Key.Enter)
        LiquidOptions_LostFocus (this, null);
      UITools.ValidateHex (ref e);
    }

    private void LCheck_Click (object sender, RoutedEventArgs e)
    {
      int value = (L0Check.IsChecked == true ? 0x01 : 0) + 
                  (L1Check.IsChecked == true ? 0x02 : 0) + 
                  (L2Check.IsChecked == true ? 0x04 : 0) + 
                  (L3Check.IsChecked == true ? 0x08 : 0) + 
                  (L4Check.IsChecked == true ? 0x10 : 0) + 
                  (L5Check.IsChecked == true ? 0x20 : 0) + 
                  (L6Check.IsChecked == true ? 0x40 : 0) + 
                  (L7Check.IsChecked == true ? 0x80 : 0);
      MainProject.SetFxLiquidOptions (value);
    }

    private void AnimationOptions_LostFocus (object sender, RoutedEventArgs e)
    {
      MainProject.SetFxAnimationOptions (Tools.HexToInt (AnimationOptionsInput.Text));
    }

    private void AnimationOptions_KeyDown (object sender, KeyEventArgs e)
    {
      if (e.Key == Key.Enter)
        AnimationOptions_LostFocus (this, null);
      UITools.ValidateHex (ref e);
    }

    private void ACheck_Click (object sender, RoutedEventArgs e)
    {
      int value = (A0Check.IsChecked == true ? 0x01 : 0) + 
                  (A1Check.IsChecked == true ? 0x02 : 0) + 
                  (A2Check.IsChecked == true ? 0x04 : 0) + 
                  (A3Check.IsChecked == true ? 0x08 : 0) + 
                  (A4Check.IsChecked == true ? 0x10 : 0) + 
                  (A5Check.IsChecked == true ? 0x20 : 0) + 
                  (A6Check.IsChecked == true ? 0x40 : 0) + 
                  (A7Check.IsChecked == true ? 0x80 : 0);
      MainProject.SetFxAnimationOptions (value);
    }

    private void PaletteOptions_LostFocus (object sender, RoutedEventArgs e)
    {
      MainProject.SetFxPaletteOptions (Tools.HexToInt (PaletteOptionsInput.Text));
    }

    private void PaletteOptions_KeyDown (object sender, KeyEventArgs e)
    {
      if (e.Key == Key.Enter)
        PaletteOptions_LostFocus (this, null);
      UITools.ValidateHex (ref e);
    }

    private void PCheck_Click (object sender, RoutedEventArgs e)
    {
      int value = (P0Check.IsChecked == true ? 0x01 : 0) + 
                  (P1Check.IsChecked == true ? 0x02 : 0) + 
                  (P2Check.IsChecked == true ? 0x04 : 0) + 
                  (P3Check.IsChecked == true ? 0x08 : 0) + 
                  (P4Check.IsChecked == true ? 0x10 : 0) + 
                  (P5Check.IsChecked == true ? 0x20 : 0) + 
                  (P6Check.IsChecked == true ? 0x40 : 0) + 
                  (P7Check.IsChecked == true ? 0x80 : 0);
      MainProject.SetFxPaletteOptions (value);
    }

  } // partial class EffectsLayerTab

}
