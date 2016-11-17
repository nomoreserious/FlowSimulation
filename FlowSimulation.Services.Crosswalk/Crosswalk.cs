using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FlowSimulation.Contracts.Services;
using System.Drawing;

namespace FlowSimulation.Services.Crosswalk
{
    public class Crosswalk : MapServiceBase
    {
        private int _openInterval = 1;
        private int _closeInterval = 1;

        private int _couter = 0;
        private bool _isClosed = true;

        public Crosswalk(Enviroment.Map map, List<Point> mapCells)
            : base(map, mapCells)
        {
            foreach (var cell in mapCells)
            {
                map[0].SetCell(cell.X, cell.Y, new Enviroment.Model.Cell(0x01) { TemporarilyClosed = true });
            }
        }

        public override void DoStep(double step_interval)
        {
            _couter++;
            if (_isClosed && _closeInterval == _couter)
            {
                ChangeValues(false);
                _isClosed = false;
                _couter = 1;
            }
            if (!_isClosed && _openInterval == _couter)
            {
                ChangeValues(true);
                _isClosed = true;
                _couter = 1;
            }
        }

        private void ChangeValues(bool closed)
        {
            foreach (var cell in _mapCells)
            {
                _map[0].Cells[cell.X, cell.Y].TemporarilyClosed = closed;
            }
        }

        public Dictionary<string, Contracts.Configuration.ParamDescriptor> CreateSettings()
        {
            var dict = new Dictionary<string, Contracts.Configuration.ParamDescriptor>();
            dict.Add("open_interval", new Contracts.Configuration.ParamDescriptor("open_interval", "Время открытия","", 10));
            dict.Add("close_interval", new Contracts.Configuration.ParamDescriptor("close_interval", "Время закрытия","", 10));
            return dict;
        }

        public override void Initialize(Dictionary<string, object> settings)
        {
            _openInterval = (int)settings["open_interval"];
            _closeInterval = (int)settings["close_interval"];
        }
    }
}
