using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;


namespace SM3E
{

  partial class Project
  {
    private const string PlmFolder = "\\Data\\PLMs";
    private const string EnemyFolder = "\\Data\\Enemies";


//========================================================================================
// Reading ROM data.

   
    // Read all data from the ROM, guided by data from the projectfile.
    public void Load (string projectFile)
    {
      string romFile;
      string [] areaNames; 
      List <int> roomAddresses;
      List <int> roomDoorCounts;
      List <string> roomNames;

      ReadProjectFile (projectFile, out romFile, out areaNames,
                       out roomAddresses, out roomDoorCounts, out roomNames);

      // Read uncompressed data from ROM.
      // Stream rom = new FileStream (romFile, FileMode.Open, FileAccess.Read);
      Rom rom = new Rom (romFile);

      // System.Windows.MessageBox.Show ("Test 0");
      ReadRooms (rom, roomAddresses, roomNames, roomDoorCounts);
      ReadDoors (rom);
      ReadScrollSets (rom);
      ReadPlmSets (rom);
      ReadScrollPlmDatas (rom);
      ReadBackgrounds (rom);
      ReadFxs (rom);
      ReadSaveRooms (rom);
      ReadEnemySets (rom);
      ReadEnemyGfxs (rom);
      ReadScrollAsms (rom);
      ReadTileSets (rom);
      ReadAreaMaps (rom, AreaMap.Addresses);

      // Read compressed data from ROM.
      ReadLevelData (rom);
      ReadTileTables (rom);
      ReadTileSheets (rom);
      ReadPalettes (rom);
      LoadMapTiles (rom);

      ReadPlmTypes ();
      ReadEnemyTypes ();

      // Connect the data objects.
      Connect ();
      LoadRoomTiles (0); //
      SelectArea (0);

      // Raise events.
      AreaListChanged (this, new ListLoadEventArgs (0));
      //RoomListChanged (this, null);
      //RoomStateListChanged (this, null);
    }


    // Read information about the ROM from the project file, including:
    // - The file name of the ROM.
    // - A list of area names.
    // - A list of room names, addresses and number of doors per room.
    // - Information about available spaces in different banks.
    private bool ReadProjectFile (string filename,
                                  out string romFile,
                                  out string [] areaNames, 
                                  out List <int> roomAddresses,
                                  out List <int> roomDoorCounts,
                                  out List <string> roomNames)
    {
      romFile = "Test";
      areaNames = new string [AreaMap.Count];
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
        if (segments [0] == "rom" && segments.Count > 1)
          romFile = segments [1];
        if (segments [0] == "areas") {
          for (int i = 0; i < AreaMap.Count; i++) {
            if (segments.Count > n + 1)
              areaNames [i] = segments [i + 1];
            else
              areaNames [i] = Tools.IntToHex (n);
          }
        }

        /* [wip] add this back in.
        if (segments [0] == "bank83")
          strings_to_bank (segments, bank83);
        if (segments [0] == "bank8F") 
          strings_to_bank (segments, bank8F);
        if (segments [0] == "bankA1")
          strings_to_bank (segments, bankA1);
        if (segments [0] == "bankB4")
          strings_to_bank (segments, bankB4);
        if (segments [0] == "bankCX")
          strings_to_bank (segments, bankCX);
        */

        if (segments [0] == "room") {
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


    // Connect all loaded data objects.
    private bool Connect ()
    {
      List <Room> AllRooms = new List <Room> ();
      foreach (var areaRooms in Rooms)
        AllRooms.AddRange (areaRooms);

      for (int n = 0; n < AllRooms.Count; n++)
        AllRooms [n].Connect (DoorSets);
      for (int n = 0; n < DoorSets.Count; n++)
        DoorSets [n].Connect (Doors);
      for (int n = 0; n < Doors.Count; n++)
        Doors [n].Connect (AllRooms, ScrollAsms);
      for (int n = 0; n < RoomStates.Count; n++)
        RoomStates [n].Connect (PlmSets, ScrollSets, Backgrounds, Fxs, 
                                LevelDatas, EnemySets, EnemyGfxs);
      for (int n = 0; n < PlmSets.Count; n++)
        PlmSets [n].Connect (ScrollPlmDatas, PlmTypes);
      for (int n = 0; n < Fxs.Count; n++)
        Fxs [n].Connect (Doors);
      for (int n = 0; n < SaveRooms.Count; n++)
        SaveRooms [n].Connect (AllRooms, Doors);
      for (int n = 0; n < EnemySets.Count; n++)
        EnemySets [n].Connect (EnemyTypes);
      for (int n = 0; n < EnemyGfxs.Count; n++)
        EnemyGfxs [n].Connect (EnemyTypes);
      for (int n = 0; n < TileSets.Count; n++)
        TileSets [n].Connect (TileTables, TileSheets, Palettes);

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
        DoorSets.Add (new DoorSet ());
        DoorSets [n].DoorCount = DoorCounts [n];
        DoorSets [n].ReadFromROM (rom, newRoom.DoorsPtrPC);
        for (int i = 0; i < newRoom.RoomStates.Count; i++)
          RoomStates.Add (newRoom.RoomStates [i]);
      }
    }


    // Read all doors from ROM.
    private void ReadDoors (Rom rom)
    {
      List <int> addressesPC = new List <int> ();
      for (int n = 0; n < DoorSets.Count; n++) {
        for (int i = 0; i < DoorSets [n].DoorCount; i++) {
          int ad_PC = Tools.LRtoPC (DoorSets [n].DoorPtrs [i]);
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
        for (int n = 0; n < Rooms [k].Count; n++)
        {
          int roomArea = Rooms [k] [n].RoomW * Rooms [k] [n].RoomH;
          int stateCount = Rooms [k] [n].RoomStates.Count;
          addressesPC.Clear ();
          for (int i = 0; i < stateCount; i++)
          {
            int address = Tools.LRtoPC (Rooms [k] [n].RoomStates [i].RoomScrollsPtr);
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
      for (int n = 0; n < RoomStates.Count; n++)
      {
        int address = Tools.LRtoPC (RoomStates [n].PlmSetPtr);
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
      for (int n = 0; n < PlmSets.Count; n++) {
        int plmCount = PlmSets [n].PlmCount;
        for (int i = 0; i < plmCount; i++) {
          int address = Tools.LRtoPC (PlmSets [n].Plms [i].ScrollDataPtr);
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
      for (int n = 0; n < RoomStates.Count; n++) {
        int address = Tools.LRtoPC (RoomStates [n].BackgroundPtr);
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
      for (int n = 0; n < RoomStates.Count; n++) {
        int address = Tools.LRtoPC (RoomStates [n].FxPtr);
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
      for (int n = 0; n < RoomStates.Count; n++) {
        int address = Tools.LRtoPC (RoomStates [n].LevelDataPtr);
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
      for (int n = 0; n < RoomStates.Count; n++) {
        int address = Tools.LRtoPC (RoomStates [n].EnemySetPtr);
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
      for (int n = 0; n < RoomStates.Count; n++) {
        int address = Tools.LRtoPC (RoomStates [n].EnemyGfxPtr);
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
      for (int n = 0; n < Doors.Count; n++) {
        int address = Tools.LRtoPC (Doors [n].DoorAsmPtr);
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
      for (int n = 0; n < TileSets.Count; n++) {
        int address = Tools.LRtoPC (TileSets [n].SceTablePtr);
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
      for (int n = 0; n < TileSets.Count; n++) {
        int address = Tools.LRtoPC (TileSets [n].SceSheetPtr);
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
      for (int n = 0; n < TileSets.Count; n++) {
        int address = Tools.LRtoPC (TileSets [n].PalettePtr);
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

  } // partial class project

}