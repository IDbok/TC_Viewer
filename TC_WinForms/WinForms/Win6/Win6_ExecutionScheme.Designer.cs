namespace TC_WinForms.WinForms
{
    partial class Win6_ExecutionScheme
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
            pictureBoxExecutionScheme = new PictureBox();
            btnUploadExecutionScheme = new Button();
            ((System.ComponentModel.ISupportInitialize)pictureBoxExecutionScheme).BeginInit();
            SuspendLayout();
            // 
            // pictureBoxExecutionScheme
            // 
            pictureBoxExecutionScheme.BackgroundImageLayout = ImageLayout.None;
            pictureBoxExecutionScheme.Dock = DockStyle.Fill;
            pictureBoxExecutionScheme.Location = new Point(0, 0);
            pictureBoxExecutionScheme.Name = "pictureBoxExecutionScheme";
            pictureBoxExecutionScheme.Size = new Size(588, 585);
            pictureBoxExecutionScheme.TabIndex = 0;
            pictureBoxExecutionScheme.TabStop = false;
            // 
            // btnUploadExecutionScheme
            // 
            btnUploadExecutionScheme.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnUploadExecutionScheme.Location = new Point(483, 0);
            btnUploadExecutionScheme.Name = "btnUploadExecutionScheme";
            btnUploadExecutionScheme.Size = new Size(105, 31);
            btnUploadExecutionScheme.TabIndex = 1;
            btnUploadExecutionScheme.Text = "Загрузить";
            btnUploadExecutionScheme.UseVisualStyleBackColor = true;
            btnUploadExecutionScheme.Click += btnUploadExecutionScheme_Click;
            // 
            // Win6_ExecutionScheme
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(588, 585);
            Controls.Add(btnUploadExecutionScheme);
            Controls.Add(pictureBoxExecutionScheme);
            Name = "Win6_ExecutionScheme";
            Text = "Win6_ExecutionScheme";
            Load += Win6_ExecutionScheme_Load;
            ((System.ComponentModel.ISupportInitialize)pictureBoxExecutionScheme).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private PictureBox pictureBoxExecutionScheme;
        private Button btnUploadExecutionScheme;
    }
}