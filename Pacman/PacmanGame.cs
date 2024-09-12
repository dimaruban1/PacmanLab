using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using Entities;
using System.Linq;
using System.Diagnostics;
using Walls;

namespace MyGame
{
    public enum GameStatus
    {
        Win,
        Loss,
        Quit,
    }
    public class PacmanGame : Game
    {
        private GraphicsDeviceManager _graphics;

        public SpriteBatch SpriteBatch { get; set; }
        public List<List<LevelCell>> Level { get; set; }
        public GameStatus Status { get; private set; }
        public int CoinsGained { get; private set; }
        public List<LevelCell> FlattenLevel { get; set; }
        public Pacman Pacman { get; set; }
        


        Texture2D wallTexture;
        Texture2D pacmanTexture;
        Texture2D coinTexture;
        Texture2D cellTexture;
        SpriteFont mainFont;

        private int coinsLeft;
        private int levelNumber;
        private int difficulty;
        private int enemyCount;
        private Action playerAction = Action.Stop;
        private Action previousPlayerAction = Action.Stop;

        private List<Enemy> enemies;
        double _timeElapsedPacman = 0;

        public const double TICK_INTERVAL = 0.5;

        public PacmanGame(List<List<int>> level, double enemySpeed, int difficulty, int levelNumber, int coinsGained, int enemyCount)
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            Level = new();
            for (int i = 0; i < level.Count; i++)
            {
                Level.Add(new List<LevelCell>());
                for (int j = 0; j < level[i].Count; j++)
                {
                    if (level[i][j] == 0) Level[i].Add(new LevelCell()
                    {
                        CellStatus = CellStatus.Wall,
                        Position = new Point(i, j),
                    });
                    else Level[i].Add(new LevelCell()
                    {
                        CellStatus = CellStatus.Coin,
                        Position = new Point(i, j),
                    });
                }
            }
            FlattenLevel = Level.SelectMany(x => x).ToList();
            coinsLeft = FlattenLevel.Where(c => c.CellStatus == CellStatus.Coin).Count();
            this.levelNumber = levelNumber;
            CoinsGained = coinsGained;
            this.difficulty = difficulty;
            this.enemyCount = enemyCount;
        }

        protected override void Initialize()
        {
            var rand = new System.Random();
            initPlayer(rand, 2);
            initEnemies(rand, enemyCount);
            foreach (var enemy in enemies)
            {
                enemy.Initialize();
            }
            base.Initialize();
        }

        protected override void LoadContent()
        {
            SpriteBatch = new SpriteBatch(GraphicsDevice);
            wallTexture = Content.Load<Texture2D>("wall");
            pacmanTexture = Content.Load<Texture2D>("pacman");
            coinTexture = Content.Load<Texture2D>("coin");
            cellTexture = Content.Load<Texture2D>("cell");
            mainFont = Content.Load<SpriteFont>("alice");
        }

        protected override void Update(GameTime gameTime)
        {
            if (coinsLeft == 0)
            {
                Status = GameStatus.Win;
                Exit();
            }

            if (enemies.Where(e => e.Position.X == Pacman.Position.X && e.Position.Y == Pacman.Position.Y).Count() > 0)
            {
                Status = GameStatus.Loss;
                Exit();
            }
            
            handleUserInput();

            _timeElapsedPacman += gameTime.ElapsedGameTime.TotalSeconds;
            foreach (var enemy in enemies)
            {
                enemy.ElapsedTime += gameTime.ElapsedGameTime.TotalSeconds;
            }

            if (_timeElapsedPacman >= TICK_INTERVAL / (Pacman.Speed - 0.05 * difficulty)) // 2 is default speed, 1.5 is min speed
            {
                _timeElapsedPacman = 0;
                movePacman(playerAction);
            }

            foreach (var enemy in enemies)
            {
                enemy.Update(gameTime);
            }




            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            SpriteBatch.Begin();
            SpriteBatch.DrawString(mainFont, $"Your score: {CoinsGained}", new Vector2(0, 0), Color.White);
            SpriteBatch.DrawString(mainFont, $"Level number: {levelNumber}", new Vector2(0, 20), Color.White);
            SpriteBatch.DrawString(mainFont, $"Difficulty: {difficulty}", new Vector2(0, 50), Color.White);

            foreach (var enemy in enemies)
            {
                enemy.Draw(gameTime);
            }

            DrawLevel(170);

            SpriteBatch.End();

            base.Draw(gameTime);
        }

        private void initPlayer(System.Random rand, double speed)
        {
            var possibleCells = FlattenLevel.Where(c => c.CellStatus != CellStatus.Wall);
            var cell = possibleCells.ElementAt(rand.Next(possibleCells.Count()));
            Pacman = new Pacman
            {
                Position = cell.Position,
                Speed = speed,
            };
        }

        private void handleUserInput()
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            {
                Status = GameStatus.Quit;
                Exit();
            }

            var keyInfo = Keyboard.GetState();

                if (WasKeyPressed(Keys.Up) || WasKeyPressed(Keys.W))
                {
                    playerAction = Action.Up;
                }
                if (WasKeyPressed(Keys.Down) || WasKeyPressed(Keys.S))
                {
                    playerAction = Action.Down;
                }
                if (WasKeyPressed(Keys.Left) || WasKeyPressed(Keys.A))
                {
                    playerAction = Action.Left;
                }
                if (WasKeyPressed(Keys.Right) || WasKeyPressed(Keys.D))
                {
                    playerAction = Action.Right;
                }
        }

        private void movePacman(Action playerAction, bool error = false)
        {
            try
            {
                LevelCell cell = null;
                switch (playerAction)
                {
                    case Action.Stop:
                        cell = FlattenLevel.SingleOrDefault(c => c.Position.X == Pacman.Position.X && c.Position.Y == Pacman.Position.Y && c.CellStatus != CellStatus.Wall);
                        break;
                    case Action.Up:
                        cell = FlattenLevel.SingleOrDefault(c => c.Position.X == Pacman.Position.X && c.Position.Y == Pacman.Position.Y - 1 && c.CellStatus != CellStatus.Wall);
                        break;
                    case Action.Down:
                        cell = FlattenLevel.SingleOrDefault(c => c.Position.X == Pacman.Position.X && c.Position.Y == Pacman.Position.Y + 1 && c.CellStatus != CellStatus.Wall);

                        break;
                    case Action.Left:
                        cell = FlattenLevel.SingleOrDefault(c => c.Position.X == Pacman.Position.X - 1 && c.Position.Y == Pacman.Position.Y && c.CellStatus != CellStatus.Wall);

                        break;
                    case Action.Right:
                        cell = FlattenLevel.SingleOrDefault(c => c.Position.X == Pacman.Position.X + 1 && c.Position.Y == Pacman.Position.Y && c.CellStatus != CellStatus.Wall);
                        break;
                    default:
                        break;
                }
                if (cell != null)
                {
                    processPacmanMovement(cell);
                }
                else
                {
                    throw new System.ArgumentException();
                }
                previousPlayerAction = playerAction;
            }
            catch (System.ArgumentException)
            {
                if (!error) movePacman(previousPlayerAction, true);
            }


        }

        private void processPacmanMovement(LevelCell cell)
        {
            if (cell.CellStatus == CellStatus.Coin || cell.CellStatus == CellStatus.EnemyWCoin)
            {
                coinsLeft--;
                CoinsGained++;
            }
            if (cell.CellStatus == CellStatus.Enemy || cell.CellStatus == CellStatus.EnemyWCoin)
            {
                Status = GameStatus.Loss;
                Exit();
            }
            var oldCell = FlattenLevel.SingleOrDefault(c => c.CellStatus == CellStatus.Pacman);
            if (oldCell != null) oldCell.CellStatus = CellStatus.Empty;
            cell.CellStatus = CellStatus.Pacman;
            Pacman.Position = cell.Position;
        }

        bool WasKeyPressed(Keys key)
        {
            var keyboardState = Keyboard.GetState();
            if (keyboardState.IsKeyDown(key))
            {
                return true;
            }
            return false;
        }

        private LevelCell getCellClosestToPacman(LevelCell cell)
        {

            var neighbors = new List<LevelCell>()
                {
                    FlattenLevel.SingleOrDefault(c=>c.Position.X == cell.Position.X-1 && c.Position.Y == cell.Position.Y),
                    FlattenLevel.SingleOrDefault(c=>c.Position.X == cell.Position.X+1 && c.Position.Y == cell.Position.Y),
                    FlattenLevel.SingleOrDefault(c=>c.Position.X == cell.Position.X && c.Position.Y -1== cell.Position.Y),
                    FlattenLevel.SingleOrDefault(c=>c.Position.X == cell.Position.X && c.Position.Y +1== cell.Position.Y),
                };
            neighbors = neighbors.Where(n => n != null && n.CellStatus != CellStatus.Wall).ToList();
            var results = new List<double>();
            foreach (var neighbor in neighbors)
            {
                results.Add(System.Math.Pow(neighbor.Position.X - Pacman.Position.X, 2) + System.Math.Pow(neighbor.Position.Y - Pacman.Position.Y, 2));
            }
            return neighbors.ElementAt(results.IndexOf(results.OrderBy(r => r).First()));
        }

        private void initEnemies(System.Random rand, int numOfGhosts)
        {
            enemies = new();
            for (int i = 0; i < numOfGhosts; i++)
            {
                var possibleCells = FlattenLevel.Where(c => c.CellStatus != CellStatus.Wall && c.CellStatus != CellStatus.Pacman && c.CellStatus != CellStatus.Enemy && isFarFromPacman(c));
                var cell = possibleCells.ElementAt(rand.Next(possibleCells.Count()));

                EnemyType enemyType = EnemyType.common;
                if (i % 3 == 1)
                {
                    enemyType = EnemyType.strong;
                }
                if (i == 5)
                {
                    enemyType = EnemyType.king;
                }

                enemies.Add(new Enemy(this, enemyType)
                {
                    Position = new Point(cell.Position.X, cell.Position.Y),
                });
            }
        }

        private bool isFarFromPacman(LevelCell levelCell)
        {
            if (System.Math.Pow(levelCell.Position.X - Pacman.Position.X, 2) + System.Math.Pow(levelCell.Position.Y - Pacman.Position.Y, 2) < 25) return false;
            return true;
        }

        private void DrawLevel(int offset)
        {
            for (int i = 0; i < Level.Count; i++)
            {
                for (int j = 0; j < Level[i].Count; j++)
                {
                    var cell = Level[i][j];
                    switch (cell.CellStatus)
                    {
                        case CellStatus.Wall:
                            SpriteBatch.Draw(wallTexture, new Rectangle(offset + cell.Position.X * Cell.Side,
                                cell.Position.Y * Cell.Side,
                                Cell.Side,
                                Cell.Side), Color.White);
                            break;

                        case CellStatus.Coin:
                            SpriteBatch.Draw(cellTexture, new Rectangle(offset + cell.Position.X * Cell.Side,
                                cell.Position.Y * Cell.Side,
                                Cell.Side,
                                Cell.Side), Color.White);

                            SpriteBatch.Draw(coinTexture, new Rectangle(offset + cell.Position.X * Cell.Side + Cell.Side / 2 - 2,
                                cell.Position.Y * Cell.Side + Cell.Side / 2 - 2,
                                4,
                                4), Color.White);
                            break;

                        case CellStatus.Empty:
                            SpriteBatch.Draw(cellTexture, new Rectangle(offset + i * Cell.Side,
                                j * Cell.Side,
                                Cell.Side,
                                Cell.Side), Color.White);
                            break;

                        case CellStatus.Pacman:
                            SpriteBatch.Draw(pacmanTexture, new Rectangle(offset + i * Cell.Side,
                                j * Cell.Side,
                                Cell.Side,
                                Cell.Side), Color.White);
                            break;

                        default:
                            break;
                    }
                }
            }
        }
    }
}
