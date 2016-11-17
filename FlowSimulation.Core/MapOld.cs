using System;
using System.Collections.Generic;
using System.Text;

namespace FlowSimulation
{
    [Obsolete]
    public class MapOld
    {
        public const double CellSize = 0.4;

        [Flags]
        public enum CellState : byte
        {
            Closed = 0x80,
            Busy = 0x40,
            Locked = 0x20,
            Input = 0x10,
            Output = 0x08,
            Service = 0x04,
            Reserv4 = 0x02,
            Reserv5 = 0x01
        }

        private byte[,] map;
        private long sizeX;
        private long sizeY;

        public MapOld(int width, int height)
        {
            this.sizeX = width;
            this.sizeY = height;
            map = new byte[sizeX, sizeY];
        }

        public MapOld(byte[,] map)
        {
            this.map = map;
            this.sizeX = map.GetLength(0);
            this.sizeY = map.GetLength(1);
        }

        public byte[,] GetMap()
        {
            return map;
        }

        public void UnlockAllCells()
        {
            for (int i = 0; i < map.GetLength(0); i++)
            {
                for (int j = 0; j < map.GetLength(1); j++)
                {
                    map[i, j] &= 0x9F;
                }
            }
        }

        public void SetMapFrameFlag(CellState flag, bool state, int fromX, int fromY, int width, int height)
        {
            lock (map)
            {
                for (long i = 0; i < width; i++)
                {
                    for (long j = 0; j < height; j++)
                    {
                        if (state)
                        {
                            map[fromX + i, fromY + j] |= (byte)flag;
                        }
                        else
                        {
                            map[fromX + i, fromY + j] ^= (byte)flag;
                        }
                    }
                }
            }
        }

        public byte[,] GetAndLockMapFrame(int fromX, int fromY, int width, int height)
        {
            lock (map)
            {
                byte[,] temp = GetMapFrame(fromX, fromY, width, height);
                SetMapFrameLock(true, fromX, fromY, width, height);
                return temp;
            }
        }

        public byte[,] GetMapFrame(int fromX, int fromY, int width, int height)
        {
            byte[,] temp = new byte[width, height];
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    temp[i, j] = map[fromX + i, fromY + j];
                }
            }
            return temp;
        }

        public void SetMapFrameLock(bool value, int fromX, int fromY, int width, int height)
        {
            lock (map)
            {
                for (int i = 0; i < width; i++)
                {
                    for (int j = 0; j < height; j++)
                    {
                        if (value)
                        {
                            map[fromX + i, fromY + j] |= 0x20;
                        }
                        else
                        {
                            map[fromX + i, fromY + j] &= 0xDF;
                        }
                    }
                }
            }
        }

        public void SetMapCellState(bool value, int x, int y)
        {
            if (x < sizeX && y < sizeY && x >= 0 && y >= 0)
            {
                lock (map)
                {
                    if (value)
                    {
                        map[x, y] |= 0x01;
                    }
                    else
                    {
                        map[x, y] &= 0xFE;
                    }
                }
            }
        }

        public void SetMapCellLock(bool value, int x, int y)
        {
            if (x < sizeX && y < sizeY && x >= 0 && y >= 0)
            {
                lock (map)
                {
                    if (value)
                    {
                        map[x, y] |= 0x20;
                    }
                    else
                    {
                        map[x, y] &= 0xDF;
                    }
                }
            }
        }

        public bool SetMapCellTake(bool value, int x, int y)
        {
            if (x < sizeX && y < sizeY && x >= 0 && y >= 0)
            {
                lock (map)
                {
                    if (value)
                    {
                        if ((map[x, y] & 0xC0) == 0x00)
                        {
                            map[x, y] |= 0x40;
                            return true;
                        }
                    }
                    else
                    {
                        map[x, y] &= 0xBF;
                    }
                }
            }
            return false;
        }

    }
}
