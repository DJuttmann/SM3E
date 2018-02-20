using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Reflection;
using System.Text;
using System.Collections;
using System.Collections.Generic;


namespace SM3E
{

//========================================================================================
// CLASS LISTARRAY <T>
// An array of List <T> that is indexed with [int] [int] and enumerable over the T.
//========================================================================================


  class ListArray <T>: IEnumerable <T>
  {
    private List <T> [] Data;

    public int Length
    {
      get {return Data.Length;}
    }

    public List <T> this [int index]
    {
      get
      {
        return Data [index];
      }
    }


    // Constructor
    public ListArray (int length)
    {
      Data = new List <T> [length];
      for (int i = 0; i < length; i++)
        Data [i] = new List <T> ();
    }


    public IEnumerator <T> GetEnumerator ()
    {
      return new ListArrayEnumerator <T> (this);
    }


    IEnumerator IEnumerable.GetEnumerator ()
    {
      return GetEnumerator ();
    }


    // Clear all lists in the array.
    public void Clear ()
    {
      foreach (List <T> list in Data)
        list.Clear ();
    }


//========================================================================================
// class Enumerator


    public class ListArrayEnumerator <U>: IEnumerator <U>
    {
      private ListArray <U> Source;
      private int ArrayIndex = 0;
      private int ListIndex = -1;


      // Constructor
      public ListArrayEnumerator (ListArray <U> source)
      {
        Source = source;
      }


      public U Current
      {
        get
        {
          if (Source == null)
            throw new ObjectDisposedException ("ListArrayEnumerator <T>");
          return Source [ArrayIndex] [ListIndex];
        }
      }


      object IEnumerator.Current
      {
        get {return Current;}
      }


      public bool MoveNext ()
      {
        if (Source == null)
          throw new ObjectDisposedException ("ListArrayEnumerator <T>");
        if (ArrayIndex >= Source.Data.Length)
          return false;
        ListIndex++;
        if (ListIndex >= Source.Data [ArrayIndex].Count)
        {
          do {ArrayIndex++;}
          while (ArrayIndex < Source.Data.Length && Source.Data [ArrayIndex].Count == 0);
          ListIndex = 0;
        }
        return ArrayIndex < Source.Data.Length;
      }


      public void Reset ()
      {
        if (Source == null)
          throw new ObjectDisposedException ("ListArrayEnumerator <T>");
        ArrayIndex = 0;
        ListIndex = 0;
      }


      public void Dispose ()
      {
        Source = null;
      }
    
    } // class ListArrayEnumerator

  } // class ListArray

}