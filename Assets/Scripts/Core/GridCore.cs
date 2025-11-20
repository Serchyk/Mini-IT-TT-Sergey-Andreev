using System;
using System.Collections.Generic;

namespace MiniIT.CORE
{
    /// <summary>
    /// Generic 2D grid container.
    /// </summary>
    /// <typeparam name="T">Reference type stored in grid.</typeparam>
    public class GridCore<T> where T : class
    {
        private readonly int width;
        private readonly int height;
        private readonly T[,] cells;

        /// <summary>Grid width.</summary>
        public int Width => width;

        /// <summary>Grid height.</summary>
        public int Height => height;

        /// <summary>Construct grid with given size.</summary>
        public GridCore(int width, int height)
        {
            this.width = width;
            this.height = height;
            cells = new T[width, height];
        }

        /// <summary>Get cell or null if out of range.</summary>
        public T Get(int x, int y)
        {
            if (x < 0 || x >= width || y < 0 || y >= height)
            {
                return null;
            }

            return cells[x, y];
        }

        /// <summary>Set cell value (ignore out of range).</summary>
        public void Set(int x, int y, T value)
        {
            if (x < 0 || x >= width || y < 0 || y >= height)
            {
                return;
            }

            cells[x, y] = value;
        }

        /// <summary>Enumerate all cells.</summary>
        public IEnumerable<(int x, int y, T value)> All()
        {
            for (int ix = 0; ix < width; ++ix)
            {
                for (int iy = 0; iy < height; ++iy)
                {
                    yield return (ix, iy, cells[ix, iy]);
                }
            }
        }
    }
}