using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;
using OxyPlot;
using OxyPlot.Series;
using OxyPlot.WindowsForms;

namespace DynamicPolygonSystem
{
    public partial class PlotForm : Form
    {
        private PlotView _plot;
        private PlotModel _plotModel;
        private LineSeries _lineSeries;
        private ColumnSeries _columnSeriesGraham;
        private ColumnSeries _columnSeriesDefined;
        private DynamicPolygon _parentForm;
        private string _plotMode;

        public int _verticiesNumber { get; set; }


        public PlotForm(DynamicPolygon parentForm)
        {
            InitializeComponent();

            _plotMode = "Histogram"; // Гистограмма

            _parentForm = parentForm;
            //_parentForm._analysesReady += mainForm_AnalysingReady;

            _plot = new PlotView();
            _plotModel = new PlotModel { Title = "Plot Model" };
            _lineSeries = new LineSeries { Title = "Line series" };
            _columnSeriesGraham = new ColumnSeries { Title = "Graham Algo" };
            _columnSeriesDefined= new ColumnSeries { Title = "Algo by defenition" };

            warningLabel.Visible = false;
            //_plotPoints = new List<DataPoint>();
        }

        private void PlotForm_Load(object sender, EventArgs e)
        {
            
        }


        private void AddToLineSeries(ref List<DataPoint> dataPoints_list)
        {
            _lineSeries.Points.Clear();
            _lineSeries.Points.AddRange(dataPoints_list);
        }

        private void CreatePlot(long grahamTime, long defTime)
        {
            if (_plotMode == "Plot")
            {
                _plotModel.Series.Clear();
                _plotModel.Series.Add(_lineSeries);
                _plot.Model = _plotModel;

                _plot.Dock = System.Windows.Forms.DockStyle.Bottom;
                _plot.Location = new System.Drawing.Point(0, 0);
                _plot.Size = new System.Drawing.Size(500, 500);
                _plot.TabIndex = 0;

                //Add plot control to form
                panel1.Controls.Add(_plot);
            }
            else if (_plotMode == "Histogram")
            {
                ColumnItem columnItem = new ColumnItem(grahamTime);
                _plotModel.Series.Clear();

                _columnSeriesGraham.Items.Add(columnItem);
                _plotModel.Series.Add(_columnSeriesGraham);

                columnItem = new ColumnItem(defTime);
                _columnSeriesDefined.Items.Add(columnItem);
                _plotModel.Series.Add(_columnSeriesDefined);

                _plot.Model = _plotModel;

                _plot.Dock = System.Windows.Forms.DockStyle.Bottom;
                _plot.Location = new System.Drawing.Point(0, 0);
                _plot.Size = new System.Drawing.Size(500, 500);
                _plot.TabIndex = 0;
               //Add plot control to form
                panel1.Controls.Add(_plot);
            }
        }


        public void mainForm_AnalysingReady(long testingResultGraham, long testingResultDefined)
        {
            CreatePlot(testingResultGraham, testingResultDefined);
            warningLabel.Visible = false;
            //MessageBox.Show(testingResultGraham + " " + testingResultDefined);
        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = (e.KeyChar == (char)Keys.Enter);

            if (e.KeyChar != (char)(Keys.Enter))
                return;
            else
            {
                _verticiesNumber = Convert.ToInt32(textBox1.Text);
                warningLabel.Visible = true;
                _parentForm.plot_analysesStart();
            }
        }

        private void PlotForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            _parentForm._isTesting = false;
        }

        private void istogrammaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _plotMode = "Histogram";
        }

        private void plotToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _plotMode = "Plot";
        }
    }
}
