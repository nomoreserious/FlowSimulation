using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FlowSimulation.Helpers.Graph;
using System.Drawing;

namespace FlowSimulation.Enviroment.FindPathMethods
{
    public class DekstraAlgorithm<T> where T : IGraphNode<T>, ICloneable
    {
        private Graph<T, int> _graph;
        private T _begin, _end;
        private bool done;

        public DekstraAlgorithm(Graph<T, int> graph, T begin, T end)
        {
            _graph = graph;
            _begin = begin;
            _end = end;
            AlgoritmRun();
        }

        /// 
        /// Запуск алгоритма расчета
        /// 
        private void AlgoritmRun()
        {
            foreach (var node in _graph)
            {
                if (node.Equals(_begin))
                    node.Value = 0;
                else
                    node.Value = int.MaxValue;
                node.IsChecked = false;
                node.ParentNode = default(T);
            }
            OneStep(_begin);

            while (true)
            {
                //var now = DateTime.Now;
                var node = GetUncheckedNode(_begin);
                //Console.WriteLine((DateTime.Now - now).Ticks);
                if (node == null)
                {
                    break;
                }
                OneStep(node);
                if (done) break;

            }
            Console.WriteLine("Количество просмотренных вершин: " + _graph.Count(n => n.IsChecked));
        }

        /// <summary>
        /// Возвращает непройденную вершину с минимальным весом
        /// </summary>
        /// <param name="begin"></param>
        /// <returns></returns>
        private T GetUncheckedNode(T begin)
        {
            List<T> ucn = new List<T>();
            foreach (var node in _graph.Where(n => n.IsChecked).OrderBy(n => n.Value))
            {
                ucn.AddRange(_graph.GetNodesFrom(node).Where(n => !n.IsChecked && n.Value != int.MaxValue));
            }
            return ucn.OrderBy(n => n.Value).FirstOrDefault();
        }

        /// 
        /// Метод, делающий один шаг алгоритма. Принимает на вход вершину
        /// 
        public void OneStep(T point)
        {
            //foreach (T nextNode in Pred(beginpoint))
            foreach (var node in _graph.GetNodesFrom(point))
            {
                if (node.IsChecked == false)//не отмечена
                {
                    int newValue = point.Value + _graph.GetEdgeData(point,node);
                    if (node.Value > newValue)
                    {
                        node.Value = newValue;
                        node.ParentNode = point;
                    }
                }
            }
            point.IsChecked = true;
            if (point.Equals(_end))
                done = true;
        }

        public List<T> GetPath()
        {
            List<T> listOfpoints = new List<T>();

            T temp = _end;
            while (!temp.Equals(_begin))
            {
                listOfpoints.Insert(0, temp);
                if (temp.ParentNode == null)
                    return null;
                temp = temp.ParentNode;
            }
            listOfpoints.Insert(0, temp);
            return listOfpoints;
        }

        public IEnumerable<T> GetCloserNodes(Point p)
        {
            return _graph;
        }
    }
}
