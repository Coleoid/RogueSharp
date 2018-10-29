using System;
using System.Collections.Generic;
using System.Linq;
using RogueSharp.Algorithms;

namespace RogueSharp
{
   /// <summary>
   /// A class which can be used to find shortest path from a source to a destination in a Map
   /// </summary>
   public class PathFinder
   {
      private readonly EdgeWeightedDigraph _graph;
      private readonly IMap _map;

      /// <summary>
      /// Constructs a new PathFinder instance for the specified Map that will not consider diagonal movements to be valid.
      /// </summary>
      /// <param name="map">The Map that this PathFinder instance will run shortest path algorithms on</param>
      /// <exception cref="ArgumentNullException">Thrown when a null map parameter is passed in</exception>
      public PathFinder( IMap map )
      {
         if ( map == null )
         {
            throw new ArgumentNullException( "map", "Map cannot be null" );
         }

         _map = map;
         _graph = new EdgeWeightedDigraph( _map.Width * _map.Height );
         foreach ( Cell cell in _map.GetAllCells() )
         {
            if ( cell.IsWalkable )
            {
               int v = IndexFor( cell.Coord );
               foreach ( Cell neighbor in _map.GetBorderCellsInDiamond( cell.Coord.X, cell.Coord.Y, 1 ) )
               {
                  if ( neighbor.IsWalkable )
                  {
                     int w = IndexFor( neighbor.Coord );
                     _graph.AddEdge( new DirectedEdge( v, w, 1.0 ) );
                     _graph.AddEdge( new DirectedEdge( w, v, 1.0 ) );
                  }
               }
            }
         }
      }

      /// <summary>
      /// Constructs a new PathFinder instance for the specified Map that will consider diagonal movement by using the specified diagonalCost
      /// </summary>
      /// <param name="map">The Map that this PathFinder instance will run shortest path algorithms on</param>
      /// <param name="diagonalCost">
      /// The cost of diagonal movement compared to horizontal or vertical movement. 
      /// Use 1.0 if you want the same cost for all movements.
      /// On a standard cartesian map, it should be sqrt(2) (1.41)
      /// </param>
      /// <exception cref="ArgumentNullException">Thrown when a null map parameter is passed in</exception>
      public PathFinder(IMap map, double diagonalCost)
      {
         if (map == null)
         {
            throw new ArgumentNullException("map", "Map cannot be null");
         }

         _map = map;
         _graph = new EdgeWeightedDigraph(_map.Width * _map.Height);
         foreach (Cell cell in _map.GetAllCells())
         {
            if (cell.IsWalkable)
            {
               int v = IndexFor(cell.Coord);
               foreach (Cell neighbor in _map.GetBorderCellsInSquare(cell.Coord.X, cell.Coord.Y, 1))
               {
                  if (neighbor.IsWalkable)
                  {
                     int w = IndexFor(neighbor.Coord);
                     if (neighbor.Coord.X != cell.Coord.X && neighbor.Coord.Y != cell.Coord.Y)
                     {
                        _graph.AddEdge(new DirectedEdge(v, w, diagonalCost));
                        _graph.AddEdge(new DirectedEdge(w, v, diagonalCost));
                     }
                     else
                     {
                        _graph.AddEdge(new DirectedEdge(v, w, 1.0));
                        _graph.AddEdge(new DirectedEdge(w, v, 1.0));
                     }
                  }
               }
            }
         }
      }

      public PathFinder(IMap map, double cardinalCost, double diagonalCost)
      {
         _map = map ?? throw new ArgumentNullException("map");
         _graph = new EdgeWeightedDigraph(_map.Width * _map.Height);

         foreach (Cell cell in _map.GetAllCells())
         {
            if (!cell.IsWalkable) continue;

            int cellIndex = IndexFor(cell.Coord);
            var neighborCoords = new List<(Coord,double)>
            {
               (new Coord(cell.Coord.X + 0, cell.Coord.Y - 1), cardinalCost),
               (new Coord(cell.Coord.X + 0, cell.Coord.Y + 1), cardinalCost),
               (new Coord(cell.Coord.X + 1, cell.Coord.Y + 0), cardinalCost),
               (new Coord(cell.Coord.X - 1, cell.Coord.Y + 0), cardinalCost),
               (new Coord(cell.Coord.X - 1, cell.Coord.Y - 1), diagonalCost),
               (new Coord(cell.Coord.X - 1, cell.Coord.Y + 1), diagonalCost),
               (new Coord(cell.Coord.X + 1, cell.Coord.Y - 1), diagonalCost),
               (new Coord(cell.Coord.X + 1, cell.Coord.Y + 1), diagonalCost),
            };

            foreach (var (coord, cost) in neighborCoords)
            {
               if (!map.IsWithinMap(coord)) continue;
               if (!map.IsWalkable(coord.X, coord.Y)) continue;

               int neighborIndex = IndexFor(coord);
               _graph.AddEdge(new DirectedEdge(cellIndex, neighborIndex, cost));
            }
         }
      }

      /// <summary>
      /// Returns a shortest Path containing a list of Cells from a specified source Cell to a destination Cell
      /// </summary>
      /// <param name="source">The Cell which is at the start of the path</param>
      /// <param name="destination">The Cell which is at the end of the path</param>
      /// <exception cref="ArgumentNullException">Thrown when source or destination is null</exception>
      /// <exception cref="PathNotFoundException">Thrown when there is not a path from the source to the destination</exception>
      /// <returns>Returns a shortest Path containing a list of Cells from a specified source Cell to a destination Cell</returns>
      public Path ShortestPath( Cell source, Cell destination )
      {
         Path shortestPath = TryFindShortestPath( source, destination );

         if ( shortestPath == null )
         {
            throw new PathNotFoundException( string.Format( "Path from ({0}, {1}) to ({2}, {3}) not found", source.Coord.X, source.Coord.Y, destination.Coord.X, destination.Coord.Y ) );
         }

         return shortestPath;
      }

      /// <summary>
      /// Returns a shortest Path containing a list of Cells from a specified source Cell to a destination Cell
      /// </summary>
      /// <param name="source">The Cell which is at the start of the path</param>
      /// <param name="destination">The Cell which is at the end of the path</param>
      /// <exception cref="ArgumentNullException">Thrown when source or destination is null</exception>
      /// <returns>Returns a shortest Path containing a list of Cells from a specified source Cell to a destination Cell. If no path is found null will be returned</returns>
      public Path TryFindShortestPath( Cell source, Cell destination )
      {
         if ( source == null )
         {
            throw new ArgumentNullException( "source" );
         }

         if ( destination == null )
         {
            throw new ArgumentNullException( "destination" );
         }

         var cells = ShortestPathCells( source, destination ).ToList();
         if ( cells[0] == null )
         {
            return null;
         }
         return new Path( cells );
      }

      public List<Coord> ShortestPathList(Coord source, Coord destination)
      {
         var list = new List<Coord>();
         IEnumerable<DirectedEdge> path = DijkstraShortestPath
            .FindPath(_graph, IndexFor(source), IndexFor(destination));

         if (path == null) return list;

         foreach (DirectedEdge edge in path)
         {
            list.Add(CoordFor(edge.To));
         }

         return list;
      }

      private IEnumerable<Cell> ShortestPathCells(Cell source, Cell destination)
      {
         IEnumerable<DirectedEdge> path = DijkstraShortestPath
            .FindPath(_graph, IndexFor(source.Coord), IndexFor(destination.Coord));
         if (path == null) yield break;

         yield return source;
         foreach (DirectedEdge edge in path)
         {
            yield return CellFor(edge.To);
         }
      }

      private int IndexFor( Coord cell )
      {
         return ( cell.Y * _map.Width ) + cell.X;
      }

      private Coord CoordFor(int index)
      {
         int x = index % _map.Width;
         int y = index / _map.Width;

         return new Coord(x, y);
      }

      private Cell CellFor(int index)
      {
         int x = index % _map.Width;
         int y = index / _map.Width;

         return _map.GetCell(x, y);
      }
   }
}