using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing.Drawing2D;
using System.IO;
using System.Drawing;
using System.Xml;

namespace FlowSimulation.SimulationScenario.IO
{
    class MapReader
    {
        public Size size { get; set; }
        private List<PaintObject> PaintObjectList;
        private System.Windows.Media.Imaging.BitmapImage image;

        public List<PaintObject> GetPaintObjectList()
        {
            return PaintObjectList;
        }

        public System.Windows.Media.Imaging.BitmapImage GetImage()
        {
            return image;
        }
        public MapReader(XmlReader reader)
        {
            size = new Size();
            PaintObjectList = new List<PaintObject>();
            Read(reader);
        }

        private void Read(XmlReader reader)
        {
            PaintObjectList.Clear();
            string MapTransform = "";
            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element)
                {
                    if (reader.Name == "Map")
                    {
                        size = new Size(int.Parse(reader.GetAttribute("width").Split('.')[0]), int.Parse(reader.GetAttribute("height").Split('.')[0]));
                        continue;
                    }
                    if (reader.Name == "svg")
                    {
                        size = new Size(int.Parse(reader.GetAttribute("width").Split('.')[0]), int.Parse(reader.GetAttribute("height").Split('.')[0]));
                        continue;
                    }
                    if (reader.Name == "g")
                    {
                        MapTransform = reader.GetAttribute("transform");
                        continue;
                    }
                    if (reader.Name == "path")
                    {
                        PaintObject obj = new PaintObject(reader.Name);
                        obj.AddAttribute("data", reader.GetAttribute("d"));
                        obj.AddAttribute("style", reader.GetAttribute("style"));
                        obj.AddAttribute("transform", reader.GetAttribute("transform"));
                        if (!string.IsNullOrEmpty(MapTransform))
                        {
                            obj.AddAttribute("maptransform", MapTransform);
                        }
                        PaintObjectList.Add(obj);
                        continue;
                    }
                    if (reader.Name == "rect")
                    {
                        PaintObject obj = new PaintObject(reader.Name);
                        obj.AddAttribute("x", reader.GetAttribute("x"));
                        obj.AddAttribute("y", reader.GetAttribute("y"));
                        obj.AddAttribute("width", reader.GetAttribute("width"));
                        obj.AddAttribute("height", reader.GetAttribute("height"));
                        obj.AddAttribute("style", reader.GetAttribute("style"));
                        obj.AddAttribute("transform", reader.GetAttribute("transform"));
                        if (!string.IsNullOrEmpty(MapTransform))
                        {
                            obj.AddAttribute("maptransform", MapTransform);
                        }
                        PaintObjectList.Add(obj);
                    }
                }
            }
            reader.Close();
        }

        private PointF StringToPoint(string value)
        {
            string[] point = value.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
            return new PointF(Convert.ToSingle(PaintObject.StringToDoubleConvertor(point[0])), Convert.ToSingle(PaintObject.StringToDoubleConvertor(point[1])));
        }

        public byte[,] GetMap()
        {
            Bitmap bmp = new Bitmap(size.Width, size.Height);
            Graphics g = Graphics.FromImage(bmp);
            g.Clear(Color.White);
            for (int n = 0; n < PaintObjectList.Count; n++)
            {
                if (PaintObjectList[n].GetName() == "path")
                {
                    string data;
                    if (!PaintObjectList[n].TryGetAttributeValue("data", out data))
                    {
                        continue;
                    }
                    string[] val = data.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                    PointF currentPoint = new PointF();
                    SVSObjectStyle style = PaintObjectList[n].GetStyle();
                    GraphicsPath path = new GraphicsPath();
                    for (int i = 0; i < val.Length; i++)
                    {
                        if (val[i] == "m")
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
                            break;
                        }
                    }
                    path.Transform(PaintObjectList[n].GetTransformMatrix());
                    g.FillPath(style.GetBrush(), path);
                    g.DrawPath(style.GetPen(), path);
                }
                if (PaintObjectList[n].GetName() == "rect")
                {
                    float x = PaintObject.StringToFloatConvertor(PaintObjectList[n].GetAttributeValue("x"));
                    float y = PaintObject.StringToFloatConvertor(PaintObjectList[n].GetAttributeValue("y"));
                    float w = PaintObject.StringToFloatConvertor(PaintObjectList[n].GetAttributeValue("width"));
                    float h = PaintObject.StringToFloatConvertor(PaintObjectList[n].GetAttributeValue("height"));
                    SVSObjectStyle style = PaintObjectList[n].GetStyle();
                    g.Transform = PaintObjectList[n].GetTransformMatrix();
                    g.FillRectangle(style.GetBrush(), x, y, w, h);
                    g.DrawRectangle(style.GetPen(), x, y, w, h);
                    g.ResetTransform();
                }
            }
            MemoryStream ms = new MemoryStream();
            bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            ms.Position = 0;
            image = new System.Windows.Media.Imaging.BitmapImage();
            image.BeginInit();
            image.StreamSource = ms;
            image.EndInit();
            byte[,] map = new byte[size.Width, size.Height];
            for (int i = 0; i < size.Width; i++)
            {
                for (int j = 0; j < size.Height; j++)
                {
                    Color color = bmp.GetPixel(i, j);

                    if (color != Color.FromArgb(255, 255, 255, 255))
                    {
                        map[i, j] |= 0x80;
                    }
                }
            }
            return map;
        }

        public static System.Windows.Media.Imaging.BitmapImage GetBitmapImageFromMapMask(byte[,] map_mass)
        {
            System.Windows.Media.Imaging.BitmapImage image;
            Bitmap bmp = new Bitmap(map_mass.GetLength(0), map_mass.GetLength(1));
            for (int i = 0; i < map_mass.GetLength(0); i++)
            {
                for (int j = 0; j < map_mass.GetLength(1); j++)
                {
                    Color color;
                    if (((MapOld.CellState)map_mass[i, j] & MapOld.CellState.Closed) == MapOld.CellState.Closed)
                    {
                        color = Color.Black;
                    }
                    //else if (((Map.CellState)map_mass[i, j] & Map.CellState.Busy) == Map.CellState.Busy)
                    //{
                    //    color = Color.Red;
                    //}
                    //else if (((Map.CellState)map_mass[i, j] & Map.CellState.Locked) == Map.CellState.Locked)
                    //{
                    //    color = Color.Lime;
                    //}
                    else
                    {
                        color = Color.White;
                    }
                    bmp.SetPixel(i, j, color);
                }
            }
            MemoryStream ms = new MemoryStream();
            bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            ms.Position = 0;
            image = new System.Windows.Media.Imaging.BitmapImage();
            image.BeginInit();
            image.StreamSource = ms;
            image.EndInit();
            return image;
        }

        public Matrix GetTransformMatrixFromString(string tranform)
        {
            if (!string.IsNullOrEmpty(tranform))
            {
                if (tranform.Contains("translate"))
                {
                    int begin = tranform.IndexOf("(") + 1, end = tranform.IndexOf(")");
                    string[] values = tranform.Substring(begin, end - begin).Split(',');
                    return new System.Drawing.Drawing2D.Matrix(1, 0, 0, 1, PaintObject.StringToFloatConvertor(values[0]), PaintObject.StringToFloatConvertor(values[1]));
                }
                if (tranform.Contains("matrix"))
                {
                    int begin = tranform.IndexOf("(") + 1, end = tranform.IndexOf(")");
                    string[] values = tranform.Substring(begin, end - begin).Split(',');
                    return new System.Drawing.Drawing2D.Matrix(
                        PaintObject.StringToFloatConvertor(values[0]),
                        PaintObject.StringToFloatConvertor(values[1]),
                        PaintObject.StringToFloatConvertor(values[2]),
                        PaintObject.StringToFloatConvertor(values[3]),
                        PaintObject.StringToFloatConvertor(values[4]),
                        PaintObject.StringToFloatConvertor(values[5]));
                }
            }
            return new System.Drawing.Drawing2D.Matrix(1, 0, 0, 1, 0, 0);
        }
    }
    public class PaintObject
    {
        private Dictionary<string, string> Attributes;

        private string name;

        public PaintObject(string object_name)
        {
            name = object_name;
            Attributes = new Dictionary<string, string>();
        }

        public bool AddAttribute(string name, string value)
        {
            try
            {
                Attributes.Add(name, value);
                return true;
            }
            catch (ArgumentException)
            {
                return false;
            }
        }

        public bool TryGetAttributeValue(string name, out string value)
        {
            if (Attributes.TryGetValue(name, out value))
            {
                return true;
            }
            return false;
        }

        public string GetAttributeValue(string name)
        {
            string value = "";
            Attributes.TryGetValue(name, out value);
            return value;
        }

        public string GetName()
        {
            return name;
        }

        public SVSObjectStyle GetStyle()
        {
            string style;
            if (Attributes.TryGetValue("style", out style))
            {
                return SVSObjectStyle.FromString(style);
            }
            return null;
        }

        public static double StringToDoubleConvertor(string str)
        {
            return double.Parse(str.Replace(".", ","));
        }

        public static float StringToFloatConvertor(string str)
        {
            return float.Parse(str.Replace(".", ","));
        }

        public Matrix GetTransformMatrix()
        {
            string tranform, maptransform;
            float MapOffsetX = 0.0F, MapOffsetY = 0.0F;
            if (Attributes.TryGetValue("maptransform", out maptransform))
            {
                if (!string.IsNullOrEmpty(maptransform))
                {
                    if (maptransform.Contains("translate"))
                    {
                        int begin = maptransform.IndexOf("(") + 1, end = maptransform.IndexOf(")");
                        string[] values = maptransform.Substring(begin, end - begin).Split(',');
                        MapOffsetX = StringToFloatConvertor(values[0]);
                        MapOffsetY = StringToFloatConvertor(values[1]);
                    }
                }
            }
            if (Attributes.TryGetValue("transform", out tranform))
            {
                if (!string.IsNullOrEmpty(tranform))
                {
                    if (tranform.Contains("translate"))
                    {
                        int begin = tranform.IndexOf("(") + 1, end = tranform.IndexOf(")");
                        string[] values = tranform.Substring(begin, end - begin).Split(',');
                        return new Matrix(1, 0, 0, 1, StringToFloatConvertor(values[0]) + MapOffsetX, StringToFloatConvertor(values[1]) + MapOffsetY);
                    }
                    if (tranform.Contains("matrix"))
                    {
                        int begin = tranform.IndexOf("(") + 1, end = tranform.IndexOf(")");
                        string[] values = tranform.Substring(begin, end - begin).Split(',');
                        return new Matrix(
                            StringToFloatConvertor(values[0]),
                            StringToFloatConvertor(values[1]),
                            StringToFloatConvertor(values[2]),
                            StringToFloatConvertor(values[3]),
                            StringToFloatConvertor(values[4]) + MapOffsetX,
                            StringToFloatConvertor(values[5]) + MapOffsetY);
                    }
                }
            }
            return new Matrix(1, 0, 0, 1, MapOffsetX, MapOffsetY);
        }
    }

    public class SVSObjectStyle
    {
        private Color _BorderColor = Color.Transparent;
        private Color _FillColor = Color.Transparent;

        private float _BorderSize = 1.0F;

        public Color BorderColor
        {
            get { return _BorderColor; }
            set { _BorderColor = value; }
        }
        public Color FillColor
        {
            get { return _FillColor; }
            set { _FillColor = value; }
        }

        public float BorderSize
        {
            get { return _BorderSize; }
            set { _BorderSize = value; }
        }

        public SVSObjectStyle()
        {

        }

        public Brush GetBrush()
        {
            return new SolidBrush(_FillColor);
        }

        public Pen GetPen()
        {
            return new Pen(_BorderColor, _BorderSize);
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
                            style._FillColor = ColorTranslator.FromHtml(pair[1]);
                        }
                        else
                        {
                            style._FillColor = Color.Transparent;
                        }
                        break;
                    case "fill-opacity":
                        style._FillColor = Color.FromArgb((int)(Convert.ToSingle(pair[1].Replace(".", ",")) * 255), Convert.ToInt32(style._FillColor.R), Convert.ToInt32(style._FillColor.G), Convert.ToInt32(style._FillColor.B));
                        break;
                    case "stroke":
                        if (pair[1].Contains("#") && !pair[1].Contains("url"))
                        {
                            style._BorderColor = ColorTranslator.FromHtml(pair[1]);
                        }
                        else
                        {
                            style._BorderColor = Color.Transparent;
                        }
                        break;
                    case "stroke-opacity":
                        style._BorderColor = Color.FromArgb((int)(Convert.ToSingle(pair[1].Replace(".", ",")) * 255), Convert.ToInt32(style._BorderColor.R), Convert.ToInt32(style._BorderColor.G), Convert.ToInt32(style._BorderColor.B));
                        break;
                    case "stroke-width":
                        style._BorderSize = Convert.ToSingle(pair[1].Replace(".", ",").Replace("px", ""));
                        break;
                }
            }
            return style;
        }
    }
}
