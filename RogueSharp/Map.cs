using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using RogueSharp.MapCreation;

namespace RogueSharp
{
   /// <summary>
   /// A Map represents a rectangular grid of Cells, each of which has a number of properties for determining walkability, field-of-view and so on
   /// The upper left corner of the Map is Cell (0,0) and the X value increases to the right, as the Y value increases downward
   /// </summary>
   public class Map : IMap
   {
      private FieldOfView _fieldOfView;
      private bool[,] _isTransparent;
      private bool[,] _isWalkable;
      private bool[,] _isExplored;

      /// <summary>
      /// Constructor creates a new uninitialized Map
      /// </summary>
      public Map()
      {
      }

      /// <summary>
      /// Constructor creates a new Map and immediately initializes it
      /// </summary>
      /// <param name="width">How many Cells wide the Map will be</param>
      /// <param name="height">How many Cells tall the Map will be</param>
      public Map(int width, int height)
      {
         Initialize(width, height);
      }

      /// <summary>
      /// How many Cells wide the Map is
      /// </summary>
      /// <remarks>
      /// A Map with a Width of 10 will have Cells with X locations of 0 through 9
      /// Cells with an X value of 0 will be the leftmost Cells
      /// </remarks>
      public int Width { get; private set; }

      /// <summary>
      /// How many Cells tall the Map is
      /// </summary>
      /// <remarks>
      /// A Map with a Height of 20 will have Cells with Y locations of 0 through 19
      /// Cells with an Y value of 0 will be the topmost Cells
      /// </remarks>
      public int Height { get; private set; }

      /// <summary>
      /// Create a new map with the properties of all Cells set to false
      /// </summary>
      /// <remarks>
      /// This is basically a solid stone map that would then need to be modified to have interesting features
      /// </remarks>
      /// <param name="width">How many Cells wide the Map will be</param>
      /// <param name="height">How many Cells tall the Map will be</param>
      public void Initialize(int width, int height)
      {
         Width = width;
         Height = height;
         _isTransparent = new bool[width, height];
         _isWalkable = new bool[width, height];
         _isExplored = new bool[width, height];
         _fieldOfView = new FieldOfView(this);
      }

      public bool IsTransparent(int x, int y) => _isTransparent[x, y];

      public bool IsTransparent(Point point) => _isTransparent[point.X, point.Y];

      public bool IsWalkable(int x, int y) => _isWalkable[x, y];

      public bool IsWalkable(Point point) => _isWalkable[point.X, point.Y];

      public bool IsInFov(Point point) => _fieldOfView.IsInFov(point);

      public bool IsWithinMap(Point point)
      {
         return
            0 <= point.X && point.X < Width &&
            0 <= point.Y && point.Y < Height;
      }

      /// <summary>
      /// Check if the Cell is flagged as ever having been explored by the player
      /// </summary>
      /// <remarks>
      /// The explored property of a Cell can be used to track if the Cell has ever been in the field-of-view of a character controlled by the player
      /// This property will not automatically be updated based on FOV calculations or any other built-in functions of the RogueSharp library.
      /// </remarks>
      /// <example>
      /// As the player moves characters around a Map, Cells will enter and exit the currently computed field-of-view
      /// This property can be used to keep track of those Cells that have been "seen" and could be used to show fog-of-war type effects when rendering the map
      /// </example>
      /// <param name="x">X location of the Cell to check starting with 0 as the farthest left</param>
      /// <param name="y">Y location of the Cell to check, starting with 0 as the top</param>
      /// <returns>True if the Cell has been flagged as being explored by the player, false otherwise</returns>
      public bool IsExplored(int x, int y)
      {
         return _isExplored[x, y];
      }

      public bool IsExplored(Point point)
      {
         return _isExplored[point.X, point.Y];
      }

      public void SetIsExplored(Point point, bool isExplored)
      {
         _isExplored[point.X, point.Y] = isExplored;
      }

      public void SetIsWalkable(Point point, bool isWalkable)
      {
         _isWalkable[point.X, point.Y] = isWalkable;
      }

      public void SetIsTransparent(Point point, bool isTransparent)
      {
         _isTransparent[point.X, point.Y] = isTransparent;
      }

      public Cell GetCellAt(Point point)
      {
         //if (point is ICell cell) return cell;
         return GetCell(point.X, point.Y);
      }

      ///// <summary>
      ///// Set the properties of a Cell to the specified values
      ///// </summary>
      ///// <remarks>
      ///// IsInFov cannot be set through this method as it is always calculated by calling ComputeFov and/or AppendFov
      ///// </remarks>
      ///// <param name="x">X location of the Cell to set properties on, starting with 0 as the farthest left</param>
      ///// <param name="y">Y location of the Cell to set properties on, starting with 0 as the top</param>
      ///// <param name="isTransparent">True if line-of-sight is not blocked by this Cell. False otherwise</param>
      ///// <param name="isWalkable">True if a character could walk across the Cell normally. False otherwise</param>
      ///// <param name="isExplored">True if the Cell has ever been in the field-of-view of the player. False otherwise</param>
      //public void SetCellProperties(int x, int y, bool isTransparent, bool isWalkable, bool isExplored)
      //{
      //   _isTransparent[x, y] = isTransparent;
      //   _isWalkable[x, y] = isWalkable;
      //   _isExplored[x, y] = isExplored;
      //}

      ///// <summary>
      ///// Set the properties of an unexplored Cell to the specified values
      ///// </summary>
      ///// <remarks>
      ///// IsInFov cannot be set through this method as it is always calculated by calling ComputeFov and/or AppendFov
      ///// </remarks>
      ///// <param name="x">X location of the Cell to set properties on, starting with 0 as the farthest left</param>
      ///// <param name="y">Y location of the Cell to set properties on, starting with 0 as the top</param>
      ///// <param name="isTransparent">True if line-of-sight is not blocked by this Cell. False otherwise</param>
      ///// <param name="isWalkable">True if a character could walk across the Cell normally. False otherwise</param>
      //public void SetCellProperties( int x, int y, bool isTransparent, bool isWalkable )
      //{
      //   SetCellProperties( x, y, isTransparent, isWalkable, false );
      //}

      /// <summary>
      /// Sets the properties of all Cells in the Map to be transparent and walkable
      /// </summary>
      public void Clear()
      {
         Clear(true, true);
      }

      /// <summary>
      /// Sets the properties of all Cells in the Map to the specified values
      /// </summary>
      /// <param name="isTransparent">Optional parameter defaults to true if not provided. True if line-of-sight is not blocked by this Cell. False otherwise</param>
      /// <param name="isWalkable">Optional parameter defaults to true if not provided. True if a character could walk across the Cell normally. False otherwise</param>
      public void Clear(bool isTransparent, bool isWalkable)
      {
         foreach (Cell cell in GetAllCells())
         {
            SetIsTransparent(cell.Point, isTransparent);
            SetIsWalkable(cell.Point, isWalkable);
         }
      }

      /// <summary>
      /// Create and return a deep copy of an existing Map
      /// </summary>
      /// <returns>IMap deep copy of the original Map</returns>
      public IMap Clone()
      {
         var map = new Map(Width, Height);
         foreach (Cell cell in GetAllCells())
         {
            map.CopyCellToMapLocation(cell);
         }

         return map;
      }

      public void CopyCellToMapLocation(Cell cell) => CopyCellToMapLocation(cell, cell.Point);

      public void CopyCellToMapLocation(Cell cell, Point point)
      {
         SetIsTransparent(point, cell.IsTransparent);
         SetIsWalkable(point, cell.IsWalkable);
         SetIsExplored(point, cell.IsExplored);
      }


      /// <summary>
      /// Copies the Cell properties of a smaller source Map into this destination Map at location (0,0)
      /// </summary>
      /// <param name="sourceMap">An IMap which must be of smaller size and able to fit in this destination Map at the specified location</param>
      public void Copy(IMap sourceMap)
      {
         Copy(sourceMap, 0, 0);
      }

      /// <summary>
      /// Copies the Cell properties of a smaller source Map into this destination Map at the specified location
      /// </summary>
      /// <param name="sourceMap">An IMap which must be of smaller size and able to fit in this destination Map at the specified location</param>
      /// <param name="left">Optional parameter defaults to 0 if not provided. X location of the Cell to start copying parameters to, starting with 0 as the farthest left</param>
      /// <param name="top">Optional parameter defaults to 0 if not provided. Y location of the Cell to start copying parameters to, starting with 0 as the top</param>
      /// <exception cref="ArgumentNullException">Thrown on null source map</exception>
      /// <exception cref="ArgumentException">Thrown on invalid source map dimensions</exception>
      public void Copy(IMap sourceMap, int left, int top)
      {
         if (sourceMap == null)
         {
            throw new ArgumentNullException("sourceMap", "Source map cannot be null");
         }

         if (sourceMap.Width + left > Width)
         {
            throw new ArgumentException("Source map 'width' + 'left' cannot be larger than the destination map width");
         }

         if (sourceMap.Height + top > Height)
         {
            throw new ArgumentException("Source map 'height' + 'top' cannot be larger than the destination map height");
         }

         foreach (Cell cell in sourceMap.GetAllCells())
         {
            CopyCellToMapLocation(cell, new Point(cell.Point.X + left, cell.Point.Y + top));
         }
      }

      /// <summary>
      /// Performs a field-of-view calculation with the specified parameters.
      /// Field-of-view (FOV) is basically a calculation of what is observable in the Map from a given Cell with a given light radius.
      /// Any existing field-of-view calculations will be overwritten when calling this method.
      /// </summary>
      /// <param name="xOrigin">X location of the Cell to perform the field-of-view calculation with 0 as the farthest left</param>
      /// <param name="yOrigin">Y location of the Cell to perform the field-of-view calculation with 0 as the top</param>
      /// <param name="radius">The number of Cells in which the field-of-view extends from the origin Cell. Think of this as the intensity of the light source.</param>
      /// <param name="lightWalls">True if walls should be included in the field-of-view when they are within the radius of the light source. False excludes walls even when they are within range.</param>
      /// <returns>List of Cells representing what is observable in the Map based on the specified parameters</returns>
      public ReadOnlyCollection<Cell> ComputeFov(int xOrigin, int yOrigin, int radius, bool lightWalls)
      {
         return _fieldOfView.ComputeFov(xOrigin, yOrigin, radius, lightWalls);
      }

      /// <summary>
      /// Performs a field-of-view calculation with the specified parameters and appends it any existing field-of-view calculations.
      /// Field-of-view (FOV) is basically a calculation of what is observable in the Map from a given Cell with a given light radius.
      /// </summary>
      /// <example>
      /// When a character is holding a light source in a large area that also has several other sources of light such as torches along the walls
      /// ComputeFov could first be called for the character and then AppendFov could be called for each torch to give us the final combined FOV given all the light sources
      /// </example>
      /// <param name="xOrigin">X location of the Cell to perform the field-of-view calculation with 0 as the farthest left</param>
      /// <param name="yOrigin">Y location of the Cell to perform the field-of-view calculation with 0 as the top</param>
      /// <param name="radius">The number of Cells in which the field-of-view extends from the origin Cell. Think of this as the intensity of the light source.</param>
      /// <param name="lightWalls">True if walls should be included in the field-of-view when they are within the radius of the light source. False excludes walls even when they are within range.</param>
      /// <returns>List of Cells representing what is observable in the Map based on the specified parameters</returns>
      public ReadOnlyCollection<Cell> AppendFov(int xOrigin, int yOrigin, int radius, bool lightWalls)
      {
         return _fieldOfView.AppendFov(xOrigin, yOrigin, radius, lightWalls);
      }

      /// <summary>
      /// Get an IEnumerable of all Cells in the Map
      /// </summary>
      /// <returns>IEnumerable of all Cells in the Map</returns>
      public IEnumerable<Cell> GetAllCells()
      {
         for (int y = 0; y < Height; y++)
         {
            for (int x = 0; x < Width; x++)
            {
               yield return GetCell(x, y);
            }
         }
      }

      /// <summary>
      /// Get an IEnumerable of Cells in a line from the Origin Cell to the Destination Cell
      /// The resulting IEnumerable includes the Origin and Destination Cells
      /// Uses Bresenham's line algorithm to determine which Cells are in the closest approximation to a straight line between the two Cells
      /// </summary>
      /// <param name="xOrigin">X location of the Origin Cell at the start of the line with 0 as the farthest left</param>
      /// <param name="yOrigin">Y location of the Origin Cell at the start of the line with 0 as the top</param>
      /// <param name="xDestination">X location of the Destination Cell at the end of the line with 0 as the farthest left</param>
      /// <param name="yDestination">Y location of the Destination Cell at the end of the line with 0 as the top</param>
      /// <returns>IEnumerable of Cells in a line from the Origin Cell to the Destination Cell which includes the Origin and Destination Cells</returns>
      public IEnumerable<Cell> GetCellsAlongLine(int xOrigin, int yOrigin, int xDestination, int yDestination)
      {
         xOrigin = ClampX(xOrigin);
         yOrigin = ClampY(yOrigin);
         xDestination = ClampX(xDestination);
         yDestination = ClampY(yDestination);

         int dx = Math.Abs(xDestination - xOrigin);
         int dy = Math.Abs(yDestination - yOrigin);

         int sx = xOrigin < xDestination ? 1 : -1;
         int sy = yOrigin < yDestination ? 1 : -1;
         int err = dx - dy;

         while (true)
         {
            yield return GetCell(xOrigin, yOrigin);
            if (xOrigin == xDestination && yOrigin == yDestination)
            {
               break;
            }

            int e2 = 2 * err;
            if (e2 > -dy)
            {
               err = err - dy;
               xOrigin = xOrigin + sx;
            }

            if (e2 < dx)
            {
               err = err + dx;
               yOrigin = yOrigin + sy;
            }
         }
      }

      private int ClampX(int x)
      {
         return (x < 0) ? 0 : (x > Width - 1) ? Width - 1 : x;
      }

      private int ClampY(int y)
      {
         return (y < 0) ? 0 : (y > Height - 1) ? Height - 1 : y;
      }

      /// <summary>
      /// Get an IEnumerable of Cells in a circle around the center Cell up to the specified radius using Bresenham's midpoint circle algorithm
      /// </summary>
      /// <seealso href="https://en.wikipedia.org/wiki/Midpoint_circle_algorithm">Based on Bresenham's midpoint circle algorithm</seealso>
      /// <param name="xCenter">X location of the center Cell with 0 as the farthest left</param>
      /// <param name="yCenter">Y location of the center Cell with 0 as the top</param>
      /// <param name="radius">The number of Cells to get in a radius from the center Cell</param>
      /// <returns>IEnumerable of Cells in a circle around the center Cell</returns>
      public IEnumerable<Cell> GetCellsInCircle(int xCenter, int yCenter, int radius)
      {
         var discovered = new HashSet<int>();

         int d = (5 - radius * 4) / 4;
         int x = 0;
         int y = radius;

         do
         {
            foreach (Cell cell in GetCellsAlongLine(xCenter + x, yCenter + y, xCenter - x, yCenter + y))
            {
               if (AddToHashSet(discovered, cell))
               {
                  yield return cell;
               }
            }

            foreach (Cell cell in GetCellsAlongLine(xCenter - x, yCenter - y, xCenter + x, yCenter - y))
            {
               if (AddToHashSet(discovered, cell))
               {
                  yield return cell;
               }
            }

            foreach (Cell cell in GetCellsAlongLine(xCenter + y, yCenter + x, xCenter - y, yCenter + x))
            {
               if (AddToHashSet(discovered, cell))
               {
                  yield return cell;
               }
            }

            foreach (Cell cell in GetCellsAlongLine(xCenter + y, yCenter - x, xCenter - y, yCenter - x))
            {
               if (AddToHashSet(discovered, cell))
               {
                  yield return cell;
               }
            }

            if (d < 0)
            {
               d += 2 * x + 1;
            }
            else
            {
               d += 2 * (x - y) + 1;
               y--;
            }

            x++;
         } while (x <= y);
      }

      /// <summary>
      /// Get an IEnumerable of Cells in a diamond (Rhombus) shape around the center Cell up to the specified distance
      /// </summary>
      /// <param name="xCenter">X location of the center Cell with 0 as the farthest left</param>
      /// <param name="yCenter">Y location of the center Cell with 0 as the top</param>
      /// <param name="distance">The number of Cells to get in a distance from the center Cell</param>
      /// <returns>IEnumerable of Cells in a diamond (Rhombus) shape around the center Cell</returns>
      public IEnumerable<Cell> GetCellsInDiamond(int xCenter, int yCenter, int distance)
      {
         var discovered = new HashSet<int>();

         int xMin = Math.Max(0, xCenter - distance);
         int xMax = Math.Min(Width - 1, xCenter + distance);
         int yMin = Math.Max(0, yCenter - distance);
         int yMax = Math.Min(Height - 1, yCenter + distance);

         for (int i = 0; i <= distance; i++)
         {
            for (int j = distance; j >= 0 + i; j--)
            {
               Cell cell;
               if (AddToHashSet(discovered, Math.Max(xMin, xCenter - i), Math.Min(yMax, yCenter + distance - j),
                  out cell))
               {
                  yield return cell;
               }

               if (AddToHashSet(discovered, Math.Max(xMin, xCenter - i), Math.Max(yMin, yCenter - distance + j),
                  out cell))
               {
                  yield return cell;
               }

               if (AddToHashSet(discovered, Math.Min(xMax, xCenter + i), Math.Min(yMax, yCenter + distance - j),
                  out cell))
               {
                  yield return cell;
               }

               if (AddToHashSet(discovered, Math.Min(xMax, xCenter + i), Math.Max(yMin, yCenter - distance + j),
                  out cell))
               {
                  yield return cell;
               }
            }
         }
      }

      public IEnumerable<Point> GetCoordsInSquare(int xCenter, int yCenter, int distance)
      {
         int xMin = Math.Max(0, xCenter - distance);
         int xMax = Math.Min(Width - 1, xCenter + distance);
         int yMin = Math.Max(0, yCenter - distance);
         int yMax = Math.Min(Height - 1, yCenter + distance);

         for (int y = yMin; y <= yMax; y++)
         {
            for (int x = xMin; x <= xMax; x++)
            {
               yield return new Point(x, y);
            }
         }
      }


      /// <summary>
      /// Get an IEnumerable of Cells in a square area around the center Cell up to the specified distance
      /// </summary>
      /// <param name="xCenter">X location of the center Cell with 0 as the farthest left</param>
      /// <param name="yCenter">Y location of the center Cell with 0 as the top</param>
      /// <param name="distance">The number of Cells to get in each direction from the center Cell</param>
      /// <returns>IEnumerable of Cells in a square area around the center Cell</returns>
      public IEnumerable<Cell> GetCellsInSquare(int xCenter, int yCenter, int distance)
      {
         int xMin = Math.Max(0, xCenter - distance);
         int xMax = Math.Min(Width - 1, xCenter + distance);
         int yMin = Math.Max(0, yCenter - distance);
         int yMax = Math.Min(Height - 1, yCenter + distance);

         for (int y = yMin; y <= yMax; y++)
         {
            for (int x = xMin; x <= xMax; x++)
            {
               yield return GetCell(x, y);
            }
         }
      }

      /// <summary>
      /// Get an IEnumerable of outermost border Cells in a circle around the center Cell up to the specified radius using Bresenham's midpoint circle algorithm
      /// </summary>
      /// <seealso href="https://en.wikipedia.org/wiki/Midpoint_circle_algorithm">Based on Bresenham's midpoint circle algorithm</seealso>
      /// <param name="xCenter">X location of the center Cell with 0 as the farthest left</param>
      /// <param name="yCenter">Y location of the center Cell with 0 as the top</param>
      /// <param name="radius">The number of Cells to get in a radius from the center Cell</param>
      /// <returns>IEnumerable of outermost border Cells in a circle around the center Cell</returns>
      public IEnumerable<Cell> GetBorderCellsInCircle(int xCenter, int yCenter, int radius)
      {
         var discovered = new HashSet<int>();

         int d = (5 - radius * 4) / 4;
         int x = 0;
         int y = radius;

         Cell centerCell = GetCell(xCenter, yCenter);

         do
         {
            Cell cell;
            if (AddToHashSet(discovered, ClampX(xCenter + x), ClampY(yCenter + y), centerCell, out cell))
            {
               yield return cell;
            }

            if (AddToHashSet(discovered, ClampX(xCenter + x), ClampY(yCenter - y), centerCell, out cell))
            {
               yield return cell;
            }

            if (AddToHashSet(discovered, ClampX(xCenter - x), ClampY(yCenter + y), centerCell, out cell))
            {
               yield return cell;
            }

            if (AddToHashSet(discovered, ClampX(xCenter - x), ClampY(yCenter - y), centerCell, out cell))
            {
               yield return cell;
            }

            if (AddToHashSet(discovered, ClampX(xCenter + y), ClampY(yCenter + x), centerCell, out cell))
            {
               yield return cell;
            }

            if (AddToHashSet(discovered, ClampX(xCenter + y), ClampY(yCenter - x), centerCell, out cell))
            {
               yield return cell;
            }

            if (AddToHashSet(discovered, ClampX(xCenter - y), ClampY(yCenter + x), centerCell, out cell))
            {
               yield return cell;
            }

            if (AddToHashSet(discovered, ClampX(xCenter - y), ClampY(yCenter - x), centerCell, out cell))
            {
               yield return cell;
            }

            if (d < 0)
            {
               d += 2 * x + 1;
            }
            else
            {
               d += 2 * (x - y) + 1;
               y--;
            }

            x++;
         } while (x <= y);
      }

      /// <summary>
      /// Get an IEnumerable of outermost border Cells in a diamond (Rhombus) shape around the center Cell up to the specified distance
      /// </summary>
      /// <param name="xCenter">X location of the center Cell with 0 as the farthest left</param>
      /// <param name="yCenter">Y location of the center Cell with 0 as the top</param>
      /// <param name="distance">The number of Cells to get in a distance from the center Cell</param>
      /// <returns>IEnumerable of outermost border Cells in a diamond (Rhombus) shape around the center Cell</returns>
      public IEnumerable<Cell> GetBorderCellsInDiamond(int xCenter, int yCenter, int distance)
      {
         var discovered = new HashSet<int>();

         int xMin = Math.Max(0, xCenter - distance);
         int xMax = Math.Min(Width - 1, xCenter + distance);
         int yMin = Math.Max(0, yCenter - distance);
         int yMax = Math.Min(Height - 1, yCenter + distance);

         Cell centerCell = GetCell(xCenter, yCenter);
         Cell cell;
         if (AddToHashSet(discovered, xCenter, yMin, centerCell, out cell))
         {
            yield return cell;
         }

         if (AddToHashSet(discovered, xCenter, yMax, centerCell, out cell))
         {
            yield return cell;
         }

         for (int i = 1; i <= distance; i++)
         {
            if (AddToHashSet(discovered, Math.Max(xMin, xCenter - i), Math.Min(yMax, yCenter + distance - i),
               centerCell, out cell))
            {
               yield return cell;
            }

            if (AddToHashSet(discovered, Math.Max(xMin, xCenter - i), Math.Max(yMin, yCenter - distance + i),
               centerCell, out cell))
            {
               yield return cell;
            }

            if (AddToHashSet(discovered, Math.Min(xMax, xCenter + i), Math.Min(yMax, yCenter + distance - i),
               centerCell, out cell))
            {
               yield return cell;
            }

            if (AddToHashSet(discovered, Math.Min(xMax, xCenter + i), Math.Max(yMin, yCenter - distance + i),
               centerCell, out cell))
            {
               yield return cell;
            }
         }
      }

      /// <summary>
      /// Get an IEnumerable of outermost border Cells in a square area around the center Cell up to the specified distance
      /// </summary>
      /// <param name="xCenter">X location of the center Cell with 0 as the farthest left</param>
      /// <param name="yCenter">Y location of the center Cell with 0 as the top</param>
      /// <param name="distance">The number of Cells to get in each direction from the center Cell</param>
      /// <returns>IEnumerable of outermost border Cells in a square area around the center Cell</returns>
      public IEnumerable<Cell> GetBorderCellsInSquare(int xCenter, int yCenter, int distance)
      {
         int xMin = Math.Max(0, xCenter - distance);
         int xMax = Math.Min(Width - 1, xCenter + distance);
         int yMin = Math.Max(0, yCenter - distance);
         int yMax = Math.Min(Height - 1, yCenter + distance);
         List<Cell> borderCells = new List<Cell>();

         for (int x = xMin; x <= xMax; x++)
         {
            borderCells.Add(GetCell(x, yMin));
            borderCells.Add(GetCell(x, yMax));
         }

         for (int y = yMin + 1; y <= yMax - 1; y++)
         {
            borderCells.Add(GetCell(xMin, y));
            borderCells.Add(GetCell(xMax, y));
         }

         Cell centerCell = GetCell(xCenter, yCenter);
         borderCells.Remove(centerCell);

         return borderCells;
      }

      /// <summary>
      /// Get an IEnumerable of all the Cells in the specified row numbers
      /// </summary>
      /// <param name="rowNumbers">Integer array of row numbers with 0 as the top</param>
      /// <returns>IEnumerable of all the Cells in the specified row numbers</returns>
      public IEnumerable<Cell> GetCellsInRows(params int[] rowNumbers)
      {
         foreach (int y in rowNumbers)
         {
            for (int x = 0; x < Width; x++)
            {
               yield return GetCell(x, y);
            }
         }
      }

      /// <summary>
      /// Get an IEnumerable of all the Cells in the specified column numbers
      /// </summary>
      /// <param name="columnNumbers">Integer array of column numbers with 0 as the farthest left</param>
      /// <returns>IEnumerable of all the Cells in the specified column numbers</returns>
      public IEnumerable<Cell> GetCellsInColumns(params int[] columnNumbers)
      {
         foreach (int x in columnNumbers)
         {
            for (int y = 0; y < Height; y++)
            {
               yield return GetCell(x, y);
            }
         }
      }

      /// <summary>
      /// Get a Cell at the specified location
      /// </summary>
      /// <param name="x">X location of the Cell to get starting with 0 as the farthest left</param>
      /// <param name="y">Y location of the Cell to get, starting with 0 as the top</param>
      /// <returns>Cell at the specified location</returns>
      public Cell GetCell(int x, int y)
      {
         return new Cell(x, y, _isTransparent[x, y], _isWalkable[x, y], _fieldOfView.IsInFov(x, y), _isExplored[x, y]);
      }

      public Cell GetCell(Point point) => GetCell(point.X, point.Y);

      /// <summary>
      /// Provides a simple visual representation of the map using the following symbols:
      /// - `%`: `Cell` is not in field-of-view
      /// - `.`: `Cell` is transparent, walkable, and in field-of-view
      /// - `s`: `Cell` is walkable and in field-of-view (but not transparent)
      /// - `o`: `Cell` is transparent and in field-of-view (but not walkable)
      /// - `#`: `Cell` is in field-of-view (but not transparent or walkable)
      /// </summary>
      /// <param name="useFov">True if field-of-view calculations will be used when creating the string represenation of the Map. False otherwise</param>
      /// <returns>A string representation of the map using special symbols to denote Cell properties</returns>
      public string ToString(bool useFov)
      {
         var mapRepresentation = new StringBuilder();
         int lastY = 0;
         foreach (Cell cell in GetAllCells())
         {
            if (cell.Point.Y != lastY)
            {
               lastY = cell.Point.Y;
               mapRepresentation.Append(Environment.NewLine);
            }

            mapRepresentation.Append(cell.ToString(useFov));
         }

         return mapRepresentation.ToString().TrimEnd('\r', '\n');
      }

      /// <summary>
      /// Get a MapState POCO which represents this Map and can be easily serialized
      /// Use Restore with the MapState to get back a full Map
      /// </summary>
      /// <returns>Mapstate POCO (Plain Old C# Object) which represents this Map and can be easily serialized</returns>
      public MapState Save()
      {
         var mapState = new MapState();
         mapState.Width = Width;
         mapState.Height = Height;
         mapState.Cells = new MapState.CellProperties[Width * Height];
         foreach (Cell cell in GetAllCells())
         {
            var cellProperties = MapState.CellProperties.None;
            if (cell.IsInFov)
            {
               cellProperties |= MapState.CellProperties.Visible;
            }

            if (cell.IsTransparent)
            {
               cellProperties |= MapState.CellProperties.Transparent;
            }

            if (cell.IsWalkable)
            {
               cellProperties |= MapState.CellProperties.Walkable;
            }

            mapState.Cells[cell.Point.Y * Width + cell.Point.X] = cellProperties;
         }

         return mapState;
      }

      /// <summary>
      /// Restore the state of this Map from the specified MapState
      /// </summary>
      /// <param name="state">Mapstate POCO (Plain Old C# Object) which represents this Map and can be easily serialized and deserialized</param>
      /// <exception cref="ArgumentNullException">Thrown on null map state</exception>
      public void Restore(MapState state)
      {
         if (state == null)
         {
            throw new ArgumentNullException("state", "Map state cannot be null");
         }

         var inFov = new HashSet<int>();

         Initialize(state.Width, state.Height);
         foreach (Cell cell in GetAllCells())
         {
            MapState.CellProperties cellProperties = state.Cells[cell.Point.Y * Width + cell.Point.X];
            if (cellProperties.HasFlag(MapState.CellProperties.Visible))
            {
               inFov.Add(IndexFor(cell.Point));
            }

            _isTransparent[cell.Point.X, cell.Point.Y] = cellProperties.HasFlag(MapState.CellProperties.Transparent);
            _isWalkable[cell.Point.X, cell.Point.Y] = cellProperties.HasFlag(MapState.CellProperties.Walkable);
         }

         _fieldOfView = new FieldOfView(this, inFov);
      }

      /// <summary>
      /// Static factory method which creates a new Map using the specified IMapCreationStrategy
      /// </summary>
      /// <remarks>
      /// Several classes implementing IMapCreationStrategy are included with RogueSharp but they are very basic
      /// It recommended that you create your own class implementing this interface to create more interesting Maps
      /// </remarks>
      /// <param name="mapCreationStrategy">A class that implements IMapCreationStrategy and has CreateMap method which defines algorithms for creating interesting Maps</param>
      /// <returns>Map created by calling CreateMap from the specified IMapCreationStrategy</returns>
      /// <exception cref="ArgumentNullException">Thrown on null map creation strategy</exception>
      public static Map Create(IMapCreationStrategy<Map> mapCreationStrategy)
      {
         if (mapCreationStrategy == null)
         {
            throw new ArgumentNullException("mapCreationStrategy", "Map creation strategy cannot be null");
         }

         return mapCreationStrategy.CreateMap();
      }

      /// <summary>
      /// Get the Cell at the specified single dimensional array index using the formulas: x = index % Width; y = index / Width;
      /// </summary>
      /// <param name="index">The single dimensional array index for the Cell that we want to get</param>
      /// <returns>Cell at the specified single dimensional array index</returns>
      public Cell CellFor(int index)
      {
         int x = index % Width;
         int y = index / Width;

         return GetCell(x, y);
      }

      /// <summary>
      /// Get the single dimensional array index for a Cell at the specified location using the formula: index = ( y * Width ) + x
      /// </summary>
      /// <param name="x">X location of the Cell index to get starting with 0 as the farthest left</param>
      /// <param name="y">Y location of the Cell index to get, starting with 0 as the top</param>
      /// <returns>An index for the Cell at the specified location useful if storing Cells in a single dimensional array</returns>
      public int IndexFor(int x, int y)
      {
         return (y * Width) + x;
      }

      /// <summary>
      /// Get the single dimensional array index for the specified Cell
      /// </summary>
      /// <param name="cell">The Cell to get the index for</param>
      /// <returns>An index for the Cell which is useful if storing Cells in a single dimensional array</returns>
      /// <exception cref="ArgumentNullException">Thrown on null cell</exception>
      public int IndexFor(Point cell)
      {
         return (cell.Y * Width) + cell.X;
      }

      private bool AddToHashSet(HashSet<int> hashSet, int x, int y, out Cell cell)
      {
         cell = GetCell(x, y);
         return hashSet.Add(IndexFor(cell.Point));
      }

      private bool AddToHashSet(HashSet<int> hashSet, int x, int y, Cell centerCell, out Cell cell)
      {
         cell = GetCell(x, y);
         if (cell.Equals(centerCell)) return false;
         return hashSet.Add(IndexFor(cell.Point));
      }

      private bool AddToHashSet(HashSet<int> hashSet, Cell cell)
      {
         return hashSet.Add(IndexFor(cell.Point));
      }

      /// <summary>
      /// Provides a simple visual representation of the map using the following symbols:
      /// - `.`: `Cell` is transparent and walkable
      /// - `s`: `Cell` is walkable (but not transparent)
      /// - `o`: `Cell` is transparent (but not walkable)
      /// - `#`: `Cell` is not transparent or walkable
      /// </summary>
      /// <remarks>
      /// This call ignores field-of-view. If field-of-view is important use the ToString overload with a "true" parameter
      /// </remarks>
      /// <returns>A string representation of the map using special symbols to denote Cell properties</returns>
      public override string ToString()
      {
         return ToString(false);
      }
   }
}
