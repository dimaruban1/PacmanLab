using Microsoft.Xna.Framework;
using Walls;
using System.Collections.Generic;
using System;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using Microsoft.Xna.Framework.Graphics;
using SharpDX.Direct3D9;
using SharpDX.MediaFoundation;
using MyGame;

// add more algorithms
// check generation of levels

namespace Entities
{
    enum Direction
    {
        Up,
        Down,
        Left,
        Right,
    }

    enum EnemyType
    {
        common,
        strong,
        king,
    }
    
    internal class Enemy : DrawableGameComponent
    {
        public double Speed { get; set; }
        public Point Position { get; set; }
        public double ElapsedTime { get; set; }
        public EnemyType Type { get; private set; }

        Texture2D texture;

        Direction Direction;
        Random rand = new Random();
        private List<Direction> verticalDirections = new List<Direction>() { Direction.Down, Direction.Up };
        private List<Direction> horizontalDirections = new List<Direction>() { Direction.Left, Direction.Right };
        private PacmanGame pacmanGame;
        private List<LevelCell> flattenLevel;

        public Enemy(PacmanGame game, EnemyType type) : base(game)
        {
            pacmanGame = game;
            switch (type)
            {
                case EnemyType.king:
                    Speed = 1.5;
                    break;
                case EnemyType.strong:
                    Speed = 0.8;
                    break;
                case EnemyType.common:
                    Speed = 1;
                    break;
            }
            Position = new Point();
            Direction = Direction.Left;
            ElapsedTime = 0;
            Type = type;
        }

        public override void Initialize()
        {
            flattenLevel = pacmanGame.FlattenLevel;
            switch (Type)
            {
                case EnemyType.king:
                    texture = pacmanGame.Content.Load<Texture2D>("enemy-king");
                    break;
                case EnemyType.strong:
                    texture = pacmanGame.Content.Load<Texture2D>("enemy-strong");
                    break;
                case EnemyType.common:
                    texture = pacmanGame.Content.Load<Texture2D>("enemy");
                    break;
            }
            base.Initialize();
        }

        public override void Update(GameTime gameTime)
        {
            if (ElapsedTime >= PacmanGame.TICK_INTERVAL / Speed)
            {
                LevelCell nextCell = null;
                switch (Type)
                {
                    case EnemyType.king:
                        nextCell = GetNextCellDijkstra();
                        break;
                    case EnemyType.strong:
                        nextCell = getNextCellBasic();
                        break;
                    case EnemyType.common:
                        if (SeesPacman(pacmanGame.Pacman))
                        {
                            Speed = 2;
                            nextCell = getNextCellBasic();
                        }
                        else
                        {
                            Speed = 1;
                            nextCell = getNextCellAimless(pacmanGame.Level.Count);
                        }
                        break;
                }
                ElapsedTime = 0;
                MoveTo(nextCell);
            }

            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            int size = Type == EnemyType.common ? Cell.Side : (int)(Cell.Side * 1.3);
            pacmanGame.SpriteBatch.Draw(texture, new Rectangle(170 + Position.X * Cell.Side,
                                Position.Y * Cell.Side,
                                size,
                                size), Color.White);
            base.Draw(gameTime);
        }

        public bool SeesPacman(Pacman pacman)
        {
            int wallsBetweenEntities = 999;
            if (Position.X == pacman.Position.X)
            {
                var min = Math.Min(Position.Y, pacman.Position.Y);
                var max = Math.Max(Position.Y, pacman.Position.Y);

                wallsBetweenEntities = flattenLevel.Where(t => t.Position.X == Position.X && t.Position.Y >= min && t.Position.Y <= max && t.CellStatus == CellStatus.Wall).Count();
            }
            else if (Position.Y == pacman.Position.Y)
            {
                var min = Math.Min(Position.X, pacman.Position.X);
                var max = Math.Max(Position.X, pacman.Position.X);

                wallsBetweenEntities = flattenLevel.Where(t => t.Position.Y == Position.Y && t.Position.X >= min && t.Position.X <= max && t.CellStatus == CellStatus.Wall).Count();
            }
            if (wallsBetweenEntities == 0)
            {
                return true;
            };
            return false;
        }

        public LevelCell getNextCellAimless(int size)
        {

            LevelCell nextCell = getInertionNextCell();

            var borderCells = flattenLevel.Where(c =>
            (Math.Abs(c.Position.X - Position.X) == 1 && c.Position.Y == Position.Y) ||
            (Math.Abs(c.Position.Y - Position.Y) == 1 && c.Position.X == Position.X))
                .Where(c => c.CellStatus != CellStatus.Wall).ToList();

            if (nextCell == null || nextCell.CellStatus == CellStatus.Wall)
            {
                changeDirection(flattenLevel);
            }
            else if (borderCells.Count() > 2 && rand.Next(1, 2) == 1)
            {
                if (verticalDirections.Contains(Direction))
                {
                    changeDirection(horizontalDirections);
                }
                else if (horizontalDirections.Contains(Direction))
                {
                    changeDirection(verticalDirections);
                }
            }

            nextCell = getInertionNextCell();

            return nextCell;
        }
        
        private void MoveTo(LevelCell nextCell)
        {
            var previousCell = flattenLevel.Single(c => c.Position == Position);
            if (previousCell.CellStatus == CellStatus.Enemy) previousCell.CellStatus = CellStatus.Empty;
            if (previousCell.CellStatus == CellStatus.EnemyWCoin) previousCell.CellStatus = CellStatus.Coin;

            if (nextCell.CellStatus == CellStatus.Coin) nextCell.CellStatus = CellStatus.EnemyWCoin;
            if (nextCell.CellStatus == CellStatus.Empty) nextCell.CellStatus = CellStatus.Enemy;

            Position = nextCell.Position;
        }

        private void changeDirection(List<Direction> possibleDirections)
        {
            LevelCell nextCell = null;

            while (nextCell == null || nextCell.CellStatus == CellStatus.Wall)
            {
                Direction = possibleDirections[rand.Next(0, possibleDirections.Count)];
                nextCell = getInertionNextCell();
            }
        }

        private void changeDirection(List<LevelCell> flattenLevel)
        {
            LevelCell nextCell = null;
            var possibleDirections = new List<Direction>
            {
                Direction.Up, Direction.Down, Direction.Left, Direction.Right
            };

            while (nextCell == null || nextCell.CellStatus == CellStatus.Wall)
            {
                Direction = possibleDirections[rand.Next(0, possibleDirections.Count)];
                nextCell = getInertionNextCell();
            }
        }

        private LevelCell getInertionNextCell()
        {

            switch (Direction)
            {
                case Direction.Up:
                    return flattenLevel.Single(c => c.Position.X == Position.X && c.Position.Y == Position.Y - 1);
                case Direction.Down:
                    return flattenLevel.Single(c => c.Position.X == Position.X && c.Position.Y == Position.Y + 1);
                case Direction.Left:
                    return flattenLevel.Single(c => c.Position.X == Position.X - 1 && c.Position.Y == Position.Y);
                case Direction.Right:
                    return flattenLevel.Single(c => c.Position.X == Position.X + 1 && c.Position.Y == Position.Y);
                default:
                    return null;
            }
        }

        private LevelCell getNextCellBasic()
        {
            var neighbors = new List<LevelCell>()
                {
                    flattenLevel.SingleOrDefault(c=>c.Position.X == Position.X-1 && c.Position.Y == Position.Y),
                    flattenLevel.SingleOrDefault(c=>c.Position.X == Position.X+1 && c.Position.Y == Position.Y),
                    flattenLevel.SingleOrDefault(c=>c.Position.X == Position.X && c.Position.Y -1== Position.Y),
                    flattenLevel.SingleOrDefault(c=>c.Position.X == Position.X && c.Position.Y +1== Position.Y),
                };
            neighbors = neighbors.Where(n => n != null && n.CellStatus != CellStatus.Wall).ToList();
            var results = new List<double>();
            foreach (var neighbor in neighbors)
            {
                results.Add(Math.Pow(neighbor.Position.X - pacmanGame.Pacman.Position.X, 2) + Math.Pow(neighbor.Position.Y - pacmanGame.Pacman.Position.Y, 2));
            }
            return neighbors.ElementAt(results.IndexOf(results.OrderBy(r => r).First()));
        }

        public LevelCell? GetNextCellDijkstra()
        {
            var target = pacmanGame.Pacman.Position;
            var Map = pacmanGame.Level;
            var directions = new List<Point>
        {
            new Point(0, 1),  // right
            new Point(1, 0),  // down
            new Point(0, -1), // left
            new Point(-1, 0)  // up
        };

            var distance = new Dictionary<Point, int>();
            var previous = new Dictionary<Point, Point?>();
            var unvisited = new HashSet<Point>();

            foreach (var row in Map)
            {
                foreach (var cell in row)
                {
                    distance[cell.Position] = int.MaxValue;
                    previous[cell.Position] = null;
                    if (cell.CellStatus != CellStatus.Wall)
                    {
                        unvisited.Add(cell.Position);
                    }
                }
            }

            distance[Position] = 0;

            var priorityQueue = new PriorityQueue<Point, int>();
            priorityQueue.Enqueue(Position, 0);

            while (priorityQueue.Count > 0)
            {
                var current = priorityQueue.Dequeue();

                if (current == target)
                    break;

                foreach (var direction in directions)
                {
                    var neighbor = new Point(current.X + direction.X, current.Y + direction.Y);

                    if (IsWithinBounds(neighbor) && unvisited.Contains(neighbor))
                    {
                        int newDist = distance[current] + 1;

                        if (newDist < distance[neighbor])
                        {
                            distance[neighbor] = newDist;
                            previous[neighbor] = current;
                            priorityQueue.Enqueue(neighbor, newDist);
                        }
                    }
                }

                unvisited.Remove(current);
            }

            var path = BuildPath(previous, Position, pacmanGame.Pacman.Position);
            var nextCell = flattenLevel.Single(c => c.Position == path[0]);
            return nextCell;
        }

        private bool IsWithinBounds(Point point)
        {
            return point.X >= 0 && point.X < pacmanGame.Level.Count && point.Y >= 0 && point.Y < pacmanGame.Level[0].Count;
        }

        private List<Point> BuildPath(Dictionary<Point, Point?> previous, Point start, Point target)
        {
            var path = new List<Point>();
            var current = target;

            while (previous[current] != null)
            {
                path.Add(current);
                current = previous[current].Value;
            }

            path.Reverse();
            return path;
        }
    }
}
