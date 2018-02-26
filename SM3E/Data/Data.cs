using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Reflection;
//using MBuild.Properties;
using System.Collections.Generic;


namespace SM3E
{

//========================================================================================
// ENUMS & CONSTS
//========================================================================================

  public enum DataType // [wip] unsure if this is needed.
  {
    type_raw_data,
    type_room_state_hdr,
    type_room_state,
    type_room,
    type_plm,
    type_plm_set,
    type_scroll_plm_data,
    type_scroll_set,
    type_background,
    type_door,
    type_door_set,
    type_fx,
    type_save_room,
    type_level_data,
    type_enemy,
    type_enemy_set,
    type_enemy_gfx,
    type_scroll_asm,
    type_tile_sheet,
    type_palette,
    type_tile_table,
    type_tile_set,
    type_area_map
  }


  // Types of room states
  public enum StateType: int
  {
    None = 0x0000,
    Standard = 0xE5E6,
    Events = 0xE612,
    Bosses = 0xE629,
    TourianBoss = 0xE5FF,
    Morph = 0xE640,
    MorphMissiles = 0xE652,
    PowerBombs = 0xE669,
    SpeedBooster = 0xE676
  }

   
  // Interface for data classes that contains pointers to reallocatable objects.
  interface IRepointable
  {
    void Repoint ();
  }


  // Interface for data classes that are stored in compressed format on the rom.
  interface ICompressed
  {
    bool Compress ();
  }


  // Interface for data classes that may be referenced by other data classes.
  interface IReferenceableBy <T>: IReferenceable where T: Data
  {
    bool ReferenceMe (T source);   // return value indicates success/failure
    int  UnreferenceMe (T source); // return value is number of remaining references
  }


  interface IReferenceable
  {
    void DetachAllReferences ();   // removes all references from all implemented types T
  }



  // Interface for data classes that may be referenced by multiple other data classes.
  interface IReusable
  {
    int ReferenceCount {get;}
  }


//========================================================================================
// CLASS DATA
//========================================================================================


  abstract class Data
  {
    protected const int DefaultStartAddress = 0x3FFFFF; // Largest possible address.

    protected int startAddressPC; // PC address in ROM of data.

    public int StartAddressPC
    {
      get {return startAddressPC;}
    }

    public int StartAddressLR
    {
      get {return Tools.PCtoLR (startAddressPC);}
    }

    public abstract int Size {get;}
    // Read data from ROM at given PC address.
    public abstract bool ReadFromROM (Rom rom, int addressPC);
    // Write data to ROM at current position (addressPC), which is updated.
    public abstract bool WriteToROM (Stream rom, ref int addressPC);
    // Set default values.
    public abstract void SetDefault ();

    // Constructor;
    public Data () 
    {
      startAddressPC = DefaultStartAddress;
    }


    // Change the location within the ROM.
    public virtual void Reallocate (int addressPC)
    {
      startAddressPC = addressPC;
    }


    // Log data to text file.
    public virtual void Log ()
    {
      Logging.WriteLine (
        "DATA at $" + Tools.IntToHex (startAddressPC) +
        " of size " + Size + " byte");
      Logging.WriteLine ("");
    }
  } 


//========================================================================================
// CLASS RAW DATA
//========================================================================================


  class RawData: Data
  {
    protected List <byte> Bytes;

    public override int Size
    {
      get {return Bytes.Count;}
    }

    public byte this [int index]
    {
      get
      {
        if (index >= 0 && index < Bytes.Count)
          return Bytes [index];
        return 0;
      }
      set
      {
        if (index >= 0 && index < Bytes.Count)
          Bytes [index] = value;
      }
    }


    // Constructor.
    public RawData (): base ()
    {
      Bytes = new List <byte> ();
    }


    // Resize the data.
    public void SetSize (int newSize)
    {
      if (newSize < Bytes.Count)
        Bytes.RemoveRange (newSize, Bytes.Count - newSize);
      else
        for (int i = Bytes.Count; i < newSize; i++)
          Bytes.Add (0);
    }


    // Read raw data from ROM at given PC address.
    public override bool ReadFromROM (Rom rom, int addressPC)
    {
      rom.Seek (addressPC);
      byte [] newData = new byte [Bytes.Count];

      if (!rom.Read (newData, 0, Bytes.Count))
        return false;
      for (int n = 0; n < Bytes.Count; n++)
        Bytes [n] = newData [n];

      startAddressPC = addressPC;
      return true;
    }


    // Write raw data to ROM at current position (addressPC), which is updated.
    public override bool WriteToROM (Stream rom, ref int addressPC)
    {
      rom.Write (Bytes.ToArray (), 0, Bytes.Count);
      addressPC += Size;
      return true;
    }


    // Log data to text file.
    public override void Log ()
    {
      Logging.WriteLine (
        "RAW DATA at $" + Tools.IntToHex (startAddressPC) +
        " of size " + Size + " byte");
      Logging.WriteLine ("");
    }


    public override void SetDefault ()
    {
      Bytes.Clear ();
      startAddressPC = DefaultStartAddress;
    }

  } // class RawData

}