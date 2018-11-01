using System;
using System.Collections.Generic;
using RogueSharp.Algorithms;
using RogueSharp.Random;

namespace RogueSharp.MapCreation
{
   /// <summary>
   /// The CaveMapCreationStrategy creates a Map of the specified type by using a cellular automata algorithm for creating a cave-like map.
   /// </summary>
   /// <seealso href="http://www.roguebasin.com/index.php?title=Cellular_Automata_Method_for_Generating_Random_Cave-Like_Levels">Cellular Automata Method from RogueBasin</seealso>
   /// <typeparam name="T">The type of IMap that will be created</typeparam>
   public class CaveMapCreationStrategy<T> : IMapCreationStrategy<T> where T : class, IMap, new()
   {
      private readonly int _width;
      private readonly int _height;
      private readonly int _fillProbability;
      private readonly int _totalIterations;
      private readonly int _cutoffOfBigAreaFill;
      private readonly IRandom _random;
      private T _map;

      /// <summary>
      /// Constructs a new CaveMapCreationStrategy with the specified parameters
      /// </summary>
      /// <param name="width">The width of the Map to be created</param>
      /// <param name="height">The height of the Map to be created</param>
      /// <param name="fillProbability">Recommend int between 40 and 60. Percent chance that a given cell will be a floor when randomizing all cells.</param>
      /// <param name="totalIterations">Recommend int between 2 and 5. Number of times to execute the cellular automata algorithm.</param>
      /// <param name="cutoffOfBigAreaFill">Recommend int less than 4. The interation number to switch from the large area fill algorithm to a nearest neighbor algorithm</param>
      /// <param name="random">A class implementing IRandom that will be used to generate pseudo-random numbers necessary to create the Map</param>
      public CaveMapCreationStrategy( int width, int height, int fillProbability, int totalIterations, int cutoffOfBigAreaFill, IRandom random )
      {
         _width = width;
         _height = height;
         _fillProbability = fillProbability;
         _totalIterations = totalIterations;
         _cutoffOfBigAreaFill = cutoffOfBigAreaFill;
         _random = random;
         _map = new T();
      }

      /// <summary>
      /// Constructs a new CaveMapCreationStrategy with the specified parameters
      /// </summary>
      /// <param name="width">The width of the Map to be created</param>
      /// <param name="height">The height of the Map to be created</param>
      /// <param name="fillProbability">Recommend int between 40 and 60. Percent chance that a given cell will be a floor when randomizing all cells.</param>
      /// <param name="totalIterations">Recommend int between 2 and 5. Number of times to execute the cellular automata algorithm.</param>
      /// <param name="cutoffOfBigAreaFill">Recommend int less than 4. The interation number to switch from the large area fill algorithm to a nearest neighbor algorithm</param>
      /// <remarks>Uses DotNetRandom as its RNG</remarks>
      public CaveMapCreationStrategy( int width, int height, int fillProbability, int totalIterations, int cutoffOfBigAreaFill )
      {
         _width = width;
         _height = height;
         _fillProbability = fillProbability;
         _totalIterations = totalIterations;
         _cutoffOfBigAreaFill = cutoffOfBigAreaFill;
         _random = Singleton.DefaultRandom;
         _map = new T();
      }

      /// <summary>
      /// Creates a new IMap of the specified type.
      /// </summary>
      /// <remarks>
      /// The map will be generated using cellular automata. First each cell in the map will be set to a floor or wall randomly based on the specified fillProbability.
      /// Next each cell will be examined a number of times, and in each iteration it may be turned into a wall if there are enough other walls near it.
      /// Once finished iterating and examining neighboring cells, any isolated map regions will be connected with paths.
      /// </remarks>
      /// <returns>An IMap of the specified type</returns>
      public T CreateMap()
      {
         _map.Initialize( _width, _height );

         RandomlyFillCells();

         for ( int i = 0; i < _totalIterations; i++ )
         {
            if ( i < _cutoffOfBigAreaFill )
            {
               CellularAutomataBigAreaAlgorithm();
            }
            else if ( i >= _cutoffOfBigAreaFill )
            {
               CellularAutomaNearestNeighborsAlgorithm();
            }
         }

         //ConnectCaves();

         return _map;
      }

      private void RandomlyFillCells()
      {
         foreach (Cell cell in _map.GetAllCells())
         {
            bool isClear =
               (!IsBorderCoord(cell.Point) && _random.Next(1, 100) > _fillProbability);

            _map.SetIsTransparent(cell.Point, isClear);
            _map.SetIsWalkable(cell.Point, isClear);
         }
      }

      private void CellularAutomataBigAreaAlgorithm()
      {
         var updatedMap = _map.Clone() as T;

         foreach ( Cell cell in _map.GetAllCells() )
         {
            if (IsBorderCoord(cell.Point)) continue;

            bool isClear =
               !(CountWallsNear(cell.Point, 1) >= 5) || (CountWallsNear(cell.Point, 2) <= 2);

            updatedMap.SetIsTransparent(cell.Point, isClear);
            updatedMap.SetIsWalkable(cell.Point, isClear);
         }

         _map = updatedMap;
      }

      private void CellularAutomaNearestNeighborsAlgorithm()
      {
         var updatedMap = _map.Clone() as T;

         foreach ( Cell cell in _map.GetAllCells() )
         {
            if (IsBorderCoord(cell.Point)) continue;

            bool isClear = !(CountWallsNear(cell.Point, 1) >= 5);

            updatedMap.SetIsTransparent(cell.Point, isClear);
            updatedMap.SetIsWalkable(cell.Point, isClear);
         }

         _map = updatedMap;
      }

      private bool IsBorderCoord( Point point )
      {
         return point.X == 0 || point.X == _map.Width - 1
                || point.Y == 0 || point.Y == _map.Height - 1;
      }

      private int CountWallsNear(Point point, int distance)
      {
         int count = 0;
         foreach (Cell nearbyCell in _map.GetCellsInSquare(point.X, point.Y, distance))
         {
            if (point.Equals(nearbyCell.Point)) continue;
            if (!nearbyCell.IsWalkable) count++;
         }

         return count;
      }

      //private void ConnectCaves()
      //{
      //   var floodFillAnalyzer = new FloodFillAnalyzer(_map);
      //   List<MapSection> mapSections = floodFillAnalyzer.GetMapSections();
      //   var unionFind = new UnionFind( mapSections.Count );
      //   while ( unionFind.Count > 1 )
      //   {
      //      for ( int i = 0; i < mapSections.Count; i++ )
      //      {
      //         int closestMapSectionIndex = FindNearestMapSection( mapSections, i, unionFind );
      //         MapSection closestMapSection = mapSections[closestMapSectionIndex];
      //         IEnumerable<Cell> tunnelCells = _map.GetCellsAlongLine( mapSections[i].Bounds.Center.X, mapSections[i].Bounds.Center.Y,
      //            closestMapSection.Bounds.Center.X, closestMapSection.Bounds.Center.Y );

      //         bool firstCell = true;
      //         Cell previousCell = null;
      //         foreach ( Cell cell in tunnelCells )
      //         {
      //            _map.SetCellProperties( cell.X, cell.Y, true, true );
      //            if ( previousCell != null )
      //            {
      //               if ( cell.X != previousCell.X || cell.Y != previousCell.Y )
      //               {
      //                  _map.SetCellProperties( cell.X + 1, cell.Y, true, true );
      //               }
      //            }
      //            previousCell = cell;
      //         }
      //         unionFind.Union( i, closestMapSectionIndex );
      //      }
      //   }
      //}

      private static int FindNearestMapSection( IList<MapSection> mapSections, int mapSectionIndex, UnionFind unionFind )
      {
         MapSection start = mapSections[mapSectionIndex];
         int closestIndex = mapSectionIndex;
         int distance = Int32.MaxValue;
         for ( int i = 0; i < mapSections.Count; i++ )
         {
            if ( i == mapSectionIndex )
            {
               continue;
            }
            if ( unionFind.Connected( i, mapSectionIndex ) )
            {
               continue;
            }
            int distanceBetween = DistanceBetween( start, mapSections[i] );
            if ( distanceBetween < distance )
            {
               distance = distanceBetween;
               closestIndex = i;
            }
         }
         return closestIndex;
      }

      private static int DistanceBetween( MapSection startMapSection, MapSection destinationMapSection )
      {
         return Math.Abs( startMapSection.Bounds.Center.X - destinationMapSection.Bounds.Center.X ) + Math.Abs( startMapSection.Bounds.Center.Y - destinationMapSection.Bounds.Center.Y );
      }

      private class FloodFillAnalyzer
      {
         private readonly IMap _map;
         private readonly List<MapSection> _mapSections;

         private readonly int[][] _offsets =
         {
            new[] { 0, -1 }, new[] { -1, 0 }, new[] { 1, 0 }, new[] { 0, 1 }
         };

         private readonly bool[][] _visited;

         public FloodFillAnalyzer( IMap map )
         {
            _map = map;
            _mapSections = new List<MapSection>();
            _visited = new bool[_map.Height][];
            for (int i = 0; i < _visited.Length; i++)
            {
               _visited[i] = new bool[_map.Width];
            }
         }

         public List<MapSection> GetMapSections()
         {
            IEnumerable<Cell> cells = _map.GetAllCells();
            foreach ( Cell cell in cells )
            {
               MapSection section = Visit(cell);
               if ( section.Cells.Count > 0 )
               {
                  _mapSections.Add( section );
               }
            }

            return _mapSections;
         }

         private MapSection Visit(Cell cell)
         {
            Stack<Cell> stack = new Stack<Cell>(new List<Cell>());
            MapSection mapsection = new MapSection();
            stack.Push(cell);
            while ( stack.Count != 0 )
            {
               cell = stack.Pop();
               if ( _visited[cell.Point.Y][cell.Point.X] || !cell.IsWalkable )
               {
                  continue;
               }
               mapsection.AddCell( cell );
               _visited[cell.Point.Y][cell.Point.X] = true;
               //foreach ( Cell neighbor in GetNeighbors(cell.Point) )
               //{
               //   if ( cell.IsWalkable == neighbor.IsWalkable && !_visited[neighbor.Y][neighbor.X] )
               //   {
               //      stack.Push( neighbor );
               //   }
               //}
            }
            return mapsection;
         }

         private Cell GetCell( int x, int y )
         {
            if (x < 0 || y < 0)
            {
               throw new Exception("garga;foiugg");
            }
            if (x >= _map.Width || y >= _map.Height)
            {
               throw new Exception("garga;foiugg on the far edge");
            }
            return _map.GetCell( x, y );
         }

         private IEnumerable<Point> GetNeighbors( Point cell )
         {
            List<Point> neighbors = new List<Point>(8);
            foreach (int[] offset in _offsets)
            {
               var neighbor = GetCell(cell.X + offset[0], cell.Y + offset[1]);
               if ( neighbor == null )
               {
                  continue;
               }
               neighbors.Add( neighbor.Point );
            }

             return neighbors;
         }
      }

      private class MapSection
      {
         private int _top;
         private int _bottom;
         private int _right;
         private int _left;

         public Rectangle Bounds
         {
            get
            {
               return new Rectangle( _left, _top, _right - _left + 1, _bottom - _top + 1 );
            }
         }

         public HashSet<Cell> Cells { get; private set; }

         public MapSection()
         {
            Cells = new HashSet<Cell>();
            _top = int.MaxValue;
            _left = int.MaxValue;
         }

         public void AddCell( Cell cell )
         {
            Cells.Add( cell );
            UpdateBounds( cell.Point );
         }

         private void UpdateBounds( Point point )
         {
            if ( point.X > _right )
            {
              _right = point.X;
            }
            if ( point.X < _left )
            {
              _left = point.X;
            }
            if ( point.Y > _bottom )
            {
              _bottom = point.Y;
            }
            if ( point.Y < _top )
            {
              _top = point.Y;
            }
         }

         public override string ToString()
         {
            return string.Format( "Bounds: {0}", Bounds );
         }
      }
   }
}
