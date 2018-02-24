using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Reflection;
//using MBuild.Properties;
using System.Collections.Generic;


namespace SM3E
{
  
  public enum ScrollColor: byte
  {
    Red = 0x00,
    Blue = 0x01,
    Green = 0x02,
    Unchanged = 0x03,
    None = 0xFF
  }


  interface IScrollData
  {
    ScrollColor this [int scrollIndex] {get; set;}
  }


//========================================================================================
// CLASS SCROLL SET 
//========================================================================================


  class ScrollSet: RawData, IScrollData, IReusable, IReferenceableBy <RoomState>
  {
    public const int AllBlue = 0x8F0000;
    public const int AllGreen = 0x8F0001;

    public HashSet <RoomState> MyRoomStates;


    public new ScrollColor this [int scrollIndex]
    {
      get
      {
        if (scrollIndex >= 0 && scrollIndex < Bytes.Count)
          return (ScrollColor) Bytes [scrollIndex];
        return ScrollColor.None;
      }
      set
      {
        if (scrollIndex >= 0 && scrollIndex < Bytes.Count)
        {
          byte color = (byte) value;
          if (color >= 0 && color < 3)
            Bytes [scrollIndex] = color;
        }
      }
    }

    public int ReferenceCount {get {return MyRoomStates.Count;}}


    // Constructor
    public ScrollSet (): base ()
    {
      MyRoomStates = new HashSet <RoomState> ();
    }


    // Constructor
    public ScrollSet (int screenCount, ScrollColor color): base ()
    {
      MyRoomStates = new HashSet <RoomState> ();
      byte b = (byte) color;
      for (int n = 0; n < screenCount; n++)
        Bytes.Add (b);
    }


    // Constructor, coping from existing scroll set.
    public ScrollSet (ScrollSet source): base ()
    {
      MyRoomStates = new HashSet <RoomState> ();
      for (int n = 0; n < source.Bytes.Count; n++)
        Bytes.Add (source.Bytes [n]);
    }


    // Set default values.
    public override void SetDefault ()
    {
      Bytes.Clear ();
      Bytes.Add (1);

      startAddressPC = DefaultStartAddress;
    }


    public bool ReferenceMe (RoomState source)
    {
      MyRoomStates.Add (source);
      return true;
    }


    public int UnreferenceMe (RoomState source)
    {
      MyRoomStates.Remove (source);
      return MyRoomStates.Count;
    }


    public void DetachAllReferences ()
    {
      foreach (RoomState r in MyRoomStates)
        r.SetScrollSet (null, out var ignore);
    }

//----------------------------------------------------------------------------------------

    public void Resize (int currentWidth, int newX, int newY, int newW, int newH)
    {
      var newBytes = new List <byte> ();
      int currentHeight = Bytes.Count / currentWidth;
      for (int y = newY; y < newY + newH; y++)
      {
        for (int x = newX; x < newX + newW; x++)
        {
          if (x >= 0 && x < currentWidth && y >= 0 && y < currentHeight)
            newBytes.Add (Bytes [currentWidth * y + x]);
          else
            newBytes.Add ((byte) ScrollColor.Red);
        }
      }
      Bytes = newBytes;
    }

  } // class ScrollSet


//========================================================================================
// ABSTRACT CLASS SCROLL MODIFICATION
//========================================================================================

  
  abstract class ScrollModification: Data, IScrollData
  {
    public List <int> Entries;

    
    public ScrollColor this [int scrollIndex]
    {
      get
      {
        foreach (int value in Entries)
        {
          if ((value & 0xFF) == scrollIndex)
            return (ScrollColor) (value >> 8);
        }
        return ScrollColor.Unchanged;
      }
      set
      {
        if (value == ScrollColor.Unchanged)
        {
          for (int n = 0; n < Entries.Count; n++)
            if ((Entries [n] & 0xFF) == scrollIndex)
            {
              Entries.RemoveAt (n);
              break;
            }
        }
        else
        {
          for (int n = 0; n < Entries.Count; n++)
            if ((Entries [n] & 0xFF) == scrollIndex)
            {
              Entries [n] = ((byte) value << 8) + scrollIndex;
              return;
            }
          Entries.Add (((byte) value << 8) + scrollIndex);
        }
      }
    }

//----------------------------------------------------------------------------------------

    public void Resize (int currentWidth, int currentHeight,
                        int newX, int newY, int newW, int newH)
    {
      var newEntries = new List <int> ();
      foreach (int scrollMod in Entries)
      {
        int index = scrollMod & 0xFF;
        int x = index % currentWidth - newX;
        int y = index / currentWidth - newY;
        if (x >= 0 && x < newW && y >= 0 && y < newH)
          newEntries.Add ((scrollMod & 0xFF00) | y * newW + x); 
      }
      Entries = newEntries;
    }

  } // abstract class ScrollModification


//========================================================================================
// CLASS SCROLL PLM DATA 
//========================================================================================


  class ScrollPlmData: ScrollModification, IReferenceableBy <Plm>
  {
    public const int BlockSize = 2;
    public const int TerminatorSize = 1;

    //public List <int> Entries;
    
    public HashSet <Plm> MyPlms;

    public override int Size
    {
      get {return Entries.Count * BlockSize + TerminatorSize;}
    }


    // Constructor.
    public ScrollPlmData (): base ()
    {
      Entries = new List <int> ();
      MyPlms = new HashSet <Plm> ();
    }

 
    // Constructor, copy from existing scroll PLM data.
    public ScrollPlmData (ScrollPlmData source): base ()
    {
      Entries = new List <int> ();
      MyPlms = new HashSet <Plm> ();

      foreach (int i in source.Entries)
        Entries.Add (i);
    }


    // Read data from ROM at given PC address.
    public override bool ReadFromROM (Rom rom, int addressPC)
    {
      byte [] b = new byte [2];
      rom.Seek (addressPC);
      if (!rom.Read (b, 0, 1))
        return false;

      while (b [0] != 0x80 && Entries.Count < 100)
      {
        if (!rom.Read (b, 1, 1))
          return false;
        Entries.Add (Tools.ConcatBytes (b [0], b [1]));
        if (!rom.Read (b, 0, 1))
          return false;
      }
      startAddressPC = addressPC;
      return true;
    }


    // Write data to ROM at current position (addressPC), which is updated.
    public override bool WriteToROM (Stream rom, ref int addressPC)
    {
      byte [] b = new byte [2];
      for (int n = 0; n < Entries.Count; n++)
      {
        Tools.CopyBytes (Entries [n], b, 0, 2);
        rom.Write (b, 0, 2);
      }
      b [0] = 0x80;
      rom.Write (b, 0, 1);
      addressPC += Size;
      return true;
    }


    // Set default values.
    public override void SetDefault ()
    {
      Entries.Clear ();

      startAddressPC = DefaultStartAddress;
    }


    public bool ReferenceMe (Plm source)
    {
      MyPlms.Add (source);
      return true;
    }


    public int UnreferenceMe (Plm source)
    {
      MyPlms.Remove (source);
      return MyPlms.Count;
    }


    public void DetachAllReferences ()
    {
      foreach (Plm p in MyPlms)
        p.SetScrollPlmData (null, out var ignore);
    }

  } // Class ScrollPlmData


//========================================================================================
// CLASS SCROLL ASM 
//========================================================================================


  class ScrollAsm: ScrollModification, IReferenceableBy <Door>
  {
    public const int ColourCommandSize = 2;
    public const int ScrollCommandSize = 4;
    public const int HeaderTerminatorSize = 5;

    //public List <int> Entries;
    public HashSet <Door> MyDoors;

    public override int Size
    {
      get
      {
        int ColourCount = 0;
        byte CurrentColour = 0xFF;

        Entries.Sort ();
        for (int n = 0; n < Entries.Count; n++) {
          byte NewColour = (byte) (Entries [n] >> 8);
          if (NewColour != CurrentColour) {
            ColourCount++;
            CurrentColour = NewColour;
          }
        }
        return ColourCommandSize * ColourCount +
               ScrollCommandSize * Entries.Count +
               HeaderTerminatorSize;
      }
    }


    // Constructor.
    public ScrollAsm (): base ()
    {
      Entries = new List <int> ();
      MyDoors = new HashSet <Door> ();
    }


    // Constructor, copy from existing scroll ASM.
    public ScrollAsm (ScrollAsm source): base ()
    {
      Entries = new List <int> ();
      MyDoors = new HashSet <Door> ();
      foreach (int entry in source.Entries)
        Entries.Add (entry);
    }


    // Read data from ROM at given PC address.
    public override bool ReadFromROM (Rom rom, int addressPC)
    {
      byte ScrollColour = 0x00;

      byte [] b = new byte [3];
      rom.Seek (addressPC);
      if (!rom.Read (b, 0, 3) || 
          b [0] != 0x08 || b [1] != 0xE2 || b [2] != 0x20)
        return false;

      Entries.Clear ();
      do
      {
        if (!rom.Read (b, 0, 2))
          return false;
        switch (b [0])
        {
        case 0xA9: // set scroll colour
          ScrollColour = b [1];
          break;
        case 0x8F: // set scroll
          Entries.Add (Tools.ConcatBytes ((byte) (b [1] - 0x20), ScrollColour));
          if (!rom.Read (b, 0, 2) || (b [0] != 0xCD && b [1] != 0x7E))
            return false;
          break;
        case 0x28:
          if (b [1] != 0x60)
            return false;
          break;
        default:
          return false;
        }
      } while (b [0] != 0x28);

      startAddressPC = addressPC;
      return true;
    }


    // Write data to ROM at current position (addressPC), which is updated.
    public override bool WriteToROM (Stream rom, ref int addressPC)
    {
      byte CurrentColour = 0xFF;
      byte NewColour;
      byte Scroll;
      // byte [] b = new byte [4];

      addressPC += Size;
      rom.Write (new byte [] {0x08, 0xE2, 0x20}, 0, 3);
      for (int n = 0; n < Entries.Count; n++)
      {
        Scroll = (byte) Entries [n];
        NewColour = (byte) (Entries [n] >> 8);
        if (NewColour != CurrentColour)
        {
          rom.Write (new byte [] {0xA9, NewColour}, 0, 2);
          CurrentColour = NewColour;
        }
        rom.Write (new byte [] {0x8F, (byte) (Scroll + 0x20), 0xCD, 0x7E}, 0, 4);
      }
      rom.Write (new byte [] {0x28, 0x60}, 0, 2);
      return true;
    }


    // Set default values.
    public override void SetDefault ()
    {
      Entries.Clear ();

      startAddressPC = DefaultStartAddress;
    }


    public bool ReferenceMe (Door source)
    {
      MyDoors.Add (source);
      return true;
    }


    public int UnreferenceMe (Door source)
    {
      MyDoors.Remove (source);
      return MyDoors.Count;
    }


    public void DetachAllReferences ()
    {
      foreach (Door d in MyDoors)
        d.SetScrollAsm (null, out var ignore);
    }

  } // class ScrollAsm

}