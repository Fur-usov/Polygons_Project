using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamicPolygonSystem
{
    public class VerticiesComparer : IComparer<Shape>  // Для сортировки вершин
    {

        public int Compare(Shape s1, Shape s2)
        {
            if (s1.X > s2.X) return 1;
            if (s1.X < s2.X) return -1;
            else if (s1.X == s2.X && s1.Y < s2.Y) return -1;
            else return 0;
        }
    }



    class MKS : Shape // "Мнимальная Выпуклая Оболочка"
    {

        private List<Shape> _verticies; // Будем хранить вершины, по которым строится полигон

        public MKS() : base()
        {
            _verticies = new List<Shape>();
        }

        public override void Draw(Graphics Gr)
        {
            Point[] points = new Point[_verticies.Count];

            for (int i = 0; i < _verticies.Count; ++i)
            {
                points[i] = new Point(_verticies[i].X, _verticies[i].Y);
            }

            Gr.FillPolygon(new SolidBrush(System.Drawing.ColorTranslator.FromHtml("#964661")), points);
        }


        public override bool IsInside(int x0, int y0)                                                                                                       // Подглядел в https://ru.stackoverflow.com/questions/464787/%D0%A2%D0%BE%D1%87%D0%BA%D0%B0-%D0%B2%D0%BD%D1%83%D1%82%D1%80%D0%B8-%D0%BC%D0%BD%D0%BE%D0%B3%D0%BE%D1%83%D0%B3%D0%BE%D0%BB%D1%8C%D0%BD%D0%B8%D0%BA%D0%B0
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


        public new void MoveBy(int x0, int y0, int xt, int yt)
        {
            foreach (var vertex in _verticies)
            {
                vertex.X += (x0 - xt);
                vertex.Y += (y0 - yt);
            }
        }


        // Построение массива вершин оболочки алгоритмом Грэхэма-Эндрю
        // Подглядел в https://e-maxx.ru/algo/convex_hull_graham
        public void ConvexHull(List<Shape> a)
        {
            if (a.Count == 1) return;

            a.Sort(new VerticiesComparer()); // Я таки познал компараторы в шарпе!)

            Shape p1 = a[0];
            Shape p2 = a[a.Count - 1];

            List<Shape> up = new List<Shape>(); // "Собираем" нижнюю и верхнюю половики, из которых склеим полигон
            List<Shape> down = new List<Shape>();
            up.Add(p1);
            down.Add(p1);

            for (int i = 1; i < a.Count; ++i)
            {
                if (i == a.Count - 1 || CW(p1, a[i], p2))
                {
                    while (up.Count >= 2 && !CW(up[up.Count - 2], up[up.Count - 1], a[i]))
                        up.RemoveAt(up.Count-1);
                    up.Add(a[i]);
                }
                if (i == a.Count - 1 || CCW(p1, a[i], p2))
                {
                    while (down.Count >= 2 && !CCW(down[down.Count - 2], down[down.Count - 1], a[i]))
                        down.RemoveAt(down.Count-1);
                    down.Add(a[i]);
                }
            }

            a.Clear();

            for (int i = 0; i < up.Count; ++i)
                a.Add(up[i]);
            for (int i = down.Count - 2; i > 0; --i)
                a.Add(down[i]);

            _verticies = a;
        }


        private bool CW(Shape a, Shape b, Shape c)
        {
            return a.X * (b.Y - c.Y) + b.X * (c.Y - a.Y) + c.X * (a.Y - b.Y) < 0;
        }


        private bool CCW(Shape a, Shape b, Shape  c)
        {
            return a.X * (b.Y - c.Y) + b.X * (c.Y - a.Y) + c.X * (a.Y - b.Y) > 0;
        }
    }
}
