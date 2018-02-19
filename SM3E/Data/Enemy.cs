using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Reflection;
//using MBuild.Properties;
using System.Collections.Generic;


namespace SM3E
{

//========================================================================================
// CLASS Enemy 
//========================================================================================


  class Enemy: Data
  {
    public const int DefaultSize = 16;

    public int EnemyID;
    public int PosX;
    public int PosY;
    public int Tilemaps;
    public int Special;
    public int Graphics;
    public int Speed;
    public int Speed2;

    public EnemyType MyEnemyType;

    public override int Size
    {
      get {return DefaultSize;}
    }


    // Constructor.
    public Enemy (): base ()
    {
      EnemyID  = 0;
      PosX     = 0;
      PosY     = 0;
      Tilemaps = 0;
      Special  = 0;
      Graphics = 0;
      Speed    = 0;
      Speed2   = 0;

      MyEnemyType = null;
    }


    // Constructor, copy from existing enemy.
    public Enemy (Enemy source): base ()
    {
      EnemyID  = source.EnemyID;
      PosX     = source.PosX;
      PosY     = source.PosY;
      Tilemaps = source.Tilemaps;
      Special  = source.Special;
      Graphics = source.Graphics;
      Speed    = source.Speed;
      Speed2   = source.Speed2;

      MyEnemyType = source.MyEnemyType;
    }


    // Read data from ROM at given PC address.
    public override bool ReadFromROM (Rom rom, int addressPC)
    {
      byte [] b = new byte [DefaultSize];
      rom.Seek (addressPC);
      if (!rom.Read (b, 0, DefaultSize))
        return false;
      EnemyID  = Tools.ConcatBytes (b [ 0], b [ 1]);
      PosX     = Tools.ConcatBytes (b [ 2], b [ 3]);
      PosY     = Tools.ConcatBytes (b [ 4], b [ 5]);
      Tilemaps = Tools.ConcatBytes (b [ 6], b [ 7]);
      Special  = Tools.ConcatBytes (b [ 8], b [ 9]);
      Graphics = Tools.ConcatBytes (b [10], b [11]);
      Speed    = Tools.ConcatBytes (b [12], b [13]);
      Speed2   = Tools.ConcatBytes (b [14], b [15]);
      startAddressPC = addressPC;
      return true;
    }


    // Write data to ROM at current position (addressPC), which is updated.
    public override bool WriteToROM (Stream rom, ref int addressPC)
    {
      byte [] b = new byte [DefaultSize];
      Tools.CopyBytes (EnemyID , b,  0, 2);
      Tools.CopyBytes (PosX    , b,  2, 2);
      Tools.CopyBytes (PosY    , b,  4, 2);
      Tools.CopyBytes (Tilemaps, b,  6, 2);
      Tools.CopyBytes (Special , b,  8, 2);
      Tools.CopyBytes (Graphics, b, 10, 2);
      Tools.CopyBytes (Speed   , b, 12, 2);
      Tools.CopyBytes (Speed2  , b, 14, 2);
      rom.Write (b, 0, DefaultSize);
      addressPC += Size;
      return true;
    }


    // Set default values.
    public override void SetDefault ()
    {
      EnemyID  = 0xDCFF;
      PosX     = 0;
      PosY     = 0;
      Tilemaps = 0;
      Special  = 0; // [wip] look into better default values;
      Graphics = 0;
      Speed    = 0;
      Speed2   = 0;

      startAddressPC = DefaultStartAddress;
    }


    public bool Connect (List <EnemyType> enemyTypes)
    {
      MyEnemyType = enemyTypes.Find (x => x.EnemyID == EnemyID);
      return MyEnemyType != null;
    }


    public void Shift (int dx, int dy)
    {
      PosX += dx;
      PosY += dy;
    }

  } // class Enemy


//========================================================================================
// CLASS ENEMY SET 
//========================================================================================


  class EnemySet: Data, IReusable, IReferenceableBy <RoomState>
  {
    public const int TerminatorSize = 3;
    public const byte TerminatorByte = 0xFF;

    public List <Enemy> Enemies;
    public byte RequiredToKill;

    public List <RoomState> MyRoomStates;

    public override int Size
    {
      get {return Enemies.Count * Enemy.DefaultSize + TerminatorSize;}
    }

    public int EnemyCount
    {
      get {return Enemies.Count;}
    }

    public int ReferenceCount {get {return MyRoomStates.Count;}}


    // Constructor.
    public EnemySet (): base ()
    {
      Enemies = new List <Enemy> ();
      MyRoomStates = new List <RoomState> ();
      RequiredToKill = 0;
    }

    
    // Constructor, copy from exsisting enemy set.
    public EnemySet (EnemySet source): base ()
    {
      Enemies = new List <Enemy> ();
      MyRoomStates = new List <RoomState> ();
      RequiredToKill = source.RequiredToKill;
      foreach (Enemy e in source.Enemies)
        Enemies.Add (new Enemy (e));
    }


    // Read data from ROM at given PC address.
    public override bool ReadFromROM (Rom rom, int addressPC)
    {
      int sAddress = addressPC;
      byte [] b = new byte [2];
      rom.Seek (addressPC);
      if (!rom.Read (b, 0, 2))
        return false;

      Enemies.Clear ();
      while (b [0] != TerminatorByte || b [1] != TerminatorByte)
      {
        Enemy newEnemy = new Enemy ();
        if (!newEnemy.ReadFromROM (rom, addressPC))
          return false;
        addressPC += newEnemy.Size;
        Enemies.Add (newEnemy);
        rom.Seek (addressPC);
        if (!rom.Read (b, 0, 2))
          return false;
      }
      rom.Seek (addressPC + 2);
      if (!rom.Read (b, 0, 1))
        return false;
      RequiredToKill = b [0];
      startAddressPC = sAddress;
      return true;
    }


    // Write data to ROM at current position (addressPC), which is updated.
    public override bool WriteToROM (Stream rom, ref int addressPC)
    {
      for (int n = 0; n < Enemies.Count; n++)
        if (!Enemies [n].WriteToROM (rom, ref addressPC))
          return false;

      byte [] b = new byte [] {TerminatorByte, TerminatorByte, RequiredToKill};
      rom.Write (b, 0, TerminatorSize);
      addressPC += TerminatorSize;
      return true;
    }


    // Set default values.
    public override void SetDefault ()
    {
      Enemies.Clear ();
      RequiredToKill = 0;

      startAddressPC = DefaultStartAddress;
    }


    public bool Connect (List <EnemyType> enemyTypes)
    {
      bool success = true;
      for (int n = 0; n < Enemies.Count; n++)
        if (!Enemies [n].Connect (enemyTypes))
          success = false;
      return success;
    }


    public override void Reallocate (int addressPC)
    {
      startAddressPC = addressPC;
      for (int n = 0; n < Enemies.Count; n++)
      {
        Enemies [n].Reallocate (addressPC);
        addressPC += Enemies [n].Size;
      }
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
        r.SetEnemySet (null, out var ignore);
    }

//----------------------------------------------------------------------------------------

    public void Shift (int dx, int dy)
    {
      for (int n = 0; n < Enemies.Count; n++)
        Enemies [n].Shift (dx, dy);
    }

  } // class EnemySet


//========================================================================================
// CLASS ENEMY GFX 
//========================================================================================


  public enum EnemyGfxPalette
  {
    P1 = 0x0001,
    P2 = 0x0002,
    P3 = 0x0003,
    P4 = 0x0007
  }


  class EnemyGfx: Data, IReusable, IReferenceableBy <RoomState>
  {
    public const int BlockSize = 4;
    public const int TerminatorSize = 2;
    public const byte TerminatorByte = 0xFF;

    public List <int> EnemyIDs; // [wip] these 3 lists may be a bit redundant
    public List <int> Palettes; // [wip] these 3 lists may be a bit redundant

    public List <EnemyType> MyEnemyTypes; // [wip] these 3 lists may be a bit redundant
    public HashSet <RoomState> MyRoomStates;

    public override int Size
    {
      get {return EnemyIDs.Count * BlockSize + TerminatorSize;}
    }

    public int EnemyGfxCount
    {
      get {return EnemyIDs.Count;}
    }

    public int ReferenceCount {get {return MyRoomStates.Count;}}


    // Constructor.
    public EnemyGfx (): base ()
    {
      EnemyIDs = new List <int> ();
      Palettes = new List <int> ();
      MyEnemyTypes = new List <EnemyType> ();
      MyRoomStates = new HashSet <RoomState> ();
    }

    
    // Constructor, copy from existing enemy gfx.
    public EnemyGfx (EnemyGfx source): base ()
    {
      EnemyIDs = new List <int> ();
      Palettes = new List <int> ();
      MyEnemyTypes = new List <EnemyType> ();
      MyRoomStates = new HashSet <RoomState> ();
      for (int i = 0; i < source.EnemyIDs.Count; i++)
      {
        EnemyIDs.Add (source.EnemyIDs [i]);
        Palettes.Add (source.Palettes [i]);
        MyEnemyTypes.Add (source.MyEnemyTypes [i]);
      }
    }


    // Read data from ROM at given PC address.
    public override bool ReadFromROM (Rom rom, int addressPC)
    {
      int sAddress = addressPC;
      byte [] b = new byte [BlockSize];
      rom.Seek (addressPC);
      if (!rom.Read (b, 2, 2))
        return false;

      EnemyIDs.Clear ();
      Palettes.Clear ();
      while (b [2] != TerminatorByte || b [3] != TerminatorByte)
      {
        EnemyIDs.Add (Tools.ConcatBytes (b [2], b [3]));
        if (!rom.Read (b, 0, 4))
          return false;
        Palettes.Add (Tools.ConcatBytes (b [0], b [1]));
      }
      startAddressPC = addressPC;
      return true;
    }


    // Write data to ROM at current position (addressPC), which is updated.
    public override bool WriteToROM (Stream rom, ref int addressPC)
    {
      byte [] b = new byte [BlockSize];
      for (int n = 0; n < EnemyIDs.Count; n++)
      {
        Tools.CopyBytes (EnemyIDs [n], b, 0, 2);
        Tools.CopyBytes (Palettes [n], b, 2, 2);
        rom.Write (b, 0, BlockSize);
      }
      rom.Write (new byte [] {TerminatorByte, TerminatorByte}, 0, 2);
      addressPC += Size;
      return true;
    }


    // Set default values.
    public override void SetDefault ()
    {
      EnemyIDs.Clear ();
      Palettes.Clear ();

      startAddressPC = DefaultStartAddress;
    }


    public bool Connect (List <EnemyType> enemyTypes)
    {
      bool success = true;
      for (int i = 0; i < EnemyIDs.Count; i++) {
        MyEnemyTypes.Add (enemyTypes.Find (x => x.EnemyID == EnemyIDs [i]));
        if (MyEnemyTypes [i] == null)
          success = false;
      }
      return success;
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
        r.SetEnemyGfx (null, out var ignore);
    }

  } // class EnemyGfx


//========================================================================================
// CLASS ENEMY TYPE 
//========================================================================================


  class EnemyType
  {
    public int EnemyID = 0;
    // public int TileX = 0;  //  <- [wip] are these two necessary?
    // public int TileY = 0;  //  <-
    // public int TileWidth = 0;
    // public int TileHeight = 0;
    public string Name = String.Empty;
    public BlitImage Graphics;

    public int Index = 0;  // index in the enemy_types array that stores all available types.
  } // class EnemyType

}