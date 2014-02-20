namespace PuppetMaster
{
    partial class PuppetMasterForm
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
			this.Display = new System.Windows.Forms.Label();
			this.file_location = new System.Windows.Forms.TextBox();
			this.ScriptFileLabel = new System.Windows.Forms.Label();
			this.LaunchClient = new System.Windows.Forms.Button();
			this.LoadScript = new System.Windows.Forms.Button();
			this.Run = new System.Windows.Forms.Button();
			this.Next = new System.Windows.Forms.Button();
			this.LaunchMD = new System.Windows.Forms.Button();
			this.LaunchDS = new System.Windows.Forms.Button();
			this.DSFreeze = new System.Windows.Forms.Button();
			this.DSUnfreeze = new System.Windows.Forms.Button();
			this.Write = new System.Windows.Forms.Button();
			this.Read = new System.Windows.Forms.Button();
			this.FailButton = new System.Windows.Forms.Button();
			this.RecoverButton = new System.Windows.Forms.Button();
			this.ServerComboBox = new System.Windows.Forms.ComboBox();
			this.label1 = new System.Windows.Forms.Label();
			this.debug = new System.Windows.Forms.Label();
			this.ClientWR = new System.Windows.Forms.Label();
			this.ClientsComboBox = new System.Windows.Forms.ComboBox();
			this.textToWrite = new System.Windows.Forms.TextBox();
			this.text = new System.Windows.Forms.Label();
			this.FileToWR = new System.Windows.Forms.Label();
			this.FileToWRTextBox = new System.Windows.Forms.TextBox();
			this.Create = new System.Windows.Forms.Button();
			this.Open = new System.Windows.Forms.Button();
			this.Delete = new System.Windows.Forms.Button();
			this.CloseButton = new System.Windows.Forms.Button();
			this.NumDSTextBox = new System.Windows.Forms.TextBox();
			this.ReadQTextBox = new System.Windows.Forms.TextBox();
			this.WriteQTextBox = new System.Windows.Forms.TextBox();
			this.label2 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			this.label5 = new System.Windows.Forms.Label();
			this.label6 = new System.Windows.Forms.Label();
			this.FileRegistersComboBox = new System.Windows.Forms.ComboBox();
			this.StringRegistersComboBox = new System.Windows.Forms.ComboBox();
			this.WriteReg = new System.Windows.Forms.Button();
			this.DumpTextBox = new System.Windows.Forms.TextBox();
			this.Dump = new System.Windows.Forms.Label();
			this.migration_button = new System.Windows.Forms.Button();
			this.dump_button = new System.Windows.Forms.Button();
			this.DisplayTB = new System.Windows.Forms.TextBox();
			this.DumpClient = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// Display
			// 
			this.Display.AutoSize = true;
			this.Display.Location = new System.Drawing.Point(25, 307);
			this.Display.Name = "Display";
			this.Display.Size = new System.Drawing.Size(0, 13);
			this.Display.TabIndex = 4;
			// 
			// file_location
			// 
			this.file_location.Location = new System.Drawing.Point(54, 145);
			this.file_location.Name = "file_location";
			this.file_location.Size = new System.Drawing.Size(216, 20);
			this.file_location.TabIndex = 5;
			// 
			// ScriptFileLabel
			// 
			this.ScriptFileLabel.AutoSize = true;
			this.ScriptFileLabel.Location = new System.Drawing.Point(16, 148);
			this.ScriptFileLabel.Name = "ScriptFileLabel";
			this.ScriptFileLabel.Size = new System.Drawing.Size(37, 13);
			this.ScriptFileLabel.TabIndex = 6;
			this.ScriptFileLabel.Text = "Input: ";
			// 
			// LaunchClient
			// 
			this.LaunchClient.Location = new System.Drawing.Point(381, 12);
			this.LaunchClient.Name = "LaunchClient";
			this.LaunchClient.Size = new System.Drawing.Size(82, 23);
			this.LaunchClient.TabIndex = 7;
			this.LaunchClient.Text = "LaunchClient";
			this.LaunchClient.UseVisualStyleBackColor = true;
			this.LaunchClient.Click += new System.EventHandler(this.LaunchClient_Click);
			// 
			// LoadScript
			// 
			this.LoadScript.Location = new System.Drawing.Point(39, 171);
			this.LoadScript.Name = "LoadScript";
			this.LoadScript.Size = new System.Drawing.Size(69, 47);
			this.LoadScript.TabIndex = 8;
			this.LoadScript.Text = "Load Script";
			this.LoadScript.UseVisualStyleBackColor = true;
			this.LoadScript.Click += new System.EventHandler(this.LoadScript_Click);
			// 
			// Run
			// 
			this.Run.Location = new System.Drawing.Point(114, 170);
			this.Run.Name = "Run";
			this.Run.Size = new System.Drawing.Size(75, 48);
			this.Run.TabIndex = 9;
			this.Run.Text = "Run Loaded Script";
			this.Run.UseVisualStyleBackColor = true;
			this.Run.Click += new System.EventHandler(this.Run_Click);
			// 
			// Next
			// 
			this.Next.Location = new System.Drawing.Point(195, 171);
			this.Next.Name = "Next";
			this.Next.Size = new System.Drawing.Size(75, 47);
			this.Next.TabIndex = 10;
			this.Next.Text = "Next Step in Loaded Script";
			this.Next.UseVisualStyleBackColor = true;
			this.Next.Click += new System.EventHandler(this.Next_Click);
			// 
			// LaunchMD
			// 
			this.LaunchMD.Location = new System.Drawing.Point(28, 12);
			this.LaunchMD.Name = "LaunchMD";
			this.LaunchMD.Size = new System.Drawing.Size(82, 23);
			this.LaunchMD.TabIndex = 12;
			this.LaunchMD.Text = "LaunchMD";
			this.LaunchMD.UseVisualStyleBackColor = true;
			this.LaunchMD.Click += new System.EventHandler(this.LaunchMD_Click);
			// 
			// LaunchDS
			// 
			this.LaunchDS.Location = new System.Drawing.Point(188, 12);
			this.LaunchDS.Name = "LaunchDS";
			this.LaunchDS.Size = new System.Drawing.Size(82, 23);
			this.LaunchDS.TabIndex = 13;
			this.LaunchDS.Text = "LaunchDS";
			this.LaunchDS.UseVisualStyleBackColor = true;
			this.LaunchDS.Click += new System.EventHandler(this.LaunchDS_Click);
			// 
			// DSFreeze
			// 
			this.DSFreeze.BackColor = System.Drawing.Color.PowderBlue;
			this.DSFreeze.Location = new System.Drawing.Point(114, 100);
			this.DSFreeze.Name = "DSFreeze";
			this.DSFreeze.Size = new System.Drawing.Size(75, 34);
			this.DSFreeze.TabIndex = 18;
			this.DSFreeze.Text = "DS Freeze";
			this.DSFreeze.UseVisualStyleBackColor = false;
			this.DSFreeze.Click += new System.EventHandler(this.DSFreeze_Click);
			// 
			// DSUnfreeze
			// 
			this.DSUnfreeze.Location = new System.Drawing.Point(195, 100);
			this.DSUnfreeze.Name = "DSUnfreeze";
			this.DSUnfreeze.Size = new System.Drawing.Size(75, 34);
			this.DSUnfreeze.TabIndex = 19;
			this.DSUnfreeze.Text = "DS Unfreeze";
			this.DSUnfreeze.UseVisualStyleBackColor = true;
			this.DSUnfreeze.Click += new System.EventHandler(this.DSUnfreeze_Click);
			// 
			// Write
			// 
			this.Write.Location = new System.Drawing.Point(381, 240);
			this.Write.Name = "Write";
			this.Write.Size = new System.Drawing.Size(75, 23);
			this.Write.TabIndex = 20;
			this.Write.Text = "Write";
			this.Write.UseVisualStyleBackColor = true;
			this.Write.Click += new System.EventHandler(this.Write_Click);
			// 
			// Read
			// 
			this.Read.Location = new System.Drawing.Point(462, 240);
			this.Read.Name = "Read";
			this.Read.Size = new System.Drawing.Size(75, 23);
			this.Read.TabIndex = 21;
			this.Read.Text = "Read";
			this.Read.UseVisualStyleBackColor = true;
			this.Read.Click += new System.EventHandler(this.Read_Click);
			// 
			// FailButton
			// 
			this.FailButton.BackColor = System.Drawing.Color.IndianRed;
			this.FailButton.ForeColor = System.Drawing.Color.Black;
			this.FailButton.Location = new System.Drawing.Point(114, 71);
			this.FailButton.Name = "FailButton";
			this.FailButton.Size = new System.Drawing.Size(75, 23);
			this.FailButton.TabIndex = 22;
			this.FailButton.Text = "Fail";
			this.FailButton.UseVisualStyleBackColor = false;
			this.FailButton.Click += new System.EventHandler(this.FailButton_Click);
			// 
			// RecoverButton
			// 
			this.RecoverButton.BackColor = System.Drawing.Color.LightGreen;
			this.RecoverButton.Location = new System.Drawing.Point(195, 71);
			this.RecoverButton.Name = "RecoverButton";
			this.RecoverButton.Size = new System.Drawing.Size(75, 23);
			this.RecoverButton.TabIndex = 23;
			this.RecoverButton.Text = "Recover";
			this.RecoverButton.UseVisualStyleBackColor = false;
			this.RecoverButton.Click += new System.EventHandler(this.RecoverButton_Click);
			// 
			// ServerComboBox
			// 
			this.ServerComboBox.FormattingEnabled = true;
			this.ServerComboBox.Location = new System.Drawing.Point(149, 44);
			this.ServerComboBox.Name = "ServerComboBox";
			this.ServerComboBox.Size = new System.Drawing.Size(121, 21);
			this.ServerComboBox.TabIndex = 24;
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(25, 47);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(115, 13);
			this.label1.TabIndex = 25;
			this.label1.Text = "Server to Fail/Recover";
			// 
			// debug
			// 
			this.debug.AutoSize = true;
			this.debug.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.debug.Location = new System.Drawing.Point(34, 233);
			this.debug.Name = "debug";
			this.debug.Size = new System.Drawing.Size(0, 25);
			this.debug.TabIndex = 26;
			// 
			// ClientWR
			// 
			this.ClientWR.AutoSize = true;
			this.ClientWR.Location = new System.Drawing.Point(295, 47);
			this.ClientWR.Name = "ClientWR";
			this.ClientWR.Size = new System.Drawing.Size(33, 13);
			this.ClientWR.TabIndex = 27;
			this.ClientWR.Text = "Client";
			// 
			// ClientsComboBox
			// 
			this.ClientsComboBox.FormattingEnabled = true;
			this.ClientsComboBox.Location = new System.Drawing.Point(381, 44);
			this.ClientsComboBox.Name = "ClientsComboBox";
			this.ClientsComboBox.Size = new System.Drawing.Size(156, 21);
			this.ClientsComboBox.TabIndex = 28;
			// 
			// textToWrite
			// 
			this.textToWrite.Location = new System.Drawing.Point(417, 212);
			this.textToWrite.Name = "textToWrite";
			this.textToWrite.Size = new System.Drawing.Size(121, 20);
			this.textToWrite.TabIndex = 29;
			// 
			// text
			// 
			this.text.AutoSize = true;
			this.text.Location = new System.Drawing.Point(331, 215);
			this.text.Name = "text";
			this.text.Size = new System.Drawing.Size(72, 13);
			this.text.TabIndex = 30;
			this.text.Text = "Text To Write";
			// 
			// FileToWR
			// 
			this.FileToWR.AutoSize = true;
			this.FileToWR.Location = new System.Drawing.Point(295, 76);
			this.FileToWR.Name = "FileToWR";
			this.FileToWR.Size = new System.Drawing.Size(23, 13);
			this.FileToWR.TabIndex = 31;
			this.FileToWR.Text = "File";
			// 
			// FileToWRTextBox
			// 
			this.FileToWRTextBox.Location = new System.Drawing.Point(381, 73);
			this.FileToWRTextBox.Name = "FileToWRTextBox";
			this.FileToWRTextBox.Size = new System.Drawing.Size(156, 20);
			this.FileToWRTextBox.TabIndex = 32;
			// 
			// Create
			// 
			this.Create.Location = new System.Drawing.Point(381, 125);
			this.Create.Name = "Create";
			this.Create.Size = new System.Drawing.Size(75, 23);
			this.Create.TabIndex = 33;
			this.Create.Text = "Create";
			this.Create.UseVisualStyleBackColor = true;
			this.Create.Click += new System.EventHandler(this.Create_Click);
			// 
			// Open
			// 
			this.Open.Location = new System.Drawing.Point(462, 125);
			this.Open.Name = "Open";
			this.Open.Size = new System.Drawing.Size(75, 23);
			this.Open.TabIndex = 34;
			this.Open.Text = "Open";
			this.Open.UseVisualStyleBackColor = true;
			this.Open.Click += new System.EventHandler(this.Open_Click);
			// 
			// Delete
			// 
			this.Delete.Location = new System.Drawing.Point(381, 154);
			this.Delete.Name = "Delete";
			this.Delete.Size = new System.Drawing.Size(75, 23);
			this.Delete.TabIndex = 35;
			this.Delete.Text = "Delete";
			this.Delete.UseVisualStyleBackColor = true;
			this.Delete.Click += new System.EventHandler(this.Delete_Click);
			// 
			// CloseButton
			// 
			this.CloseButton.Location = new System.Drawing.Point(462, 154);
			this.CloseButton.Name = "CloseButton";
			this.CloseButton.Size = new System.Drawing.Size(75, 23);
			this.CloseButton.TabIndex = 36;
			this.CloseButton.Text = "Close";
			this.CloseButton.UseVisualStyleBackColor = true;
			this.CloseButton.Click += new System.EventHandler(this.CloseButton_Click);
			// 
			// NumDSTextBox
			// 
			this.NumDSTextBox.Location = new System.Drawing.Point(381, 99);
			this.NumDSTextBox.Name = "NumDSTextBox";
			this.NumDSTextBox.Size = new System.Drawing.Size(22, 20);
			this.NumDSTextBox.TabIndex = 37;
			// 
			// ReadQTextBox
			// 
			this.ReadQTextBox.Location = new System.Drawing.Point(450, 99);
			this.ReadQTextBox.Name = "ReadQTextBox";
			this.ReadQTextBox.Size = new System.Drawing.Size(22, 20);
			this.ReadQTextBox.TabIndex = 38;
			// 
			// WriteQTextBox
			// 
			this.WriteQTextBox.Location = new System.Drawing.Point(515, 99);
			this.WriteQTextBox.Name = "WriteQTextBox";
			this.WriteQTextBox.Size = new System.Drawing.Size(22, 20);
			this.WriteQTextBox.TabIndex = 39;
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(331, 102);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(44, 13);
			this.label2.TabIndex = 40;
			this.label2.Text = "NumDB";
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(421, 102);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(23, 13);
			this.label3.TabIndex = 41;
			this.label3.Text = "RQ";
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(483, 102);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(26, 13);
			this.label4.TabIndex = 42;
			this.label4.Text = "WQ";
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Location = new System.Drawing.Point(146, 245);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(70, 13);
			this.label5.TabIndex = 43;
			this.label5.Text = "File Registers";
			// 
			// label6
			// 
			this.label6.AutoSize = true;
			this.label6.Location = new System.Drawing.Point(146, 271);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(81, 13);
			this.label6.TabIndex = 44;
			this.label6.Text = "String Registers";
			// 
			// FileRegistersComboBox
			// 
			this.FileRegistersComboBox.FormattingEnabled = true;
			this.FileRegistersComboBox.Items.AddRange(new object[] {
            "0",
            "1",
            "2",
            "3",
            "4",
            "5",
            "6",
            "7",
            "8",
            "9"});
			this.FileRegistersComboBox.Location = new System.Drawing.Point(244, 242);
			this.FileRegistersComboBox.Name = "FileRegistersComboBox";
			this.FileRegistersComboBox.Size = new System.Drawing.Size(121, 21);
			this.FileRegistersComboBox.TabIndex = 45;
			// 
			// StringRegistersComboBox
			// 
			this.StringRegistersComboBox.FormattingEnabled = true;
			this.StringRegistersComboBox.Items.AddRange(new object[] {
            "0",
            "1",
            "2",
            "3",
            "4",
            "5",
            "6",
            "7",
            "8",
            "9"});
			this.StringRegistersComboBox.Location = new System.Drawing.Point(244, 268);
			this.StringRegistersComboBox.Name = "StringRegistersComboBox";
			this.StringRegistersComboBox.Size = new System.Drawing.Size(121, 21);
			this.StringRegistersComboBox.TabIndex = 46;
			// 
			// WriteReg
			// 
			this.WriteReg.Location = new System.Drawing.Point(381, 266);
			this.WriteReg.Name = "WriteReg";
			this.WriteReg.Size = new System.Drawing.Size(75, 23);
			this.WriteReg.TabIndex = 47;
			this.WriteReg.Text = "WriteReg";
			this.WriteReg.UseVisualStyleBackColor = true;
			this.WriteReg.Click += new System.EventHandler(this.WriteReg_Click);
			// 
			// DumpTextBox
			// 
			this.DumpTextBox.Location = new System.Drawing.Point(28, 322);
			this.DumpTextBox.Multiline = true;
			this.DumpTextBox.Name = "DumpTextBox";
			this.DumpTextBox.ReadOnly = true;
			this.DumpTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.DumpTextBox.Size = new System.Drawing.Size(509, 158);
			this.DumpTextBox.TabIndex = 48;
			// 
			// Dump
			// 
			this.Dump.AutoSize = true;
			this.Dump.Location = new System.Drawing.Point(31, 306);
			this.Dump.Name = "Dump";
			this.Dump.Size = new System.Drawing.Size(41, 13);
			this.Dump.TabIndex = 49;
			this.Dump.Text = "Dump: ";
			// 
			// migration_button
			// 
			this.migration_button.Location = new System.Drawing.Point(28, 70);
			this.migration_button.Name = "migration_button";
			this.migration_button.Size = new System.Drawing.Size(75, 23);
			this.migration_button.TabIndex = 50;
			this.migration_button.Text = "Migration";
			this.migration_button.UseVisualStyleBackColor = true;
			this.migration_button.Click += new System.EventHandler(this.migration_button_Click);
			// 
			// dump_button
			// 
			this.dump_button.Location = new System.Drawing.Point(28, 100);
			this.dump_button.Name = "dump_button";
			this.dump_button.Size = new System.Drawing.Size(75, 23);
			this.dump_button.TabIndex = 51;
			this.dump_button.Text = "Dump";
			this.dump_button.UseVisualStyleBackColor = true;
			this.dump_button.Click += new System.EventHandler(this.dump_button_Click);
			// 
			// DisplayTB
			// 
			this.DisplayTB.Location = new System.Drawing.Point(548, 14);
			this.DisplayTB.Multiline = true;
			this.DisplayTB.Name = "DisplayTB";
			this.DisplayTB.ReadOnly = true;
			this.DisplayTB.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.DisplayTB.Size = new System.Drawing.Size(455, 466);
			this.DisplayTB.TabIndex = 52;
			// 
			// DumpClient
			// 
			this.DumpClient.Location = new System.Drawing.Point(381, 184);
			this.DumpClient.Name = "DumpClient";
			this.DumpClient.Size = new System.Drawing.Size(75, 23);
			this.DumpClient.TabIndex = 53;
			this.DumpClient.Text = "Dump Client";
			this.DumpClient.UseVisualStyleBackColor = true;
			this.DumpClient.Click += new System.EventHandler(this.DumpClient_Click);
			// 
			// PuppetMasterForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(1015, 498);
			this.Controls.Add(this.DumpClient);
			this.Controls.Add(this.DisplayTB);
			this.Controls.Add(this.dump_button);
			this.Controls.Add(this.migration_button);
			this.Controls.Add(this.Dump);
			this.Controls.Add(this.DumpTextBox);
			this.Controls.Add(this.WriteReg);
			this.Controls.Add(this.StringRegistersComboBox);
			this.Controls.Add(this.FileRegistersComboBox);
			this.Controls.Add(this.label6);
			this.Controls.Add(this.label5);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.WriteQTextBox);
			this.Controls.Add(this.ReadQTextBox);
			this.Controls.Add(this.NumDSTextBox);
			this.Controls.Add(this.CloseButton);
			this.Controls.Add(this.Delete);
			this.Controls.Add(this.Open);
			this.Controls.Add(this.Create);
			this.Controls.Add(this.FileToWRTextBox);
			this.Controls.Add(this.FileToWR);
			this.Controls.Add(this.text);
			this.Controls.Add(this.textToWrite);
			this.Controls.Add(this.ClientsComboBox);
			this.Controls.Add(this.ClientWR);
			this.Controls.Add(this.debug);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.ServerComboBox);
			this.Controls.Add(this.RecoverButton);
			this.Controls.Add(this.FailButton);
			this.Controls.Add(this.Read);
			this.Controls.Add(this.Write);
			this.Controls.Add(this.DSUnfreeze);
			this.Controls.Add(this.DSFreeze);
			this.Controls.Add(this.LaunchDS);
			this.Controls.Add(this.LaunchMD);
			this.Controls.Add(this.Next);
			this.Controls.Add(this.Run);
			this.Controls.Add(this.LoadScript);
			this.Controls.Add(this.LaunchClient);
			this.Controls.Add(this.ScriptFileLabel);
			this.Controls.Add(this.file_location);
			this.Controls.Add(this.Display);
			this.Name = "PuppetMasterForm";
			this.Text = "PuppetMaster";
			this.ResumeLayout(false);
			this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label Display;
        private System.Windows.Forms.TextBox file_location;
        private System.Windows.Forms.Label ScriptFileLabel;
        private System.Windows.Forms.Button LaunchClient;
        private System.Windows.Forms.Button LoadScript;
        private System.Windows.Forms.Button Run;
        private System.Windows.Forms.Button Next;
				private System.Windows.Forms.Button LaunchMD;
				private System.Windows.Forms.Button LaunchDS;
                private System.Windows.Forms.Button DSFreeze;
                private System.Windows.Forms.Button DSUnfreeze;
                private System.Windows.Forms.Button Write;
                private System.Windows.Forms.Button Read;
								private System.Windows.Forms.Button FailButton;
								private System.Windows.Forms.Button RecoverButton;
								private System.Windows.Forms.ComboBox ServerComboBox;
								private System.Windows.Forms.Label label1;
								private System.Windows.Forms.Label debug;
                                private System.Windows.Forms.Label ClientWR;
                                private System.Windows.Forms.ComboBox ClientsComboBox;
                                private System.Windows.Forms.TextBox textToWrite;
                                private System.Windows.Forms.Label text;
                                private System.Windows.Forms.Label FileToWR;
                                private System.Windows.Forms.TextBox FileToWRTextBox;
                                private System.Windows.Forms.Button Create;
                                private System.Windows.Forms.Button Open;
                                private System.Windows.Forms.Button Delete;
                                private System.Windows.Forms.Button CloseButton;
                                private System.Windows.Forms.TextBox NumDSTextBox;
                                private System.Windows.Forms.TextBox ReadQTextBox;
                                private System.Windows.Forms.TextBox WriteQTextBox;
                                private System.Windows.Forms.Label label2;
                                private System.Windows.Forms.Label label3;
                                private System.Windows.Forms.Label label4;
																private System.Windows.Forms.Label label5;
																private System.Windows.Forms.Label label6;
																private System.Windows.Forms.ComboBox FileRegistersComboBox;
																private System.Windows.Forms.ComboBox StringRegistersComboBox;
																private System.Windows.Forms.Button WriteReg;
																private System.Windows.Forms.TextBox DumpTextBox;
																private System.Windows.Forms.Label Dump;
																private System.Windows.Forms.Button migration_button;
																private System.Windows.Forms.Button dump_button;
																private System.Windows.Forms.TextBox DisplayTB;
																private System.Windows.Forms.Button DumpClient;
    }
}

