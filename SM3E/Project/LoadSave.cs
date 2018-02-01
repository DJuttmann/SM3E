﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;

namespace SM3E
{

  partial class Project
  {
    private const string PlmFolder = "\\Data\\PLMs";
    private const string EnemyFolder = "\\Data\\Enemies";

    // The rom that is being edited.
    private Rom CurrentRom;
    private string RomFileName;
    private string ProjectFileName;


//========================================================================================
// Reading ROM data.

   
    // Read all data from the ROM, guided by data from the projectfile.
    public void Load (string projectFile)
    {
      List <int> roomAddresses;
      List <int> roomDoorCounts;
      List <string> roomNames;

      ReadProjectFile (projectFile, out roomAddresses, out roomDoorCounts, out roomNames);

      // Read uncompressed data from ROM.
      ReadRooms (CurrentRom, roomAddresses, roomNames, roomDoorCounts);
      ReadDoors (CurrentRom);
      ReadScrollSets (CurrentRom);
      ReadPlmSets (CurrentRom);
      ReadScrollPlmDatas (CurrentRom);
      ReadBackgrounds (CurrentRom);
      ReadFxs (CurrentRom);
      ReadSaveRooms (CurrentRom);
      ReadEnemySets (CurrentRom);
      ReadEnemyGfxs (CurrentRom);
      ReadScrollAsms (CurrentRom);
      ReadTileSets (CurrentRom);
      ReadAreaMaps (CurrentRom, AreaMap.Addresses);

      // Read Compressed data from ROM.
      ReadLevelData (CurrentRom);
      ReadTileTables (CurrentRom);
      ReadTileSheets (CurrentRom);
      ReadPalettes (CurrentRom);
      LoadMapTiles (CurrentRom);

      // Read data from Data folder.
      ReadPlmTypes ();
      ReadEnemyTypes ();

      // Connect the data objects.
      Connect ();
      LoadRoomTiles (0); //

      // Raise events.
      AreaListChanged?.Invoke (this, new ListLoadEventArgs (0));
      SelectArea (0);
      PlmTypeListChanged?.Invoke (this, new ListLoadEventArgs (0));
      SelectPlmType (0);
      EnemyTypeListChanged?.Invoke (this, new ListLoadEventArgs (0));
      SelectEnemyType (0);
      ScrollColorListChanged?.Invoke (this, new ListLoadEventArgs (0));
    }


    // Read information about the ROM from the project file, including:
    // - The file name of the ROM.
    // - A list of area names.
    // - A list of room names, addresses and number of doors per room.
    // - Information about available spaces in different banks.
    private bool ReadProjectFile (string filename,
                                  out List <int> roomAddresses,
                                  out List <int> roomDoorCounts,
                                  out List <string> roomNames)
    {
      RomFileName = null;
      roomAddresses = new List <int> ();
      roomDoorCounts = new List <int> ();
      roomNames = new List <string> ();

      string [] lines;
      try {lines = System.IO.File.ReadAllLines (filename);}
      catch {return false;}

      for (int n = 0; n < lines.Length; n++)
      {
        List <string> segments = Tools.SplitString (lines [n]);
        if (segments.Count == 0)
          continue;

        // Load rom file.
        if (segments [0] == "rom" && segments.Count > 1)
        {
          RomFileName = segments [1];
          CurrentRom = new Rom (RomFileName);
        }

        // Load area names.
        else if (segments [0] == "areas") {
          for (int i = 0; i < AreaCount; i++) {
            if (i + 1 < segments.Count)
              Areas [i] = segments [i + 1];
            else
              Areas [i] = Tools.IntToHex (n);
          }
        }

        // Load a ROM section.
        else if (segments [0] == "section" && segments.Count > 2)
        {
          CurrentRom.AddSection (segments [1], RomSection.StringToType (segments [2]));
          for (int i = 3; i < segments.Count; i++)
          {
            string dataName = segments [i].ToLower ();
              CurrentRom.AddDataList (segments [1], dataName,
                                      StringToDataList (segments [1]));
          }
        }

        // Load a data block in a section
        else if (segments [0] == "block" && segments.Count > 3)
        {
          int start = Tools.HexToInt (segments [2]);
          int end = Tools.HexToInt (segments [3]);
          CurrentRom.AddBlock (segments [1], start, end - start);
        }

        // Load room data
        else if (segments [0] == "room") {
          if (segments.Count > 1) {
            roomAddresses.Add (Tools.HexToInt (segments [1]));
            if (segments.Count > 2)
              roomDoorCounts.Add (Tools.DecToInt (segments [2]));
            else
              roomDoorCounts.Add (0);
            if (segments.Count > 3)
              roomNames.Add (segments [3]);
            else
              roomNames.Add ("");
          }
        }
      }
      return true;
    }


    // Convert string to data collection reference.
    private List <Data> StringToDataList (string s)
    {
      s = s.ToLower ();
      if (s.Length == 5 && s.Substring (0, 4) == "room")
      {
        int index = s [4] - '0';
        if (index >= 0 && index < 8)
          return Rooms [index];
        return null;
      }
      switch (s)
      {
      case "doorsets":
        return DoorSets;
      case "doors":
        return Doors;
      case "scrollsets":
        return ScrollSets;
      case "plmsets":
        return PlmSets;
      case "scrollplmdatas":
        return ScrollPlmDatas;
      case "backgrounds":
        return Backgrounds;
      case "fxs":
        return Fxs;
      case "saverooms":
        return SaveRooms;
      case "leveldatas":
        return LevelDatas;
      case "enemysets":
        return EnemySets;
      case "enemygfxs":
        return EnemyGfxs;
      case "scrollasms":
        return ScrollAsms;
      case "tilesets":
        return TileSets;
      case "tiletables":
        return TileTables;
      case "tilesheets":
        return TileSheets;
      case "palettes":
        return Palettes;
      case "areamaps":
        return AreaMaps;
      default:
        return null;
      }
    }



    // Connect all loaded data objects.
    private bool Connect ()
    {
      var AllRooms = new List <Data> ();
      foreach (var areaRooms in Rooms)
        AllRooms.AddRange (areaRooms);

      for (int n = 0; n < AllRooms.Count; n++)
        ((Room) AllRooms [n]).Connect (DoorSets);
      for (int n = 0; n < DoorSets.Count; n++)
        ((DoorSet) DoorSets [n]).Connect (Doors);
      for (int n = 0; n < Doors.Count; n++)
        ((Door) Doors [n]).Connect (AllRooms, ScrollAsms);
      for (int n = 0; n < RoomStates.Count; n++)
        ((RoomState) RoomStates [n]).Connect (PlmSets, ScrollSets, Backgrounds, Fxs, 
                                LevelDatas, EnemySets, EnemyGfxs);
      for (int n = 0; n < PlmSets.Count; n++)
        ((PlmSet) PlmSets [n]).Connect (ScrollPlmDatas, PlmTypes);
      for (int n = 0; n < Fxs.Count; n++)
        ((Fx) Fxs [n]).Connect (Doors);
      for (int n = 0; n < SaveRooms.Count; n++)
        ((SaveRoom) SaveRooms [n]).Connect (AllRooms, Doors);
      for (int n = 0; n < EnemySets.Count; n++)
        ((EnemySet) EnemySets [n]).Connect (EnemyTypes);
      for (int n = 0; n < EnemyGfxs.Count; n++)
        ((EnemyGfx) EnemyGfxs [n]).Connect (EnemyTypes);
      for (int n = 0; n < TileSets.Count; n++)
        ((TileSet) TileSets [n]).Connect (TileTables, TileSheets, Palettes);

      return true;
    }

//----------------------------------------------------------------------------------------

    // Read all rooms from ROM.
    private void ReadRooms (Rom rom, List <int> Addresses, List <string> Names,
                            List <int> DoorCounts)
    {
      foreach (var areaRooms in Rooms)
        areaRooms.Clear ();
      for (int n = 0; n < Addresses.Count; n++)
      {
        Room newRoom = new Room ();
        newRoom.ReadFromROM (rom, Addresses [n]);
        newRoom.Name = Names [n];
        Rooms [newRoom.Area].Add (newRoom);
        var newDoorSet = new DoorSet () {DoorCount = DoorCounts [n]};
        newDoorSet.ReadFromROM (rom, newRoom.DoorsPtrPC);
        DoorSets.Add (newDoorSet);
        for (int i = 0; i < newRoom.RoomStates.Count; i++)
          RoomStates.Add (newRoom.RoomStates [i]);
      }
    }


    // Read all doors from ROM.
    private void ReadDoors (Rom rom)
    {
      List <int> addressesPC = new List <int> ();
      foreach (DoorSet d in DoorSets) {
        for (int i = 0; i < d.DoorCount; i++) {
          int ad_PC = Tools.LRtoPC (d.DoorPtrs [i]);
          addressesPC.Add (ad_PC);
        }
      }
      Tools.RemoveDuplicates (addressesPC);
      Doors.Clear ();
      for (int n = 0; n < addressesPC.Count; n++)
      {
        Doors.Add (new Door ());
        Doors [n].ReadFromROM (rom, addressesPC [n]);
      }
    }


    // Read all scroll sets from ROM.
    private void ReadScrollSets (Rom rom)
    {
      ScrollSets.Clear ();
      List <int> addressesPC = new List <int> ();
      for (int k = 0; k < AreaCount; k++)
        foreach (Room r in Rooms [k])
        {
          int roomArea = r.RoomW * r.RoomH;
          int stateCount = r.RoomStates.Count;
          addressesPC.Clear ();
          for (int i = 0; i < stateCount; i++)
          {
            int address = Tools.LRtoPC (r.RoomStates [i].RoomScrollsPtr);
            if (address != ScrollSet.AllBlue && address != ScrollSet.AllGreen)
            addressesPC.Add (address);
          }
          Tools.RemoveDuplicates (addressesPC);
          for (int i = 0; i < addressesPC.Count; i++)
          {
            var s = new ScrollSet ();
            s.SetSize (roomArea);
            s.ReadFromROM (rom, addressesPC [i]);
            ScrollSets.Add (s);
          }
        }
    }


    // Read all PLM sets from ROM.
    private void ReadPlmSets (Rom rom)
    {
      List <int> addressesPC = new List <int> ();
      foreach (RoomState r in RoomStates)
      {
        int address = Tools.LRtoPC (r.PlmSetPtr);
        if (address != 0)
          addressesPC.Add (address);
      }
      Tools.RemoveDuplicates (addressesPC);
      PlmSets.Clear ();
      for (int n = 0; n < addressesPC.Count; n++)
      {
        PlmSets.Add (new PlmSet ());
        PlmSets [n].ReadFromROM (rom, addressesPC [n]);
      }
    }


    // Read all scroll plm datas from ROM.
    private void ReadScrollPlmDatas (Rom rom)
    {
      List <int> addressesPC = new List <int> ();
      foreach (PlmSet p in PlmSets) {
        int plmCount = p.PlmCount;
        for (int i = 0; i < plmCount; i++) {
          int address = Tools.LRtoPC (p.Plms [i].ScrollDataPtr);
          if (address != 0)     // Skip non-scroll PLMs
            addressesPC.Add (address);
        }
      }
      Tools.RemoveDuplicates (addressesPC);
      ScrollPlmDatas.Clear ();
      for (int n = 0; n < addressesPC.Count; n++)
      {
        ScrollPlmDatas.Add (new ScrollPlmData ());
        ScrollPlmDatas [n].ReadFromROM (rom, addressesPC [n]);
      }
    }


    // Read all backgrounds from ROM.
    private void ReadBackgrounds (Rom rom)
    {
      List <int> addressesPC = new List <int> ();
      foreach (RoomState r in RoomStates) {
        int address = Tools.LRtoPC (r.BackgroundPtr);
        if (address != 0)     // Skip invalid addresses
          addressesPC.Add (address);
      }
      Tools.RemoveDuplicates (addressesPC);
      Backgrounds.Clear ();
      for (int n = 0; n < addressesPC.Count; n++)
      {
        Backgrounds.Add (new Background ());
        Backgrounds [n].ReadFromROM (rom, addressesPC [n]);
      }
    }


    // Read all fxs from ROM.
    private void ReadFxs (Rom rom)
    {
      List <int> addressesPC = new List <int> ();
      foreach (RoomState r in RoomStates) {
        int address = Tools.LRtoPC (r.FxPtr);
        if (address != 0)     // Skip invalid addresses
          addressesPC.Add (address);
      }
      Tools.RemoveDuplicates (addressesPC);
      Fxs.Clear ();
      for (int n = 0; n < addressesPC.Count; n++)
      {
        Fxs.Add (new Fx ());
        Fxs [n].ReadFromROM (rom, addressesPC [n]);
      }
    }


    // Read all save rooms from ROM.
    private void ReadSaveRooms (Rom rom)
    {
      int address = SaveRoom.SaveRoomsAddress;
      SaveRooms.Clear ();
      for (int n = 0; n < SaveRoom.Count; n++) {
        SaveRooms.Add (new SaveRoom ());
        SaveRooms [n].ReadFromROM (rom, address);
        address += SaveRoom.DefaultSize;
      }
    }


    // Read all level datas from ROM.
    private void ReadLevelData (Rom rom)
    {
      List <int> addressesPC = new List <int> ();
      foreach (RoomState r in RoomStates) {
        int address = Tools.LRtoPC (r.LevelDataPtr);
        if (address != 0)     // Skip invalid addresses
          addressesPC.Add (address);
      }
      Tools.RemoveDuplicates (addressesPC);
      LevelDatas.Clear ();
      for (int n = 0; n < addressesPC.Count; n++)
      {
        LevelDatas.Add (new LevelData ());
        LevelDatas [n].ReadFromROM (rom, addressesPC [n]);
      }
    }


    // Read all enemy sets from ROM.
    private void ReadEnemySets (Rom rom)
    {
      List <int> addressesPC = new List <int> ();
      foreach (RoomState r in RoomStates) {
        int address = Tools.LRtoPC (r.EnemySetPtr);
        if (address != 0)     // Skip invalid addresses
          addressesPC.Add (address);
      }
      Tools.RemoveDuplicates (addressesPC);
      EnemySets.Clear ();
      for (int n = 0; n < addressesPC.Count; n++)
      {
        EnemySets.Add (new EnemySet ());
        EnemySets [n].ReadFromROM (rom, addressesPC [n]);
      }
    }


    // Read all enemy gfxs from ROM.
    private void ReadEnemyGfxs (Rom rom)
    {
      List <int> addressesPC = new List <int> ();
      foreach (RoomState r in RoomStates) {
        int address = Tools.LRtoPC (r.EnemyGfxPtr);
        if (address != 0)     // Skip invalid addresses
          addressesPC.Add (address);
      }
      Tools.RemoveDuplicates (addressesPC);
      EnemyGfxs.Clear ();
      for (int n = 0; n < addressesPC.Count; n++)
      {
        EnemyGfxs.Add (new EnemyGfx ());
        EnemyGfxs [n].ReadFromROM (rom, addressesPC [n]);
      }
    }


    // Read all scroll ASMs from ROM.
    private void ReadScrollAsms (Rom rom)
    {
      List <int> addressesPC = new List <int> ();
      foreach (Door d in Doors) {
        int address = Tools.LRtoPC (d.DoorAsmPtr);
        if (address != 0)     // Skip invalid addresses
          addressesPC.Add (address);
      }
      Tools.RemoveDuplicates (addressesPC);
      ScrollAsms.Clear ();
      for (int n = 0; n < addressesPC.Count; n++)
      {
        var s = new ScrollAsm ();
        if (s.ReadFromROM (rom, addressesPC [n]))
          ScrollAsms.Add (s);
      }
    }


    // Read all tile sets from ROM.
    private void ReadTileSets (Rom rom)
    {
      int addresPC = TileSet.TileSetsAddresPC;

      TileSets.Clear ();
      for (int n = 0; n < TileSet.Count; n++) {
        TileSets.Add (new TileSet ());
        TileSets [n].ReadFromROM (rom, addresPC);
        addresPC += TileSet.DefaultSize;
      }
    }


    // Read all tile tables from ROM.
    private void ReadTileTables (Rom rom)
    {
      List <int> addressesPC = new List <int> ();
      foreach (TileSet t in TileSets) {
        int address = Tools.LRtoPC (t.SceTablePtr);
        if (address != 0)     // Skip invalid addresses
          addressesPC.Add (address);
      }
      Tools.RemoveDuplicates (addressesPC);
      TileTables.Clear ();
      TileTables.Add (new TileTable ());
      TileTables [0].ReadFromROM (rom, TileTable.CreAddressPC);
      for (int n = 0; n < addressesPC.Count; n++) {
        TileTables.Add (new TileTable ());
        TileTables [n + 1].ReadFromROM (rom, addressesPC [n]);
      }
    }


    // Read all tile sheets from ROM.
    private void ReadTileSheets (Rom rom)
    {
      List <int> addressesPC = new List <int> ();
      foreach (TileSet t in TileSets) {
        int address = Tools.LRtoPC (t.SceSheetPtr);
        if (address != 0)     // Skip invalid addresses
          addressesPC.Add (address);
      }
      Tools.RemoveDuplicates (addressesPC);
      TileSheets.Clear ();
      TileSheets.Add (new CompressedTileSheet ());
      TileSheets [0].ReadFromROM (rom, TileSheet.CreAddressPC);
      for (int n = 0; n < addressesPC.Count; n++) {
        TileSheets.Add (new CompressedTileSheet ());
        TileSheets [n + 1].ReadFromROM (rom, addressesPC [n]);
      }
    }


    // Read all palettes from ROM.
    private void ReadPalettes (Rom rom)
    {
      List <int> addressesPC = new List <int> ();
      foreach (TileSet t in TileSets) {
        int address = Tools.LRtoPC (t.PalettePtr);
        if (address != 0)     // Skip invalid addresses
          addressesPC.Add (address);
      }
      Tools.RemoveDuplicates (addressesPC);
      Palettes.Clear ();
      for (int n = 0; n < addressesPC.Count; n++)
      {
        Palettes.Add (new Palette ());
        Palettes [n].ReadFromROM (rom, addressesPC [n]);
      }
    }


    // Read all area maps from ROM.
    private void ReadAreaMaps (Rom rom, int [] addressesPC)
    {
      AreaMaps.Clear ();
      for (int n = 0; n < AreaMap.Count; n++)
      {
        AreaMaps.Add (new AreaMap ());
        AreaMaps [n].ReadFromROM (rom, addressesPC [n]);
      }
    }


    // Read all PLM types from PLM folder.
    private void ReadPlmTypes ()
    {
      string [] paths = Directory.GetFiles (Environment.CurrentDirectory +
                                            PlmFolder);
      for (int i = 0; i < paths.Length; i++)
      {
        var newPlm = new PlmType ();
        string filename = Tools.FilenameFromPath (paths [i]);
        Tools.TrimFileExtension (ref filename, out string extension);
        if (extension.ToLower () == ".png" && filename.Length > 5)
        {
          newPlm.PlmID = Tools.HexToInt (filename.Substring (0, 4));
          newPlm.Name = filename.Substring (5, filename.Length - 5);
          newPlm.Graphics = new BlitImage (GraphicsIO.LoadBitmap (paths [i]));
          PlmTypes.Add (newPlm);
        }
      }
      PlmTypes.Sort ((x, y) => x.PlmID - y.PlmID);
      for (int i = 0; i < PlmTypes.Count; i++)
        PlmTypes [i].Index = i;
    }


    // Read all enemy types from enemy folder.
    private void ReadEnemyTypes ()
    {
      string [] paths = Directory.GetFiles (Environment.CurrentDirectory +
                                            EnemyFolder);
      for (int i = 0; i < paths.Length; i++)
      {
        var newEnemy = new EnemyType ();
        string filename = Tools.FilenameFromPath (paths [i]);
        Tools.TrimFileExtension (ref filename, out string extension);
        if (extension.ToLower () == ".png" && filename.Length > 5)
        {
          newEnemy.EnemyID = Tools.HexToInt (filename.Substring (0, 4));
          newEnemy.Name = filename.Substring (5, filename.Length - 5);
          newEnemy.Graphics = new BlitImage (GraphicsIO.LoadBitmap (paths [i]));
          EnemyTypes.Add (newEnemy);
        }
      }
      EnemyTypes.Sort ((x, y) => x.EnemyID - y.EnemyID);
      for (int i = 0; i < EnemyTypes.Count; i++)
        EnemyTypes [i].Index = i;
    }


//========================================================================================
// Writing ROM data.

    
    // Reallocate all data objects.
    private void ReallocateAll ()
    {
      foreach (RomSection s in CurrentRom.Sections)
        s.Reallocate ();
    }
    
    
    // Repoint all data objects.
    private void RepointAll ()
    {
      List <Data> objects = CurrentRom.AllData;
      foreach (Data d in objects)
        if (d is IRepointable r)
          r.Repoint ();
    }

    
    // Write information about the ROM to the project file
    private void WriteProjectFile ()
    {
      StreamWriter output = new StreamWriter (ProjectFileName);

      // Write ROM filename.
      output.Write ("rom " + RomFileName);
      output.Write (Environment.NewLine);

      // Write area names.
      output.Write ("Areas");
      for (int i = 0; i < AreaCount; i++)
        output.Write (" " + Areas [i]);
      output.Write (Environment.NewLine);

      // Write section data.
      foreach (RomSection s in CurrentRom.Sections)
      {
        output.Write ("section \"" + s.Name + "\" " + s.SectionType.ToString ());
        foreach (KeyValuePair <string, List <Data>> kv in s.Data)
          output.Write (" \"" + kv.Key + "\"");
        output.Write (Environment.NewLine);

        // Write blocks
        foreach (var block in s.Blocks)
        {
          output.Write ("block \"" + s.Name + "\" " +
                        Tools.IntToHex (block.Item1) + " " + 
                        Tools.IntToHex (block.Item2));
          output.Write (Environment.NewLine);
        }
      }

      // Write room data.
      for (int i = 0; i < AreaCount; i++)
      {
        foreach (Room r in Rooms [i])
        {
          output.Write ("room\t" +  Tools.IntToHex (r.StartAddressPC) + "\t" +
                        r.RoomStates.Count.ToString () + "\t\"" +
                        r.Name + "\"");
          output.Write (Environment.NewLine);
        }
      }
    }


    // Write all data to the Rom.
    public void Save ()
    {
      List <Data> objects = CurrentRom.AllData;
      objects.Sort ((x, y) => x.StartAddressPC - y.StartAddressPC);
      
      Stream output = new FileStream (RomFileName, FileMode.Create);
      int address = 0;
      for (int n = 0; n < objects.Count; n++)
      {
        if (address < objects [n].StartAddressPC)
        {
          CurrentRom.Seek (address);
          CurrentRom.WriteToFile (output, objects [n].StartAddressPC - address);
        }
        objects [n].WriteToROM (output, ref address);
      }

      // Round address up to multiple of bank size and write remaining data up to there.
      int roundedSize = (address + 0x7FFF) ^ 0x7FFF;
      if (address < roundedSize)
      {
        CurrentRom.Seek (address);
        CurrentRom.WriteToFile (output, roundedSize - address);
      }
    }

  } // partial class project

}