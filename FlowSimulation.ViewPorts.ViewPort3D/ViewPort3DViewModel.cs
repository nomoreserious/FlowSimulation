using FlowSimulation.Contracts.Attributes;
using FlowSimulation.Contracts.Configuration;
using FlowSimulation.Contracts.ViewPort;
using FlowSimulation.Helpers.Geometry;
using FlowSimulation.Helpers.MVVM;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;

namespace FlowSimulation.ViewPort.ViewPort3D
{
    [Export(typeof(IViewPort))]
    [ViewPortMetadata("3D", "Трехмерная визуализация", "pack://application:,,,/FlowSimulation.ViewPort.ViewPort3D;component/3d.png")]
    public class ViewPort3DViewModel : ViewModelBase, IViewPort
    {
        private UserControl _view;
        private FlowSimulation.Enviroment.Map _map;
        private Vector3D _lightDirection = new Vector3D(0, 50, 50);
        private Vector3D _cameraDirection = new Vector3D(100, 100, -100);
        private Point3D _cameraPosition = new Point3D(-100, -100, 100);
        private double _cameraDistance = 500;
        private double _cameraWidth = 400;
        private double _lightAngle = 200;
        private string _modelPath = string.Empty;
        private bool _useTransparensy = false;

        private double[,] _topography;
        
        public double CameraDistance
        {
            get { return _cameraDistance; }
            set
            {
                if (_cameraDistance == value)
                    return;
                _cameraDistance = value;
                double x = _cameraDistance, y = _cameraWidth, z = _cameraDistance;
                CameraPosition = new Point3D(x, y, z);
                CameraDirection = new Vector3D(-x, -y, -z);
            }
        }

        public double CameraWidth
        {
            get { return _cameraWidth; }
            set
            {
                if (_cameraWidth == value)
                    return;
                _cameraWidth = value;
                double x = _cameraPosition.X, y = _cameraWidth, z = _cameraPosition.Z;
                CameraPosition = new Point3D(x, y, z);
                CameraDirection = new Vector3D(-x, -y, -z);
            }
        }

        public double LightAngle
        {
            get { return _lightAngle; }
            set
            {
                if (_lightAngle == value)
                    return;
                _lightAngle = value;
                LightDirectionChanged();
            }
        }

        public Vector3D LightDirection
        {
            get { return _lightDirection; }
            set
            {
                if (_lightDirection == value)
                    return;
                _lightDirection = value;
                OnPropertyChanged("LightDirection");
            }
        }

        public Vector3D CameraDirection
        {
            get { return _cameraDirection; }
            set
            {
                if (_cameraDirection == value)
                    return;
                _cameraDirection = value;
                OnPropertyChanged("CameraDirection");
            }
        }


        public Point3D CameraPosition
        {
            get { return _cameraPosition; }
            set
            {
                if (_cameraPosition == value)
                    return;
                _cameraPosition = value;
                OnPropertyChanged("CameraPosition");
            }
        }

        public Model3DGroup Background { get; private set; }

        public Model3DGroup Agents { get; private set; }

        public UserControl ViewPort
        {
            get
            {
                if (_view == null)
                {
                    _view = new ViewPort3DView();
                    _view.DataContext = this;
                }
                return _view;
            }
        }

        private double GetZCoordiante(System.Drawing.Point location, int? layerId = null)
        {
            if (layerId.HasValue)
            {
                return layerId.Value * 4 + 0.5;
            }
            var scale = Enviroment.Constants.CELL_SIZE;
            int x = Convert.ToInt32(location.X * scale);
            int y = Convert.ToInt32(location.Y * scale);

            if (!object.ReferenceEquals(_topography, null) &&
                x >= 0 && y >= 0 &&
                _topography.GetLength(0) > x &&
                _topography.GetLength(1) > y)
            {
                return _topography[x, y];
            }
            return 0;
        }

        public void Update(IEnumerable<Contracts.Agents.AgentBase> agents)
        {
            //Делаем группу для отрисовки агентов
            Model3DGroup group = new Model3DGroup();

            Dictionary<ulong, MeshGeometry3D> dict = new Dictionary<ulong, MeshGeometry3D>();

            var col = agents.ToList();
            for (int i = 0; i < col.Count; i++)
            {
                if (col[i] is Contracts.Agents.VehicleAgentBase)
                {
                    group.Children.Add((col[i] as Contracts.Agents.VehicleAgentBase).GetModel());
                }
                else
                {   
                    if (!dict.ContainsKey(col[i].GroupId))
                    {
                        dict.Add(col[i].GroupId, new MeshGeometry3D());
                    }
                    var mesh = dict[col[i].GroupId];
                    int? layerId = null;
                    if (_map.Count > 1)
                    {
                        layerId = col[i].LayerId;
                    }

                    double zCoordinate = GetZCoordiante(col[i].Position, layerId);
                    col[i].AddAgentGeometry(zCoordinate, ref mesh);
                }
            }

            
            //Добавляем группы агентов в группу отрисовки
            foreach (var pair in dict)
            {
                GeometryModel3D geom = new GeometryModel3D(pair.Value, new DiffuseMaterial(Geometry3DHelper.GetColorByGroup(pair.Key)));
                //geom.Transform = new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(0, 0, 1), 180), Width / 2, Height / 2, 0);
                //geom.Transform = new TranslateTransform3D(-Width / 2, 0, -Height / 2);
                group.Children.Add(geom);
            }

            //GeometryModel3D geom = new GeometryModel3D(meshGeometry, new DiffuseMaterial(GetColorByGroup()));
            //geom.Transform = new TranslateTransform3D(-Width / 2, 0, -Height / 2);
            //group.Children.Add(geom);
            Agents = group;
            LightDirectionChanged();

            OnPropertyChanged("Agents");
        }

        public Dictionary<string, ParamDescriptor> CreateSettings()
        {
            var settings = new Dictionary<string, ParamDescriptor>();
            settings.Add("path", new ParamDescriptor("path", "Путь к 3D модели", "Абсолютный путь с файлу в одном из следующих форматов: *.3ds, *.lwo, *.obj, *.objz, *.stl, *.off", _modelPath));

            settings.Add("transparent", new ParamDescriptor("transparent", "Полупрозрачные текстуры", "Использовать для отрисовки модели полупрозрачный материал", _useTransparensy));

            settings.Add("transformation_order", new ParamDescriptor("transformation_order", "Порядок применения трансформаций",
                "Формат: 'RST',\r\n" +
                "где R - вращение, S - масштабирование, T - смещение. Максимум 3 символа, повторы игнорируются\r\n" +
                "Примеры: 'RST', 'STR', 'ST', 'T'", string.Empty));

            settings.Add("scale", new ParamDescriptor("scale", "Трансформация (масштабирование)",
                "Формат: '(scalex;scaley;scalez)',\r\n" +
                "где scalex, scaley, scalez - коэффициенты мастабирования по соответствующим осям.\r\n" +
                "Пример: '(4.2;1;1)'", string.Empty));


            settings.Add("translate", new ParamDescriptor("translate", "Трансформация (смещение)", 
                "Формат: '(transx;transy;transz)',\r\n" +
                "где transx, transy, transz - смещения по соответствующим осям.\r\n" +
                "Пример: '(4.2;10.1;1)'", string.Empty));

            settings.Add("rotate", new ParamDescriptor("rotate", "Трансформация (вращение)", string.Empty));
            return settings;
        }

        public void Initialize(FlowSimulation.Enviroment.Map map, Dictionary<string, object> settings)
        {
            _map = map;
            if (settings.ContainsKey("path"))
            {
                _modelPath = (string)settings["path"];
            }
            if (settings.ContainsKey("transparent"))
            {
                _useTransparensy = (bool)settings["transparent"];
            }

            var model = LoadModel(_modelPath);
            
            if (model != null)
            {
                Background = model;
                Background.Transform = ReadTransformation(settings);
                CreateTopografy();
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Не удалось найти файл 3D модели");
                if (_map != null && _map.Count > 0)
                {
                    BitmapImage bi = Helpers.Imaging.ImageManager.BitmapToBitmapImage(_map[0].Substrate);
                    Background = CreateMockModel(bi, _map[0].Width, _map[0].Height);
                    CreateTopografy(true);
                }
            }

            OnPropertyChanged("Background");
        }

        private Transform3D ReadTransformation(Dictionary<string, object> settings)
        {
            Transform3DGroup trgr = new Transform3DGroup();

            if (settings.ContainsKey("transformation_order") && !string.IsNullOrWhiteSpace((string)settings["transformation_order"]))
            {
                var order = ((string)settings["transformation_order"]).ToLower().Distinct();
                int count = 0;
                foreach (var symbol in order)
                {
                    if (symbol == 's')
                    {
                        if (settings.ContainsKey("scale"))
                        {
                            var transform = GetTransformationFromString("scale", (string)settings["scale"]);
                            if (transform != null)
                            {
                                trgr.Children.Add(transform);
                            }
                        }
                        count++;
                    }
                    else if (symbol == 't')
                    {
                        if (settings.ContainsKey("translate"))
                        {
                            var transform = GetTransformationFromString("translate", (string)settings["translate"]);
                            if (transform != null)
                            {
                                trgr.Children.Add(transform);
                            }
                        }
                        count++;
                    }
                    else if (symbol == 'r')
                    {
                        if (settings.ContainsKey("rotate"))
                        {
                            var transform = GetTransformationFromString("rotate", (string)settings["rotate"]);
                            if (transform != null)
                            {
                                trgr.Children.Add(transform);
                            }
                        }
                        count++;
                    }
                    if (count == 3)
                    {
                        break;
                    }
                }
            }
            return trgr;
        }

        private Transform3D GetTransformationFromString(string typeCode, string value)
        {
            try
            {
                var values = value.Trim('(', ')').Split(';');
                switch (typeCode)
                {
                    case "scale":
                        if (values.Count() == 3)
                        {
                            double x, y, z;
                            if (double.TryParse(values[0], System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.InvariantCulture, out x) &&
                                double.TryParse(values[1], System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.InvariantCulture, out y) &&
                                double.TryParse(values[2], System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.InvariantCulture, out z))
                            {
                                return new ScaleTransform3D(x, y, z);
                            }
                        }
                        return null;
                    case "translate":
                        if (values.Count() == 3)
                        {
                            double x, y, z;
                            if (double.TryParse(values[0], System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.InvariantCulture, out x) &&
                                double.TryParse(values[1], System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.InvariantCulture, out y) &&
                                double.TryParse(values[2], System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.InvariantCulture, out z))
                            {
                                return new TranslateTransform3D(x, y, z);
                            }
                        }
                        return null;
                    case "rotate":
                        if (values.Count() == 4)
                        {
                            double a, x, y, z;
                            if (double.TryParse(values[0], System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.InvariantCulture, out a) &&
                                double.TryParse(values[1], System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.InvariantCulture, out x) &&
                                double.TryParse(values[2], System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.InvariantCulture, out y) &&
                                double.TryParse(values[3], System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.InvariantCulture, out z))
                            {
                                return new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(x, y, z), a));
                            }
                        }
                        return null;
                    default:
                        return null;
                }
            }
            catch { return null; }
        }

        private Model3DGroup LoadModel(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return null;
            }
            if (!System.IO.Path.HasExtension(path) || !System.IO.File.Exists(path))
            {
                return null;
            }

            HelixToolkit.Wpf.IModelReader reader = null;
            string fileExtension = System.IO.Path.GetExtension(path).ToLower();
            switch (fileExtension)
            {
                case ".3ds":
                        reader = new HelixToolkit.Wpf.StudioReader();
                        break;
                case ".lwo":
                        reader = new HelixToolkit.Wpf.LwoReader();
                        break;
                case ".obj":
                        reader = new HelixToolkit.Wpf.ObjReader();
                        break;
                case ".objz":
                        reader = new HelixToolkit.Wpf.ObjReader();
                        break;
                case ".stl":
                        reader = new HelixToolkit.Wpf.StLReader();
                        break;
                case ".off":
                        reader = new HelixToolkit.Wpf.OffReader();
                        break;
                default:
                        return null;
            }
            Model3DGroup model = reader.Read(path);
            if (_useTransparensy)
            {
                foreach (var mod in model.Children)
                {
                    if (mod is GeometryModel3D)
                    {
                        ((GeometryModel3D)mod).Material = new DiffuseMaterial(new SolidColorBrush((Color)ColorConverter.ConvertFromString("#40FFFFFF")));
                        ((GeometryModel3D)mod).BackMaterial = new DiffuseMaterial(new SolidColorBrush((Color)ColorConverter.ConvertFromString("#40FFFFFF")));
                    }
                }
            }
            return model;
        }

        private Model3DGroup CreateMockModel(ImageSource image, double width, double height)
        {
            //Создаем фон
            Point3D p1 = new Point3D(0, 0, 0);
            Point3D p2 = new Point3D(0, width, 0);
            Point3D p3 = new Point3D(height, width, 0);
            Point3D p4 = new Point3D(height, 0, 0);
            ImageBrush brush = new ImageBrush(image);
            //добавляем фон в группу
            Model3DGroup background = new Model3DGroup();
            background.Children.Add(CreateBackgroundModelGroup(p1, p2, p3, p4, brush));
            background.Freeze();

            return background;
        }

        private Model3DGroup CreateBackgroundModelGroup(Point3D p0, Point3D p1, Point3D p2, Point3D p3, ImageBrush image)
        {
            MeshGeometry3D mesh = new MeshGeometry3D();

            mesh.Positions.Add(p0);
            mesh.Positions.Add(p1);
            mesh.Positions.Add(p2);
            mesh.Positions.Add(p3);

            mesh.TriangleIndices.Add(0);
            mesh.TriangleIndices.Add(2);
            mesh.TriangleIndices.Add(1);

            mesh.TriangleIndices.Add(0);
            mesh.TriangleIndices.Add(3);
            mesh.TriangleIndices.Add(2);

            //Vector3D normal = Geometry3DHelper.CalculateNormal(p0, p2, p1);
            Vector3D normal = Geometry3DHelper.CalculateNormal(p0, p1, p2);

            mesh.Normals.Add(normal);
            mesh.Normals.Add(normal);
            mesh.Normals.Add(normal);
            mesh.Normals.Add(normal);

            mesh.TextureCoordinates.Add(new Point(0, 0));
            mesh.TextureCoordinates.Add(new Point(1, 0));
            mesh.TextureCoordinates.Add(new Point(1, 1));
            mesh.TextureCoordinates.Add(new Point(0, 1));

            GeometryModel3D model = new GeometryModel3D(mesh, new DiffuseMaterial(image));
            Model3DGroup group = new Model3DGroup();
            group.Children.Add(model);
            return group;
        }

        private void CreateTopografy(bool flat = false)
        {
            int width = Convert.ToInt32(Background.Bounds.Y + Background.Bounds.SizeY);
            int height = Convert.ToInt32(Background.Bounds.X + Background.Bounds.SizeX);

            _topography = new double[width, height];

            if(!flat)
            {
                foreach (GeometryModel3D geometryModel in Background.Children)
                {
                    if (geometryModel.Geometry is MeshGeometry3D)
                    {
                        var mesh = (MeshGeometry3D)geometryModel.Geometry;
                        for (int k = 0; k < mesh.TriangleIndices.Count; k += 3)
                        {
                            var triangle = new List<Point3D>() 
                            { 
                                mesh.Positions[mesh.TriangleIndices[k]], 
                                mesh.Positions[mesh.TriangleIndices[k + 1]], 
                                mesh.Positions[mesh.TriangleIndices[k + 2]] 
                            };
                            double minX = triangle.Min(x => x.X);
                            double maxX = triangle.Max(x => x.X);
                            double minY = triangle.Min(x => x.Y);
                            double maxY = triangle.Max(x => x.Y);
                            double avgZ = triangle.Average(x => x.Z);

                            for (int j = Convert.ToInt32(Math.Floor(minX)); j <= maxX; j++)
                            {
                                for (int i = Convert.ToInt32(Math.Floor(minY)); i <= maxY; i++)
                                {
                                    if (i >= 0 && j >= 0 && i < width && j < height)
                                    {
                                        if (_topography[i, j] == 0 || avgZ < _topography[i, j])
                                        {
                                            _topography[i, j] = avgZ;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        //private bool IsPointInTriangle(Point point, Point3D[] triangle)
        //{
        //    if (triangle == null || triangle.Length < 3)
        //    {
        //        return false;
        //    }
        //    double Xa, Ya, Xb, Yb, Xc, Yc, m, l;
        //    Xa = triangle[1].X - triangle[0].X;
        //    Ya = triangle[1].Y - triangle[0].Y;
        //    Xb = triangle[2].X - triangle[0].X;
        //    Yb = triangle[2].Y - triangle[0].Y;
        //    Xc = point.X - triangle[0].X;
        //    Yc = point.Y - triangle[0].Y;
        //    m = (Xc * Ya - Xa * Yc) / (Xb * Ya - Xa * Yb);
        //    if (m >= 0 && m <= 1)
        //    {
        //        l = (Xc - m * Xb) / Xa;
        //        if (l >= 0 && (m + l) <= 1)
        //        {
        //            return true;
        //        }
        //    }
        //    return false;
        //}

        private void LightDirectionChanged()
        {
            double angle = _lightAngle;
            double R = 1000 / 2;
            double x = 0, y, z;
            z = -R * Math.Cos(angle * Math.PI / 180);
            y = -R * Math.Sin(angle * Math.PI / 180);
            LightDirection = new Vector3D(x, y, z);
        }

    }
}
