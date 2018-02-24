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

    private static readonly Regex HexPattern = new Regex ("[0-9A-Fa-f]");
    private static readonly Regex HexOrCommaPattern = new Regex ("[0-9A-Fa-f,]");


    // Check if keyboard input is hexadecimal digit, discard if it isn't.
    public static void ValidateHex (ref KeyEventArgs e)
    {
      if ((e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9) ||
          e.Key == Key.Tab || e.Key == Key.Enter)
        return;
      if (!HexPattern.IsMatch (e.Key.ToString ()))
        e.Handled = true;
    }


    // Check if keyboard input is hexadecimal digit pr comma, discard if it isn't.
    public static void ValidateHexOrComma (ref KeyEventArgs e)
    {
      if ((e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9) ||
          e.Key == Key.Tab || e.Key == Key.Enter)
        return;
      if (!HexOrCommaPattern.IsMatch (e.Key.ToString ()))
        e.Handled = true;
    }

  } // class UITools

}