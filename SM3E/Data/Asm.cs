using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Reflection;
//using MBuild.Properties;
using System.Collections.Generic;


namespace SM3E
{

//========================================================================================
// CLASS ASM
//========================================================================================


  class Asm: RawData, IReusable
  {
    public string Name;

    public HashSet <Data> MyReferringData;
    
    public int ReferenceCount {get {return MyReferringData.Count;}}


    public int EndAddressPC
    {
      get {return StartAddressPC + Size;}
      set
      {
        if (value > StartAddressPC)
          SetSize (EndAddressPC - StartAddressPC);
      }
    }


    public Asm ()
    {
      MyReferringData = new HashSet <Data> ();
    }
  } // class Asm

}