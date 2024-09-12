using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Walls
{
    internal class Cell
    {
        public bool[] Walls { get; set; } = new bool[4];
        public bool Visited { get; set; }

        public Point Position { get; set; }
        public static int Side = 20;
    }
}
