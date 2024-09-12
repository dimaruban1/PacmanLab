using Microsoft.Xna.Framework;

namespace Entities
{
    public class Pacman
    {
        public Point Position { get; set; }
        public double Speed { get; set; }
    }
    public enum Action
    {
        Stop = 4,
        Left = 0,
        Up = 1,
        Right = 2,
        Down = 3,
    }
}
