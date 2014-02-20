namespace PuppetMaster
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
            this.LoadScript = new System.Windows.Forms.Button();
            this.Run = new System.Windows.Forms.Button();
            this.Next = new System.Windows.Forms.Button();
            this.Display = new System.Windows.Forms.Label();
            this.file_location = new System.Windows.Forms.TextBox();
            this.ScriptFileLabel = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // LoadScript
            // 
            this.LoadScript.Location = new System.Drawing.Point(28, 209);
            this.LoadScript.Name = "LoadScript";
            this.LoadScript.Size = new System.Drawing.Size(75, 23);
            this.LoadScript.TabIndex = 0;
            this.LoadScript.Text = "Load Script";
            this.LoadScript.UseVisualStyleBackColor = true;
            this.LoadScript.Click += new System.EventHandler(this.button1_Click);
            // 
            // Run
            // 
            this.Run.Location = new System.Drawing.Point(127, 202);
            this.Run.Name = "Run";
            this.Run.Size = new System.Drawing.Size(73, 36);
            this.Run.TabIndex = 1;
            this.Run.Text = "run loaded script";
            this.Run.UseVisualStyleBackColor = true;
            this.Run.Click += new System.EventHandler(this.button2_Click);
            // 
            // Next
            // 
            this.Next.Location = new System.Drawing.Point(228, 202);
            this.Next.Name = "Next";
            this.Next.Size = new System.Drawing.Size(75, 36);
            this.Next.TabIndex = 2;
            this.Next.Text = "next step in loaded script";
            this.Next.UseVisualStyleBackColor = true;
            this.Next.Click += new System.EventHandler(this.button3_Click);
            // 
            // Display
            // 
            this.Display.AutoSize = true;
            this.Display.Location = new System.Drawing.Point(124, 118);
            this.Display.Name = "Display";
            this.Display.Size = new System.Drawing.Size(67, 13);
            this.Display.TabIndex = 4;
            this.Display.Text = "gsdfgsdfgsdf";
            this.Display.Click += new System.EventHandler(this.label2_Click);
            // 
            // file_location
            // 
            this.file_location.Location = new System.Drawing.Point(100, 71);
            this.file_location.Name = "file_location";
            this.file_location.Size = new System.Drawing.Size(100, 20);
            this.file_location.TabIndex = 5;
            this.file_location.Text = "xsexe";
            // 
            // ScriptFileLabel
            // 
            this.ScriptFileLabel.AutoSize = true;
            this.ScriptFileLabel.Location = new System.Drawing.Point(38, 74);
            this.ScriptFileLabel.Name = "ScriptFileLabel";
            this.ScriptFileLabel.Size = new System.Drawing.Size(51, 13);
            this.ScriptFileLabel.TabIndex = 6;
            this.ScriptFileLabel.Text = "script file:";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(332, 281);
            this.Controls.Add(this.ScriptFileLabel);
            this.Controls.Add(this.file_location);
            this.Controls.Add(this.Display);
            this.Controls.Add(this.Next);
            this.Controls.Add(this.Run);
            this.Controls.Add(this.LoadScript);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button LoadScript;
        private System.Windows.Forms.Button Run;
        private System.Windows.Forms.Button Next;
        private System.Windows.Forms.Label Display;
        private System.Windows.Forms.TextBox file_location;
        private System.Windows.Forms.Label ScriptFileLabel;
    }
}

