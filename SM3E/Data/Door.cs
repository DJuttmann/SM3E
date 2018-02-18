using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Reflection;
//using MBuild.Properties;
using System.Collections.Generic;


namespace SM3E
{

    public enum DoorAsmType
    {
      None,
      Regular,
      Scroll
    }


//========================================================================================
// CLASS DOOR
//========================================================================================


  class Door: Data, IRepointable, IReferenceableBy <DoorSet>
  {
    public const int DefaultSize = 12;
    public const int ElevatorPadSize = 2;

    public int RoomPtr; // LoROM address
    public byte Bitflag;
    public byte Direction;
    public byte DoorCapX;
    public byte DoorCapY;
    public byte ScreenX;
    public byte ScreenY;
    public int DistanceToSpawn;
    public int DoorAsmPtr; // LoROM address
    public bool ElevatorPad; // true if the door is an elevator pad

    public Room MyTargetRoom;
    public ScrollAsm MyScrollAsm;
    public Asm MyDoorAsm;
    public HashSet <DoorSet> MyDoorSets;

    public override int Size
    {
      get {return ElevatorPad ? ElevatorPadSize : DefaultSize;}
    }


    // Constructor.
    public Door (): base ()
    {
      RoomPtr         = 0;
      Bitflag         = 0;
      Direction       = 0;
      DoorCapX        = 0;
      DoorCapY        = 0;
      ScreenX         = 0;
      ScreenY         = 0;
      DistanceToSpawn = 0;
      DoorAsmPtr      = 0;
      ElevatorPad = false;

      MyTargetRoom = null;
      MyScrollAsm = null;
      MyDoorSets = new HashSet <DoorSet> ();
    }


    // Read data from ROM at given PC address.
    public override bool ReadFromROM (Rom rom, int addressPC)
    {
      byte [] b = new byte [DefaultSize];
      rom.Seek (addressPC);
      if (!rom.Read (b, 0, 2))
        return false;

      if (b [0] == 0 && b [1] == 0)
      {
        ElevatorPad = true;
        startAddressPC = addressPC;
        return true;
      }

      if (!rom.Read (b, 2, DefaultSize - 2))
        return false;
      RoomPtr         = Tools.ConcatBytes (b [0], b [1], 0x8F);
      Bitflag         = b [2];
      Direction       = b [3];
      DoorCapX        = b [4];
      DoorCapY        = b [5];
      ScreenX         = b [6];
      ScreenY         = b [7];
      DistanceToSpawn = Tools.ConcatBytes (b [8], b [9]);
      DoorAsmPtr      = Tools.ConcatBytes (b [10], b [11], 0x8F);
      startAddressPC = addressPC;
      return true;
    }


    // Write data to ROM at current position (addressPC), which is updated.
    public override bool WriteToROM (Stream rom, ref int addressPC)
    {
      if (ElevatorPad)
        rom.Write (new byte [] {0, 0}, 0, 2);
      else
      {
        byte [] b = new byte [DefaultSize];
        Tools.CopyBytes (RoomPtr        , b,  0, 2);
        Tools.CopyBytes (Bitflag        , b,  2, 1);
        Tools.CopyBytes (Direction      , b,  3, 1);
        Tools.CopyBytes (DoorCapX       , b,  4, 1);
        Tools.CopyBytes (DoorCapY       , b,  5, 1);
        Tools.CopyBytes (ScreenX        , b,  6, 1);
        Tools.CopyBytes (ScreenY        , b,  7, 1);
        Tools.CopyBytes (DistanceToSpawn, b,  8, 2);
        Tools.CopyBytes (DoorAsmPtr     , b, 10, 2);
        rom.Write (b, 0, DefaultSize);
      }
      addressPC += Size;
      return true;
    }


    // Set default values.
    public override void SetDefault ()
    {
      RoomPtr         = 0;
      Bitflag         = 0;
      Direction       = 0;
      DoorCapX        = 0;
      DoorCapY        = 0;
      ScreenX         = 0;
      ScreenY         = 0;
      DistanceToSpawn = 0x8000;
      DoorAsmPtr      = 0;

      startAddressPC = DefaultStartAddress;
    }


    public bool Connect (List <Data> Rooms, List <Data> ScrollAsms, List <Data> DoorAsms)
    {
      MyTargetRoom = (Room) Rooms.Find (x => x.StartAddressLR == RoomPtr);
      if (MyTargetRoom != null)
        MyTargetRoom.MyIncomingDoors.Add (this);
      MyScrollAsm = (ScrollAsm) ScrollAsms.Find (x => x.StartAddressLR == DoorAsmPtr);
      if (MyScrollAsm != null)
        MyScrollAsm.MyDoors.Add (this);
      else
      {
        MyDoorAsm = (Asm) DoorAsms.Find (x => x.StartAddressLR == DoorAsmPtr);
        if (MyDoorAsm != null)
          MyDoorAsm.MyReferringData.Add (this);
      }
      return MyTargetRoom != null;
    }


    public void Repoint ()
    {
      if (MyTargetRoom != null)
        RoomPtr = MyTargetRoom.StartAddressLR;
      if (MyScrollAsm != null)
        DoorAsmPtr = MyScrollAsm.StartAddressLR;
    }


    public bool ReferenceMe (DoorSet source)
    {
      MyDoorSets.Add (source);
      return true;
    }


    public int UnreferenceMe (DoorSet source)
    {
      MyDoorSets.Remove (source);
      return MyDoorSets.Count;
    }


    public void DetachAllReferences ()
    {
      foreach (DoorSet d in MyDoorSets)
        d.RemoveDoor (this);
    }

//----------------------------------------------------------------------------------------

    public bool GetElevatorBit ()
    {
      return (Bitflag & 0x80) > 0;
    }


    public int GetDirection ()
    {
      return Direction & 3;
    }


    public bool GetDoorCloses ()
    {
      return (Direction & 4) > 0;
    }


    public void SetElevatorBit (bool isElevator)
    {
      if (isElevator)
        Bitflag |= 0x80;
      else
        Bitflag &= 0x7F;
    }


    public void SetAreaTransitionBit (bool isAreaTransition)
    {
      if (isAreaTransition)
        Bitflag |= 0x40;
      else
        Bitflag &= 0xBF;
    }


    public void SetDirection (int direction)
    {
      if (direction < 4)
      {
        Direction &= 4;
        Direction |= (byte) direction;
      }
    }


    public void SetDoorCloses (bool doorCloses)
    {
      if (doorCloses)
        Direction |= 4;
      else
        Direction &= 3;
    }


    public void SetDestination (Room target)
    {
      MyTargetRoom.UnreferenceMe (this);
      MyTargetRoom = null;
      if (target?.ReferenceMe (this) ?? false)
        MyTargetRoom = target;
    }


    public void SetScrollAsm (ScrollAsm target, out ScrollAsm DeleteScrollAsm)
    {
      MyDoorAsm?.UnreferenceMe (this);
      MyDoorAsm = null;
      DeleteScrollAsm = MyScrollAsm?.UnreferenceMe (this) == 0 ? MyScrollAsm : null;
      MyScrollAsm = null;
      if (target?.ReferenceMe (this) ?? false)
        MyScrollAsm = target;
    }


    public void SetDoorAsm (Asm target, out ScrollAsm DeleteScrollAsm)
    {
      MyDoorAsm?.UnreferenceMe (this);
      MyDoorAsm = null;
      DeleteScrollAsm = MyScrollAsm?.UnreferenceMe (this) == 0 ? MyScrollAsm : null;
      MyScrollAsm = null;
      if (target?.ReferenceMe (this) ?? false)
        MyDoorAsm = target;
    }

  } // Class Door


//========================================================================================
// CLASS DOOR SET
//========================================================================================


  class DoorSet: Data, IRepointable, IReferenceableBy <Room>
  {
    public List <int> DoorPtrs; // LoRom Addresses

    public List <Door> MyDoors;
    public Room MyRoom;

    public override int Size
    {
      get {return DoorPtrs.Count * 2;}
    }

    public int DoorCount
    {
      get {return DoorPtrs.Count;}
      set
      {
        if (value < DoorPtrs.Count)
          DoorPtrs.RemoveRange (value, DoorPtrs.Count - value);
        else
          for (int i = DoorPtrs.Count; i < value; i++)
            DoorPtrs.Add (0x838000); // add default null ptrs
      }
    }


    // Constructor
    public DoorSet (): base ()
    {
      DoorPtrs = new List <int> ();
      MyDoors = new List <Door> ();
      MyRoom = null;
    }


    // Read data from ROM at given PC address. (DoorCount must be set beforehand)
    public override bool ReadFromROM (Rom rom, int addressPC)
    {
      byte [] b = new byte [Size];
      rom.Seek (addressPC);
      if (!rom.Read (b, 0, Size))
        return false;

      for (int n = 0; n < DoorCount; n++)
        DoorPtrs [n] = Tools.ConcatBytes (b [2 * n], b [2 * n + 1], 0x83);

      startAddressPC = addressPC;
      return true;
    }


    // Write data to ROM at current position (addressPC), which is updated.
    public override bool WriteToROM (Stream rom, ref int addressPC)
    {
      byte [] b = new byte [Size];
      for (int n = 0; n < DoorCount; n++)
        Tools.CopyBytes (DoorPtrs [n], b, 2 * n, 2);
      rom.Write (b, 0, Size);
      addressPC += Size;
      return true;
    }


    // Set default values.
    public override void SetDefault ()
    {
      DoorPtrs.Clear ();
      startAddressPC = DefaultStartAddress;
    }


    public bool Connect (List <Data> Doors)
    {
      bool success = true;
      for (int n = 0; n < DoorPtrs.Count; n++) {
        MyDoors.Add ((Door) Doors.Find (x => x.StartAddressLR == DoorPtrs [n]));
        if (MyDoors [n] != null)
          MyDoors [n].MyDoorSets.Add (this);
        else
          success = false;
      }
      return success;
    }


    public void Repoint ()
    {
      DoorPtrs.Clear ();
      for (int n = 0; n < MyDoors.Count; n++)
        if (MyDoors [n] != null)
          DoorPtrs. Add (MyDoors [n].StartAddressLR);
    }


    public bool ReferenceMe (Room source)
    {
      if (MyRoom != null)
        return false;
      MyRoom = source;
      return true;
    }


    public int UnreferenceMe (Room source)
    {
      if (MyRoom == source)
        MyRoom = null;
      return MyRoom == null ? 0 : 1;
    }


    public void DetachAllReferences ()
    {
      MyRoom.SetDoorSet (null);
    }

//----------------------------------------------------------------------------------------

    // Add door and update Doors list
    void AddDoor (List <Door> Doors)
    {
      Door newDoor = new Door ();
      newDoor.Bitflag = 0;
      newDoor.Direction = 0;
      newDoor.DoorCapX = 0;
      newDoor.DoorCapY = 0;
      newDoor.ScreenX = 0;
      newDoor.ScreenY = 0;
      newDoor.DistanceToSpawn = 0;
      newDoor.MyTargetRoom = null;
      newDoor.MyDoorSets.Add (this);
      MyDoors.Add (newDoor);
      Doors.Add (newDoor);
    }

    
    // [wip] it may be dangerous to set a pointer in MyDoors to null!
    // perhaps remove it, as well remove the door in corresponding level data?
    public void RemoveDoor (Door target)
    {
      target.UnreferenceMe (this);
      int index = MyDoors.FindIndex (x => x == target);
      if (index > 0)
        MyDoors [index] = null;
    }

  } // class DoorSet

}