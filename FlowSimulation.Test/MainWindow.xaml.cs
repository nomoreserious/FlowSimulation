using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Drawing;
using System.IO;
using FlowSimulation.Enviroment;
using FlowSimulation.Enviroment.IO;
using FlowSimulation.Helpers.Timer;
using FlowSimulation.Contracts.Services;
using System.Windows.Threading;
using FlowSimulation.Contracts.Agents;

namespace FlowSimulation.Test
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            //PortTest pt = new PortTest();
            //ExportToPng(new Uri(@"C:\saved.png", UriKind.Absolute), pt.svg2985);
            Paint();
        }

        public void ExportToPng(Uri path, Canvas surface)
        {
            if (path == null)
                return;

            // Save current canvas transform
            Transform transform = surface.LayoutTransform;
            // reset current transform (in case it is scaled or rotated)
            surface.LayoutTransform = null;

            // Get the size of canvas
            System.Windows.Size size = new System.Windows.Size(surface.Width, surface.Height);
            // Measure and arrange the surface
            // VERY IMPORTANT
            surface.Measure(size);
            surface.Arrange(new Rect(size));

            // Create a render bitmap and push the surface to it
            RenderTargetBitmap renderBitmap =
              new RenderTargetBitmap(
                (int)size.Width,
                (int)size.Height,
                96d,
                96d,
                PixelFormats.Pbgra32);
            renderBitmap.Render(surface);

            // Create a file stream for saving image
            using (FileStream outStream = new FileStream(path.LocalPath, FileMode.Create))
            {
                // Use png encoder for our data
                PngBitmapEncoder encoder = new PngBitmapEncoder();
                // push the rendered bitmap to it
                encoder.Frames.Add(BitmapFrame.Create(renderBitmap));
                // save the data to the stream
                encoder.Save(outStream);
            }

            // Restore previously saved layout
            surface.LayoutTransform = transform;
        }

        Bitmap BaseBmp;
        Map map;

        public void Paint()
        {
            MapReader reader = new MapReader(@"test.svg");
            HighResolutionTime.Start();
            if (reader.Read())
            {
                map = reader.GetMap(new Dictionary<string,byte>());
            }
            Console.WriteLine("Чтение карты: " + HighResolutionTime.GetTime());
            BaseBmp = map.GetLayerMask(0);
            Graphics g = Graphics.FromImage(BaseBmp);
            foreach (var node in map._patensyGraph.Vertices)
            {
                g.FillRectangle(System.Drawing.Brushes.Red, node.SourceWP.X, node.SourceWP.Y, node.SourceWP.PointWidth, node.SourceWP.PointHeight);
                g.FillRectangle(System.Drawing.Brushes.Red, node.TargetWP.X, node.TargetWP.Y, node.TargetWP.PointWidth, node.TargetWP.PointHeight);
            }
            foreach (var edge in map._patensyGraph.Edges)
            {
                g.DrawLine(System.Drawing.Pens.Green, edge.Source.SourceWP.Center, edge.Target.SourceWP.Center);
            }

            Agents.Human.HumanManager mng = new Agents.Human.HumanManager();
            //agent = mng.GetInstance(map, null, new System.Windows.Media.Media3D.Size3D(0.5, 0.3, 2.0), 1.4, 1.0, 1.0);
            //agent.Initialize(new System.Drawing.Point(10, 3), new List<WayPoint> { new WayPoint(300, 10), new WayPoint(154, 550), new WayPoint(630, 130) });

            g.FillEllipse(System.Drawing.Brushes.Orange, 300, 10, 5, 5);
            g.FillEllipse(System.Drawing.Brushes.Orange, 154, 550, 5, 5);
            g.FillEllipse(System.Drawing.Brushes.Orange, 630, 130, 5, 5);

            SetMapImage(BaseBmp);
            
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(10);
            timer.Tick+=new EventHandler(timer_Tick);
            timer.Start();
        }

        AgentBase agent;

        void timer_Tick(object sender, EventArgs e)
        {
            agent.DoStep(50);
            PaintPanel.Children.Clear();
            PaintPanel.Children.Add(agent.GetAgentShape());
            PaintPanel.InvalidateVisual();
        }

        private void SetMapImage(Bitmap bmp)
        {
            MemoryStream ms = new MemoryStream();

            bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            ms.Position = 0;
            System.Windows.Media.Imaging.BitmapImage image = new System.Windows.Media.Imaging.BitmapImage();
            image.BeginInit();
            image.StreamSource = ms;
            image.EndInit();
            PaintPanel.Width = bmp.Width;
            PaintPanel.Height = bmp.Height;
            PaintPanel.Background = new ImageBrush(image);

        }

        System.Drawing.Point from = new System.Drawing.Point(-1, -1), to = new System.Drawing.Point(-1, -1);

        private void PaintPanel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.RightButton == MouseButtonState.Pressed)
            {
                to = new System.Drawing.Point((int)e.GetPosition((IInputElement)e.OriginalSource).X, (int)e.GetPosition((IInputElement)e.OriginalSource).Y);
            }
            else if (e.LeftButton == MouseButtonState.Pressed)
            {
                from = new System.Drawing.Point((int)e.GetPosition((IInputElement)e.OriginalSource).X, (int)e.GetPosition((IInputElement)e.OriginalSource).Y);
            }
            if (to != new System.Drawing.Point(-1, -1) && to != from)
            {
                //HighResolutionTime.Start();
                var route = map.GetRoute(from, to);
                //Console.WriteLine("Построение маршрута: " + HighResolutionTime.GetTime());
                if (route != null)
                {
                    var bitmap = (Bitmap)BaseBmp.Clone();
                    HighResolutionTime.Start();
                    for (int i = 0; i < route.Count - 1; i++)
                    {

                        var path = map.GetWay(route[i], route[i + 1]);
                        if (path == null)
                            continue;

                        foreach (var point in path)
                        {
                            bitmap.SetPixel(point.X - 1, point.Y - 1, System.Drawing.Color.Blue);
                        }
                    }
                    Console.WriteLine("Построение всех путей: " + HighResolutionTime.GetTime());

                    SetMapImage(bitmap);
                }
            }
        }
    }
}
