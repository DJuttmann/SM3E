using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;

namespace SM3E
{

  partial class Project
  {
    private const string PlmFolder = "\\Data\\PLMs";
    private const string EnemyFolder = "\\Data\\Enemies";

    // The rom that is being edited.
    private Rom CurrentRom;
    public string ProjectPath;
    public string RomFileName;
    public string ProjectFileName;

    public bool ProjectLoaded {get; private set;} = false;

    private bool changesMade = false;
    public bool ChangesMade
    {
      get {return changesMade;}
      private set
      {
        changesMade = value;
        if (changesMade)
          ProjectChanged?.Invoke (this, null);
      }
    }


//========================================================================================
// Load and close project.

   
    // Read all data from the ROM, guided by data from the projectfile.
    public void Load (string projectFile)
    {
      if (ProjectLoaded)
        Close ();

      List <Tuple <int, int, string>> rooms;
      List <Tuple <int, int, string>> doorAsms;
      List <Tuple <int, int, string>> setupAsms;
      List <Tuple <int, int, string>> mainAsms;
      List <Tuple <int, string>> backgrounds;

      ProjectStartLoading?.Invoke (this, null);
      ProjectPath = Tools.FolderFromPath (projectFile);
      ProjectFileName = Tools.FilenameFromPath (projectFile);
      try
      {
        ReadProjectFileXml (out rooms,
                            out doorAsms,
                            out setupAsms,
                            out mainAsms,
                            out backgrounds);
      }
      catch (ProjectLoadException ex)
      {
        ProjectFailedLoading?.Invoke (this, new LoadFailEventArgs (ex.ExceptionType));
        return;
      }

      // Read uncompressed data from ROM.
      ReadRooms (CurrentRom, rooms, out List <RoomState> roomStates);
      ReadDoors (CurrentRom);
      ReadScrollSets (CurrentRom);
      ReadPlmSets (CurrentRom, roomStates);
      ReadScrollPlmDatas (CurrentRom);
      ReadBackgrounds (CurrentRom, backgrounds);
      ReadFxs (CurrentRom, roomStates);
      ReadSaveRooms (CurrentRom);
      ReadEnemySets (CurrentRom, roomStates);
      ReadEnemyGfxs (CurrentRom, roomStates);
      ReadScrollAsms (CurrentRom);
      ReadTileSets (CurrentRom);
      ReadAreaMaps (CurrentRom, AreaMap.Addresses);
      ReadDoorAsms (CurrentRom, doorAsms);
      ReadSetupAsms (CurrentRom, setupAsms);
      ReadMainAsms (CurrentRom, mainAsms);

      // Read Compressed data from ROM.
      ReadLevelDatas (CurrentRom, roomStates);
      ReadTileTables (CurrentRom);
      ReadTileSheets (CurrentRom);
      ReadPalettes (CurrentRom);
      LoadMapTiles (CurrentRom);

      // Read data from Data folder.
      ReadPlmTypes ();
      ReadEnemyTypes ();

      // Connect the data objects.
      Connect (roomStates);
      LoadRoomTiles (0); //


      ProjectLoaded = true;
      ChangesMade = false;
      ProjectFinishedLoading?.Invoke (this, null);
    }


    public void Start ()
    {
      // Raise events.
      TileSetListChanged?.Invoke (this, new ListLoadEventArgs (-1));
      AreaListChanged?.Invoke (this, new ListLoadEventArgs (0));
      SelectArea (0);
      PlmTypeListChanged?.Invoke (this, new ListLoadEventArgs (0));
      SelectPlmType (0);
      EnemyTypeListChanged?.Invoke (this, new ListLoadEventArgs (0));
      SelectEnemyType (0);
      ScrollColorListChanged?.Invoke (this, new ListLoadEventArgs (0));
      MapPaletteSelected?.Invoke (this, null);
      TileIndex = 0;
      BtsType = 0;
    }


    public void Close ()
    {
      if (!ProjectLoaded)
        return;

      Rooms         .Clear ();
      DoorSets      .Clear ();
      Doors         .Clear ();
      ScrollSets    .Clear ();
      PlmSets       .Clear ();
      ScrollPlmDatas.Clear ();
      Backgrounds   .Clear ();
      Fxs           .Clear ();
      SaveStations     .Clear ();
      LevelDatas    .Clear ();
      EnemySets     .Clear ();
      EnemyGfxs     .Clear ();
      ScrollAsms    .Clear ();
      DoorAsms      .Clear ();
      SetupAsms     .Clear ();
      MainAsms      .Clear ();
      TileSets      .Clear ();
      TileTables    .Clear ();
      TileSheets    .Clear ();
      Palettes      .Clear ();
      AreaMaps      .Clear ();
      PlmTypes      .Clear ();
      EnemyTypes    .Clear ();

      CurrentRom = null;
      ProjectPath = String.Empty;
      RomFileName = String.Empty;
      ProjectFileName = String.Empty;

      RoomTiles.Clear ();
      MapTiles.Clear ();
      BackgroundImage = null;

      ChangesMade = false;
      ProjectLoaded = false;
      ProjectClosed?.Invoke (this, null);
    }


//========================================================================================
// Reading ROM data.


    // Read the project file in Xml format.
    private bool ReadProjectFileXml (
                                     out List <Tuple <int, int, string>> rooms,
                                     out List <Tuple <int, int, string>> doorAsms,
                                     out List <Tuple <int, int, string>> setupAsms,
                                     out List <Tuple <int, int, string>> mainAsms,
                                     out List <Tuple <int, string>> backgrounds)
    {
      RomFileName = null;
      rooms     = new List <Tuple <int, int, string>> ();
      doorAsms  = new List <Tuple <int, int, string>> ();
      setupAsms = new List <Tuple <int, int, string>> ();
      mainAsms  = new List <Tuple <int, int, string>> ();
      backgrounds = new List <Tuple <int, string>> ();

      Stream stream;
      try {stream = new FileStream (ProjectPath + ProjectFileName, FileMode.Open, 
                                    FileAccess.Read, FileShare.Read);}
      catch
      {
        throw new ProjectLoadException (ProjectLoadException.Type.ProjectFileNotAccessible,
                                        ProjectPath + ProjectFileName);
      }
      var reader = XmlReader.Create (stream);
      XElement root = XElement.Load (reader);
      stream.Close ();

      RomFileName = root.Attribute ("rom")?.Value;
      if (root.Name != "Project" || RomFileName == null)
        throw new ProjectLoadException (ProjectLoadException.Type.RomFileNotSpecified,
                                        ProjectPath + ProjectFileName);
      try
      {
        CurrentRom = new Rom (ProjectPath + RomFileName);
      }
      catch (FileNotFoundException)
      {
        throw new ProjectLoadException (ProjectLoadException.Type.RomFileNotFound,
                                        ProjectPath + RomFileName);
      }
      catch
      {
        throw new ProjectLoadException (ProjectLoadException.Type.RomFileNotAccessible,
                                        ProjectPath + RomFileName);
      }

      foreach (XElement x in root.Elements ())
      {
        switch (x.Name.ToString ())
        {
        case "Areas":
          foreach (XElement area in x.Elements ("Area"))
          {
            string areaIndex = area.Attribute ("index")?.Value;
            string areaName = area.Attribute ("name")?.Value;
            if (areaIndex != null && areaName != null)
            {
              int i = Convert.ToInt32 (areaIndex);
              Areas [i] = areaName;
            }
          }
          break;

        case "Sections":
          foreach (XElement section in x.Elements ("Section"))
          {
            string sectionName = section.Attribute ("name")?.Value;
            string sectionType = section.Attribute ("type")?.Value;
            if (sectionName == null || sectionType == null)
              continue;
            CurrentRom.AddSection (sectionName, RomSection.StringToType (sectionType));
            foreach (XElement data in section.Elements ("Data"))
            {
              string dataName = data.Attribute ("name")?.Value;
              if (dataName != null && DataLists.ContainsKey (dataName))
                CurrentRom.AddDataList (sectionName, dataName, DataLists [dataName]);
            }
            foreach (XElement block in section.Elements ("Block"))
            {
              string blockAddress = block.Attribute ("address")?.Value;
              string blockEnd = block.Attribute ("end")?.Value;
              if (blockAddress != null && blockEnd != null)
              {
                CurrentRom.AddBlock (sectionName, Tools.HexToInt (blockAddress),
                  Tools.HexToInt (blockEnd) - Tools.HexToInt (blockAddress));
              }
            }
          }
          break;

        case "Rooms":
          foreach (XElement room in x.Elements ("Room"))
          {
            string roomAddress = room.Attribute ("address")?.Value;
            string doorCount = room.Attribute ("doorCount")?.Value;
            string roomName = room.Attribute ("name")?.Value;
            if (roomAddress != null)
            {
              int address = Tools.HexToInt (roomAddress);
              int count = doorCount != null ? Tools.DecToInt (doorCount) : 0;
              rooms.Add (new Tuple <int, int, string> (address, count, 
                                                       roomName ?? String.Empty));
            }
          }
          break;

        case "Asms":
          foreach (XElement asm in x.Elements ())
          {
            List <Tuple <int, int, string>> asmList;
            switch (asm.Name.ToString ())
            {
            case "DoorAsm":
              asmList = doorAsms;
              break;
            case "SetupAsm":
              asmList = setupAsms;
              break;
            case "MainAsm":
              asmList = mainAsms;
              break;
            default:
              continue;
            }
            string asmName = asm.Attribute ("name")?.Value;
            string asmAddress = asm.Attribute ("address")?.Value;
            string asmEnd = asm.Attribute ("end")?.Value;
            if (asmAddress != null && asmEnd != null)
            {
              asmList.Add (new Tuple <int, int, string> (Tools.HexToInt (asmAddress),
                                                         Tools.HexToInt (asmEnd),
                                                         asmName ?? asmAddress));
            }
          }
          break;

        case "Backgrounds":
          foreach (XElement background in x.Elements ("Background"))
          {
            string address = background.Attribute ("address")?.Value;
            string name = background.Attribute ("name")?.Value;
            if (address != null)
              backgrounds.Add (new Tuple<int, string> (Tools.HexToInt (address),
                                                       name ?? address));
          }
          break;

        default:
          break;
        }
      }

      return true;
    }


    // Connect all loaded data objects.
    private bool Connect (List <RoomState> roomStates)
    {
      var RoomsList = new List <Data> (Rooms);
      foreach (Room r in Rooms);

      foreach (Room r in Rooms)
        r.Connect (DoorSets);
      foreach (DoorSet d in DoorSets)
        d.Connect (Doors);
      foreach (Door d in Doors)
        d.Connect (RoomsList, ScrollAsms, DoorAsms);
      foreach (RoomState r in roomStates)
        r.Connect (PlmSets, ScrollSets, Backgrounds, Fxs, LevelDatas, 
                   EnemySets, EnemyGfxs, SetupAsms, MainAsms);
      foreach (PlmSet p in PlmSets)
        p.Connect (ScrollPlmDatas, PlmTypes);
      foreach (Fx f in Fxs)
        f.Connect (Doors);
      foreach (SaveStation s in SaveStations)
        s.Connect (RoomsList, Doors);
      foreach (EnemySet e in EnemySets)
        e.Connect (EnemyTypes);
      foreach (EnemyGfx e in EnemyGfxs)
        e.Connect (EnemyTypes);
      foreach (TileSet t in TileSets)
        t.Connect (TileTables, TileSheets, Palettes);

      return true;
    }

//----------------------------------------------------------------------------------------

    // Read all rooms from ROM.
    private void ReadRooms (Rom rom, List <Tuple <int, int, string>> rooms,
                            out List <RoomState> roomStates)
      
    {
      roomStates = new List <RoomState> ();
      Rooms.Clear ();
      for (int n = 0; n < rooms.Count; n++)
      {
        Room newRoom = new Room ();
        newRoom.ReadFromROM (rom, rooms [n].Item1);
        newRoom.Name = rooms [n].Item3;
        Rooms [newRoom.Area].Add (newRoom);
        var newDoorSet = new DoorSet () {DoorCount = rooms [n].Item2};
        newDoorSet.ReadFromROM (rom, newRoom.DoorsPtrPC);
        DoorSets.Add (newDoorSet);
        for (int i = 0; i < newRoom.RoomStates.Count; i++)
          roomStates.Add (newRoom.RoomStates [i]);
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
          int roomArea = r.Width * r.Height;
          int stateCount = r.RoomStates.Count;
          addressesPC.Clear ();
          for (int i = 0; i < stateCount; i++)
          {
            int address = Tools.LRtoPC (r.RoomStates [i].ScrollSetPtr);
            if (address != 0)
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
    private void ReadPlmSets (Rom rom, List <RoomState> roomStates)
    {
      List <int> addressesPC = new List <int> ();
      foreach (RoomState r in roomStates)
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
    private void ReadBackgrounds (Rom rom, List <Tuple <int, string>> backgrounds)
    {
      foreach (var b in backgrounds)
      {
        Background newBackground = new Background () {Name = b.Item2};
        newBackground.ReadFromROM (rom, b.Item1);
        Backgrounds.Add (newBackground);
      }
      /*
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
      */
    }


    // Read all fxs from ROM.
    private void ReadFxs (Rom rom, List <RoomState> roomStates)
    {
      List <int> addressesPC = new List <int> ();
      foreach (RoomState r in roomStates) {
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
      int address = SaveStation.SaveStationsAddress;
      SaveStations.Clear ();
      for (int n = 0; n < SaveStation.Count; n++) {
        SaveStations.Add (new SaveStation ());
        SaveStations [n].ReadFromROM (rom, address);
        address += SaveStation.DefaultSize;
      }
    }


    // Read all level datas from ROM.
    private void ReadLevelDatas (Rom rom, List <RoomState> roomStates)
    {
      List <int> addressesPC = new List <int> ();
      foreach (RoomState r in roomStates) {
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
    private void ReadEnemySets (Rom rom, List <RoomState> roomStates)
    {
      List <int> addressesPC = new List <int> ();
      foreach (RoomState r in roomStates) {
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
    private void ReadEnemyGfxs (Rom rom, List <RoomState> roomStates)
    {
      List <int> addressesPC = new List <int> ();
      foreach (RoomState r in roomStates) {
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


    // Read all Door ASMs from ROM.
    private void ReadDoorAsms (Rom rom, List <Tuple <int, int, string>> asms)
    {
      foreach (var a in asms)
      {
        Asm newAsm = new Asm () {Name = a.Item3};
        newAsm.SetSize (a.Item2 - a.Item1);
        newAsm.ReadFromROM (rom, a.Item1);
        DoorAsms.Add (newAsm);
      }
    }


    // Read all setup ASMs from ROM.
    private void ReadSetupAsms (Rom rom, List <Tuple <int, int, string>> asms)
    {
      foreach (var a in asms)
      {
        Asm newAsm = new Asm () {Name = a.Item3};
        newAsm.SetSize (a.Item2 - a.Item1);
        newAsm.ReadFromROM (rom, a.Item1);
        SetupAsms.Add (newAsm);
      }
    }


    // Read all main ASMs from ROM.
    private void ReadMainAsms (Rom rom, List <Tuple <int, int, string>> asms)
    {
      foreach (var a in asms)
      {
        Asm newAsm = new Asm () {Name = a.Item3};
        newAsm.SetSize (a.Item2 - a.Item1);
        newAsm.ReadFromROM (rom, a.Item1);
        MainAsms.Add (newAsm);
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
        Palettes.Add (new CompressedPalette ());
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


    // [test] Write project file as XML.
    private void WriteProjectFileXml ()
    {
      var stream = new FileStream (ProjectPath + ProjectFileName, FileMode.Create);
      var settings = new XmlWriterSettings ()
      {
        NewLineChars = Environment.NewLine,
        Indent = true,
      };
      var writer = XmlWriter.Create (stream, settings);

      // Root element.
      XElement root = new XElement ("Project");
      root.SetAttributeValue ("rom", RomFileName);

      // Areas element.
      XElement areasElement = new XElement ("Areas");
      for (int n = 0; n < AreaCount; n++)
      {
        XElement area = new XElement ("Area");
        area.SetAttributeValue ("index", n);
        area.SetAttributeValue ("name", AreaNames [n]);
        areasElement.Add (area);
      }
      root.Add (areasElement);

      // Sections element.
      XElement sectionsElement = new XElement ("Sections");
      foreach (RomSection s in CurrentRom.Sections)
      {
        XElement section = new XElement ("Section");
        section.SetAttributeValue ("name", s.Name);
        section.SetAttributeValue ("type", s.SectionType.ToString ());
        sectionsElement.Add (section);
        // Add data.
        foreach (KeyValuePair <string, IEnumerable <Data>> kv in s.Data)
        {
          XElement data = new XElement ("Data");
          data.SetAttributeValue ("name", kv.Key);
          section.Add (data);
        }
        // Add blocks.
        foreach (var b in s.Blocks)
        {
          XElement block = new XElement ("Block");
          block.SetAttributeValue ("address", Tools.IntToHex (b.Item1));
          block.SetAttributeValue ("end"    , Tools.IntToHex (b.Item2));
          section.Add (block);
        }
      }
      root.Add (sectionsElement);

      // Write room data.
      XElement roomsElement = new XElement ("Rooms");
      for (int i = 0; i < AreaCount; i++)
        foreach (Room r in Rooms [i])
        {
          XElement room = new XElement ("Room");
          room.SetAttributeValue ("address", Tools.IntToHex (r.StartAddressPC));
          room.SetAttributeValue ("doorCount", r.MyDoorSet?.DoorCount ?? 0);
          room.SetAttributeValue ("name", r.Name);
          roomsElement.Add (room);
        }
      root.Add (roomsElement);

      // Write asm data.
      XElement asmElement = new XElement ("Asms");
      WriteAsmDataXml (asmElement, "DoorAsm", DoorAsms);
      WriteAsmDataXml (asmElement, "SetupAsm", SetupAsms);
      WriteAsmDataXml (asmElement, "MainAsm", MainAsms);
      root.Add (asmElement);

      // Write background data.
      XElement backgroundsElement = new XElement ("Backgrounds");
      foreach (Background b in Backgrounds)
      {
        XElement background = new XElement ("Background");
        background.SetAttributeValue ("address", Tools.IntToHex (b.StartAddressPC));
        background.SetAttributeValue ("name", b.Name);
        backgroundsElement.Add (background);
      }
      root.Add (backgroundsElement);
      
      root.WriteTo (writer);
      writer.Close ();
      stream.Close ();
    }


    // Write asm data to xml element.
    private void WriteAsmDataXml (XElement asmElement, string name, List <Data> data)
    {
      foreach (Asm a in data)
      {
        XElement asm = new XElement (name);
        asm.SetAttributeValue ("address", Tools.IntToHex (a.StartAddressPC));
        asm.SetAttributeValue ("end", Tools.IntToHex (a.EndAddressPC));
        asm.SetAttributeValue ("name", a.Name);
        asmElement.Add (asm);
      }
    }


    // Write all data to the Rom.
    public void Save ()
    {
      if (!ProjectLoaded)
        return;
      
      ReallocateAll ();
      RepointAll ();
      List <Data> objects = CurrentRom.AllData;
      objects.Sort ((x, y) => x.StartAddressPC - y.StartAddressPC);
      
      Stream output = new FileStream (ProjectPath + RomFileName, FileMode.Create);
      int address = 0;
      for (int n = 0; n < objects.Count; n++)
      {
        if (address < objects [n].StartAddressPC)
        {
          CurrentRom.Seek (address);
          CurrentRom.WriteToFile (output, objects [n].StartAddressPC - address);
          address = objects [n].StartAddressPC;
        }
        if (address == objects [n].StartAddressPC)
        {
          int oldAddress = address;
          long oldPosition = output.Position;
          objects [n].WriteToROM (output, ref address);
          if (address - oldAddress != output.Position - oldPosition)
            Logging.WriteLine ("Write error: incorrect # bytes written for " +
                               objects [n].GetType ().ToString () + " at " +
                               Tools.IntToHex (objects [n].StartAddressPC) +
                               " - should be " + (address - oldAddress) + 
                               " instead of " + (output.Position - oldPosition));
          if (address != output.Position)
            Logging.WriteLine ("Desync: " + address + " ~ " + output.Position);
        }
        else
          Logging.WriteLine ("Write error: Invalid address for " +
                             objects [n].GetType ().ToString () + " at " +
                             Tools.IntToHex (objects [n].StartAddressPC) +
                             " - output stream already at " + Tools.IntToHex (address));
      }

      // Round address up to multiple of bank size and write remaining data up to there.
      int roundedSize = Math.Max (CurrentRom.Data.Length, (address + 0x7FFF) & ~0x7FFF);
      if (address < roundedSize)
      {
        CurrentRom.Seek (address);
        CurrentRom.WriteToFile (output, roundedSize - address);
      }
      output.Close ();

      WriteProjectFileXml ();
      ChangesMade = false;
      ProjectSaved?.Invoke (this, null);
    }

  } // partial class project


//========================================================================================
// Exceptions


  public class ProjectLoadException: Exception
  {
    public enum Type
    {
      ProjectFileNotAccessible,
      RomFileNotSpecified,
      RomFileNotFound,
      RomFileNotAccessible,
    }

    public Type ExceptionType;
    public string Filename = string.Empty;


    public ProjectLoadException () {}

    
    public ProjectLoadException (Type type, string filename):
      base (GenerateMessage(type, filename))
    {
      ExceptionType = type;
      Filename = filename;
    }


    public ProjectLoadException (Type type, string filename, Exception inner):
      base (GenerateMessage(type, filename), inner)
    {
      ExceptionType = type;
      Filename = filename;
    }


    public static string GenerateMessage (Type t, string filename)
    {
      string message;
      switch (t)
      {
      case Type.ProjectFileNotAccessible:
        message = "Project file is not accessible:" + Environment.NewLine + filename;
        break;
      case Type.RomFileNotSpecified:
        message = "ROM file not specified in project file:" + Environment.NewLine + filename;
        break;
      case Type.RomFileNotFound:
        message = "ROM file not found:" + Environment.NewLine + filename;
        break;
      case Type.RomFileNotAccessible:
        message = "ROM file is not accessible:" + Environment.NewLine + filename;
        break;
      default:
        message = "Unknown error loading file:" + Environment.NewLine + filename;
        break;
      }
      return message;
    }

  }

}