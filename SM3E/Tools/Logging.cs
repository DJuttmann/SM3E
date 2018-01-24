using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Reflection;
//using MBuild.Properties;
using System.Text;
using System.Collections.Generic;


namespace SM3E
{

//========================================================================================
// CLASS Log
//========================================================================================

  class Logging
  {
    public static string LogFile = "log.txt";
    private static StreamWriter Stream = null;
    public static bool Verbose = false;


    public static bool Open ()
    {
      try
      {
        Stream = new StreamWriter (LogFile);
      }
      catch
      {
        return false;
      }
      return true;
    }


    public static void WriteLine (string s)
    {
      Stream.WriteLine (s);
    }


    public static void Write (string s)
    {
      Stream.Write (s);
    }


    public static void Close ()
    {
      if (Stream != null)
      {
        Stream.Close ();
        Stream = null;
      }
    }
  }

}