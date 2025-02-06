namespace WindowsFormsApp1
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.dateTimePicker1 = new System.Windows.Forms.DateTimePicker();
            this.groupBoxBefore = new System.Windows.Forms.GroupBox();
            this.radioButton1Subtract = new System.Windows.Forms.RadioButton();
            this.radioButton1NoChange = new System.Windows.Forms.RadioButton();
            this.radioButton1AddHour = new System.Windows.Forms.RadioButton();
            this.groupBoxAfter = new System.Windows.Forms.GroupBox();
            this.radioButton4 = new System.Windows.Forms.RadioButton();
            this.radioButton5 = new System.Windows.Forms.RadioButton();
            this.radioButton6 = new System.Windows.Forms.RadioButton();
            this.btnSubmit = new System.Windows.Forms.Button();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.gbFileSource = new System.Windows.Forms.GroupBox();
            this.rbNinjaTrader = new System.Windows.Forms.RadioButton();
            this.rbTSWebsite = new System.Windows.Forms.RadioButton();
            this.rbTradeStation = new System.Windows.Forms.RadioButton();
            this.lblDatePicker = new System.Windows.Forms.Label();
            this.lblAdjustForDST = new System.Windows.Forms.Label();
            this.groupBoxBefore.SuspendLayout();
            this.groupBoxAfter.SuspendLayout();
            this.gbFileSource.SuspendLayout();
            this.SuspendLayout();
            // 
            // dateTimePicker1
            // 
            this.dateTimePicker1.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.dateTimePicker1.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dateTimePicker1.Location = new System.Drawing.Point(80, 125);
            this.dateTimePicker1.Margin = new System.Windows.Forms.Padding(4);
            this.dateTimePicker1.Name = "dateTimePicker1";
            this.dateTimePicker1.Size = new System.Drawing.Size(380, 38);
            this.dateTimePicker1.TabIndex = 2;
            this.dateTimePicker1.ValueChanged += new System.EventHandler(this.dateTimePicker1_ValueChanged);
            // 
            // groupBoxBefore
            // 
            this.groupBoxBefore.Controls.Add(this.radioButton1Subtract);
            this.groupBoxBefore.Controls.Add(this.radioButton1NoChange);
            this.groupBoxBefore.Controls.Add(this.radioButton1AddHour);
            this.groupBoxBefore.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBoxBefore.Location = new System.Drawing.Point(1017, 125);
            this.groupBoxBefore.Margin = new System.Windows.Forms.Padding(4);
            this.groupBoxBefore.Name = "groupBoxBefore";
            this.groupBoxBefore.Padding = new System.Windows.Forms.Padding(4);
            this.groupBoxBefore.Size = new System.Drawing.Size(377, 198);
            this.groupBoxBefore.TabIndex = 7;
            this.groupBoxBefore.TabStop = false;
            this.groupBoxBefore.Text = "Before";
            // 
            // radioButton1Subtract
            // 
            this.radioButton1Subtract.AutoSize = true;
            this.radioButton1Subtract.Location = new System.Drawing.Point(20, 139);
            this.radioButton1Subtract.Margin = new System.Windows.Forms.Padding(4);
            this.radioButton1Subtract.Name = "radioButton1Subtract";
            this.radioButton1Subtract.Size = new System.Drawing.Size(288, 35);
            this.radioButton1Subtract.TabIndex = 7;
            this.radioButton1Subtract.TabStop = true;
            this.radioButton1Subtract.Text = "Subtract One Hour";
            this.radioButton1Subtract.UseVisualStyleBackColor = true;
            this.radioButton1Subtract.CheckedChanged += new System.EventHandler(this.radioButton1Subtract_CheckedChanged);
            // 
            // radioButton1NoChange
            // 
            this.radioButton1NoChange.AutoSize = true;
            this.radioButton1NoChange.Location = new System.Drawing.Point(20, 95);
            this.radioButton1NoChange.Margin = new System.Windows.Forms.Padding(4);
            this.radioButton1NoChange.Name = "radioButton1NoChange";
            this.radioButton1NoChange.Size = new System.Drawing.Size(185, 35);
            this.radioButton1NoChange.TabIndex = 6;
            this.radioButton1NoChange.TabStop = true;
            this.radioButton1NoChange.Text = "No change";
            this.radioButton1NoChange.UseVisualStyleBackColor = true;
            this.radioButton1NoChange.CheckedChanged += new System.EventHandler(this.radioButton1NoChange_CheckedChanged);
            // 
            // radioButton1AddHour
            // 
            this.radioButton1AddHour.AutoSize = true;
            this.radioButton1AddHour.Location = new System.Drawing.Point(20, 49);
            this.radioButton1AddHour.Margin = new System.Windows.Forms.Padding(4);
            this.radioButton1AddHour.Name = "radioButton1AddHour";
            this.radioButton1AddHour.Size = new System.Drawing.Size(229, 35);
            this.radioButton1AddHour.TabIndex = 5;
            this.radioButton1AddHour.TabStop = true;
            this.radioButton1AddHour.Text = "Add One Hour";
            this.radioButton1AddHour.UseVisualStyleBackColor = true;
            this.radioButton1AddHour.CheckedChanged += new System.EventHandler(this.radioButton1_CheckedChanged);
            // 
            // groupBoxAfter
            // 
            this.groupBoxAfter.Controls.Add(this.radioButton4);
            this.groupBoxAfter.Controls.Add(this.radioButton5);
            this.groupBoxAfter.Controls.Add(this.radioButton6);
            this.groupBoxAfter.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBoxAfter.Location = new System.Drawing.Point(1017, 370);
            this.groupBoxAfter.Margin = new System.Windows.Forms.Padding(4);
            this.groupBoxAfter.Name = "groupBoxAfter";
            this.groupBoxAfter.Padding = new System.Windows.Forms.Padding(4);
            this.groupBoxAfter.Size = new System.Drawing.Size(377, 198);
            this.groupBoxAfter.TabIndex = 8;
            this.groupBoxAfter.TabStop = false;
            this.groupBoxAfter.Text = "After";
            // 
            // radioButton4
            // 
            this.radioButton4.AutoSize = true;
            this.radioButton4.Location = new System.Drawing.Point(44, 126);
            this.radioButton4.Margin = new System.Windows.Forms.Padding(4);
            this.radioButton4.Name = "radioButton4";
            this.radioButton4.Size = new System.Drawing.Size(288, 35);
            this.radioButton4.TabIndex = 10;
            this.radioButton4.TabStop = true;
            this.radioButton4.Text = "Subtract One Hour";
            this.radioButton4.UseVisualStyleBackColor = true;
            // 
            // radioButton5
            // 
            this.radioButton5.AutoSize = true;
            this.radioButton5.Location = new System.Drawing.Point(44, 82);
            this.radioButton5.Margin = new System.Windows.Forms.Padding(4);
            this.radioButton5.Name = "radioButton5";
            this.radioButton5.Size = new System.Drawing.Size(185, 35);
            this.radioButton5.TabIndex = 9;
            this.radioButton5.TabStop = true;
            this.radioButton5.Text = "No change";
            this.radioButton5.UseVisualStyleBackColor = true;
            // 
            // radioButton6
            // 
            this.radioButton6.AutoSize = true;
            this.radioButton6.Location = new System.Drawing.Point(44, 36);
            this.radioButton6.Margin = new System.Windows.Forms.Padding(4);
            this.radioButton6.Name = "radioButton6";
            this.radioButton6.Size = new System.Drawing.Size(229, 35);
            this.radioButton6.TabIndex = 8;
            this.radioButton6.TabStop = true;
            this.radioButton6.Text = "Add One Hour";
            this.radioButton6.UseVisualStyleBackColor = true;
            // 
            // btnSubmit
            // 
            this.btnSubmit.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSubmit.Location = new System.Drawing.Point(495, 628);
            this.btnSubmit.Margin = new System.Windows.Forms.Padding(4);
            this.btnSubmit.Name = "btnSubmit";
            this.btnSubmit.Size = new System.Drawing.Size(431, 91);
            this.btnSubmit.TabIndex = 11;
            this.btnSubmit.Text = "Submit";
            this.btnSubmit.UseVisualStyleBackColor = true;
            this.btnSubmit.Click += new System.EventHandler(this.btnSubmit_Click);
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            this.openFileDialog1.FileOk += new System.ComponentModel.CancelEventHandler(this.openFileDialog1_FileOk);
            // 
            // gbFileSource
            // 
            this.gbFileSource.Controls.Add(this.rbNinjaTrader);
            this.gbFileSource.Controls.Add(this.rbTSWebsite);
            this.gbFileSource.Controls.Add(this.rbTradeStation);
            this.gbFileSource.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)), true);
            this.gbFileSource.Location = new System.Drawing.Point(80, 252);
            this.gbFileSource.Margin = new System.Windows.Forms.Padding(4);
            this.gbFileSource.Name = "gbFileSource";
            this.gbFileSource.Padding = new System.Windows.Forms.Padding(4);
            this.gbFileSource.Size = new System.Drawing.Size(449, 244);
            this.gbFileSource.TabIndex = 14;
            this.gbFileSource.TabStop = false;
            this.gbFileSource.Text = "File Source";
            // 
            // rbNinjaTrader
            // 
            this.rbNinjaTrader.AutoSize = true;
            this.rbNinjaTrader.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)), true);
            this.rbNinjaTrader.Location = new System.Drawing.Point(0, 65);
            this.rbNinjaTrader.Margin = new System.Windows.Forms.Padding(4);
            this.rbNinjaTrader.Name = "rbNinjaTrader";
            this.rbNinjaTrader.Size = new System.Drawing.Size(290, 35);
            this.rbNinjaTrader.TabIndex = 2;
            this.rbNinjaTrader.TabStop = true;
            this.rbNinjaTrader.Text = "NinjaTrader Export";
            this.rbNinjaTrader.UseVisualStyleBackColor = true;
            this.rbNinjaTrader.CheckedChanged += new System.EventHandler(this.rbNTExport_CheckedChanged_1);
            // 
            // rbTSWebsite
            // 
            this.rbTSWebsite.AutoSize = true;
            this.rbTSWebsite.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)), true);
            this.rbTSWebsite.Location = new System.Drawing.Point(0, 175);
            this.rbTSWebsite.Margin = new System.Windows.Forms.Padding(4);
            this.rbTSWebsite.Name = "rbTSWebsite";
            this.rbTSWebsite.Size = new System.Drawing.Size(334, 35);
            this.rbTSWebsite.TabIndex = 1;
            this.rbTSWebsite.TabStop = true;
            this.rbTSWebsite.Text = "Trade Station Website";
            this.rbTSWebsite.UseVisualStyleBackColor = true;
            this.rbTSWebsite.CheckedChanged += new System.EventHandler(this.rbTSWebsite_CheckedChanged);
            // 
            // rbTradeStation
            // 
            this.rbTradeStation.AutoSize = true;
            this.rbTradeStation.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)), true);
            this.rbTradeStation.Location = new System.Drawing.Point(0, 118);
            this.rbTradeStation.Margin = new System.Windows.Forms.Padding(4);
            this.rbTradeStation.Name = "rbTradeStation";
            this.rbTradeStation.Size = new System.Drawing.Size(280, 35);
            this.rbTradeStation.TabIndex = 0;
            this.rbTradeStation.TabStop = true;
            this.rbTradeStation.Text = "Trade Station App";
            this.rbTradeStation.UseVisualStyleBackColor = true;
            this.rbTradeStation.CheckedChanged += new System.EventHandler(this.rbTradeStation_CheckedChanged);
            // 
            // lblDatePicker
            // 
            this.lblDatePicker.AutoSize = true;
            this.lblDatePicker.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblDatePicker.Location = new System.Drawing.Point(111, 28);
            this.lblDatePicker.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblDatePicker.Name = "lblDatePicker";
            this.lblDatePicker.Size = new System.Drawing.Size(226, 44);
            this.lblDatePicker.TabIndex = 15;
            this.lblDatePicker.Text = "Select Date";
            // 
            // lblAdjustForDST
            // 
            this.lblAdjustForDST.AutoSize = true;
            this.lblAdjustForDST.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblAdjustForDST.Location = new System.Drawing.Point(1053, 28);
            this.lblAdjustForDST.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblAdjustForDST.Name = "lblAdjustForDST";
            this.lblAdjustForDST.Size = new System.Drawing.Size(279, 44);
            this.lblAdjustForDST.TabIndex = 16;
            this.lblAdjustForDST.Text = "Adjust for DST";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1727, 921);
            this.Controls.Add(this.lblAdjustForDST);
            this.Controls.Add(this.lblDatePicker);
            this.Controls.Add(this.gbFileSource);
            this.Controls.Add(this.btnSubmit);
            this.Controls.Add(this.groupBoxAfter);
            this.Controls.Add(this.groupBoxBefore);
            this.Controls.Add(this.dateTimePicker1);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "Form1";
            this.Text = "Form1";
            this.groupBoxBefore.ResumeLayout(false);
            this.groupBoxBefore.PerformLayout();
            this.groupBoxAfter.ResumeLayout(false);
            this.groupBoxAfter.PerformLayout();
            this.gbFileSource.ResumeLayout(false);
            this.gbFileSource.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.DateTimePicker dateTimePicker1;
        private System.Windows.Forms.GroupBox groupBoxBefore;
        private System.Windows.Forms.RadioButton radioButton1Subtract;
        private System.Windows.Forms.RadioButton radioButton1NoChange;
        private System.Windows.Forms.RadioButton radioButton1AddHour;
        private System.Windows.Forms.GroupBox groupBoxAfter;
        private System.Windows.Forms.RadioButton radioButton4;
        private System.Windows.Forms.RadioButton radioButton5;
        private System.Windows.Forms.RadioButton radioButton6;
        private System.Windows.Forms.Button btnSubmit;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.GroupBox gbFileSource;
        private System.Windows.Forms.RadioButton rbTSWebsite;
        private System.Windows.Forms.RadioButton rbTradeStation;
        private System.Windows.Forms.Label lblDatePicker;
        private System.Windows.Forms.Label lblAdjustForDST;
        private System.Windows.Forms.RadioButton rbNinjaTrader;
    }
}

