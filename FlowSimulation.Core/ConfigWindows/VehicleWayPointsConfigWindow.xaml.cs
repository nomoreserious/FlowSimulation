using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using FlowSimulation.Agents;
using FlowSimulation.SimulationScenario;
using FlowSimulation.Map.Model;
using FlowSimulation.Helpers.Graph;

namespace FlowSimulation.ConfigWindows
{
    /// <summary>
    /// Логика взаимодействия для wndWayPointsConfig.xaml
    /// </summary>
    public partial class VehicleWayPointsConfigWindow : Window
    {
        private Scenario scena;
        private double _zoom;

        private Graph<WayPoint, PathFigure> RoadGraph;
        private List<VertexVisual> VertexVisualsList;
        private List<EdgeVisual> EdgeVisualsList;

        private double Zoom
        {
            get { return _zoom; }
            set
            {
                double old_zoom = _zoom;
                _zoom = value;
                if (scena != null)
                {
                    ZoomMap(old_zoom);
                }
            }
        }

        public VehicleWayPointsConfigWindow(Graph<WayPoint, PathFigure> roadGraph)
        {
            VertexVisualsList = new List<VertexVisual>();
            EdgeVisualsList = new List<EdgeVisual>();
            RoadGraph = roadGraph;
            InitializeComponent();

            scena = (Application.Current.MainWindow as MainWindow).Scena;
            Zoom = 4.0;
            PaintMap();
            lblMapSize.Content = string.Format("{0} x {1}", Math.Round(pnlMap.Width / Zoom), Math.Round(pnlMap.Height / Zoom));
            
            foreach (var edge in RoadGraph.Edges)
            {
                EdgeVisual ev = new EdgeVisual(edge.Data)
                {
                    NodeFrom = edge.Start,
                    NodeTo = edge.End,
                    Zoom = Zoom
                };
                EdgeVisualsList.Add(ev);
                pnlMap.Children.Add(ev);
            }

            foreach (var node in RoadGraph.Nodes)
            {
                int in_count = 0, out_count = 0;
                foreach (var edge in RoadGraph.GetEdgesTo(node))
                {
                    in_count++;
                }
                foreach (var edge in RoadGraph.GetEdgesFrom(node))
                {
                    out_count++;
                }

                VertexVisual vv = new VertexVisual(node)
                {
                    Number = (VertexVisualsList.Count + 1).ToString(),
                    InCount = in_count,
                    OutCount = out_count,
                    Zoom = Zoom
                };
                vv.Node.Number = int.Parse(vv.Number);
                if (in_count != 0)
                {
                    cbTo.Items.Add(vv.Node);
                }
                if (out_count != 0)
                {
                    cbFrom.Items.Add(vv.Node);
                }
                VertexVisualsList.Add(vv);
                pnlMap.Children.Add(vv);
            }
        }

        private void ZoomMap(double oldZoom)
        {
            pnlMap.Width = scena.map.GetMap().GetLength(0) * Zoom;
            pnlMap.Height = scena.map.GetMap().GetLength(1) * Zoom;

            for (int i = 0; i < VertexVisualsList.Count; i++)
            {
                VertexVisualsList[i].Zoom = Zoom;
            }
            for (int i = 0; i < EdgeVisualsList.Count; i++)
            {
                EdgeVisualsList[i].Zoom = Zoom;
            }
            for (int i = 0; i < pnlMap.Children.Count; i++)
            {
                if (pnlMap.Children[i] is Ellipse)
                {
                    Ellipse el = pnlMap.Children[i] as Ellipse;
                    el.SetValue(Canvas.TopProperty, (double)el.GetValue(Canvas.TopProperty) / oldZoom * Zoom);
                    el.SetValue(Canvas.LeftProperty, (double)el.GetValue(Canvas.LeftProperty) / oldZoom * Zoom);
                }
            }
        }

        private void PaintMap()
        {
            pnlMap.Width = scena.map.GetMap().GetLength(0) * Zoom;
            pnlMap.Height = scena.map.GetMap().GetLength(1) * Zoom;
            if (System.IO.File.Exists(Properties.Settings.Default.ScenarioPath + "image.jpg"))
            {
                pnlMap.Background = new ImageBrush(new System.Windows.Media.Imaging.BitmapImage(new Uri(Properties.Settings.Default.ScenarioPath + "image.jpg")));
            }
            else
            {
                pnlMap.Background = new ImageBrush(scena.Image);
            }
        }

        private void Grid_MouseMove(object sender, MouseEventArgs e)
        {
            lblMouseLocation.Content = string.Format("{0}, {1}", Math.Round(e.GetPosition(pnlMap).X / Zoom), Math.Round(e.GetPosition(pnlMap).Y / Zoom));
        }

        private void Grid_MouseLeave(object sender, MouseEventArgs e)
        {
            lblMouseLocation.Content = "";
        }

        private void sliderZoom_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Zoom = e.NewValue;
            lblZoomPersent.Content = Math.Round(Zoom * 25).ToString() + "%";
        }

        private void pnlMap_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (e.Source is VertexVisual)
            {
                VertexVisual vv = e.Source as VertexVisual;
                WayPoint wp = vv.Node;
                wp.Number = int.Parse(vv.Number);
                if (lvWPItems.Items.Count != 0)
                {
                    WayPoint lastWP = (lvWPItems.Items[lvWPItems.Items.Count - 1] as WayPoint);
                    if (lastWP == wp)
                    {
                        tbInfo.Text = "Граф дорожной сети не поддерживает рефлексию";
                        return;
                    }
                    bool linked = false;
                    foreach (var node in RoadGraph.GetNodesFrom(lastWP))
                    {
                        if (node == wp)
                        {
                            linked = true;
                        }
                    }
                    if (!linked)
                    {
                        tbInfo.Text = "Нет связи с предыдущей точкой";
                        return;
                    }
                }
                lvWPItems.Items.Add(wp);
                ChangeCBFrom();
                tbInfo.Text = "Маршрутная точка добавлена";
            }
        }

        private void ChangeCBFrom()
        {
            if (lvWPItems.Items.Count == 0)
            {
                cbFrom.IsEnabled = true;
                btnCreateWay.IsEnabled = true;
            }
            else
            {
                cbFrom.IsEnabled = false;
                WayPoint lastWP = (lvWPItems.Items[lvWPItems.Items.Count - 1] as WayPoint);
                if (cbFrom.Items.Contains(lastWP))
                {
                    cbFrom.SelectedItem = lastWP;
                    btnCreateWay.IsEnabled = true;
                }
                else
                {
                    btnCreateWay.IsEnabled = false;
                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void CreateWay_Click(object sender, RoutedEventArgs e)
        {
            if(cbFrom.SelectedIndex==-1 || cbTo.SelectedIndex==-1)
            {
                tbInfo.Text = "Укажите начальную и конечную точки";
                return;
            }
            WayPoint from = (cbFrom.SelectedItem as WayPoint);
            WayPoint to = (cbTo.SelectedItem as WayPoint);
            RoadGraph.ViewedNodes.Clear();
            List<WayPoint> wpList = RoadGraph.GetNodesWay(from, to);
            if (wpList.Count == 0)
            {
                tbInfo.Text = "Не удалось проложить маршрут между выбранными точками";
                return;
            }
            for (int i = 0; i < wpList.Count; i++)
            {
                if (cbFrom.IsEnabled == false && i == 0)
                {
                    continue;
                }
                lvWPItems.Items.Add(wpList[i]);
            }
            tbInfo.Text = string.Format("Маршрут между точками {0} и {1} добавлен", from.Number, to.Number);
            ChangeCBFrom();
        }

        private void ButtonRemove_Click(object sender, RoutedEventArgs e)
        {
            lvWPItems.Items.Remove(VertexVisualsList.Find(delegate(VertexVisual vv) { return int.Parse(vv.Number) == (int)(sender as Button).Tag; }).Node);
            ChangeCBFrom();
        }

        internal List<WayPoint> GetWayPointsList()
        {
            List<WayPoint> wpl = new List<WayPoint>();
            for (int i = 0; i < lvWPItems.Items.Count; i++)
            {
                wpl.Add(lvWPItems.Items[i] as WayPoint);
            }
            return wpl;
        }
    }
}
