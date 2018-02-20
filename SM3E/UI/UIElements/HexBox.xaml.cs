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

namespace SM3E.UI.UIElements
{
  /// <summary>
  /// Interaction logic for HexBox.xaml
  /// </summary>
  public partial class HexBox: UserControl
  {

    public int MaxLength
    {
      get {return (int) GetValue (MaxLengthProperty);}
      set
      {
        SetValue (MaxLengthProperty, value);
      }
    }

    public static readonly DependencyProperty MaxLengthProperty = 
      DependencyProperty.Register ("MaxLength", typeof (int), typeof (HexBox));


    public HexBox ()
    {
      InitializeComponent ();
      DataContext = LayoutRoot;
    }

    private void ValidateHexInput (object sender, KeyEventArgs e)
    {
      if (e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9)
        return;
      if (System.Text.RegularExpressions.Regex.IsMatch (e.Key.ToString (), "[^0-9^A-F^a-f]"))
        e.Handled = true;
    }
  }
}
