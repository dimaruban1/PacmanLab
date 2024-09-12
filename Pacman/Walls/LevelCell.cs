using Microsoft.Xna.Framework;

namespace Walls
{
    public enum CellStatus
    {
        Wall = 0,
        Coin = 1,
        Empty = 3,
        Pacman = 4,
        Enemy = 5,
        EnemyWCoin = 6,
    }
    public class LevelCell
    {
        public Point Position { get; set; }
        public CellStatus CellStatus { get; set; }
    }
}
