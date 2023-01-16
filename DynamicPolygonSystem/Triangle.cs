using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamicPolygonSystem
{
    [Serializable]
    public class Triangle : Shape
    {

        private Point _p1, _p2, _p3;


        public Triangle(int x, int y, Color color) : base(x, y, color)
        {
            _p1 = new Point();
            _p2 = new Point();
            _p3 = new Point();
        }
         
        public override void Draw(Graphics Gr)
        {
            List<Point> points = new List<Point>(); // FillPolygon съедает массив точек, ну либо прямоугольник, но тут не прокатит

            // Верхняя вершина треугольника
            _p1.X = _x;
            _p1.Y = _y - _radius;
            // Правая
            _p2.X = _x + (int)(Math.Sqrt(3) * (_radius / 2));
            _p2.Y = _y + (_radius / 2);
             // Левая
            _p3.X = _x - (int)(Math.Sqrt(3) * (_radius / 2));
            _p3.Y = _y + (_radius / 2);


            points.Add(new Point(_p1.X, _p1.Y));
            points.Add(new Point(_p2.X, _p2.Y));
            points.Add(new Point(_p3.X, _p3.Y));
            if (RainbowMode)
            {
                Gr.FillPolygon(new SolidBrush(Color.FromArgb(_rand.Next(0, 250), _rand.Next(0, 250), _rand.Next(0, 250))), points.ToArray());
                Gr.DrawPolygon(new Pen(Color.FromArgb(_rand.Next(0, 255), _rand.Next(0, 255), _rand.Next(0, 255))), points.ToArray());
                
            }
            else
            {
                Gr.FillPolygon(new SolidBrush(_color), points.ToArray());
                Gr.DrawPolygon(new Pen(Color.PaleVioletRed), points.ToArray());
            }
           
        }

        public override bool IsInside(int x0, int y0)
        {
            int[] mults = new int[] {
                ((_p1.X - x0) * (_p2.Y - _p1.Y) - (_p2.X - _p1.X) * (_p1.Y - y0)),
                ((_p2.X - x0) * (_p3.Y - _p2.Y) - (_p3.X - _p2.X) * (_p2.Y - y0)),
                ((_p3.X - x0) * (_p1.Y - _p3.Y) - (_p1.X - _p3.X) * (_p3.Y - y0))
            };

            if ((mults[0] >= 0 && mults[1] >= 0 && mults[2] >= 0) ||
                (mults[0] <= 0 && mults[1] <= 0 && mults[2] <= 0))
                return true;
            else
                return false;
        }
    }

    


    [Serializable]
    public class Square : Shape
    {

        private Point _p1, _p2, _p3, _p4;

        public Square(int x, int y, Color color) : base(x, y, color) { }



        public override void Draw(Graphics Gr)
        {
            int side = (int)(Math.Sqrt(2) * _radius);

            //_isChangedPosition = false;

            // Левая Верхняя
            _p1.X = _x - side / 2;
            _p1.Y = _y - side / 2;
            
            // Правая верхняя
            _p2.X = _p1.X + side;
            _p2.Y = _p1.Y;

            // Правая нижняя
            _p3.X = _p2.X;
            _p3.Y = _p2.Y + side;

            // Левавя нижняя
            _p4.X = _p1.X;
            _p4.Y = _p3.Y;

            Gr.FillRectangle(new SolidBrush(_color), _p1.X, _p1.Y, side, side);
            Gr.DrawRectangle(new Pen(Color.PaleVioletRed), _p1.X, _p1.Y, side, side);
        }

        public override bool IsInside(int x0, int y0)
        {
            if (x0 <= _p2.X &&
                x0 >= _p1.X &&
                y0 >= _p1.Y &&
                y0 <= _p4.Y)
                return true;
            else
                return false;
        }
    }




    [Serializable]
    public class Circle : Shape
    {

        public Circle(int x, int y, Color color) : base(x, y, color) { }

        public override void Draw(Graphics Gr)
        {
            float side = _radius * 2f;


            Gr.FillEllipse(new SolidBrush(_color), (_x - side / 2), (_y - side / 2), side, side);
            Gr.DrawEllipse(new Pen(Color.PaleVioletRed), (_x - side / 2), (_y - side / 2), side, side);
        }

        public override bool IsInside(int x_cursor, int y_cursor)
        {
            if ((x_cursor*x_cursor - 2*x_cursor*_x + _x*_x) + (y_cursor* y_cursor - 2* y_cursor*_y + _y*_y) < _radius* _radius)
                return true;
            else
                return false;
        }
    }
}
