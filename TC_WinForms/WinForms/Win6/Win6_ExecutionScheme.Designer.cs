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
            btnDeleteES = new Button();
            ((System.ComponentModel.ISupportInitialize)pictureBoxExecutionScheme).BeginInit();
            SuspendLayout();
            // 
            // pictureBoxExecutionScheme
            // 
            pictureBoxExecutionScheme.BackgroundImageLayout = ImageLayout.None;
            pictureBoxExecutionScheme.Dock = DockStyle.Fill;
            pictureBoxExecutionScheme.Location = new Point(0, 0);
            pictureBoxExecutionScheme.Margin = new Padding(4, 4, 4, 4);
            pictureBoxExecutionScheme.Name = "pictureBoxExecutionScheme";
            pictureBoxExecutionScheme.Size = new Size(735, 731);
            pictureBoxExecutionScheme.TabIndex = 0;
            pictureBoxExecutionScheme.TabStop = false;
            // 
            // btnUploadExecutionScheme
            // 
            btnUploadExecutionScheme.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnUploadExecutionScheme.Location = new Point(604, 0);
            btnUploadExecutionScheme.Margin = new Padding(4, 4, 4, 4);
            btnUploadExecutionScheme.Name = "btnUploadExecutionScheme";
            btnUploadExecutionScheme.Size = new Size(131, 39);
            btnUploadExecutionScheme.TabIndex = 1;
            btnUploadExecutionScheme.Text = "Загрузить";
            btnUploadExecutionScheme.UseVisualStyleBackColor = true;
            btnUploadExecutionScheme.Click += btnUploadExecutionScheme_Click;
            // 
            // btnDeleteES
            // 
            btnDeleteES.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnDeleteES.Location = new Point(604, 692);
            btnDeleteES.Margin = new Padding(4);
            btnDeleteES.Name = "btnDeleteES";
            btnDeleteES.Size = new Size(131, 39);
            btnDeleteES.TabIndex = 2;
            btnDeleteES.Text = "Удалить";
            btnDeleteES.UseVisualStyleBackColor = true;
            btnDeleteES.Click += btnDeleteES_Click;
            // 
            // Win6_ExecutionScheme
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(735, 731);
            Controls.Add(btnDeleteES);
            Controls.Add(btnUploadExecutionScheme);
            Controls.Add(pictureBoxExecutionScheme);
            Margin = new Padding(4, 4, 4, 4);
            Name = "Win6_ExecutionScheme";
            Text = "Win6_ExecutionScheme";
            Load += Win6_ExecutionScheme_Load;
            ((System.ComponentModel.ISupportInitialize)pictureBoxExecutionScheme).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private PictureBox pictureBoxExecutionScheme;
        private Button btnUploadExecutionScheme;
        private Button btnDeleteES;
    }
}