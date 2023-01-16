using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamicPolygonSystem
{
    public interface ICommand
    {
        void Execute();
        void Cancel();
    }

    class CreateVertexCommand : ICommand
    {
        Shape _vertex;
        List<Shape> _vertices;
        List<Shape> _verticesRef;


        public CreateVertexCommand(List<Shape> shapes, Shape shape)
        {
            _vertex = shape;
            _verticesRef = shapes;

            _vertices = new List<Shape>();
            _vertices.AddRange(shapes);
        }
    
        public void Cancel()
        {
            if (_vertices.Count > 0)
                _vertices.Remove(_vertex);

            _verticesRef.Clear();
            _verticesRef.AddRange(_vertices);
        }

        public void Execute()
        {
            _vertices.Add(_vertex);

            _verticesRef.Clear();
            _verticesRef.AddRange(_vertices);
        }
    }

    class DeleteVertexCommand : ICommand
    {
        Shape _vertex;
        List<Shape> _vertices;

        public DeleteVertexCommand(List<Shape> shapes, Shape shape)
        {
            _vertex = shape;
            _vertices = shapes;
        }

        public void Cancel()
        {
            _vertices.Add(_vertex);
        }

        public void Execute()
        {
            if (_vertices.Count > 0)
                _vertices.Remove(_vertex);
        }
    }

    class MoveVertexCommand : ICommand
    {
        public List<Shape> VerticesToMove { get; set; }
        //private List<Shape> _verticesRef;
        //private List<Shape> _verticesCopy;

        private System.Drawing.Point Old;
        private System.Drawing.Point New;
        private System.Drawing.Point Shift;


        public MoveVertexCommand(List<Shape> list)
        {
            VerticesToMove = new List<Shape>();
            //_verticesRef = list;
            //_verticesCopy = new List<Shape>();
            //_verticesCopy.AddRange(list);

            Old = new System.Drawing.Point();
            New = new System.Drawing.Point();
            Shift = new System.Drawing.Point();

            foreach (var i in list)
            {
                if (i.IsClicked)
                    VerticesToMove.Add(i);
            }
        }


        public void SetOld()
        {
            Old.X = VerticesToMove[0].X;
            Old.Y = VerticesToMove[0].Y;
        }

        public void SetNew()
        {
            New.X = VerticesToMove[0].X;
            New.Y = VerticesToMove[0].Y;

            SetShift();
        }

        private void SetShift()
        {
            Shift.X = New.X - Old.X;
            Shift.Y = New.Y - Old.Y;
        }

        public void Cancel()
        {
            foreach (var i in VerticesToMove)
            {
                i.X -= Shift.X;
                i.Y -= Shift.Y;
            }
        }

        public void Execute()
        {
            foreach (var i in VerticesToMove)
            {
                i.X += Shift.X;
                i.Y += Shift.Y;
            }
        }
    }

    class ChangeColorCommand : ICommand
    {
        public System.Drawing.Color Color { get; set; }

        public ChangeColorCommand()
        {
            Color = new System.Drawing.Color();
        }

        public void Execute()
        {
            Color = Shape._color;
        }

        public void Cancel()
        {
            Shape._color = Color;
        }
    }

    class ChageRadiusCommand : ICommand
    {
        private int _radius;

        public ChageRadiusCommand(int radius)
        {
            _radius = radius;
        }

        public int Radius
        {
            get => _radius; 
            set => _radius = value;
        }

        public void Cancel()
        {
            Shape._radius = _radius;
        }

        public void Execute()
        {
            _radius = Shape._radius;
        }
    }
}
