using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace FlowSimulation.Enviroment.Model
{
    public struct Area
    {
        public Size size;
        public Point index;
        public Point location;

        public Area(int x, int y, int width, int height)
        {
            location = new Point(x, y);
            index = new Point((int)Math.Floor((double)x / Constants.AREA_SIZE), (int)Math.Floor((double)y / Constants.AREA_SIZE));
            size = new Size(width, height);
        }

        public bool CheckInPoint(Point p)
        {
            return p.X >= location.X && p.X < location.X + size.Width &&
                   p.Y >= location.Y && p.Y < location.Y + size.Height;
        }
    }
}
