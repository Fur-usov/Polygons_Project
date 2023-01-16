using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace DynamicPolygonSystem
{
    static class DefinedHull
    {
        static public bool isClicked;
        static public bool beautyMode;
        static public Color color;

        static DefinedHull()
        {
            isClicked = false;
            beautyMode = false;
        }

        static public void Draw(Graphics gr, List<Shape> vertices)
        {
            foreach (Shape vertex in vertices)
            {
                vertex.IsInHull = false;
            }

            if (vertices.Count > 2)
            {
                foreach (Shape i in vertices)
                {
                    foreach (Shape j in vertices)
                    {
                        if (i.X == j.X && i.Y == j.Y)
                        {
                            continue;
                        }

                        int dX = i.X - j.X;
                        int dY = j.Y - i.Y;
                        int dVec = j.X * i.Y - j.Y * i.X;

                        int up = 0, down = 0, res = 0;

                        foreach (Shape k in vertices)
                        {
                            if ((k.X == i.X && k.Y == i.Y) || (k.X == j.X && k.Y == j.Y))
                            {
                                continue;
                            }

                            res = dY * k.X + dX * k.Y + dVec;

                            if (res > 0) up++;
                            if (res < 0) down++;
                        }

                        if (down == 0 || up == 0)
                        {
                            i.IsInHull = true;
                            j.IsInHull = true;

                            gr.DrawLine(new Pen(color, 1), i.X, i.Y, j.X, j.Y);

                            if (beautyMode)
                                BeautyMode(gr, i.X, i.Y, j.X, j.Y);
                        }
                    }
                }
            }
        }

        static void BeautyMode(Graphics gr, int x1, int y1, int x2, int y2)
        {
            //Colar
            gr.DrawLine(new Pen(color, 3), x1 + 3, y1, x2 - 3, y2);
            gr.DrawLine(new Pen(color, 3), x1 - 3, y1, x2 + 3, y2);
            gr.DrawLine(new Pen(color, 3), x1 + 6, y1, x2 - 6, y2);
            gr.DrawLine(new Pen(color, 3), x1 - 6, y1, x2 + 6, y2);
            gr.DrawLine(new Pen(color, 3), x1, y1 + 3, x2, y2 - 3);
            gr.DrawLine(new Pen(color, 3), x1, y1 - 3, x2, y2 + 3);
            gr.DrawLine(new Pen(color, 3), x1, y1 + 6, x2, y2 - 6);
            gr.DrawLine(new Pen(color, 3), x1, y1 - 6, x2, y2 + 6);
        }

        static public Boolean IsInside (List<Shape> _verticies, int x0, int y0)
        {
            bool isInside = false;
            int size = _verticies.Count;
            int j = size - 1;

            for (int i = 0; i < size; i++)
            {
                if ((_verticies[i].Y < y0 && _verticies[j].Y >= y0 || _verticies[j].Y < y0 && _verticies[i].Y >= y0) &&
                     (_verticies[i].X + (y0 - _verticies[i].Y) / (float)(_verticies[j].Y - _verticies[i].Y) * (_verticies[j].X - _verticies[i].X) < x0))
                    isInside = !isInside;
                j = i;
            }

            return isInside;
        }
    }
}
