using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Runtime.Serialization;
using QuickGraph.Algorithms;

namespace FlowSimulation.Enviroment.Model
{
    [Serializable]
    public class Layer : ISerializable
    {
        #region Private members
        private QuickGraph.UndirectedGraph<GraphNode, QuickGraph.Edge<GraphNode>> _patensyGraph;
        private Dictionary<QuickGraph.Edge<GraphNode>, double> _weightsDictionary;
        private readonly object _syncObject = new object();
        private int _searchWayAreaSize; 
        #endregion

        #region Ctors
        public Layer(string name, string maskSource, double scale, Dictionary<string, byte> maskInfo)
        {
            this.Name = name;
            this.MaskSource = maskSource;
            this.Scale = scale;
            this.MaskInfo = maskInfo;
        }

        public Layer(SerializationInfo info, StreamingContext context)
        {
            foreach (var item in info)
            {
                switch (item.Name)
                {
                    case "Name":
                        this.Name = (string)item.Value;
                        break;

                    case "Scale":
                        this.Scale = (double)item.Value;
                        break;

                    case "MaskInfo":
                        this.MaskInfo = (Dictionary<string, byte>)item.Value;
                        break;

                    case "MaskSource":
                        if (item.ObjectType.Equals(typeof(string)))
                        {
                            this.MaskSource = (string)item.Value;
                        }
                        else if (item.ObjectType.Equals(typeof(string[])))
                        {
                            string[] source = (string[])item.Value;
                            StringBuilder builder = new StringBuilder();
                            foreach (var line in source)
                            {
                                builder.AppendLine(line);
                            }
                            this.MaskSource = builder.ToString();
                        }
                        break;
                }
            }
        } 
        #endregion

        #region Public Properties
        /// <summary>
        /// Имя слоя
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Источник маски слоя
        /// </summary>
        public string MaskSource { get; private set; }

        /// <summary>
        /// Словарь значений маски слоя
        /// </summary>
        public Dictionary<string, byte> MaskInfo { get; private set; }

        /// <summary>
        /// Подложка слоя
        /// </summary>
        public System.Drawing.Bitmap Substrate { get; set; }

        /// <summary>
        /// Маска слоя
        /// </summary>
        public System.Drawing.Bitmap Mask { get; set; }

        /// <summary>
        /// Клеточная матрица слоя
        /// </summary>
        public Cell[,] Cells { get; set; }

        /// <summary>
        /// Масштаб оригинальной маски (px/m)
        /// </summary>
        public double Scale { get; private set; }

        /// <summary>
        /// Длина клеточной матрицы слоя
        /// </summary>
        public int Width
        {
            get
            {
                if (Cells == null)
                    throw new InvalidOperationException("Слой не проинициализирован");
                return Cells.GetLength(0);
            }
        }

        /// <summary>
        /// Ширина клеточной матрицы слоя
        /// </summary>
        public int Height
        {
            get
            {
                if (Cells == null)
                    throw new InvalidOperationException("Слой не проинициализирован");
                return Cells.GetLength(1);
            }
        }
        #endregion

        #region Public methods

        public Cell GetCell(int x, int y)
        {
            if (x < Cells.GetLength(0) && y < Cells.GetLength(1))
                return Cells[x, y];
            throw new IndexOutOfRangeException(string.Format("Ячейка в позиции {0},{1} не найдена", x, y));
        }

        public void SetCell(int x, int y, Cell cell)
        {
            if (x >= Cells.GetLength(0) && y >= Cells.GetLength(1))
                throw new IndexOutOfRangeException(string.Format("Ячейка в позиции {0},{1} не найдена", x, y));
            Cells[x, y] = cell;
        }

        public void Clear()
        {
            for (int i = 0; i < Cells.GetLength(0); i++)
            {
                for (int j = 0; j < Cells.GetLength(1); j++)
                {
                    Cells[i, j].HasAgent = false;
                    Cells[i, j].CurrentValue = Cells[i, j].StaticValue;
                }
            }
        }

        /// <summary>
        /// Попытка занять клетку
        /// </summary>
        /// <param name="position">индекс клетки</param>
        /// <param name="weight">Вес агента</param>
        /// <param name="useCompression">Уплотнение</param>
        /// <returns>true - удалось занять, false - не удалось занять, null - клетка временно заблокирована</returns>
        public bool? TryHoldPosition(Point position, double weight, bool useCompression = false)
        {
            lock (_syncObject)
            {
                var cell = this.Cells[position.X, position.Y];
                if (cell.HasAgent || cell.StaticValue == 0x00)
                    return false;
                if (cell.TemporarilyClosed)
                    return null;
                double vol = 1.0D - (double)cell.CurrentValue / byte.MaxValue;
                if (vol > weight)
                {
                    cell.CurrentValue += (byte)(byte.MaxValue * weight);
                    cell.HasAgent = true;
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Освобождение занятой клетки
        /// </summary>
        /// <param name="position">Позиция клетки</param>
        /// <param name="weight">Вес агента</param>
        public void ReleasePosition(Point position, double weight)
        {
            lock (_syncObject)
            {
                var cell = this.Cells[position.X, position.Y];
                cell.CurrentValue -= (byte)(byte.MaxValue * weight);
                cell.HasAgent = false;
            }
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Name", this.Name);
            info.AddValue("Scale", this.Scale);
            info.AddValue("MaskSource", this.MaskSource);
            info.AddValue("MaskInfo", this.MaskInfo);
        }

        /// <summary>
        /// Поиск пути к целевой путевой точке по клеткам, используя алгоритм A*
        /// </summary>
        /// <param name="position">Текущая позиция</param>
        /// <param name="layerId">Идентификатор слоя</param>
        /// <param name="targetWayPoint">Целевая путевая точка</param>
        /// <returns>Последовательность клеток</returns>
        public List<Point> GetWay(Point position, WayPoint targetWayPoint)
        {
            var target = GetCloasureEmptyPoint(position, targetWayPoint);
            if (ReferenceEquals(target, Point.Empty))
            {
                return null;
            }
            return this.GetWay(position, target);
        }

        public Point GetCloasureEmptyPoint(Point position, WayPoint targetWayPoint)
        {
            return targetWayPoint.Center;

            //for (int i = 0; i < targetWayPoint.Width; i++)
            //{
            //    for (int j = 0; j < targetWayPoint.Height; j++)
            //    {
            //        int x = targetWayPoint.X + i;
            //        int y = targetWayPoint.Y + i;
            //        if (this.Cells[x, y].StaticValue > 0x00 && !this.Cells[x, y].HasAgent)
            //        {
            //            return new Point(x, y);
            //        }
            //    }
            //}
            //return Point.Empty;
        }

        /// <summary>
        /// Поиск пути к целевой путевой точке по клеткам, используя алгоритм A*
        /// </summary>
        /// <param name="position">Текущая позиция</param>
        /// <param name="layerId">Идентификатор слоя</param>
        /// <param name="target">Целевая клетка</param>
        /// <returns>Последовательность клеток</returns>
        public List<Point> GetWay(Point position, Point target)
        {
            if (!IsPointInLayer(position))
            {
                throw new IndexOutOfRangeException(string.Format("Исходная точка ({0},{1}) вне слоя", position.X, position.Y));
            }

            if (!IsPointInLayer(target))
            {
                throw new IndexOutOfRangeException(string.Format("Целевая точка ({0},{1}) вне слоя", target.X, target.Y));
            }

            System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
            watch.Start();
            if (_patensyGraph.IsVerticesEmpty)
            {
                //Слой небольшой, используем только A*
                double wlog = Math.Log(this.Width, 2), hlog = Math.Log(this.Height, 2);

                var area = GetValues(0, 0, (int)Math.Pow(2, Math.Ceiling(wlog)), (int)Math.Pow(2, Math.Ceiling(hlog)));
                var way = GetWayAStar(area, position, target);

                watch.Stop();
                if (way != null)
                {
                    System.Diagnostics.Debug.WriteLine("Поиск пути A*: " + watch.ElapsedMilliseconds); 
                    return way.Select(p => new Point(p.X, p.Y)).ToList();
                }

                System.Diagnostics.Debug.WriteLine("Не удалось определить целевую точку для " + position + " - " + target);
                return null;
            }
            else
            {
                //Слой большой, используем промежуточный граф переходов
                var route = GetRoute(position, target);
                if (route != null)
                {
                    List<Point> way = new List<Point>();
                    foreach (var checkPoint in route)
                    {
                        var subway = GetWay(position, checkPoint);
                        if (subway == null)
                            break;
                        way.AddRange(subway);
                        if (way.Count > 2)
                        {
                            break;
                        }
                        if (way.Count > 0)
                        {
                            position = way.Last();
                        }
                    }
                    watch.Stop();
                    if (way.Count == 0 || (way.Count == 1 && way[0] == position))
                    {
                        System.Diagnostics.Debug.WriteLine("Поиск по графу не дал результатов");
                        return null;
                    }
                    System.Diagnostics.Debug.WriteLine("Поиск пути по графу и A*:" + watch.ElapsedMilliseconds);
                    return way;
                }
                else
                {
                    watch.Stop();
                    System.Diagnostics.Debug.WriteLine("Поиск по графу не дал результатов"); 
                    return null;
                }
            }
        }

        internal void CreatePatensyGraph(int layerId)
        {
            _patensyGraph = new QuickGraph.UndirectedGraph<GraphNode, QuickGraph.Edge<GraphNode>>(false);
            _weightsDictionary = new Dictionary<QuickGraph.Edge<GraphNode>, double>();

            if (Math.Max(this.Width, this.Height) > 128)
            {
                double power = Math.Log((double)this.Width * this.Height / 1500, 2) / 2;
                power = Math.Ceiling(power);
                if (power > 7)
                {
                    power = 7;
                }
                if (power < 4)
                {
                    power = 4;
                }
                _searchWayAreaSize = (int)Math.Pow(2, power);
            }
            else
            {
                //Дополнитеный уровень иерархии при поиске не используется
                _searchWayAreaSize = 0;
                return;
            }

            //Создаем вершины
            for (int j = 0; j < Math.Ceiling((double)this.Height / _searchWayAreaSize); j++)
            {
                for (int i = 0; i < Math.Ceiling((double)this.Width / _searchWayAreaSize); i++)
                {
                    if ((j + 1) * _searchWayAreaSize < this.Height)
                    {
                        int width = (this.Width - i * _searchWayAreaSize) >= _searchWayAreaSize ? _searchWayAreaSize : this.Width - i * _searchWayAreaSize;

                        var cells = this.GetCells(i * _searchWayAreaSize, (j + 1) * _searchWayAreaSize - 1, width, 2);
                        GraphNode node = null;
                        for (int k = 0; k < width; k++)
                        {
                            //if ((cells[k, 0].StaticValue == 1 || cells[k, 1].StaticValue == 1) && cells[k, 0].StaticValue != 0 && cells[k, 1].StaticValue != 0)
                            if (cells[k, 0].StaticValue > 0x00 || cells[k, 1].StaticValue > 0x00)
                            {
                                if (node == null)
                                {
                                    node = new GraphNode(i, j, i, j + 1)
                                    {
                                        SourceWP = new WayPoint(i * _searchWayAreaSize + k, (j + 1) * _searchWayAreaSize - 1, layerId, 1, 1),
                                        TargetWP = new WayPoint(i * _searchWayAreaSize + k, (j + 1) * _searchWayAreaSize, layerId, 1, 1),
                                        Volume = cells[k, 0].StaticValue < cells[k, 1].StaticValue ? cells[k, 0].StaticValue : cells[k, 1].StaticValue
                                    };
                                }
                                else
                                {
                                    node.SourceWP.Width++;
                                    node.TargetWP.Width++;
                                    node.Volume += cells[k, 0].StaticValue < cells[k, 1].StaticValue ? cells[k, 0].StaticValue : cells[k, 1].StaticValue;
                                }
                            }
                            else
                            {
                                if (node != null)
                                {
                                    _patensyGraph.AddVertex(node);
                                    node = null;
                                }
                            }
                        }
                        if (node != null)
                        {
                            _patensyGraph.AddVertex(node);
                            node = null;
                        }
                    }
                    if ((i + 1) * _searchWayAreaSize < this.Width)
                    {
                        int height = (this.Height - j * _searchWayAreaSize) >= _searchWayAreaSize ? _searchWayAreaSize : this.Height - j * _searchWayAreaSize;

                        var cells = GetCells((i + 1) * _searchWayAreaSize - 1, j * _searchWayAreaSize, 2, height);
                        GraphNode node = null;
                        for (int k = 0; k < height; k++)
                        {
                            //if ((cells[0, k].StaticValue == 1 || cells[1, k].StaticValue == 1) && cells[0, k].StaticValue != 0 && cells[1, k].StaticValue != 0)
                            if (cells[0, k].StaticValue > 0x00 || cells[1, k].StaticValue > 0x00)
                            {
                                if (node == null)
                                {
                                    node = new GraphNode(i, j, i + 1, j)
                                    {
                                        SourceWP = new WayPoint((i + 1) * _searchWayAreaSize - 1, j * _searchWayAreaSize + k, layerId, 1, 1),
                                        TargetWP = new WayPoint((i + 1) * _searchWayAreaSize, j * _searchWayAreaSize + k, layerId, 1, 1),
                                        Volume = cells[0, k].StaticValue < cells[1, k].StaticValue ? cells[0, k].StaticValue : cells[1, k].StaticValue
                                    };
                                }
                                else
                                {
                                    node.SourceWP.Height++;
                                    node.TargetWP.Height++;
                                    node.Volume += cells[0, k].StaticValue < cells[1, k].StaticValue ? cells[0, k].StaticValue : cells[1, k].StaticValue;
                                }
                            }
                            else
                            {
                                if (node != null)
                                {
                                    _patensyGraph.AddVertex(node);
                                    node = null;
                                }
                            }
                        }
                        if (node != null)
                        {
                            _patensyGraph.AddVertex(node);
                            node = null;
                        }
                    }
                }
            }

            //Создаем ребра
            for (int j = 0; j < this.Height / _searchWayAreaSize + 1; j++)
            {
                for (int i = 0; i < this.Width / _searchWayAreaSize + 1; i++)
                {
                    Point areaIndex = new Point(i, j);
                    List<GraphNode> nodes = new List<GraphNode>(from node in _patensyGraph.Vertices
                                                                where (node.SourceArea == areaIndex || node.TargetArea == areaIndex)
                                                                select node);
                    while (nodes.Count() > 1)
                    {
                        var first = nodes.FirstOrDefault();
                        WayPoint firstWp = first.SourceArea == areaIndex ? first.SourceWP : first.TargetWP;

                        nodes.Remove(first);
                        foreach (var node in nodes)
                        {
                            WayPoint nextWp = node.SourceArea == areaIndex ? node.SourceWP : node.TargetWP;
                            //Point toP = node.Center;
                            //if (first.FromArea == areaIndex)
                            //{
                            //    if (first.FromArea.X == first.ToArea.X)
                            //    {
                            //        fromP.Offset(0, -1);
                            //    }
                            //    else
                            //    {
                            //        fromP.Offset(-1, 0);
                            //    }
                            //}
                            //if (node.FromArea == areaIndex)
                            //{
                            //    if (node.FromArea.X == node.ToArea.X)
                            //    {
                            //        toP.Offset(0, -1);
                            //    }
                            //    else
                            //    {
                            //        toP.Offset(-1, 0);
                            //    }
                            //}

                            int routeLenght;
                            if (HasRoute(first, node, out routeLenght))
                            {
                                var forwardEgde = new QuickGraph.Edge<GraphNode>(first, node);
                                _patensyGraph.AddEdge(forwardEgde);
                                _weightsDictionary.Add(forwardEgde, routeLenght);

                                //var backwardEgde = new QuickGraph.Edge<GraphNode>(node, first);
                                //_patensyGraph.AddEdge(backwardEgde);
                                //_weightsDictionary.Add(backwardEgde, routeLenght);
                            }
                        }
                    }
                }
            }
        } 
        #endregion

        #region Private methods
        private Cell[,] GetCells(int x, int y, int width, int height)
        {
            if (x + width <= Cells.GetLength(0) && y + height <= Cells.GetLength(1))
            {
                Cell[,] temp = new Cell[width, height];
                for (int i = 0; i < width; i++)
                {
                    for (int j = 0; j < height; j++)
                    {
                        temp[i, j] = Cells[x + i, y + j];
                    }
                }
                return temp;
            }
            throw new IndexOutOfRangeException(string.Format("Заданное поле выходит за пределы слоя"));
        }

        /// <summary>
        /// Возвращает байтовый массив значений всего слоя
        /// </summary>
        private byte[,] GetValues()
        {
            return GetValues(0, 0, Width, Height);
        }

        /// <summary>
        /// Возвращает байтовый массив значений заданного фрагмента слоя или null, если x или y меньше нуля
        /// </summary>
        /// <remarks>То что выходит за границы слоя, помечается как закрытая область</remarks>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        private byte[,] GetValues(int x, int y, int width, int height)
        {
            if (x >= 0 && y >= 0)
            {
                byte[,] temp = new byte[width, height];
                for (int i = 0; i < width; i++)
                {
                    for (int j = 0; j < height; j++)
                    {
                        if (x + i < Width && y + j < Height)
                        {
                            temp[i, j] = Cells[x + i, y + j].CurrentValue;
                        }
                        else
                        {
                            temp[i, j] = 0x00;
                        }
                    }
                }
                return temp;
            }
            return null;
        }

        /// <summary>
        /// Проверяет вхождение координат точки в текущий слой
        /// </summary>
        private bool IsPointInLayer(Point point)
        {
            if (Cells == null)
                return false;
            return point.X < Cells.GetLength(0) && point.Y < Cells.GetLength(1);
        }

        private List<FindPathMethods.PathFinderNode> GetWayAStar(byte[,] frame, Point source, Point target)
        {
            var rect = new Rectangle(0, 0, frame.GetLength(0), frame.GetLength(1));
            if (!rect.Contains(source) || !rect.Contains(target))
            {
                //точка начала или окончания маршрута не входит в массив;
                return null;
            }

            FindPathMethods.IPathFinder finder;
            double wlog = Math.Log(frame.GetLength(0), 2), hlog = Math.Log(frame.GetLength(1), 2);
            if (wlog == (int)wlog && hlog == (int)hlog)
            {
                finder = new FindPathMethods.PathFinderFast(frame);
            }
            else
            {
                finder = new FindPathMethods.PathFinder(frame);
            }

            finder.Diagonals = true;
            finder.SearchLimit = Constants.SEARCH_LIMIT;
            finder.HeuristicEstimate = 3;

            try
            {
                var path = finder.FindPath(source, target);
                if (path != null)
                {
                    path.Reverse();
                }
                return path;
            }
            catch (IndexOutOfRangeException)
            {
                Console.WriteLine(string.Format("Ошибка алгоритма A* ({0},{1}) - ({2},{3})", source.X, source.Y, target.X, target.Y));
                return null;
            }
        }

        private List<GraphNode> GetRoute(Point from, Point to)
        {
            lock (_syncObject)
            {
                var x = (int)Math.Floor((double)from.X / _searchWayAreaSize);
                var y = (int)Math.Floor((double)from.Y / _searchWayAreaSize);
                Point startAreaIndex = new Point(x, y);

                var startNode = new GraphNode(startAreaIndex, startAreaIndex)
                {
                    SourceWP = new WayPoint(from.X, from.Y),
                    TargetWP = new WayPoint(from.X, from.Y)
                };

                x = (int)Math.Floor((double)to.X / _searchWayAreaSize);
                y = (int)Math.Floor((double)to.Y / _searchWayAreaSize);
                Point endAreaIndex = new Point(x, y);

                var endNode = new GraphNode(endAreaIndex, endAreaIndex)
                {
                    SourceWP = new WayPoint(to.X, to.Y),
                    TargetWP = new WayPoint(to.X, to.Y)
                };

                try
                {
                    _patensyGraph.AddVertex(startNode);

                    IEnumerable<GraphNode> nodesAroundStartNode =
                        _patensyGraph.Vertices.Where(n => n.SourceArea == startAreaIndex || n.TargetArea == startAreaIndex);

                    bool hasRouteFromStart = false;
                    foreach (var node in nodesAroundStartNode)
                    {
                        int routeLenght;
                        if (HasRoute(startNode, node, out routeLenght))
                        {
                            var edgeFromStart = new QuickGraph.Edge<GraphNode>(startNode, node);
                            _patensyGraph.AddEdge(edgeFromStart);
                            _weightsDictionary.Add(edgeFromStart, routeLenght);
                            hasRouteFromStart = true;
                        }
                    }

                    if (!hasRouteFromStart)
                    {
                        return null;
                    }

                    _patensyGraph.AddVertex(endNode);

                    IEnumerable<GraphNode> nodesAroundEndNode =
                        _patensyGraph.Vertices.Where(n => n.SourceArea == endAreaIndex || n.TargetArea == endAreaIndex);

                    bool hasRouteToEnd = false;
                    foreach (var node in nodesAroundEndNode)
                    {
                        int routeLenght;
                        if (HasRoute(node, endNode, out routeLenght))
                        {
                            var edgeToEnd = new QuickGraph.Edge<GraphNode>(node, endNode);
                            _patensyGraph.AddEdge(edgeToEnd);
                            _weightsDictionary.Add(edgeToEnd, routeLenght);
                            hasRouteToEnd = true;
                        }
                    }

                    if (!hasRouteToEnd)
                    {
                        return null;
                    }

                    if (_weightsDictionary.Count < _patensyGraph.EdgeCount)
                    {
                        throw new InvalidOperationException(string.Format("Не определен вес {0} ребер", _patensyGraph.EdgeCount - _weightsDictionary.Count));
                    }

                    Func<QuickGraph.Edge<GraphNode>, double> func =
                        AlgorithmExtensions.GetIndexer<QuickGraph.Edge<GraphNode>, double>(_weightsDictionary);

                    IEnumerable<QuickGraph.Edge<GraphNode>> path = null;
                    var tryFunc = _patensyGraph.ShortestPathsDijkstra(func, startNode);
                    bool result = tryFunc(endNode, out path);

                    if (result)
                    {
                        List<GraphNode> route = new List<GraphNode>();
                        foreach (var edge in path)
                        {
                            if (route.Count == 0)
                            {
                                route.Add(edge.Target);
                            }
                            else
                            {
                                if (route.Last().Equals(edge.Target))
                                {
                                    route.Add(edge.Source);
                                }
                                else
                                {
                                    route.Add(edge.Target);
                                }
                            }
                        }
                        return route;
                    }
                    else
                    {
                        return null;
                    }
                }
                finally
                {
                    foreach (var edge in _patensyGraph.Edges.Where(e => e.Source.Equals(startNode)))
                    {
                        _weightsDictionary.Remove(edge);
                    }
                    foreach (var edge in _patensyGraph.Edges.Where(e => e.Target.Equals(endNode)))
                    {
                        _weightsDictionary.Remove(edge);
                    }

                    _patensyGraph.RemoveVertex(startNode);
                    _patensyGraph.RemoveVertex(endNode);
                }
            }
        }

        private List<Point> GetWay(Point sourcePoint, GraphNode targetNode)
        {
            Point targetPoint;
            Point sourceArea = new Point(sourcePoint.X / _searchWayAreaSize, sourcePoint.Y / _searchWayAreaSize);
            int x = sourceArea.X * _searchWayAreaSize, y = sourceArea.Y * _searchWayAreaSize, w = 1, h = 1;
            if (sourceArea == targetNode.SourceArea)
            {
                targetPoint = GetCloasureEmptyPoint(sourcePoint, targetNode.SourceWP);
            }
            else if (sourceArea == targetNode.TargetArea)
            {
                targetPoint = GetCloasureEmptyPoint(sourcePoint, targetNode.TargetWP);
            }
            else
            {
                w = 2;
                h = 2;
                if (sourceArea.X > targetNode.SourceArea.X && sourceArea.X > targetNode.TargetArea.X)
                {
                    x -= _searchWayAreaSize;
                    if (targetNode.SourceArea.X == sourceArea.X - 1 && targetNode.SourceArea.Y == sourceArea.Y)
                        targetPoint = GetCloasureEmptyPoint(sourcePoint, targetNode.SourceWP);
                    else
                        targetPoint = GetCloasureEmptyPoint(sourcePoint, targetNode.TargetWP);
                }
                if (sourceArea.Y > targetNode.SourceArea.Y && sourceArea.Y > targetNode.TargetArea.Y)
                {
                    y -= _searchWayAreaSize;
                    if (targetNode.SourceArea.X == sourceArea.X && targetNode.SourceArea.Y == sourceArea.Y - 1)
                        targetPoint = GetCloasureEmptyPoint(sourcePoint, targetNode.SourceWP);
                    else
                        targetPoint = GetCloasureEmptyPoint(sourcePoint, targetNode.TargetWP);
                }
                else
                {
                    if (targetNode.TargetArea.X > sourceArea.X)
                    {
                        if (targetNode.SourceArea.X == sourceArea.X + 1 && targetNode.SourceArea.Y == sourceArea.Y)
                            targetPoint = GetCloasureEmptyPoint(sourcePoint, targetNode.SourceWP);
                        else
                            targetPoint = GetCloasureEmptyPoint(sourcePoint, targetNode.TargetWP);
                    }
                    else
                    {
                        if (targetNode.SourceArea.X == sourceArea.X && targetNode.SourceArea.Y == sourceArea.Y + 1)
                            targetPoint = GetCloasureEmptyPoint(sourcePoint, targetNode.SourceWP);
                        else
                            targetPoint = GetCloasureEmptyPoint(sourcePoint, targetNode.TargetWP);
                    }
                }
            }

            if (ReferenceEquals(targetPoint, Point.Empty))
            {
                return null;
            }

            if (sourcePoint == targetPoint)
            {
                return new List<Point>() { sourcePoint };
            }

            int width = _searchWayAreaSize * w;
            int height = _searchWayAreaSize * h;

            var area = GetValues(x, y, width, height);
            area[sourcePoint.X - x, sourcePoint.Y - y] = 0x01;

            var way = GetWayAStar(area, new Point(sourcePoint.X - x, sourcePoint.Y - y), new Point(targetPoint.X - x, targetPoint.Y - y));

            if (way != null)
            {
                return way.Select(p => new Point(p.X + x, p.Y + y)).ToList();
            }

            return null;
        }

        private bool HasRoute(GraphNode source, GraphNode target, out int routeLenght)
        {
            Point areaIndex;
            WayPoint fromWP, toWP;
            if (source.SourceArea == target.SourceArea)
            {
                areaIndex = source.SourceArea;
                fromWP = source.SourceWP;
                toWP = target.SourceWP;
            }
            else if (source.SourceArea == target.TargetArea)
            {
                areaIndex = source.SourceArea;
                fromWP = source.SourceWP;
                toWP = target.TargetWP;
            }
            else if (source.TargetArea == target.SourceArea)
            {
                areaIndex = source.TargetArea;
                fromWP = source.TargetWP;
                toWP = target.SourceWP;
            }
            else if (source.TargetArea == target.TargetArea)
            {
                areaIndex = source.TargetArea;
                fromWP = source.TargetWP;
                toWP = target.TargetWP;
            }
            else
            {
                routeLenght = 0;
                return false;
            }

            int x = areaIndex.X * _searchWayAreaSize;
            int y = areaIndex.Y * _searchWayAreaSize;

            var p1 = new Point(fromWP.Center.X - x, fromWP.Center.Y - y);
            var p2 = new Point(toWP.Center.X - x, toWP.Center.Y - y);
            var area = GetValues(x, y, _searchWayAreaSize, _searchWayAreaSize);
            area[p1.X, p1.Y] = 0x01;

            var pointList = GetWayAStar(area, p1, p2);

            if (pointList != null)
            {
                routeLenght = pointList.Count;
                return true;
            }
            else
            {
                routeLenght = 0;
                return false;
            }
        } 
        #endregion
    }
}
