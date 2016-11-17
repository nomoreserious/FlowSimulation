using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using FlowSimulation.Enviroment.Model;

namespace FlowSimulation.Enviroment.IO
{
    public sealed class MapReader : IDisposable
    {
        private MemoryStream _sourceStream;

        /// <summary>
        /// Создает слой карты из файла SVG
        /// </summary>
        /// <param name="source">Содержимое файла SVG</param>
        public MapReader(string sourceContent)
        {
            MapSource = sourceContent;
            InitSourceStream();
        }

        private void InitSourceStream()
        {
            if (string.IsNullOrEmpty(MapSource))
            {
                throw new InvalidOperationException("Нельзя восстановить маску слоя из пустой строки!");
            }
            _sourceStream = new MemoryStream();

            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(MapSource);
            _sourceStream.Write(buffer, 0, buffer.Length);
            _sourceStream.Position = 0;
        }

        public List<PaintObject> PaintObjectList { get; private set; }
        public IEnumerable<Color> Colors { get; private set; }
        public Size MapSize { get; private set; }
        public string MapSource { get; private set; }

        public bool Read()
        {
            using (XmlTextReader reader = new XmlTextReader(_sourceStream))
            {
                PaintObjectList = new List<PaintObject>();
                Stack<string> transforms = new Stack<string>();
                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Element)
                    {
                        switch (reader.Name)
                        {
                            case "Map":
                                MapSize = new Size(int.Parse(reader.GetAttribute("width").Split('.')[0]), int.Parse(reader.GetAttribute("height").Split('.')[0]));
                                break;

                            case "svg":
                                MapSize = new Size(int.Parse(reader.GetAttribute("width").Split('.')[0]), int.Parse(reader.GetAttribute("height").Split('.')[0]));
                                break;

                            case "g":
                                transforms.Push(reader.GetAttribute("transform"));
                                break;

                            case "path":
                                PaintObject path = new PaintObject(reader.Name);
                                path.AddAttribute("data", reader.GetAttribute("d"));
                                path.AddAttribute("style", reader.GetAttribute("style"));
                                transforms.Push(reader.GetAttribute("transform"));
                                path.Transforms = transforms.ToList();
                                transforms.Pop();
                                //path.AddAttribute("transform", reader.GetAttribute("transform"));
                                //if (!string.IsNullOrEmpty(transforms))
                                //{
                                //    AddAttribute("maptransform", transforms);
                                //}
                                PaintObjectList.Add(path);
                                break;

                            case "rect":
                                PaintObject rect = new PaintObject(reader.Name);
                                rect.AddAttribute("x", reader.GetAttribute("x"));
                                rect.AddAttribute("y", reader.GetAttribute("y"));
                                rect.AddAttribute("width", reader.GetAttribute("width"));
                                rect.AddAttribute("height", reader.GetAttribute("height"));
                                rect.AddAttribute("style", reader.GetAttribute("style"));
                                transforms.Push(reader.GetAttribute("transform"));
                                rect.Transforms = transforms.ToList();
                                transforms.Pop();
                                PaintObjectList.Add(rect);
                                break;
                        }
                    }
                    else if (reader.NodeType == XmlNodeType.EndElement)
                    {
                        if (reader.Name == "g")
                        {
                            transforms.Pop();
                        }
                    }
                }
            }
            Colors = PaintObjectList.Select(po => po.GetStyle().FillColor).Concat(PaintObjectList.Select(po => po.GetStyle().BorderColor)).Distinct();
#if DEBUG
            foreach (var c in Colors)
            {
                Console.WriteLine(c.ToString());
            }
#endif
            return true;
        }

        private PointF StringToPoint(string value)
        {
            string[] point = value.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
            return new PointF(Convert.ToSingle(PaintObject.ParseToDouble(point[0])), Convert.ToSingle(PaintObject.ParseToDouble(point[1])));
        }

        public Layer InitLayer(Dictionary<string, byte> mapInfo, double scale, string name)
        {
            var layer = new Layer(name, MapSource, scale, mapInfo);

            layer.Substrate = CreateBitmapFromSvg();
            layer.Mask = scale == 1F ? layer.Substrate : CreateBitmapFromSvg((float)scale);
            layer.Cells = CreateLayerModelfromBitmap(layer.Mask, layer.MaskInfo);
            
            return layer;
        }

        public Bitmap CreateBitmapFromSvg(float scale = 1F)
        {
            Size bitmapSize = new SizeF((float)MapSize.Width / scale, (float)MapSize.Height / scale).ToSize();
            Bitmap bitmap = new Bitmap(bitmapSize.Width, bitmapSize.Height);
            Graphics g = Graphics.FromImage(bitmap);
            g.Clear(Color.White);

            foreach (var obj in PaintObjectList)
            {
                Matrix matrix = obj.GetTransformMatrix();
                matrix.Scale(1.0F / scale, 1.0F / scale, MatrixOrder.Append);
                g.Transform = matrix;

                switch (obj.Name)
                {
                    case "path":
                        {
                            string data;
                            if (!obj.TryGetAttributeValue("data", out data))
                            {
                                continue;
                            }
                            string[] val = data.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                            PointF currentPoint = new PointF(0, 0);
                            SVSObjectStyle style = obj.GetStyle();
                            GraphicsPath path = new GraphicsPath();

                            for (int i = 0; i < val.Length; i++)
                            {
                                if (val[i] == "m")
                                {
                                    i++;
                                    var p = StringToPoint(val[i]);
                                    currentPoint = PointF.Add(currentPoint, new SizeF(p.X, p.Y));
                                    while (i < val.Length - 1)
                                    {
                                        if (val[i + 1].Length == 1)
                                        {
                                            break;
                                        }
                                        i++;
                                        PointF nextPoint = StringToPoint(val[i]);
                                        nextPoint.X += currentPoint.X;
                                        nextPoint.Y += currentPoint.Y;
                                        path.AddLine(currentPoint, nextPoint);
                                        currentPoint = nextPoint;
                                    }
                                    continue;
                                }
                                if (val[i] == "M")
                                {
                                    i++;
                                    currentPoint = StringToPoint(val[i]);
                                    while (i < val.Length - 1)
                                    {
                                        if (val[i + 1].Length == 1)
                                        {
                                            break;
                                        }
                                        i++;
                                        PointF nextPoint = StringToPoint(val[i]);
                                        path.AddLine(currentPoint, nextPoint);
                                        currentPoint = nextPoint;
                                    }
                                    continue;
                                }
                                if (val[i] == "l")
                                {
                                    while (i < val.Length - 1)
                                    {
                                        if (val[i + 1].Length == 1)
                                        {
                                            break;
                                        }
                                        i++;
                                        PointF nextPoint = StringToPoint(val[i]);
                                        nextPoint.X += currentPoint.X;
                                        nextPoint.Y += currentPoint.Y;
                                        path.AddLine(currentPoint, nextPoint);
                                        currentPoint = nextPoint;
                                    }
                                }
                                if (val[i] == "L")
                                {
                                    while (i < val.Length - 1)
                                    {
                                        if (val[i + 1].Length == 1)
                                        {
                                            break;
                                        }
                                        i++;
                                        PointF nextPoint = StringToPoint(val[i]);
                                        path.AddLine(currentPoint, nextPoint);
                                        currentPoint = nextPoint;
                                    }
                                }
                                if (val[i] == "c")
                                {
                                    while (i < val.Length - 1)
                                    {
                                        PointF bez1 = new PointF(currentPoint.X + StringToPoint(val[i + 1]).X, currentPoint.Y + StringToPoint(val[i + 1]).Y);
                                        PointF bez2 = new PointF(currentPoint.X + StringToPoint(val[i + 2]).X, currentPoint.Y + StringToPoint(val[i + 2]).Y);
                                        PointF finish = new PointF(currentPoint.X + StringToPoint(val[i + 3]).X, currentPoint.Y + StringToPoint(val[i + 3]).Y);
                                        path.AddBezier(currentPoint, bez1, bez2, finish);
                                        currentPoint = finish;
                                        i += 3;
                                        if (i < val.Length - 1)
                                        {
                                            if (val[i + 1].Length == 1)
                                            {
                                                break;
                                            }
                                        }
                                    }
                                }
                                if (val[i] == "C")
                                {
                                    while (i < val.Length - 1)
                                    {
                                        PointF bez1 = new PointF(StringToPoint(val[i + 1]).X, StringToPoint(val[i + 1]).Y);
                                        PointF bez2 = new PointF(StringToPoint(val[i + 2]).X, StringToPoint(val[i + 2]).Y);
                                        PointF finish = new PointF(StringToPoint(val[i + 3]).X, StringToPoint(val[i + 3]).Y);
                                        path.AddBezier(currentPoint, bez1, bez2, finish);
                                        currentPoint = finish;
                                        i += 3;
                                        if (i < val.Length - 1)
                                        {
                                            if (val[i + 1].Length == 1)
                                            {
                                                break;
                                            }
                                        }
                                    }
                                }
                                if (val[i].ToLower() == "a")
                                {
                                    i++;
                                    PointF point = StringToPoint(val[i]);
                                    float w = point.X * 2;
                                    float h = point.Y * 2;
                                    float x = currentPoint.X - w;
                                    float y = currentPoint.Y - h / 2;
                                    path.AddEllipse(x, y, w, h);
                                }
                                if (val[i].ToLower() == "z")
                                {
                                    if (val[i - 1].Length == 1 && val[1].Length == 1)
                                    {
                                        break;
                                    }
                                    path.CloseFigure();
                                    break;
                                }
                            }

                            g.FillPath(style.GetBrush(), path);
                            g.DrawPath(style.GetPen(scale), path);

                            break;
                        }
                    case "rect":
                        {
                            float x = PaintObject.ParseToFloat(obj.GetAttributeValue("x"));
                            float y = PaintObject.ParseToFloat(obj.GetAttributeValue("y"));
                            float w = PaintObject.ParseToFloat(obj.GetAttributeValue("width"));
                            float h = PaintObject.ParseToFloat(obj.GetAttributeValue("height"));
                            SVSObjectStyle style = obj.GetStyle();

                            g.FillRectangle(style.GetBrush(), x, y, w, h);
                            g.DrawRectangle(style.GetPen(scale), x, y, w, h);

                            break;
                        }
                }
                g.ResetTransform();
            }
            return bitmap;
        }

        private Cell[,] CreateLayerModelfromBitmap(Bitmap bmp, Dictionary<string, byte> mapInfo)
        {
            var cells = new Cell[bmp.Width, bmp.Height];

            for (int i = 0; i < bmp.Width; i++)
            {
                for (int j = 0; j < bmp.Height; j++)
                {
                    Color color = bmp.GetPixel(i, j);
                    string strCol = string.Format("#FF{0:X2}{1:X2}{2:X2}", color.R, color.G, color.B).ToUpper();

                    if(mapInfo.ContainsKey(strCol))
                    {
                        cells[i, j] = new Cell(mapInfo[strCol]);
                    }
                    else
                    {
                        cells[i, j] = new Cell(0x00);
                    }
                }
            }
            return cells;
        }

        public void Dispose()
        {
            if (_sourceStream != null)
                _sourceStream.Close();
        }
    }

    public class PaintObject
    {
        private Dictionary<string, string> _attributes;

        public PaintObject(string objectName)
        {
            Name = objectName;
            _attributes = new Dictionary<string, string>();
        }

        public string Name { get; private set; }
        public List<string> Transforms { get; set; }

        public bool AddAttribute(string name, string value)
        {
            if(!string.IsNullOrWhiteSpace(name) && !string.IsNullOrWhiteSpace(value))
            {
                _attributes.Add(name, value);
                return true;
            }
            return false;
        }

        public bool TryGetAttributeValue(string name, out string value)
        {
            return _attributes.TryGetValue(name, out value);
        }

        public string GetAttributeValue(string name)
        {
            string value = "";
            _attributes.TryGetValue(name, out value);
            return value;
        }

        public SVSObjectStyle GetStyle()
        {
            string style;
            if (_attributes.TryGetValue("style", out style))
            {
                return SVSObjectStyle.FromString(style);
            }
            return null;
        }

        public static double ParseToDouble(string str)
        {
            return double.Parse(str, System.Globalization.CultureInfo.InvariantCulture);
        }

        public static float ParseToFloat(string str)
        {
            return float.Parse(str, System.Globalization.CultureInfo.InvariantCulture);
        }

        public Matrix GetTransformMatrix()
        {
            Matrix matrix = ParseToMatrix(null);
            foreach (var trans in Transforms)
            {
                Matrix m = ParseToMatrix(trans);
                matrix.Multiply(m, MatrixOrder.Append);
            }
            return matrix;
        }

        public System.Windows.Media.Matrix GetTransformMediaMatrix()
        {
            System.Windows.Media.Matrix matrix = System.Windows.Media.Matrix.Identity;
            foreach (var trans in Transforms)
            {
                var m = GetMatrixFromString(trans);
                matrix = System.Windows.Media.Matrix.Multiply(matrix, m);
            }
            return matrix;
        }

        private System.Windows.Media.Matrix GetMatrixFromString(string transform)
        {
            if (!string.IsNullOrEmpty(transform))
            {
                if (transform.Contains("translate"))
                {
                    int begin = transform.IndexOf("(") + 1, end = transform.IndexOf(")");
                    string[] values = transform.Substring(begin, end - begin).Split(',');
                    return new System.Windows.Media.Matrix(1, 0, 0, 1, PaintObject.ParseToDouble(values[0]), PaintObject.ParseToDouble(values[1]));
                }
                if (transform.Contains("matrix"))
                {
                    int begin = transform.IndexOf("(") + 1, end = transform.IndexOf(")");
                    string[] values = transform.Substring(begin, end - begin).Split(',');
                    return new System.Windows.Media.Matrix(
                        PaintObject.ParseToDouble(values[0]),
                        PaintObject.ParseToDouble(values[1]),
                        PaintObject.ParseToDouble(values[2]),
                        PaintObject.ParseToDouble(values[3]),
                        PaintObject.ParseToDouble(values[4]),
                        PaintObject.ParseToDouble(values[5]));
                }
            }
            return new System.Windows.Media.Matrix(1, 0, 0, 1, 0, 0);
        }

        private Matrix ParseToMatrix(string tranform)
        {
            if (!string.IsNullOrEmpty(tranform))
            {
                if (tranform.Contains("translate"))
                {
                    int begin = tranform.IndexOf("(") + 1, end = tranform.IndexOf(")");
                    string[] values = tranform.Substring(begin, end - begin).Split(',');
                    return new System.Drawing.Drawing2D.Matrix(1, 0, 0, 1, PaintObject.ParseToFloat(values[0]), PaintObject.ParseToFloat(values[1]));
                }
                if (tranform.Contains("matrix"))
                {
                    int begin = tranform.IndexOf("(") + 1, end = tranform.IndexOf(")");
                    string[] values = tranform.Substring(begin, end - begin).Split(',');
                    return new System.Drawing.Drawing2D.Matrix(
                        PaintObject.ParseToFloat(values[0]),
                        PaintObject.ParseToFloat(values[1]),
                        PaintObject.ParseToFloat(values[2]),
                        PaintObject.ParseToFloat(values[3]),
                        PaintObject.ParseToFloat(values[4]),
                        PaintObject.ParseToFloat(values[5]));
                }
            }
            return new System.Drawing.Drawing2D.Matrix(1, 0, 0, 1, 0, 0);
        }
    }

    public class SVSObjectStyle
    {
        private Color _borderColor = Color.Transparent;
        private Color _fillColor = Color.Transparent;

        private float _borderSize = 1.0F;

        public Color BorderColor
        {
            get { return _borderColor; }
            set { _borderColor = value; }
        }
        public Color FillColor
        {
            get { return _fillColor; }
            set { _fillColor = value; }
        }

        public float BorderSize
        {
            get { return _borderSize; }
            set { _borderSize = value; }
        }

        public SVSObjectStyle()
        {

        }

        public Brush GetBrush()
        {
            return new SolidBrush(_fillColor);
        }

        public Pen GetPen(float scale)
        {
            return new Pen(_borderColor, Math.Max(_borderSize / scale, 1.0F));
        }

        public static SVSObjectStyle FromString(string str)
        {
            SVSObjectStyle style = new SVSObjectStyle();
            if (string.IsNullOrEmpty(str))
            {
                return style;
            }
            string[] values = str.Split(';');
            for (int i = 0; i < values.Length; i++)
            {
                string[] pair = values[i].Split(':');
                switch (pair[0])
                {
                    case "fill":
                        if (pair[1].Contains("#") && !pair[1].Contains("url"))
                        {
                            style._fillColor = ColorTranslator.FromHtml(pair[1]);
                        }
                        else
                        {
                            style._fillColor = Color.Transparent;
                        }
                        break;
                    case "fill-opacity":
                        style._fillColor = Color.FromArgb((int)(Convert.ToSingle(pair[1].Replace(".", ",")) * 255), Convert.ToInt32(style._fillColor.R), Convert.ToInt32(style._fillColor.G), Convert.ToInt32(style._fillColor.B));
                        break;
                    case "stroke":
                        if (pair[1].Contains("#") && !pair[1].Contains("url"))
                        {
                            style._borderColor = ColorTranslator.FromHtml(pair[1]);
                        }
                        else
                        {
                            style._borderColor = Color.Transparent;
                        }
                        break;
                    case "stroke-opacity":
                        style._borderColor = Color.FromArgb((int)(Convert.ToSingle(pair[1].Replace(".", ",")) * 255), Convert.ToInt32(style._borderColor.R), Convert.ToInt32(style._borderColor.G), Convert.ToInt32(style._borderColor.B));
                        break;
                    case "stroke-width":
                        style._borderSize = Convert.ToSingle(pair[1].Replace(".", ",").Replace("px", ""));
                        break;
                }
            }
            return style;
        }
    }
}