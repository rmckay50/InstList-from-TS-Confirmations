using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static WindowsFormsApp1.Enums;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        public FileSource FileOrigin { get; set; }
        //public bool maleBtn { get; set; }
        public bool tSSource { get; set; }
        public Form1()
        {
            InitializeComponent();
        }
        private DateTimePicker timePicker;

        private void InitializeTimePicker()
        {
            timePicker = new DateTimePicker();
            timePicker.Format = DateTimePickerFormat.Time;
            timePicker.ShowUpDown = true;
            timePicker.Location = new Point(10, 10);
            timePicker.Width = 100;
            Controls.Add(timePicker);
        }
        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void txtName_TextChanged(object sender, EventArgs e)
        {

        }


        private void btnSubmit_Click(object sender, EventArgs e)
        {
   //         string name = txtName.Text;
   //         string address = textAddress.Text;
   //         MessageBox.Show(name + address);

   //         MessageBox.Show("The selected value is " +
   //dateTimePicker1.Text);
   //         if (radioButton1AddHour.Checked == true)
   //         {
   //             MessageBox.Show("You are selected Add !! ");

   //         }
            this.Close();

        }

        private void dateTimePicker1_ValueChanged(object sender, EventArgs e)
        {
            dateTimePicker1.Format = DateTimePickerFormat.Custom;
            // Display the date as "Mon 27 Feb 2012".  
            dateTimePicker1.CustomFormat = " MMM/ dd  yyyy";
            //dateTimePicker1.CustomFormat = "mm/dd/ yyyy";

            //dateTimePicker1.Value = new DateTime(2001, 10, 20);

        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton1AddHour.Checked == true)
            {
                MessageBox.Show("You are selected Add !! ");
                return;
            }
            //else if (radioButton2.Checked == true)
            //{
            //    MessageBox.Show("You are selected Blue !! ");
            //    return;
            //}
            //else
            //{
            //    MessageBox.Show("You are selected Add !! ");
            //    return;
            //}
        }

        private void radioButton1NoChange_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton1NoChange.Checked == true)
            {
                MessageBox.Show("You are selected No Change !! ");
                return;
            }
        }

        private void radioButton1Subtract_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton1Subtract.Checked == true)
            {
                MessageBox.Show("You are selected Subtract !! ");
                return;
            }

        }

        private void openFileDialog1_FileOk(object sender, CancelEventArgs e)
        {

        }


        private void rbTradeStation_CheckedChanged(object sender, EventArgs e)
        {
            if(rbTradeStation.Checked == true)
            { 
                FileOrigin = FileSource.TSApp;
            }
            else
            {
                FileOrigin = FileSource.TSWebsite;
            }
        }

        private void rbTSWebsite_CheckedChanged(object sender, EventArgs e)
        {
            if (rbTradeStation.Checked == true)
            {
                FileOrigin = FileSource.TSApp;
            }
            else
            {
                FileOrigin = FileSource.TSWebsite;
            }

        }
    }
}
