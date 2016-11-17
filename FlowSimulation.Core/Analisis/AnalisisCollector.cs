using System;
using System.Collections.Generic;
using System.Text;
using System.Data;

namespace FlowSimulation.Analisis
{
    public class AnalisisCollector
    {
        public ushort[,] PassengerDensity { get; set; }

        private DataTable agentsCount;
        private DataTable agentsInputOutput;
        private DataTable agentsCountByGroup;

        public List<double> routeLenght;
        public List<uint> routeTime;

        internal AnalisisCollector()
        {

        }

        internal AnalisisCollector(int mapWidth, int mapHeight, int groupCount)
        {
            PassengerDensity = new ushort[mapWidth, mapHeight];

            agentsInputOutput = new DataTable();
            agentsInputOutput.Columns.Add(new DataColumn("date", typeof(TimeSpan)));
            agentsInputOutput.Columns.Add(new DataColumn("in", typeof(int)));
            agentsInputOutput.Columns.Add(new DataColumn("out", typeof(int)));

            agentsCount = new DataTable();
            agentsCount.Columns.Add(new DataColumn("date", typeof(TimeSpan)));
            agentsCount.Columns.Add(new DataColumn("value", typeof(int)));
            
            agentsCountByGroup = new DataTable();
            agentsCountByGroup.Columns.Add(new DataColumn("date", typeof(TimeSpan)));

            for (int i = 0; i < groupCount; i++)
            {
                agentsCountByGroup.Columns.Add(new DataColumn("value" + (i + 1), typeof(int)));
            }
            routeLenght = new List<double>();
            routeTime = new List<uint>();
        }

        internal void AddAgentsCount(TimeSpan date, int value)
        {
            agentsCount.Rows.Add(date, value);
        }

        internal DataTable GetAgentsCount()
        {
            return agentsCount;
        }

        internal DataTable GetAgentInputOutput()
        {
            return agentsInputOutput;
        }

        internal void AddAgentsCountByGroup(TimeSpan date, int[] values)
        {
            agentsCountByGroup.Rows.Add(agentsCountByGroup.NewRow());
            agentsCountByGroup.Rows[agentsCountByGroup.Rows.Count - 1][0] = date;
            for (int i = 0; i < values.Length; i++)
            {
                agentsCountByGroup.Rows[agentsCountByGroup.Rows.Count - 1][i + 1] = values[i];
            }
        }

        internal DataTable GetAgentsCountByGroup()
        {
            return agentsCountByGroup;
        }

        internal void AddAgentInputOutput(TimeSpan date, int _in, int _out)
        {
            agentsInputOutput.Rows.Add(date, _in, _out);
        }
    }
}
