using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FlowSimulation.Map.Model;

namespace FlowSimulation.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            Map.IO.MapReader reader = new Map.IO.MapReader(@"F:\Source\FlowSimulation\FlowSimulation\bin\Scenarios\FirstRealySaved\Map.svg");
            reader.Read();
            Map.Model.Map.Instance.CreatePatensyGraph(8);
            //var map = Map.Model.Map.Instance.GetMapLayer(0);
            //for(int i =0;i<map.GetLength(0);i++)
            //{
            //    Console.Write(map[i, 60].Volume + " ");
            //}
            Console.ReadLine();
        }
    }
}
