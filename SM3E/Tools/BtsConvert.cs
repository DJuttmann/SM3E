using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Reflection;
using System.Text;
using System.Collections.Generic;


namespace SM3E
{

//========================================================================================
// CLASS BTS CONVERT
//========================================================================================


  static class BtsConvert
  {

    public static void TextureIndexToBts (int col, int row, out int btsType, out int btsValue)
    {
      btsType = 0;
      btsValue = 0;
      int index = 8 * row + col;
      if (row < 8) {
        btsType = 0x1;
        btsValue = index;
        return;
      }
      switch (row) {
      case 8: // Wall, door, door caps, spikes, air
        switch (col) {
        case 0:
          btsType = 0x8;
          btsValue = 0x00;
          break;
        case 1:
          btsType = 0x9;
          btsValue = 0x00;
          break;
        case 2:
        case 3:
          btsType = 0xC;
          btsValue = (col == 3 ? 0x42 : 0x40); // 0x40 + 2 * (row == 3);
          break;
        case 4:
        case 5:
        case 6:
          btsType = 0xA;
          btsValue = (col == 6 ? col - 3 : col - 4); // col - 4 + (col == 6);
          break;
        default:
          btsType = 0x0;
          btsValue = 0x00;
          break;
        }
        break;
      case 9: // Air (shot)
        btsType = 0x4;
        btsValue = col;
        break;
      case 10: // Air (bomb)
        btsType = 0x7;
        btsValue = col;
        break;
      case 11:
        switch (col) {
        case 0:
          btsType = 0x2;
          btsValue = 0x00;
          break;
        case 1:
          btsType = 0x6;
          btsValue = 0x00;
          break;
        case 2:
          btsType = 0x2;
          btsValue = 0x02;
          break;
        case 3:
          btsType = 0xA;
          btsValue = 0x0E;
          break;
        case 4:
          btsType = 0xB;
          btsValue = 0x0B;
          break;
        case 5:
          btsType = 0x3;
          btsValue = 0x08;
          break;
        case 6:
          btsType = 0x3;
          btsValue = 0x82;
          break;
        case 7:
          btsType = 0x3;
          btsValue = 0x85;
          break;
        }
        break;
      case 12: // shot
        btsType = 0xC;
        btsValue = col;
        break;
      case 13: // crumble
        btsType = 0xB;
        btsValue = col;
        break;
      case 14: // bomb
        btsType = 0xF;
        btsValue = col;
        break;
      case 15:
      case 16:
        switch (col) {
        case 0:
          btsType = 0xB;
          btsValue = (row == 16 ? 0x0F : 0x0E); // 0x0E + (row == 16);
          break;
        case 1:
          btsType = 0xC;
          btsValue = (row == 16 ? 0x09 : 0x08); // 0x08 + (row == 16);
          break;
        case 2:
          if (row == 15) {
            btsType = 0xE;
            btsValue = 0x00;
          }
          else {
            btsType = 0xA;
            btsValue = 0x0F;
          }
          break;
        case 3:
          btsType = 0xE;
          btsValue = (row == 16 ? 0x02 : 0x01); // 0x01 + (row == 16);
          break;
        case 4:
        case 5:
        case 6:
          btsType = 0xC;
          btsValue = (row == 16 ? 0x0B : 0x0A); // 0x0A + (row == 16);
          break;
        case 7:
          if (row == 15)
            btsType = 0x5;
          else
            btsType = 0xD;
          btsValue = 0x01;
          break;
        }
        break;
      default:
        break;
      }
    }

  } // class BtsConvert

}