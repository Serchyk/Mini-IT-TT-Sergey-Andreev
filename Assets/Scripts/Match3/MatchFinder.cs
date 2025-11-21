using System.Collections.Generic;
using MiniIT.CORE;

namespace MiniIT.MATCH3
{
    /// <summary>
    /// Static helper to find matching groups of same ColorId.
    /// </summary>
    public static class MatchFinder
    {
        /// <summary>
        /// Finds a pure horizontal OR pure vertical match. L-shapes are NOT allowed.
        /// </summary>
        public static List<(int x, int y)> FindMatch(GridCore<Gem> grid, int x, int y)
        {
            Gem center = grid.Get(x, y);
            if (center == null)
                return new List<(int x, int y)>();

            int color = center.SpriteId;
            int width = grid.Width;
            int height = grid.Height;

            // --- HORIZONTAL ---
            List<(int x, int y)> hor = new List<(int x, int y)>();
            hor.Add((x, y));

            // left
            for (int i = x - 1; i >= 0; i--)
            {
                Gem g = grid.Get(i, y);
                if (g == null || g.SpriteId != color) break;
                hor.Add((i, y));
            }

            // right
            for (int i = x + 1; i < width; i++)
            {
                Gem g = grid.Get(i, y);
                if (g == null || g.SpriteId != color) break;
                hor.Add((i, y));
            }

            if (hor.Count >= 3)
                return hor;  // ONLY horizontal match


            // --- VERTICAL ---
            List<(int x, int y)> ver = new List<(int x, int y)>();
            ver.Add((x, y));

            // down
            for (int i = y - 1; i >= 0; i--)
            {
                Gem g = grid.Get(x, i);
                if (g == null || g.SpriteId != color) break;
                ver.Add((x, i));
            }

            // up
            for (int i = y + 1; i < height; i++)
            {
                Gem g = grid.Get(x, i);
                if (g == null || g.SpriteId != color) break;
                ver.Add((x, i));
            }

            if (ver.Count >= 3)
                return ver; // ONLY vertical match

            return new List<(int x, int y)>();
        }
    }
}
