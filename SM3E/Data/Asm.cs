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


  class Asm: RawData, IReusable, IReferenceableBy <RoomState>
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


    public bool ReferenceMe (RoomState source)
    {
      MyReferringData.Add (source);
      return true;
    }


    public int UnreferenceMe (RoomState source)
    {
      MyReferringData.Remove (source);
      return MyReferringData.Count;
    }


    public void DetachAllReferences ()
    {
      foreach (Data d in MyReferringData)
        switch (d)
        {
        case RoomState r:
          if (r.MySetupAsm == this)
            r.SetSetupAsm (null, out var ignore);
          if (r.MyMainAsm == this)
            r.SetMainAsm (null, out var ignore);
          break;
        default:
          break;
        }
    }

  } // class Asm

}