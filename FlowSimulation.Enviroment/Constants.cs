using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FlowSimulation.Enviroment
{
    public class Constants
    {
        public const double CELL_SIZE = 0.4D;
        public const double CELL_VOLUME = CELL_SIZE * CELL_SIZE;

        //internal const int AREA_SIZE = 16;
        //internal const bool DIAGONALS = false;
        //internal const bool PUNISH_CHANGE_DIRECTION = false;
        internal const int SEARCH_LIMIT = 10000;
        //internal const int HEURISTIC_ESTIMATE = 2;
    }
}
