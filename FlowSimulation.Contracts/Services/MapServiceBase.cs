using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace FlowSimulation.Contracts.Services
{
    public abstract class MapServiceBase : ServiceBase
    {
        protected Enviroment.Map _map;
        protected List<Point> _mapCells;

        public MapServiceBase(Enviroment.Map map, List<Point> mapCells)
        {
            _map = map;
            _mapCells = mapCells;
        }

        public Enviroment.Map Map { get { return _map; } }
        public List<Point> MapCells { get { return _mapCells; } }
    }
}
