using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Reflection;
//using MBuild.Properties;
using System.Text;
using System.Collections;
using System.Collections.Generic;


namespace SM3E
{

//========================================================================================
// CLASS ROM
//========================================================================================


  class Rom
  {
    public byte [] Data;
    private int Position = 0;
    public List <RomSection> Sections;
    private Compressor Comp = new Compressor (null);

    public List <Data> AllData
    {
      get
      {
        List <Data> objects = new List <Data> ();
        foreach (RomSection s in Sections)
          foreach (KeyValuePair <string, List <Data>> kv in s.Data)
            objects.AddRange (kv.Value);
        return objects;
      }
    }


    // Constructor, read bytes from file.
    public Rom (string fileName)
    {
      Data = File.ReadAllBytes (fileName);
      Sections = new List <RomSection> ();
    }


    // Add a section to the ROM.
    public void AddSection (string name, RomSection.Type type)
    {
      Sections.Add (new RomSection (type, name));
    }


    // Add a block to a section in the rom - must be disjoint with all sections.
    public bool AddBlock (string sectionName, int address, int length)
    {
      RomSection section = null;
      foreach (RomSection s in Sections)
      {
        if (!s.DisjointFromInterval (address, length))
          return false;
        if (s.Name == sectionName)
          section = s;
      }
      if (section != null)
        return section.AddBlock (address, length);
      return false;
    }

      
    // Add a data list to a section of the ROM.
    public void AddDataList (string sectionName, string dataName, List <Data> dataList)
    {
      RomSection section = Sections.Find (x => x.Name == sectionName);
      if (section != null)
        section.AddData (dataName, dataList);
    }


    // Set the reading Position in the ROM.
    public void Seek (int position)
    {
      Position = position;
    }


    // Read <count> bytes from ROM and save in array at <offset>.
    public bool Read (byte [] dest, int offset, int count)
    {
      if (Position + count > Data.Length)
        return false;
      for (int i = 0; i < count; i++)
        dest [offset + i] = Data [Position + i];
      Position += count;
      return true;
    }


    // Read <count> bytes from ROM and append to list.
    public bool Read (IList <byte> dest, int count)
    {
      for (int i = 0; i < count; i++)
        dest.Add (Data [Position + i]);
      Position += count;
      return true;
    }


    // Decompress data to output.
    public int Decompress (out List <byte> output)
    {
      return Compressor.Decompress (Data, ref Position, out output);
    }


    public List <byte> Compress (byte [] data)
    {
      Comp.Input = data;
      return Comp.Compress ();
    }


    // Reallocate the objects in one of the ROMs sections.
    public void Reallocate (string sectionName)
    {
      RomSection section = Sections.Find (x => x.Name == sectionName);
      if (section != null)
        section.Reallocate ();
    }


    // Write part of the ROM to a file stream.
    public void WriteToFile (Stream output, int length)
    {
      int remaining = Data.Length - Position;
      if (length <= remaining)
        output.Write (Data, Position, length);
      else
      {
        output.Write (Data, Position, remaining);
        output.Write (new byte [length - remaining], 0, length - remaining);
      }
    }
  } // class Rom


//========================================================================================
// CLASS ROM SECTION
//========================================================================================


  class RomSection
  {
    public enum Type
    {
      Fixed,
      Bank,
      Free,
      Unknown
    }

    // A contigious block of data within a rom.
    private struct DataBlock
    {
      public int Address;
      public int Length;
    }


    // Fields
    public Type SectionType {get; private set;}
    private List <DataBlock> blocks;
    public Dictionary <string, List <Data>> Data {get; private set;}
    public String Name;

    // Properties
    public List <Tuple <int, int>> Blocks
    {
      get
      {
        var list = new List <Tuple <int, int>> ();
        for (int i = 0; i < blocks.Count; i++)
          list.Add (new Tuple <int, int> (blocks [i].Address,
                                          blocks [i].Address + blocks [i].Length));
        return list;
      }
    }


    // Constructor
    public RomSection (Type sectionType, string name)
    {
      SectionType = sectionType;
      Name = name;
      blocks = new List <DataBlock> ();
      Data = new Dictionary <string, List <Data>> ();
    }


    // Convert string to section type.
    public static Type StringToType (string s)
    {
      switch (s.ToLower ())
      {
      case "fixed":
        return Type.Free;
      case "bank":
        return Type.Free;
      case "free":
        return Type.Free;
      default:
        return Type.Unknown;
      }
    }


    // Check if section is disjoint from interval with given addres/length.
    public bool DisjointFromInterval (int address, int length)
    {
      foreach (DataBlock b in blocks)
        if (address + length >= b.Address && address <= b.Address + b.Length)
          return false;
      return true;
    }


    // Check if section contains interval with given addres/length.
    public bool ContainsInterval (int address, int length)
    {
      foreach (DataBlock b in blocks)
        if (address >= b.Address && address + length <= b.Address + b.Length)
          return true;
      return false;
    }


    // Add a new block to the section.
    public bool AddBlock (int address, int length)
    {
      if (address < 0 || length <= 0 || !DisjointFromInterval (address, length))
        return false;
      blocks.Add (new DataBlock () {Address = address, Length = length});
      return true;
    }


    // Add a data list to the section.
    public void AddData (string name, List <Data> dataList)
    {
      if (dataList != null)
        Data.Add (name, dataList);
    }


    // Reallocate the data from the section in its data blocks.
    public bool Reallocate ()
    {
      if (SectionType == Type.Fixed)
        return true;
      var objects = new List <Data> ();
      foreach (KeyValuePair <string, List <Data>> kv in Data)
        objects.AddRange (kv.Value);
      return Allocate (objects);
    }


    // Allocate a list of data in the data blocks
    public bool Allocate (List <Data> objects)
    {
      List <int> newAddresses = new List <int> ();
      objects.Sort ((x, y) => x.StartAddressPC - y.StartAddressPC);
      int i = 0;
      foreach (DataBlock block in blocks)
      {
        int address = block.Address;
        while (i < objects.Count &&
                address + ((Data) objects [i]).Size < block.Address + block.Length)
        {
          newAddresses.Add (address);
          address += ((Data) objects [i]).Size;
          i++;
        }
      }
      if (i < objects.Count)
        return false;

      for (int n = 0; n < objects.Count; n++)
        ((Data) objects [n]).Reallocate (newAddresses [n]);
      return true;
    }

  } // class RomSection

}