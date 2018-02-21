using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Reflection;
using System.Windows.Input;
using System.Text;
using System.Collections.Generic;
using System.Text.RegularExpressions;


namespace SM3E
{

  class UITools
  {

    private static readonly Regex HexPattern = new Regex ("[^0-9^A-F^a-f]");


    public static void ValidateHex (ref KeyEventArgs e)
    {
      if ((e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9) ||
          e.Key == Key.Tab || e.Key == Key.Enter)
        return;
      if (HexPattern.IsMatch (e.Key.ToString ()))
        e.Handled = true;
    }

  } // class UITools

}