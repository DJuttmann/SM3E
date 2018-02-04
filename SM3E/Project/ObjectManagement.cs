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

    // Returns an unused room index for given area, or -1 if area is full.
    private int NewRoomIndex (int areaIndex)
    {
      List <int> roomIndices = new List <int> ();
      if (Rooms [areaIndex].Count >= 256)
        return -1;
      foreach (Room r in Rooms [areaIndex])
        roomIndices.Add (r.RoomIndex);
      roomIndices.Sort ();
      int index = 0;
      while (index < roomIndices.Count && roomIndices [index] == index)
        index++;
      return index;
    }




  } // partial class Project

}
