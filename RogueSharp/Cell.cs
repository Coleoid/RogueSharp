namespace RogueSharp
{
   /// <summary> A coordinate with attributes of the map at this location </summary>
   public struct Cell
   {
      /// <summary> Where on the map we're describing </summary>
      public Coord Coord { get; private set; }

      /// <summary> Will a Field of View pass through this location? </summary>
      public bool IsTransparent { get; private set; }

      /// <summary> Can a normal traveler pass through this location? </summary>
      public bool IsWalkable { get; private set; }

      /// <summary> Is this location in the latest FOV? </summary>
      public bool IsInFov { get; private set; }

      /// <summary> Has this location ever been seen by the player? </summary>
      public bool IsExplored { get; private set; }

      public Cell( int x, int y, bool isTransparent, bool isWalkable, bool isInFov, bool isExplored )
      {
         Coord = new Coord(x, y);
         IsTransparent = isTransparent;
         IsWalkable = isWalkable;
         IsInFov = isInFov;
         IsExplored = isExplored;
      }

      /// <summary>
      /// Construct a new unexplored Cell located at the specified x and y location with the specified properties
      /// </summary>
      /// <param name="x">X location of the Cell starting with 0 as the farthest left</param>
      /// <param name="y">Y location of the Cell starting with 0 as the top</param>
      /// <param name="isTransparent">Is there a clear line-of-sight through this Cell</param>
      /// <param name="isWalkable">Could a character could normally walk across the Cell without difficulty</param>
      /// <param name="isInFov">Is the Cell currently in the currently observable field-of-view</param>
      public Cell( int x, int y, bool isTransparent, bool isWalkable, bool isInFov )
      {
         Coord = new Coord(x, y);
         IsTransparent = isTransparent;
         IsWalkable = isWalkable;
         IsInFov = isInFov;
         IsExplored = false;
      }


      /// <summary>
      /// Provides a simple visual representation of the Cell using the following symbols:
      /// - `.`: `Cell` is transparent and walkable
      /// - `s`: `Cell` is walkable (but not transparent)
      /// - `o`: `Cell` is transparent (but not walkable)
      /// - `#`: `Cell` is not transparent or walkable
      /// </summary>
      /// <remarks>
      /// This call ignores field-of-view. If field-of-view is important use the ToString overload with a "true" parameter
      /// </remarks>
      /// <returns>A string represenation of the Cell using special symbols to denote Cell properties</returns>
      public override string ToString()
      {
         return ToString( false );
      }

      /// <summary>
      /// Provides a simple visual representation of the Cell using the following symbols:
      /// - `%`: `Cell` is not in field-of-view
      /// - `.`: `Cell` is transparent, walkable, and in field-of-view
      /// - `s`: `Cell` is walkable and in field-of-view (but not transparent)
      /// - `o`: `Cell` is transparent and in field-of-view (but not walkable)
      /// - `#`: `Cell` is in field-of-view (but not transparent or walkable)
      /// </summary>
      /// <param name="useFov">True if field-of-view calculations will be used when creating the string represenation of the Cell. False otherwise</param>
      /// <returns>A string representation of the Cell using special symbols to denote Cell properties</returns>
      public string ToString( bool useFov )
      {
         if ( useFov && !IsInFov )
         {
            return "%";
         }
         if ( IsWalkable )
         {
            if ( IsTransparent )
            {
               return ".";
            }
            else
            {
               return "s";
            }
         }
         else
         {
            if ( IsTransparent )
            {
               return "o";
            }
            else
            {
               return "#";
            }
         }
      }

      /// <summary>
      /// Determines whether two Cell instances are equal
      /// </summary>
      /// <param name="other">The Cell to compare this instance to</param>
      /// <returns>True if the instances are equal; False otherwise</returns>
      public bool Equals(Cell other)
      {
         if (other == null) return false;
         if (ReferenceEquals(this, other)) return true;

         return Coord.Equals(other.Coord)
            && IsTransparent == other.IsTransparent
            && IsWalkable == other.IsWalkable
            && IsInFov == other.IsInFov
            && IsExplored == other.IsExplored;
      }

      /// <summary>
      /// Determines whether two Cell instances are equal
      /// </summary>
      /// <param name="obj">The Object to compare this instance to</param>
      /// <returns>True if the instances are equal; False otherwise</returns>
      public override bool Equals( object obj )
      {
         if ( ReferenceEquals( null, obj ) )
         {
            return false;
         }
         if ( ReferenceEquals( this, obj ) )
         {
            return true;
         }
         if ( obj.GetType() != this.GetType() )
         {
            return false;
         }
         return Equals( (Cell) obj );
      }

      /// <summary>
      /// Determines whether two Cell instances are equal
      /// </summary>
      /// <param name="left">Cell on the left side of the equal sign</param>
      /// <param name="right">Cell on the right side of the equal sign</param>
      /// <returns>True if a and b are equal; False otherwise</returns>
      public static bool operator ==( Cell left, Cell right )
      {
         return Equals( left, right );
      }

      /// <summary>
      /// Determines whether two Cell instances are not equal
      /// </summary>
      /// <param name="left">Cell on the left side of the equal sign</param>
      /// <param name="right">Cell on the right side of the equal sign</param>
      /// <returns>True if a and b are not equal; False otherwise</returns>
      public static bool operator !=( Cell left, Cell right )
      {
         return !Equals( left, right );
      }

      /// <summary>
      /// Gets the hash code for this object which can help for quick checks of equality
      /// or when inserting this Cell into a hash-based collection such as a Dictionary or Hashtable 
      /// </summary>
      /// <returns>An integer hash used to identify this Cell</returns>
      public override int GetHashCode()
      {
         unchecked
         {
            var hashCode = Coord.X;
            hashCode = ( hashCode * 397 ) ^ Coord.Y;
            hashCode = ( hashCode * 397 ) ^ IsTransparent.GetHashCode();
            hashCode = ( hashCode * 397 ) ^ IsWalkable.GetHashCode();
            hashCode = ( hashCode * 397 ) ^ IsInFov.GetHashCode();
            hashCode = ( hashCode * 397 ) ^ IsExplored.GetHashCode();
            return hashCode;
         }
      }
   }
}