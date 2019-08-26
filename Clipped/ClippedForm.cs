﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace Clipped
{
    public partial class ClippedForm : Form
    {
        public Queue<string> History { get; set; }

        public int HistorySize { get; set; }

        public ClippedForm()
        {
            History = new Queue<string>();
            InitializeComponent();
            InitializeBackgroudWorker();
            backgroundWorker1.RunWorkerAsync();
            this.FormClosing += OnFormClosing;
            HistorySize = 20;
        }

        void InitializeBackgroudWorker()
        {
            backgroundWorker1.DoWork += new DoWorkEventHandler(ClippedWorker);
        }

        public void ClippedWorker(object sender,
            DoWorkEventArgs e)
        {
            while (true)
            {
                this.Invoke((MethodInvoker)delegate
                {
                    var newValue = Clipboard.GetText();
                    if (!History.Contains(newValue) && !string.IsNullOrEmpty(newValue))
                    {
                        History.Enqueue(newValue);
                        if (History.Count > HistorySize && HistorySize != 0)
                        {
                            History.Dequeue();
                        }

                        RecreateForm();
                        CreateDynamicQuickCopy();
                    }
                });
                Thread.Sleep(1000);
            }
        }

        protected override void OnResize(EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                this.ShowInTaskbar = false;
                notifyIcon1.Visible = true;
                this.Hide();
            }
        }

        public void CreateDynamicForm(string text, int position, int size)
        {
            var panel = new Panel();
            var textBox = new TextBox();
            var button = new Button();
            var delete = new Button();
            var save = new Button();
            panel.Location = new System.Drawing.Point(12, 116 * (size - position) + 30);
            panel.Name = "panel" + position;
            panel.Size = new System.Drawing.Size(441, 111);

            textBox.Font = new System.Drawing.Font("Segoe UI Semilight", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            textBox.Location = new System.Drawing.Point(3, 6);
            textBox.Multiline = true;
            textBox.Name = "textBox1";
            textBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            textBox.Size = new System.Drawing.Size(435, 78);
            textBox.Text = text;
            textBox.BackColor = Color.LightYellow;
            panel.Controls.Add(textBox);
 
            button.Location = new System.Drawing.Point(364, 88);
            button.Name = "button1";
            button.Size = new System.Drawing.Size(75, 23);
            button.Text = "Copy";
            button.Click += new EventHandler(Copy_Click);
            button.UseVisualStyleBackColor = true;
            panel.Controls.Add(button);

            delete.Location = new System.Drawing.Point(282, 88);
            delete.Name = "button2";
            delete.Size = new System.Drawing.Size(75, 23);
            delete.Text = "Delete";
            delete.Click += new EventHandler(Delete_Click);
            delete.UseVisualStyleBackColor = true;
            panel.Controls.Add(delete);

            this.Controls.Add(panel);
        }

        public void RecreateForm()
        {
            this.Controls.Clear();
            this.Controls.Add(this.menuStrip1);
            for (var i = History.Count; i > 0; i--)
            {
                CreateDynamicForm(History.ElementAt(i - 1), i, History.Count);
            }

        }

        public void CreateDynamicQuickCopy()
        {
            this.quickCopyToolStripMenuItem.DropDownItems.Clear();
            for (var i = History.Count - 1; i >= 0; i--)
            {
                var optionToolStripMenuItem = new ToolStripMenuItem();
                optionToolStripMenuItem.Name = "optionToolStripMenuItem" + i;
                optionToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
                var text = History.ElementAt(i).Trim().Replace(Environment.NewLine, " ").Length >= 25 ?
                    History.ElementAt(i).Trim().Replace(Environment.NewLine, " ").Substring(0, 25)
                    : History.ElementAt(i).Trim().Replace(Environment.NewLine, " ");
                optionToolStripMenuItem.Text = text;
                optionToolStripMenuItem.AccessibleDescription = History.ElementAt(i);
                optionToolStripMenuItem.Click += new System.EventHandler(this.optionToolStripMenuItem_Click);
                quickCopyToolStripMenuItem.DropDownItems.Add(optionToolStripMenuItem);
            }
            
        }

        private void Copy_Click(object sender, EventArgs e)
        {
            var button = (Button)sender;
            var textBox = (TextBox)button.Parent.Controls.Find("textBox1", true)[0];
            Clipboard.SetText(textBox.Text);
        }

        private void Delete_Click(object sender, EventArgs e)
        {
            var button = (Button)sender;
            var textBox = (TextBox)button.Parent.Controls.Find("textBox1", true)[0];
            this.History = new Queue<string>(this.History.Where(c => c != textBox.Text));
            RecreateForm();
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            Show();
            this.WindowState = FormWindowState.Normal;
            this.ShowInTaskbar = true;
            notifyIcon1.Visible = false;
            RecreateForm();
        }

        private void OnFormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                this.WindowState = FormWindowState.Minimized;
                this.ShowInTaskbar = false;
                notifyIcon1.Visible = true;
                CreateDynamicQuickCopy();
                this.Hide();
                notifyIcon1.ShowBalloonTip(1000);
            }  
        }

        private void clippedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Show();
            this.WindowState = FormWindowState.Normal;
            this.ShowInTaskbar = true;
            notifyIcon1.Visible = false;
            RecreateForm();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var confirmResult = MessageBox.Show("Clipped History when running enables you to retain your clipboard history so that you do not lose any old copied text in case you copy a new text. \n\n " +
                "We neither send nor store any data over network. All your data is stored in a temporary session on your machine. \n\n" +
                "This app is brought to you by unMashed Tech.",
                "About Us", MessageBoxButtons.OK);
        }

        private void exitToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void optionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var option = (ToolStripMenuItem)sender;
            Clipboard.SetText(option.AccessibleDescription);
        }

        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            var option = (ToolStripMenuItem)sender;
            toolStripMenuItem2.Checked = false;
            toolStripMenuItem3.Checked = false;
            toolStripMenuItem4.Checked = false;
            toolStripMenuItem5.Checked = false;
            toolStripMenuItem6.Checked = false;
            option.Checked = true;
            HistorySize = Convert.ToInt16(option.Text);
        }
    }
}
