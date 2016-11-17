using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FlowSimulation.Enviroment.Model;
using System.Drawing;

namespace FlowSimulation.Enviroment.FindPathMethods
{
    interface IPathFinder
    {
        #region Properties
        bool Stopped
        {
            get;
        }

        bool Diagonals
        {
            get;
            set;
        }

        bool HeavyDiagonals
        {
            get;
            set;
        }

        int HeuristicEstimate
        {
            get;
            set;
        }

        bool PunishChangeDirection
        {
            get;
            set;
        }

        bool ReopenCloseNodes
        {
            get;
            set;
        }

        bool TieBreaker
        {
            get;
            set;
        }

        int SearchLimit
        {
            get;
            set;
        }

        double CompletedTime
        {
            get;
            set;
        }

        bool DebugProgress
        {
            get;
            set;
        }

        bool DebugFoundPath
        {
            get;
            set;
        }
        #endregion

        List<PathFinderNode> FindPath(Point start, Point end);
    }
}
