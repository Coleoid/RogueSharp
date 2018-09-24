namespace RogueSharp
{
   public interface ICoord
   {
      int X { get; }
      int Y { get; }
   }

   public class Coord : ICoord
   {
      public int X { get; private set; }
      public int Y { get; private set; }

      public Coord(int x, int y)
      {
         X = x;
         Y = y;
      }
   }
}