using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DynamicPolygonSystem
{
    public delegate void DelAskingButtonClicked(object sender, AskingEventArgs e);

    public partial class AskingForSaveForm : Form
    {
        public event DelAskingButtonClicked dAsk;

        public AskingForSaveForm()
        {
            InitializeComponent();
            CenterToParent();
        }

        private void saveButton_Click(object sender, EventArgs e)
        {
            dAsk(this, new AskingEventArgs("save"));
            this.Close();
        }

        private void dontsaveButton_Click(object sender, EventArgs e)
        {
            dAsk(this, new AskingEventArgs("not_save"));
            this.Close();
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            dAsk(this, new AskingEventArgs("cancel"));
            this.Close();
        }
    }

    public class AskingEventArgs : EventArgs
    {
        public string Result { get; set; }

        public AskingEventArgs(string result)
        {
            Result = result;
        }
    }
}
