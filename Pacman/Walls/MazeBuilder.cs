using SharpDX.Direct3D9;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace Walls
{
    internal class MazeBuilder
    {
        public int Width { get; set; }
        public int Height { get; set; }

        private List<List<Cell>> maze;

        private List<Cell> flattenMaze;

        private Random random = new();

        public List<List<Cell>> CreateMaze()
        {
            maze = new();

            for (int i = 0; i < Width; i++)
            {
                maze.Add(new());
                for (int j = 0; j < Height; j++)
                {
                    maze[i].Add(new Cell()
                    {
                        Walls = new bool[4] { true, true, true, true },
                        Visited = false,
                        Position = new Point(i , j),
                    });
                }
            }
            flattenMaze = maze.SelectMany(i => i).ToList();
            BuildMazeDepth1Serch();
            CreateLoops(1);
            CreateLoops(0);
            return maze;
        }


        internal List<List<int>> GetLevel(List<List<Cell>> maze)
        {
            var level = new List<List<int>>();
            for (var i = 0; i < maze.Count * 3; i++)
            {
                level.Add(new List<int>());
                for (var j = 0; j < maze[0].Count * 3; j++)
                {
                    level[i].Add(0);
                }
            }
            for (int i = 0; i < maze[0].Count; i++)
            {
                for (int j = 0; j < maze.Count; j++)
                {
                    level[j * 3][i * 3] = 0;            // top      left    corner filled in
                    level[j * 3][i * 3 + 2] = 0;        // top      right   corner filled in
                    level[j * 3 + 2][i * 3 + 2] = 0;    // bottom   right   corner filled in 
                    level[j * 3 + 2][i * 3] = 0;        // bottom   left    corner filled in

                    level[j * 3 + 1][i * 3] = maze[j][i].Walls[0] ? 0 : 1;      // left     wall
                    level[j * 3][i * 3 + 1] = maze[j][i].Walls[1] ? 0 : 1;      // top      wall
                    level[j * 3 + 1][i * 3 + 2] = maze[j][i].Walls[2] ? 0 : 1;  // right    wall
                    level[j * 3 + 2][i * 3 + 1] = maze[j][i].Walls[3] ? 0 : 1;  // bottom   wall

                    level[j * 3 + 1][i * 3 + 1] = maze[j][i].Visited ? 1 : 0; ; // middle cell
                }
            }

            return level;
        }
        private void CreateLoops(int option)
        {
            var vertices = flattenMaze.Where(c => c.Walls.Where(w => w == true).Count() >= 2 + option).ToList();
            while (vertices.Count() > 0)
            {
                var vertex = vertices.First();
                var neighbors = new List<Cell>
                {
                    flattenMaze.SingleOrDefault(c => c.Position.X == vertex.Position.X - 1 && c.Position.Y == vertex.Position.Y),
                    flattenMaze.SingleOrDefault(c => c.Position.X == vertex.Position.X && c.Position.Y == vertex.Position.Y - 1),
                    flattenMaze.SingleOrDefault(c => c.Position.X == vertex.Position.X + 1 && c.Position.Y == vertex.Position.Y),
                    flattenMaze.SingleOrDefault(c => c.Position.X == vertex.Position.X && c.Position.Y == vertex.Position.Y + 1)
                };
                neighbors = neighbors.Where(n => n != null && n.Visited == true && n.Walls.Where(w => w == true).Count() >= 2 - option).ToList();
                if (neighbors.Count == 0) break;
                for (int i = 0; i < neighbors.Count; i++)
                {
                    var neighbor = neighbors[i];
                    if (vertex.Position.X > neighbor.Position.X)
                    {
                        neighbor.Walls[3] = false;
                        vertex.Walls[1] = false;
                    }
                    if (vertex.Position.X < neighbor.Position.X)
                    {
                        neighbor.Walls[1] = false;
                        vertex.Walls[3] = false;
                    }
                    if (vertex.Position.Y > neighbor.Position.Y)
                    {
                        neighbor.Walls[2] = false;
                        vertex.Walls[0] = false;
                    }
                    if (vertex.Position.Y < neighbor.Position.Y)
                    {
                        neighbor.Walls[0] = false;
                        vertex.Walls[2] = false;
                    }
                }
                vertices.Remove(vertex);
            }
        }
        private void BuildMazeDepth1Serch()
        {
            var count = flattenMaze.Where(c => c.Visited == false).Count();
            while (count > 0)
            {

                var unvisitedCell = flattenMaze.Where(c => c.Visited == false).First();
                unvisitedCell.Visited = true;
                var neighbors = new List<Cell>
                {
                    flattenMaze.SingleOrDefault(c => c.Position.X == unvisitedCell.Position.X - 1 && c.Position.Y == unvisitedCell.Position.Y),
                    flattenMaze.SingleOrDefault(c => c.Position.X == unvisitedCell.Position.X && c.Position.Y == unvisitedCell.Position.Y - 1),
                    flattenMaze.SingleOrDefault(c => c.Position.X == unvisitedCell.Position.X + 1 && c.Position.Y == unvisitedCell.Position.Y),
                    flattenMaze.SingleOrDefault(c => c.Position.X == unvisitedCell.Position.X && c.Position.Y == unvisitedCell.Position.Y + 1)
                };
                neighbors = neighbors.Where(n => n != null && n.Visited == false).ToList();
                if (neighbors.Count != 0)
                {
                    BuildPath(ref neighbors, ref unvisitedCell);
                }
                count = flattenMaze.Where(c => c.Visited == false).Count();
            }
        }

        private void BuildPath(ref List<Cell> previousNeighbors, ref Cell previous)
        {
            var selectedCell = previousNeighbors.ElementAt(random.Next(previousNeighbors.Count));
            selectedCell.Visited = true;

            if (previous.Position.X > selectedCell.Position.X)
            {
                selectedCell.Walls[3] = false;
                previous.Walls[1] = false;
            }
            if (previous.Position.X < selectedCell.Position.X)
            {
                selectedCell.Walls[1] = false;
                previous.Walls[3] = false;
            }
            if (previous.Position.Y > selectedCell.Position.Y)
            {
                selectedCell.Walls[2] = false;
                previous.Walls[0] = false;
            }
            if (previous.Position.Y < selectedCell.Position.Y)
            {
                selectedCell.Walls[0] = false;
                previous.Walls[2] = false;
            }
            var neighbors = new List<Cell>
            {
                flattenMaze.SingleOrDefault(c => c.Position.X == selectedCell.Position.X - 1 && c.Position.Y == selectedCell.Position.Y),
                flattenMaze.SingleOrDefault(c => c.Position.X == selectedCell.Position.X && c.Position.Y == selectedCell.Position.Y - 1),
                flattenMaze.SingleOrDefault(c => c.Position.X == selectedCell.Position.X + 1 && c.Position.Y == selectedCell.Position.Y),
                flattenMaze.SingleOrDefault(c => c.Position.X == selectedCell.Position.X && c.Position.Y == selectedCell.Position.Y + 1)
            };

            neighbors = neighbors.Where(n => n != null && n.Visited == false).ToList();
            if (neighbors.Count == 0) return;
            BuildPath(ref neighbors, ref selectedCell);
        }
    }
}
