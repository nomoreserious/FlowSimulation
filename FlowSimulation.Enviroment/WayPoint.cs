using System;
using System.Xml.Serialization;
using System.Runtime.Serialization;

namespace FlowSimulation.Enviroment
{
    [Serializable]
    public sealed class WayPoint : ICloneable, IEquatable<WayPoint>, ISerializable
    {
        public WayPoint()
        {
            this.X = 0;
            this.Y = 0;
            this.LayerId = 0;

            this.IsInput = true;
            this.IsOutput = true;
            this.Width = 1;
            this.Height = 1;
        }

        public WayPoint(int x, int y, int layerId = 0)
            : this()
        {
            this.X = x;
            this.Y = y;
            this.LayerId = layerId;
        }

        public WayPoint(int x, int y, int layerId, int width, int height)
            : this(x, y, layerId)
        {
            this.Width = width;
            this.Height = height;
        }

        public int X { get; set; }
        public int Y { get; set; }
        public int LayerId { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        public int MinWait { get; set; }
        public int MaxWait { get; set; }

        public bool IsWaitPoint { get; set; }
        public bool IsFixSpeedPoint { get; set; }
        public double FixSpeed { get; set; }

        /// <summary>
        /// В этой точке агенты исчезают
        /// </summary>
        public bool IsOutput { get; set; }

        /// <summary>
        /// В этой точке агенты появляются
        /// </summary>
        public bool IsInput { get; set; }

        /// <summary>
        /// Это точка взаимодействия с сервисом
        /// </summary>
        public bool IsServicePoint { get; set; }

        /// <summary>
        /// Точка, связанная с другой точкой на другом слое
        /// </summary>
        public bool IsLinked { get; set; }

        public WayPoint LinkedPoint { get; set; }

        private ulong? _serviceId;
        public ulong? ServiceId
        {
            get { return _serviceId; }
            set { _serviceId = value; }
        }

        public string Name { get; set; }

        public System.Windows.Point Location
        {
            get
            {
                return new System.Windows.Point(X, Y);
            }
            set
            {
                X = Convert.ToInt32(value.X);
                Y = Convert.ToInt32(value.Y);
            }
        }

        public System.Drawing.Point Center
        {
            get { return new System.Drawing.Point(X + Width / 2, Y + Height / 2); }
        }

        public bool Equals(WayPoint other)
        {
            return this.X == other.X &&
                this.Y == other.Y &&
                this.Width == other.Width &&
                this.Height == other.Height &&
                this.LayerId == other.LayerId;
        }

        public object Clone()
        {
            WayPoint wp = new WayPoint()
            {
                X = this.X,
                Y = this.Y,
                LayerId = this.LayerId,
                FixSpeed = this.FixSpeed,
                IsFixSpeedPoint = this.IsFixSpeedPoint,
                IsServicePoint = this.IsServicePoint,
                IsLinked = this.IsLinked,
                LinkedPoint = this.LinkedPoint,
                IsWaitPoint = this.IsWaitPoint,
                Location = this.Location,
                Name = this.Name,
                Height = this.Height,
                Width = this.Width,
                ServiceId = this.ServiceId
            };
            return wp;
        }

        public WayPoint(SerializationInfo info, StreamingContext context)
        {
            foreach (var pv in info)
            {
                switch (pv.Name)
                {
                    case "Name":
                        this.Name = (string)pv.Value;
                        break;
                    case "X":
                        this.X = (int)pv.Value;
                        break;
                    case "Y":
                        this.Y = (int)pv.Value;
                        break;
                    case "LayerId":
                        this.LayerId = (int)pv.Value;
                        break;
                    case "W":
                        this.Width = (int)pv.Value;
                        break;
                    case "H":
                        this.Height = (int)pv.Value;
                        break;
                    case "IsInput":
                        this.IsInput = (bool)pv.Value;
                        break;
                    case "IsOutput":
                        this.IsOutput = (bool)pv.Value;
                        break;
                    case "IsService":
                        this.IsServicePoint = (bool)pv.Value;
                        break;
                    case "ServiceId":
                        this.ServiceId = (ulong)pv.Value;
                        break;
                    case "IsLinked":
                        this.IsLinked = (bool)pv.Value;
                        break;
                    case "LinkedPoint":
                        this.LinkedPoint = (WayPoint)pv.Value;
                        break;
                    default:
                        Console.WriteLine("Неизвестный атрибут в ScenarioModel: " + pv.Name);
                        break;
                }
            }
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Name", this.Name);
            info.AddValue("X", this.X);
            info.AddValue("Y", this.Y);
            info.AddValue("LayerId", this.LayerId);
            info.AddValue("W", this.Width);
            info.AddValue("H", this.Height);
            info.AddValue("IsInput", this.IsInput);
            info.AddValue("IsOutput", this.IsOutput);
            info.AddValue("IsService", this.IsServicePoint);
            if (IsServicePoint)
            {
                info.AddValue("ServiceId", this.ServiceId);
            }
            info.AddValue("IsLinked", this.IsLinked);
            if (IsLinked)
            {
                info.AddValue("LinkedPoint", this.LinkedPoint);
            }
        }

        public override string ToString()
        {
            return string.Format("{0} Слой:{1} [{2},{3}]", Name, LayerId, X, Y);
        }

        public bool Contains(System.Drawing.Point p)
        {
            return p.X >= X & p.X < X + Width & p.Y >= Y & p.Y < Y + Height;
        }
    }
}
