using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamicPolygonSystem
{
    [Serializable]
    public abstract class Shape
    {
        public static int _radius; // Радиус окружности, описанной около нашей вершины (а она - правильная геом. фигура)
        public static System.Drawing.Color _color;
        public static System.Drawing.Color _BGColor;
        public static string _projectPath = "";
        public static bool RainbowMode;//Для радуги
        public static bool _isMoving; // Для undo/redo

        protected int _x; // Координаты цетра
        protected int _y;
        protected bool _isClicked; // Нажали ли на вершину ЛКМ
        protected bool _isInHull; // Принадлежит ли оболочке

        [NonSerialized]
        protected Random _rand;
        [NonSerialized]
        private int _rX, _rY;
        [NonSerialized]
        private bool _resetCoord;


        public Shape(int x, int y, System.Drawing.Color color)
        {
            _x = x;
            _y = y;
            _rand = new Random();
            _isClicked = false;
            _resetCoord = false;
            _isInHull = false;
            _color = color;//new System.Drawing.Color();
        }
        public Shape()
        {

        }
        static Shape()
        {
            // TODO вводить радиус через немодальное, цвет через модально-диалоговое окно.
            _isMoving = false;
            _radius = 20;
            _color = System.Drawing.Color.BlanchedAlmond;
            _BGColor = System.Drawing.Color.AntiqueWhite;
        }

        public abstract void Draw(System.Drawing.Graphics Gr);
        public abstract Boolean IsInside(int x0, int y0); // x0 и y0 - координаты курсора во время щелчка ЛКМ
        public void Shake()
        {
            if (_resetCoord)
            {
                _x -= _rX;
                _y -= _rY;
                _resetCoord = false;
            }
            else
            {
                _rX = _rand.Next(-10, 10);
                _rY = _rand.Next(-10, 10);
                _x += _rX;
                _y += _rY;
                _resetCoord = true;
            }
        }
        public void MoveBy(int x0, int y0, int xt, int yt) // Будет сдвигать фигуру, зная все о мыши
        {
            _x += (x0 - xt);
            _y += (y0 - yt);
        } 

        public Int32 Radius
        {
            get
            {
                return _radius;
            }
            set
            {
                try
                {
                    if (value < 1)
                        throw new Exception("Too low radius!");
                    else if (value > 100)
                        throw new Exception("Too large radius!");
                    else
                        _radius = value;
                }
                catch(Exception expn)
                {
                    // MessageBox.Text = expn.Message;
                }
            }
        }

        public Int32 X
        {
            get => _x;
            set => _x = value;
        }

        public Int32 Y
        {
            get => _y;
            set => _y = value;
        }

        public Boolean IsClicked
        {
            get => _isClicked;
            set => _isClicked = value;
        }

        public Boolean IsInHull
        {
            get => _isInHull;
            set => _isInHull = value;
        }
    }
}
