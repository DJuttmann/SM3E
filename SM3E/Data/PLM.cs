using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Reflection;
//using MBuild.Properties;
using System.Collections.Generic;


namespace SM3E
{

//========================================================================================
// CLASS PLM 
//========================================================================================


  class Plm: Data, IRepointable
  {
    public const int DefaultSize = 6;
    public const int TerminatorSize = 2;
    public const int ScrollID = 0xB703;

    public int PlmID;
    public byte PosX;
    public byte PosY;
    public int MainVariable;

    public PlmType MyPlmType;
    public ScrollPlmData MyScrollPlmData;

    public override int Size
    {
      get {return DefaultSize;}
    }

    public int ScrollDataPtr
    {
      get
      {
        if (PlmID != ScrollID)
          return 0;
        return MainVariable;
      }
    }


    // Constructor.
    public Plm (): base ()
    {
      PlmID        = 0;
      PosX    = 0;
      PosY    = 0;
      MainVariable = 0;

      MyPlmType = null;
      MyScrollPlmData = null;
    }


    // Read data from ROM at given PC address.
    public override bool ReadFromROM (Rom rom, int addressPC)
    {
      byte [] b = new byte [DefaultSize];
      rom.Seek (addressPC);
      if (!rom.Read (b, 0, DefaultSize))
        return false;
      
      PlmID        = Tools.ConcatBytes (b [0], b [1]);
      PosX    = b [2];
      PosY    = b [3];
      MainVariable = Tools.ConcatBytes (b [4], b [5], 0x8F);

      startAddressPC = addressPC;
      return true;
    }


    // Write data to ROM at current position (addressPC), which is updated.
    public override bool WriteToROM (Stream rom, ref int addressPC)
    {
      byte [] b = new byte [DefaultSize];
      Tools.CopyBytes (PlmID       , b, 0, 2);
      Tools.CopyBytes (PosX   , b, 2, 1);
      Tools.CopyBytes (PosY   , b, 3, 1);
      Tools.CopyBytes (MainVariable, b, 4, 2);
      rom.Write (b, 0, DefaultSize);
      addressPC += Size;
      return true;
    }


    // Set default values.
    public override void SetDefault ()
    {
      PlmID        = 0xB63B;
      PosX    = 0;
      PosY    = 0;
      MainVariable = 0x8000;

      startAddressPC = -1;
    }


    public bool Connect (List <Data> ScrollPlmDatas, List <PlmType> PlmTypes)
    {
      MyPlmType = PlmTypes.Find (x => x.PlmID == PlmID);

      if (PlmID == ScrollID) {
        MyScrollPlmData =
          (ScrollPlmData) ScrollPlmDatas.Find (x => x.StartAddressLR == MainVariable);
        if (MyScrollPlmData != null)
          MyScrollPlmData.MyPlms.Add (this);
        return MyScrollPlmData != null;
      }
      else
        MyScrollPlmData = null;
      return true;
    }
    

    public void Repoint ()
    {
      if (PlmID == ScrollID)
        MainVariable = MyScrollPlmData.StartAddressLR;
    }

//----------------------------------------------------------------------------------------

    public void Shift (int dx, int dy)
    {
      PosX += (byte) dx;
      PosY += (byte) dy;
    }

  } // Class Plm


//========================================================================================
// CLASS PLM SET 
//========================================================================================


  class PlmSet: Data, IRepointable
  {
    public const int TerminatorSize = 2;
    private const int Terminator = 0;

    public List <Plm> Plms;
    public List <RoomState> MyRoomStates;

    public override int Size
    {
      get {return Plms.Count * Plm.DefaultSize + TerminatorSize;}
    }

    public int PlmCount
    {
      get {return Plms.Count;}
    }


    // Constructor.
    public PlmSet (): base ()
    {
      Plms = new List <Plm> ();
      MyRoomStates = new List <RoomState> ();
    }


    // Read data from ROM at given PC address.
    public override bool ReadFromROM (Rom rom, int addressPC)
    {
      int sAddressPC = addressPC;
      byte [] b = new byte [2];
      rom.Seek (addressPC);
      if (!rom.Read (b, 0, 2))
        return false;

      int n = 0;
      while (b [0] != 0 || b [1] != 0)
      {
        Plms.Add (new Plm ());
        if (!Plms [n].ReadFromROM (rom, addressPC))
          return false;
        addressPC += Plms [n].Size;
        rom.Seek (addressPC);
        if (!rom.Read (b, 0, 2))
          return false;
        n++;
      }
      startAddressPC = sAddressPC;
      return true;
    }


    // Write data to ROM at current position (addressPC), which is updated.
    public override bool WriteToROM (Stream rom, ref int addressPC)
    {
      for (int n = 0; n < Plms.Count; n++)
        Plms [n].WriteToROM (rom, ref addressPC);
      byte [] b = new byte [TerminatorSize];
      Tools.CopyBytes (Terminator, b, 0, 2);
      rom.Write (b, 0, TerminatorSize);
      addressPC += TerminatorSize;
      return true;
    }


    // Set default values.
    public override void SetDefault ()
    {
      Plms.Clear ();

      startAddressPC = -1;
    }


    // Change the location within the ROM.
    public override void Reallocate (int addressPC)
    {
      startAddressPC = addressPC;
      for (int n = 0; n < Plms.Count; n++)
      {
        Plms [n].Reallocate (addressPC);
        addressPC += Plms [n].Size;
      }
    }


    public bool Connect (List <Data> ScrollPlmDatas, List <PlmType> PlmTypes)
    {
      bool success = true;
      for (int n = 0; n < Plms.Count; n++) {
        success &= Plms [n].Connect (ScrollPlmDatas, PlmTypes);
      }
      return success;
    }


    public void Repoint ()
    {
      for (int n = 0; n < Plms.Count; n++)
        Plms [n].Repoint ();
    }

  } // Class PlmSet


//========================================================================================
// CLASS PLM TYPE 
//========================================================================================


  class PlmType
  {
    public int PlmID = 0;
//    public int TileX = 0;
//    public int TileY = 0;
//    public int TileWidth = 0;
//    public int TileHeight = 0;
    public string Name = String.Empty;
    public BlitImage Graphics;

    public int Index = 0;  // index in the plm_types array that stores all available types.
  } // class PlmType

}