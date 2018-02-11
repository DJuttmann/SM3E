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


  class Asm: RawData
  {
    public string Name;


    public int EndAddressPC
    {
      get {return StartAddressPC + Size;}
      set
      {
        if (value > StartAddressPC)
          SetSize (EndAddressPC - StartAddressPC);
      }
    }

  } // class Asm

}