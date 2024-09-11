namespace GrabbedRandomizer
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
            Select_ISO_Button = new Button();
            textBox1 = new TextBox();
            label1 = new Label();
            openFileDialog1 = new OpenFileDialog();
            saveFileDialog1 = new SaveFileDialog();
            CompressButton = new Button();
            decompressLabel = new Label();
            SuspendLayout();
            // 
            // Select_ISO_Button
            // 
            Select_ISO_Button.Location = new Point(755, 12);
            Select_ISO_Button.Name = "Select_ISO_Button";
            Select_ISO_Button.Size = new Size(33, 23);
            Select_ISO_Button.TabIndex = 0;
            Select_ISO_Button.Text = "...";
            Select_ISO_Button.UseVisualStyleBackColor = true;
            Select_ISO_Button.Click += Select_ISO_Button_Click;
            // 
            // textBox1
            // 
            textBox1.Location = new Point(581, 12);
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(168, 23);
            textBox1.TabIndex = 1;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(547, 16);
            label1.Name = "label1";
            label1.Size = new Size(28, 15);
            label1.TabIndex = 2;
            label1.Text = "ISO:";
            // 
            // openFileDialog1
            // 
            openFileDialog1.FileName = "openFileDialog1";
            // 
            // CompressButton
            // 
            CompressButton.Location = new Point(713, 41);
            CompressButton.Name = "CompressButton";
            CompressButton.Size = new Size(75, 23);
            CompressButton.TabIndex = 3;
            CompressButton.Text = "Comrpess";
            CompressButton.UseVisualStyleBackColor = true;
            CompressButton.Click += CompressButton_Click;
            // 
            // decompressLabel
            // 
            decompressLabel.AutoSize = true;
            decompressLabel.Location = new Point(12, 426);
            decompressLabel.Name = "decompressLabel";
            decompressLabel.Size = new Size(38, 15);
            decompressLabel.TabIndex = 4;
            decompressLabel.Text = "label2";
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(decompressLabel);
            Controls.Add(CompressButton);
            Controls.Add(label1);
            Controls.Add(textBox1);
            Controls.Add(Select_ISO_Button);
            Name = "Form1";
            Text = "Form1";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button Select_ISO_Button;
        private TextBox textBox1;
        private Label label1;
        private OpenFileDialog openFileDialog1;
        private SaveFileDialog saveFileDialog1;
        private Button CompressButton;
        private Label decompressLabel;
    }
}