﻿namespace lab6
{
    partial class Form2
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
            this.button_ok = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.textBox_x0 = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.textBox_x1 = new System.Windows.Forms.TextBox();
            this.textBox_y0 = new System.Windows.Forms.TextBox();
            this.textBox_y1 = new System.Windows.Forms.TextBox();
            this.breaks_cnt = new System.Windows.Forms.NumericUpDown();
            ((System.ComponentModel.ISupportInitialize)(this.breaks_cnt)).BeginInit();
            this.SuspendLayout();
            // 
            // button_ok
            // 
            this.button_ok.Location = new System.Drawing.Point(121, 162);
            this.button_ok.Name = "button_ok";
            this.button_ok.Size = new System.Drawing.Size(75, 23);
            this.button_ok.TabIndex = 0;
            this.button_ok.Text = "Готово";
            this.button_ok.UseVisualStyleBackColor = true;
            this.button_ok.Click += new System.EventHandler(this.button_ok_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(53, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Функция";
            // 
            // comboBox1
            // 
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Items.AddRange(new object[] {
            "x + y",
            "x^2 + y^2",
            "cos(x^2 + y^2) / (x^2 + y^2 + 1)",
            "sin(x) * cos(y)",
            "sin(x) + cos(y)"});
            this.comboBox1.Location = new System.Drawing.Point(79, 10);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(139, 21);
            this.comboBox1.TabIndex = 2;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(16, 50);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(83, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Диапазон по Х";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(16, 91);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(83, 13);
            this.label3.TabIndex = 5;
            this.label3.Text = "Диапазон по Y";
            // 
            // textBox_x0
            // 
            this.textBox_x0.Location = new System.Drawing.Point(118, 47);
            this.textBox_x0.Name = "textBox_x0";
            this.textBox_x0.Size = new System.Drawing.Size(44, 20);
            this.textBox_x0.TabIndex = 6;
            this.textBox_x0.Text = "-1";
            this.textBox_x0.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBox_KeyPress_double);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(16, 129);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(96, 13);
            this.label4.TabIndex = 8;
            this.label4.Text = "Число разбиений";
            // 
            // textBox_x1
            // 
            this.textBox_x1.Location = new System.Drawing.Point(174, 47);
            this.textBox_x1.Name = "textBox_x1";
            this.textBox_x1.Size = new System.Drawing.Size(44, 20);
            this.textBox_x1.TabIndex = 10;
            this.textBox_x1.Text = "1";
            this.textBox_x1.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBox_KeyPress_double);
            // 
            // textBox_y0
            // 
            this.textBox_y0.Location = new System.Drawing.Point(118, 88);
            this.textBox_y0.Name = "textBox_y0";
            this.textBox_y0.Size = new System.Drawing.Size(44, 20);
            this.textBox_y0.TabIndex = 11;
            this.textBox_y0.Text = "-1";
            this.textBox_y0.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBox_KeyPress_double);
            // 
            // textBox_y1
            // 
            this.textBox_y1.Location = new System.Drawing.Point(174, 88);
            this.textBox_y1.Name = "textBox_y1";
            this.textBox_y1.Size = new System.Drawing.Size(44, 20);
            this.textBox_y1.TabIndex = 12;
            this.textBox_y1.Text = "1";
            this.textBox_y1.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBox_KeyPress_double);
            // 
            // breaks_cnt
            // 
            this.breaks_cnt.Location = new System.Drawing.Point(121, 127);
            this.breaks_cnt.Name = "breaks_cnt";
            this.breaks_cnt.Size = new System.Drawing.Size(41, 20);
            this.breaks_cnt.TabIndex = 9;
            this.breaks_cnt.Value = new decimal(new int[] {
            3,
            0,
            0,
            0});
            // 
            // Form2
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(324, 193);
            this.Controls.Add(this.textBox_y1);
            this.Controls.Add(this.textBox_y0);
            this.Controls.Add(this.textBox_x1);
            this.Controls.Add(this.breaks_cnt);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.textBox_x0);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.comboBox1);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.button_ok);
            this.Name = "Form2";
            this.Text = "Form2";
            ((System.ComponentModel.ISupportInitialize)(this.breaks_cnt)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button_ok;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox comboBox1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBox_x0;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox textBox_x1;
        private System.Windows.Forms.TextBox textBox_y0;
        private System.Windows.Forms.TextBox textBox_y1;
        private System.Windows.Forms.NumericUpDown breaks_cnt;
    }
}