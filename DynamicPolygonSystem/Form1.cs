using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using System.Runtime.Serialization.Formatters.Binary;
using OxyPlot;
using System.Diagnostics;
using System.Reflection;

using PolygonsClassLibrary;


namespace DynamicPolygonSystem
{

    public partial class DynamicPolygon : Form
    {
        #region FIELDS
        private Graphics gr;
        private ColorDialog _colorDialog;
        private List<Shape> _vertices; // Храним вершины
        private List<Shape> _testingVertices;
        private MKS _shell; // Выпуклая оболочка
        private MKS _testingShell;
        private bool _lmbDown;      // Нажата ли ЛКМ
        private bool _refreshShell; // Перерисовывать ли оболочку
        private bool _shake;  // нужно ли трясти
        private bool _isThreadStarted;
        private int _Xclick, _Yclick;// Координаты курсора во время клика (для drag&drop-а)
        private string _stripMenuItem; /// <"TODO"> заменить на Enum!
        private BinaryFormatter _binaryFormatter;
        private RadiusChangeForm radiusChangeForm;
        private string _hullType; // "Graham" - для алгоритма Грехэма, "Defined" - для алгоритма по определению
        private PlotForm plotForm;
        private Random _rand;
        private string _testingAlgo;
        private long _testingResultGraham;
        private long _testingResultDefined;
        private Stack<ICommand> _execute;
        private Stack<ICommand> _unExecute;
        private string _AFresult;
        private MoveVertexCommand moveCommand;
        #endregion

        public bool _isTesting { get; set; }

        public DynamicPolygon()
        {
            InitializeComponent();

            _colorDialog = new ColorDialog();
            _shell = new MKS();
            _vertices = new List<Shape>();
            //_actions = new UndoRedoStack();
            _execute = new Stack<ICommand>();
            _unExecute = new Stack<ICommand>();
            _lmbDown = false;
            _shake = false;
            _isThreadStarted = false;
            _stripMenuItem = "Triangle";
            _hullType = "Graham";
            _AFresult = "";
            timer1.Interval = 50;
            _rand = new Random();
            _isTesting = false;
            _testingAlgo = "";
            _testingResultGraham = 0;
            _testingResultDefined = 0;
        }


        private void Form1_Load(object sender, EventArgs e)
        {
            //MessageBox.Show("Here is a\nSUPER EARLY NOT FINISHED UGLY-BUT-FUNCTIONAL\nversion");
            this.KeyPreview = true;
            this.menuStrip1.BackColor = System.Drawing.ColorTranslator.FromHtml("#E6D7DC");
            this.panel1.BackColor = System.Drawing.ColorTranslator.FromHtml("#B5AAB5");
            Shape._color = System.Drawing.ColorTranslator.FromHtml("#A6644D");
            DefinedHull.color = System.Drawing.ColorTranslator.FromHtml("#3A7C5C");
        }


        private void panel1_Paint(object sender, PaintEventArgs e)
        {
            gr = e.Graphics;

            if (!_isTesting)
            {

                if (_vertices != null)
                {
                    foreach (var vertex in _vertices)
                    {
                        vertex.Draw(gr);
                    }
                }

                if (_vertices.Count > 2)
                {
                    if (_hullType == "Graham")
                    {
                        // МВО Грэхемом-Эндрю
                        if (_refreshShell)
                        {
                            _shell.ConvexHull(_vertices); // Обновление облочки (перестройка и формирование НОВОГО массива точек оболочки)
                        }
                        _shell.Draw(gr); // А тут использование созданного массива из MKS. NB! Его надо создавать ConvexHull-ом прежде чем рисовать

                    }
                    if (_hullType == "Defined")
                    {
                        // МВО по определению за n^3
                        DefinedHull.Draw(gr, _vertices);
                    }
                }



            }
            else
            {
                try
                {
                    if (_testingAlgo == "Graham")
                    {
                        _testingShell = new MKS();
                        _testingShell.ConvexHull(_testingVertices);
                        _testingShell.Draw(gr);
                    }
                    else if (_testingAlgo == "Defined")
                    {
                        DefinedHull.Draw(gr, _testingVertices);
                    }
                    else
                    {
                        throw new Exception("Somethig gone wrong with test drawing");
                    }
                }
                catch (Exception expn)
                {
                    MessageBox.Show(expn.Message);
                }

            }
        }

        private void panel1_MouseDown(object sender, MouseEventArgs e)
        {

            if (e.Button == MouseButtons.Left) //  ЛКМ
            {
                bool clickedVertex = false;
                _lmbDown = true;

                _Xclick = e.X;
                _Yclick = e.Y;

                // Если выбрали вершину:
                Parallel.ForEach(_vertices, vertex =>
                {
                    if (vertex.IsInside(e.X, e.Y))
                    {
                        vertex.IsClicked = true; // Помечаем вершину как кликнутую, дабы потом двигать
                        clickedVertex = true;
                    }
                });
                if (clickedVertex)
                {
                    Shape._isMoving = true;

                    moveCommand = new MoveVertexCommand(_vertices);

                    moveCommand.SetOld();
                    _unExecute.Push(moveCommand);
                }

                {
                    /*foreach (var vertex in _vertices)
                    {
                        if (vertex.IsInside(e.X, e.Y))
                        {
                            vertex.IsClicked = true; // Помечаем вершину как кликнутую, дабы потом двигать
                            clickedVertex = true;
                        }
                    }*/
                }

                if (!clickedVertex) // Если не тыкнули в какую-то из вершин
                {
                    if (_vertices.Count > 2)
                    {
                        switch (_hullType)
                        {
                            case "Defined":
                                if (DefinedHull.IsInside(_vertices, _Xclick, _Yclick))
                                    DefinedHull.isClicked = true;
                                else
                                    DefinedHull.isClicked = false;
                                break;
                            case "Graham":
                                if (_shell.IsInside(_Xclick, _Yclick)) // Если курсор врнутри большого полигона, помечаем
                                    _shell.IsClicked = true;
                                else
                                    _shell.IsClicked = false;
                                break;
                        }
                    }
                    if (!DefinedHull.isClicked && !_shell.IsClicked)
                    {
                        CreateVertexCommand command;

                        switch (_stripMenuItem) // Создаём новую вершину в зависимости от выбранной формы
                        {
                            case "Triangle":
                                command = new CreateVertexCommand(_vertices, new Triangle(e.X, e.Y, Shape._color) { IsClicked = true });
                                command.Execute();
                                _unExecute.Push(command);
                                //_vertices.Add(new Triangle(e.X, e.Y, Shape._color) { IsClicked = true }); // Запихиваем её в список всех вершин (и рисуем)

                                break;
                            case "Square":
                                command = new CreateVertexCommand(_vertices, new Square(e.X, e.Y, Shape._color) { IsClicked = true });
                                command.Execute();
                                _unExecute.Push(command);
                                //_vertices.Add(new Square(e.X, e.Y, Shape._color) { IsClicked = true });

                                break;
                            case "Circle":
                                command = new CreateVertexCommand(_vertices, new Circle(e.X, e.Y, Shape._color) { IsClicked = true });
                                command.Execute();
                                _unExecute.Push(command);
                                //_vertices.Add(new Circle(e.X, e.Y, Shape._color) { IsClicked = true });

                                break;
                        }

                        _refreshShell = true;
                    }
                    else
                    {
                        Shape._isMoving = true;

                        Parallel.ForEach(_vertices, vertex => { vertex.IsClicked = true; });

                        moveCommand = new MoveVertexCommand(_vertices);

                        moveCommand.SetOld();
                        _unExecute.Push(moveCommand);
                    }
                }

                panel1.Refresh();
            }
            else if (e.Button == MouseButtons.Right) //  ПКМ 
            {
                DeleteVertexCommand deleteCommand;

                for (var i = _vertices.Count - 1; i >= 0; --i) // Это чтобы удалить самую новосозданную вершину в случае пересечения
                {
                    if (_vertices[i].IsInside(e.X, e.Y))
                    {
                        deleteCommand = new DeleteVertexCommand(_vertices, _vertices[i]);
                        deleteCommand.Execute();
                        _unExecute.Push(deleteCommand);
                        //_vertices.RemoveAt(i);

                        break;
                    }
                }

            }
            _refreshShell = true;
            panel1.Refresh();
        }

        // Метод, который тупо снимает все флаги
        private void panel1_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (Shape._isMoving)
                    moveCommand.SetNew();

                _lmbDown = false;
                _shell.IsClicked = false;
                _refreshShell = true;

                Parallel.ForEach(_vertices, vertex => { vertex.IsClicked = false; });

                {
                    /*foreach (var vertex in _vertices)
                    {
                        vertex.IsClicked = false;
                    }*/
                }

                if (_vertices.Count > 2)
                {
                    if (_hullType == "Defined")
                    {
                        panel1.Refresh();

                        for (int i = 0; i < _vertices.Count; ++i)
                        {
                            if (!_vertices[i].IsInHull)
                            {
                                _vertices.Remove(_vertices[i]);
                            }
                        }

                    }
                }
                panel1.Refresh();
            }
        }

        private void panel1_MouseMove(object sender, MouseEventArgs e)
        {
            if (_lmbDown)
            {
                bool isAnyVertexClicked = false;
                _refreshShell = false;

                foreach (var vertex in _vertices) // Это если мы выбрали вершину, а мышка оказалась формально внутри МВО, то двигаем именно вершину
                {
                    if (vertex.IsClicked)
                    {
                        vertex.MoveBy(e.X, e.Y, _Xclick, _Yclick);
                        isAnyVertexClicked = true;
                    }
                }
                if (!isAnyVertexClicked)
                {
                    if (_shell.IsClicked && _hullType == "Graham")
                    {
                        _shell.MoveBy(e.X, e.Y, _Xclick, _Yclick);
                    }
                    if (DefinedHull.isClicked && _hullType == "Defined")
                    {
                        foreach (var vertex in _vertices)
                        {
                            vertex.MoveBy(e.X, e.Y, _Xclick, _Yclick);
                        }
                    }
                }

                panel1.Refresh();

                _Yclick = e.Y;
                _Xclick = e.X;
            }
        }


        #region Tool Strip Events

        private void circleToolStripMenuItem1_Click(object sender, EventArgs e) => _stripMenuItem = "Circle";

        private void triangleToolStripMenuItem1_Click(object sender, EventArgs e) => _stripMenuItem = "Triangle";

        private void squareToolStripMenuItem1_Click(object sender, EventArgs e) => _stripMenuItem = "Square";

        private void colorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _colorDialog.ShowDialog();
            Shape._color = _colorDialog.Color;
            Refresh();
        }

        private void backgroundColorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult result = _colorDialog.ShowDialog();

            if (result == DialogResult.OK)
                Shape._BGColor = _colorDialog.Color;

            panel1.BackColor = Shape._BGColor;
        }


        /// <summary>
        /// SERIALIZATION
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void saveToolStripMenuItem_Click(object sender, EventArgs e) => PolygonsClassLibrary.FileAppSys.Save(this._vertices, Shape._color, Shape._radius);//SaveFile();

        private void openToolStripMenuItem_Click(object sender, EventArgs e) => Open();

        void Open()
        {
            BinaryFormatter bf = new BinaryFormatter();

            this._vertices = (List<Shape>)(PolygonsClassLibrary.FileAppSys.Open(this._vertices, bf));
            Shape._color = (Color)(PolygonsClassLibrary.FileAppSys.Open(Shape._color, bf));
            Shape._radius = (int)(PolygonsClassLibrary.FileAppSys.Open(this._vertices, bf));

            Refresh();
        }
        void Save()
        {
            BinaryFormatter bf = new BinaryFormatter();

            PolygonsClassLibrary.FileAppSys.Save(this._vertices, bf);
            PolygonsClassLibrary.FileAppSys.Save(Shape._color, bf);
            PolygonsClassLibrary.FileAppSys.Save(Shape._radius, bf);
        }
        void SaveAs()
        {
            BinaryFormatter bf = new BinaryFormatter();

            PolygonsClassLibrary.FileAppSys.SaveAs(this._vertices, bf);
            PolygonsClassLibrary.FileAppSys.SaveAs(Shape._color, bf);
            PolygonsClassLibrary.FileAppSys.SaveAs(Shape._radius, bf);
        }
    
        // Через горячие клавиши
        private void DynamicPolygon_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control == true && e.KeyCode == Keys.O) // Ctrl + O
            {
                Open(); //OpenFile();
            }
            else if (e.Control == true && e.KeyCode == Keys.S && !e.Shift) // Ctrl + S
            {
                Save();  //SaveFile();
            }
            else if (e.Control == true && e.Shift == true && e.KeyCode == Keys.S) // Ctrl + Shift + S
            {
                SaveAs();  //SaveFileAs();
            }
            else if (e.Control == true && e.KeyCode == Keys.Z) // Ctrl + Z
            {
                if (_unExecute.Count > 0)
                {
                    ICommand command = _unExecute.Pop();
                    command.Cancel();
                    _execute.Push(command);

                    _refreshShell = true;
                    panel1.Refresh();
                }
            }
            else if (e.Control == true && e.KeyCode == Keys.R) // Ctrl + R
            {
                if (_execute.Count > 0)
                {
                    ICommand command = _execute.Pop();
                    command.Execute();
                    _unExecute.Push(command);

                    _refreshShell = true;
                    panel1.Refresh();
                }
            }
            else if (e.Control == true && e.KeyCode == Keys.N) // Ctrl + N
            {
                if (_unExecute.Count > 0)
                {
                    AskingForSaveForm askingForm = new AskingForSaveForm();
                    askingForm.Owner = this;

                    askingForm.dAsk += AFButtonClicked;

                    askingForm.ShowDialog();

                    switch (_AFresult)
                    {
                        case "save":
                            if (PolygonsClassLibrary.FileAppSys.Save(this._vertices, Shape._color, Shape._radius) == 0)
                                ResetAllData();

                            break;
                        case "not_save":
                            ResetAllData();

                            break;
                        case "cancel":

                            break;
                        default:

                            break;
                    }
                }
            }
        }

        /*
        private void OpenFile()
        {
            try
            {
                using (var fileDialog = new OpenFileDialog())
                {
                    DialogResult result = fileDialog.ShowDialog();

                    if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fileDialog.FileName))
                    {
                        Shape._projectPath = fileDialog.FileName;

                        _binaryFormatter = new BinaryFormatter();

                        using (FileStream fstream = new FileStream(Shape._projectPath, FileMode.OpenOrCreate))
                        {
                            this._vertices = (List<Shape>)(_binaryFormatter.Deserialize(fstream));
                            Shape._color = (Color)(_binaryFormatter.Deserialize(fstream));
                            Shape._radius = (Int32)(_binaryFormatter.Deserialize(fstream));
                            DefinedHull.beautyMode = (Boolean)(_binaryFormatter.Deserialize(fstream));
                            Shape._BGColor = (Color)(_binaryFormatter.Deserialize(fstream));
                            _refreshShell = true;
                            panel1.Refresh();
                        }
                    }
                    else throw new Exception("Failed open project!");
                }
            }
            catch (Exception expn)
            {
                
            }
        }

        private int SaveFile()
        {
            try
            {
                using (var fileDialog = new SaveFileDialog())
                {
                    if (Shape._projectPath == "")
                    {
                        fileDialog.Filter = "dat files (*.dat)|*.dat";

                        DialogResult result = fileDialog.ShowDialog();

                        if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fileDialog.FileName))
                        {
                            //_projectPath = fileDialog.FileName;
                            Shape._projectPath = fileDialog.FileName;

                            _binaryFormatter = new BinaryFormatter();

                            using (FileStream fstream = new FileStream(Shape._projectPath, FileMode.OpenOrCreate))
                            {
                                _binaryFormatter.Serialize(fstream, this._vertices);
                                _binaryFormatter.Serialize(fstream, Shape._color);
                                _binaryFormatter.Serialize(fstream, Shape._radius);
                                _binaryFormatter.Serialize(fstream, DefinedHull.beautyMode);
                                _binaryFormatter.Serialize(fstream, Shape._BGColor);
                            }
                        }
                        else throw new Exception("Failed save project!");
                    }
                    else
                    {
                        _binaryFormatter = new BinaryFormatter();

                        using (FileStream fstream = new FileStream(Shape._projectPath, FileMode.OpenOrCreate))
                        {
                            _binaryFormatter.Serialize(fstream, this._vertices);
                            _binaryFormatter.Serialize(fstream, Shape._color);
                            _binaryFormatter.Serialize(fstream, Shape._radius);
                            _binaryFormatter.Serialize(fstream, DefinedHull.beautyMode);
                            _binaryFormatter.Serialize(fstream, Shape._BGColor);
                        }
                    }
                }
            }
            catch (Exception expn)
            {
                return -1;
                //MessageBox.Show(expn.Message);
            }
            return 0;
        }

        private void SaveFileAs()
        {
            try
            {
                using (var fileDialog = new SaveFileDialog())
                {

                    fileDialog.Filter = "dat files (*.dat)|*.dat";

                    DialogResult result = fileDialog.ShowDialog();

                    if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fileDialog.FileName))
                    {
                        //_projectPath = fileDialog.FileName;
                        Shape._projectPath = fileDialog.FileName;

                        _binaryFormatter = new BinaryFormatter();

                        using (FileStream fstream = new FileStream(Shape._projectPath, FileMode.OpenOrCreate))
                        {
                            _binaryFormatter.Serialize(fstream, this._vertices);
                            _binaryFormatter.Serialize(fstream, Shape._color);
                            _binaryFormatter.Serialize(fstream, Shape._radius);
                            _binaryFormatter.Serialize(fstream, DefinedHull.beautyMode);
                            _binaryFormatter.Serialize(fstream, Shape._BGColor);
                        }
                    }
                    else throw new Exception("Failed save project!");
                }
            }
            catch (Exception expn)
            {
                //MessageBox.Show(expn.Message);
            }
        }
        */

        /// <summary>
        /// UNDO/REDO FUNCTIONALITY
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cancelButton_Click(object sender, EventArgs e)
        {
            if (_unExecute.Count > 0)
            {
                ICommand command = _unExecute.Pop();
                command.Cancel();
                _execute.Push(command);

                panel1.Refresh();
            }
        }

        private void redoButton_Click(object sender, EventArgs e)
        {
            if (_execute.Count > 0)
            {
                ICommand command = _execute.Pop();
                command.Execute();
                _unExecute.Push(command);

                panel1.Refresh();
            }
        }


        private void grahamToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _hullType = "Graham";
            grahamToolStripMenuItem.Checked = true;
            byDefenitionToolStripMenuItem.Checked = false;
            Refresh();
        }

        private void byDefenitionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _hullType = "Defined";
            grahamToolStripMenuItem.Checked = false;
            byDefenitionToolStripMenuItem.Checked = true;
            Refresh();
        }

        private void toolStripButton_start_Click(object sender, EventArgs e)
        {
            timer1.Start();
            toolStripButton_stop.Checked = false;
        }

        private void toolStripButton_stop_Click(object sender, EventArgs e)
        {
            timer1.Stop();
            toolStripButton_start.Checked = false;
            toolStripButton_stop.Checked = false;
        }

        private void analysesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            plotForm = new PlotForm(this);

            plotForm.Show();
        }
        #endregion


        private void timer1_Tick(object sender, EventArgs e)
        {
            foreach (var vertex in _vertices)
            {
                vertex.Shake();
            }
            Refresh();
        }


        void OnRadiusChanged(object sender, RadiusEventArgs e)
        {
            Shape._radius = e.Radius;
            Refresh();
        }

        private void radiusToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (!RadiusChangeForm._isCreated)
            {
                radiusChangeForm = new RadiusChangeForm();
                RadiusChangeForm._isCreated = true;
            }

            radiusChangeForm.dRC += OnRadiusChanged;

            if (radiusChangeForm.WindowState == FormWindowState.Minimized)
            {
                radiusChangeForm.WindowState = FormWindowState.Normal;
            }

            radiusChangeForm.Activate();
            radiusChangeForm.Show();
        }


        private void AFButtonClicked(object sender, AskingEventArgs e) => _AFresult = e.Result;

        private void DynamicPolygon_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_unExecute.Count > 0)
            {
                AskingForSaveForm askingForm = new AskingForSaveForm();
                askingForm.Owner = this;

                askingForm.dAsk += AFButtonClicked;

                askingForm.ShowDialog();

                switch (_AFresult)
                {
                    case "save":
                        if (PolygonsClassLibrary.FileAppSys.Save(this._vertices, Shape._color, Shape._radius) == -1)
                            e.Cancel = true;
                        else e.Cancel = false;

                        break;
                    case "not_save":
                        e.Cancel = false;

                        break;
                    case "cancel":
                        e.Cancel = true;

                        break;
                    default:
                        e.Cancel = true;

                        break;
                }
            }
            else e.Cancel = false;
        }

        public void plot_analysesStart()
        {
            _testingVertices = new List<Shape>();
            Stopwatch stopwatch = new Stopwatch();

            for (int i = 0; i < plotForm._verticiesNumber; ++i)
            {
                _testingVertices.Add(new Circle(_rand.Next(0, this.Width), _rand.Next(0, this.Height), Shape._color) { Radius = 0 });
            }

            // Начинаем строить по двум алгоритмам:
            _isTesting = true;

            _testingAlgo = "Graham";

            stopwatch.Start();
            Refresh();
            stopwatch.Stop();
            _testingResultGraham = (stopwatch.ElapsedMilliseconds);

            _testingAlgo = "Defined";

            stopwatch.Start();
            Refresh();
            stopwatch.Stop();
            _testingResultDefined = (stopwatch.ElapsedMilliseconds);

            plotForm.mainForm_AnalysingReady(_testingResultGraham, _testingResultDefined);
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
                DefinedHull.beautyMode = true;
            else
                DefinedHull.beautyMode = false;

            Refresh();
        }

        private void rainbowToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void ResetAllData()
        {
            _vertices = new List<Shape>();
            Shape._projectPath = "";

            Refresh();
        }
        
        // Нужно переписать с нормальными переменными
        private void UsePlugins()
        {
            Assembly ass = Assembly.LoadFile("< plugins path >");
            Type[] types = ass.GetTypes();
            Type type = ass.GetType("< class name from [types] >");
            object instance = Activator.CreateInstance(type);

            // Достать свойство
            PropertyInfo numberPropInfo = type.GetProperty("< number >");
            double value = (double)numberPropInfo.GetValue(instance, null);

            // Запускаем метод
            MethodInfo methodInfo = type.GetMethod("< method name >");
            //А если в плагине перегружен какой-то метод?43
            var methods = type.GetMethods();
            var a = methods[0].GetParameters();
            //if (a[0].GetType() == нужный_тип_параметра && a.Length == нужное_количество_параметров) { OK; }

            Object[] arguments = new object[999999];
            for(int i = 0; i < arguments.Length; ++i)
            {
                arguments[i] = 88888;
            }
            methodInfo.Invoke(instance, arguments);
        }
    }



    // Чтобы не мерцало (у Panel свойство DoubleBuffer можно вытащить только унаследовавшись:) ). см. Дизайнер, создание новой панели, там етот класс, а не Panel.
    public class ClassForDoubleBuffer : Panel
    {
        public ClassForDoubleBuffer() : base()
        {
            DoubleBuffered = true;
        }
    }
}
