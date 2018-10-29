namespace RogueSharp
{
   ///// <summary> An ICoord is a planar, Cartesian coordinate pair </summary>
   //public interface ICoord
   //{
   //   /// <summary> 0-based horizontal index (0 at left edge)</summary>
   //   int X { get; }
   //   /// <summary> 0-based vertical index (0 at top edge)</summary>
   //   int Y { get; }
   //}

   /// <summary> A Coord is an ICoord which can be compared to others </summary>
   public struct Coord// : ICoord
   {
      /// <summary> 0-based horizontal index (0 at left edge)</summary>
      public int X { get; }
      /// <summary> 0-based vertical index (0 at top edge)</summary>
      public int Y { get; }

      /// <summary> constructor </summary>
      /// <param name="x">horizontal index</param>
      /// <param name="y">vertical index</param>
      public Coord(int x, int y)
      {
         X = x;
         Y = y;
      }
   }
}