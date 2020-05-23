namespace GITT_Analysis
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.button1 = new System.Windows.Forms.Button();
            this.gittFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.lbl_file_loaded = new System.Windows.Forms.Label();
            this.btn_analyze = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(347, 12);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(101, 43);
            this.button1.TabIndex = 0;
            this.button1.Text = "Load GITT File...";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // lbl_file_loaded
            // 
            this.lbl_file_loaded.AutoSize = true;
            this.lbl_file_loaded.Location = new System.Drawing.Point(13, 13);
            this.lbl_file_loaded.Name = "lbl_file_loaded";
            this.lbl_file_loaded.Size = new System.Drawing.Size(97, 15);
            this.lbl_file_loaded.TabIndex = 1;
            this.lbl_file_loaded.Text = "File loaded: none";
            // 
            // btn_analyze
            // 
            this.btn_analyze.Enabled = false;
            this.btn_analyze.Location = new System.Drawing.Point(13, 32);
            this.btn_analyze.Name = "btn_analyze";
            this.btn_analyze.Size = new System.Drawing.Size(97, 23);
            this.btn_analyze.TabIndex = 2;
            this.btn_analyze.Text = "Analyze";
            this.btn_analyze.UseVisualStyleBackColor = true;
            this.btn_analyze.Click += new System.EventHandler(this.btn_analyze_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(460, 347);
            this.Controls.Add(this.btn_analyze);
            this.Controls.Add(this.lbl_file_loaded);
            this.Controls.Add(this.button1);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.OpenFileDialog gittFileDialog;
        private System.Windows.Forms.Label lbl_file_loaded;
        private System.Windows.Forms.Button btn_analyze;
    }
}

