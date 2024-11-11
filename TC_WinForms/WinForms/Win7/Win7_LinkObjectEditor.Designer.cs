namespace TC_WinForms.WinForms
{
    partial class Win7_LinkObjectEditor
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
            lblName = new Label();
            txtName = new TextBox();
            txtType = new TextBox();
            lblType = new Label();
            lblUnit = new Label();
            txtClassifierCode = new TextBox();
            lblClassifierCode = new Label();
            lblDescription = new Label();
            cbxUnit = new ComboBox();
            rtxtDescription = new RichTextBox();
            dgvLinks = new DataGridView();
            lblLinks = new Label();
            btnSave = new Button();
            btnClose = new Button();
            txtPrice = new TextBox();
            lblPrice = new Label();
            lblPrice2 = new Label();
            btnAddLink = new Button();
            btnDeleteLink = new Button();
            btnEditLink = new Button();
            rtxtManufacturer = new RichTextBox();
            lblManufacturer = new Label();
            cbxCategory = new ComboBox();
            lblCategory = new Label();
            cbxIsReleased = new CheckBox();
            pictureBoxImage = new PictureBox();
            pnlPictureBox = new Panel();
            btnLoadImage = new Button();
            ((System.ComponentModel.ISupportInitialize)dgvLinks).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBoxImage).BeginInit();
            pnlPictureBox.SuspendLayout();
            SuspendLayout();
            // 
            // lblName
            // 
            lblName.AutoSize = true;
            lblName.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            lblName.Location = new Point(30, 30);
            lblName.Name = "lblName";
            lblName.Size = new Size(150, 25);
            lblName.TabIndex = 0;
            lblName.Text = "Наименование:";
            // 
            // txtName
            // 
            txtName.Location = new Point(223, 27);
            txtName.Name = "txtName";
            txtName.Size = new Size(317, 31);
            txtName.TabIndex = 1;
            // 
            // txtType
            // 
            txtType.Location = new Point(223, 90);
            txtType.Name = "txtType";
            txtType.Size = new Size(317, 31);
            txtType.TabIndex = 3;
            // 
            // lblType
            // 
            lblType.AutoSize = true;
            lblType.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            lblType.Location = new Point(30, 90);
            lblType.Name = "lblType";
            lblType.Size = new Size(174, 25);
            lblType.TabIndex = 2;
            lblType.Text = "Тип (исполнение):";
            // 
            // lblUnit
            // 
            lblUnit.AutoSize = true;
            lblUnit.Location = new Point(30, 150);
            lblUnit.Name = "lblUnit";
            lblUnit.Size = new Size(75, 25);
            lblUnit.TabIndex = 4;
            lblUnit.Text = "Ед. изм.";
            // 
            // txtClassifierCode
            // 
            txtClassifierCode.Location = new Point(223, 270);
            txtClassifierCode.Name = "txtClassifierCode";
            txtClassifierCode.Size = new Size(317, 31);
            txtClassifierCode.TabIndex = 6;
            // 
            // lblClassifierCode
            // 
            lblClassifierCode.AutoSize = true;
            lblClassifierCode.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            lblClassifierCode.Location = new Point(30, 270);
            lblClassifierCode.Name = "lblClassifierCode";
            lblClassifierCode.Size = new Size(146, 25);
            lblClassifierCode.TabIndex = 5;
            lblClassifierCode.Text = "Код в classifier:";
            // 
            // lblDescription
            // 
            lblDescription.AutoSize = true;
            lblDescription.Location = new Point(30, 380);
            lblDescription.Name = "lblDescription";
            lblDescription.Size = new Size(96, 25);
            lblDescription.TabIndex = 7;
            lblDescription.Text = "Описание:";
            // 
            // cbxUnit
            // 
            cbxUnit.FormattingEnabled = true;
            cbxUnit.Location = new Point(223, 150);
            cbxUnit.Name = "cbxUnit";
            cbxUnit.Size = new Size(218, 33);
            cbxUnit.TabIndex = 8;
            // 
            // rtxtDescription
            // 
            rtxtDescription.Location = new Point(223, 380);
            rtxtDescription.Name = "rtxtDescription";
            rtxtDescription.Size = new Size(535, 91);
            rtxtDescription.TabIndex = 9;
            rtxtDescription.Text = "";
            // 
            // dgvLinks
            // 
            dgvLinks.AllowUserToAddRows = false;
            dgvLinks.AllowUserToDeleteRows = false;
            dgvLinks.AllowUserToOrderColumns = true;
            dgvLinks.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvLinks.Location = new Point(223, 620);
            dgvLinks.Name = "dgvLinks";
            dgvLinks.RowHeadersWidth = 62;
            dgvLinks.RowTemplate.Height = 33;
            dgvLinks.Size = new Size(534, 190);
            dgvLinks.TabIndex = 10;
            // 
            // lblLinks
            // 
            lblLinks.AutoSize = true;
            lblLinks.Location = new Point(30, 620);
            lblLinks.Name = "lblLinks";
            lblLinks.Size = new Size(141, 50);
            lblLinks.TabIndex = 11;
            lblLinks.Text = "Ссылки на \r\nпроизводителя:";
            // 
            // btnSave
            // 
            btnSave.Location = new Point(30, 827);
            btnSave.Name = "btnSave";
            btnSave.Size = new Size(173, 67);
            btnSave.TabIndex = 12;
            btnSave.Text = "Сохранить";
            btnSave.UseVisualStyleBackColor = true;
            btnSave.Click += btnSave_Click;
            // 
            // btnClose
            // 
            btnClose.Location = new Point(586, 827);
            btnClose.Name = "btnClose";
            btnClose.Size = new Size(173, 67);
            btnClose.TabIndex = 13;
            btnClose.Text = "Закрыть";
            btnClose.UseVisualStyleBackColor = true;
            btnClose.Click += btnClose_Click;
            // 
            // txtPrice
            // 
            txtPrice.Location = new Point(223, 320);
            txtPrice.Name = "txtPrice";
            txtPrice.Size = new Size(198, 31);
            txtPrice.TabIndex = 15;
            // 
            // lblPrice
            // 
            lblPrice.AutoSize = true;
            lblPrice.Location = new Point(30, 320);
            lblPrice.Name = "lblPrice";
            lblPrice.Size = new Size(103, 25);
            lblPrice.TabIndex = 14;
            lblPrice.Text = "Стоимость:";
            // 
            // lblPrice2
            // 
            lblPrice2.AutoSize = true;
            lblPrice2.Location = new Point(426, 328);
            lblPrice2.Name = "lblPrice2";
            lblPrice2.Size = new Size(114, 25);
            lblPrice2.TabIndex = 16;
            lblPrice2.Text = "руб.без НДС";
            // 
            // btnAddLink
            // 
            btnAddLink.Location = new Point(30, 677);
            btnAddLink.Name = "btnAddLink";
            btnAddLink.Size = new Size(170, 40);
            btnAddLink.TabIndex = 17;
            btnAddLink.Text = "Добавить";
            btnAddLink.UseVisualStyleBackColor = true;
            btnAddLink.Click += btnAddLink_Click;
            // 
            // btnDeleteLink
            // 
            btnDeleteLink.Location = new Point(30, 770);
            btnDeleteLink.Name = "btnDeleteLink";
            btnDeleteLink.Size = new Size(170, 40);
            btnDeleteLink.TabIndex = 18;
            btnDeleteLink.Text = "Удалить";
            btnDeleteLink.UseVisualStyleBackColor = true;
            btnDeleteLink.Click += btnDeleteLink_Click;
            // 
            // btnEditLink
            // 
            btnEditLink.Location = new Point(30, 723);
            btnEditLink.Name = "btnEditLink";
            btnEditLink.Size = new Size(170, 40);
            btnEditLink.TabIndex = 19;
            btnEditLink.Text = "Изменить";
            btnEditLink.UseVisualStyleBackColor = true;
            btnEditLink.Click += btnEditLink_Click;
            // 
            // rtxtManufacturer
            // 
            rtxtManufacturer.Location = new Point(223, 500);
            rtxtManufacturer.Name = "rtxtManufacturer";
            rtxtManufacturer.Size = new Size(535, 91);
            rtxtManufacturer.TabIndex = 21;
            rtxtManufacturer.Text = "";
            // 
            // lblManufacturer
            // 
            lblManufacturer.AutoSize = true;
            lblManufacturer.Location = new Point(30, 500);
            lblManufacturer.Name = "lblManufacturer";
            lblManufacturer.Size = new Size(145, 50);
            lblManufacturer.TabIndex = 20;
            lblManufacturer.Text = "Производители:\r\n(поставщики)";
            // 
            // cbxCategory
            // 
            cbxCategory.FormattingEnabled = true;
            cbxCategory.Location = new Point(223, 210);
            cbxCategory.Name = "cbxCategory";
            cbxCategory.Size = new Size(317, 33);
            cbxCategory.TabIndex = 23;
            // 
            // lblCategory
            // 
            lblCategory.AutoSize = true;
            lblCategory.Location = new Point(30, 210);
            lblCategory.Name = "lblCategory";
            lblCategory.Size = new Size(99, 25);
            lblCategory.TabIndex = 22;
            lblCategory.Text = "Категория:";
            // 
            // cbxIsReleased
            // 
            cbxIsReleased.AutoSize = true;
            cbxIsReleased.Location = new Point(574, 28);
            cbxIsReleased.Name = "cbxIsReleased";
            cbxIsReleased.Size = new Size(149, 29);
            cbxIsReleased.TabIndex = 24;
            cbxIsReleased.Text = "Опубликован";
            cbxIsReleased.UseVisualStyleBackColor = true;
            cbxIsReleased.CheckedChanged += cbxIsReleased_CheckedChanged;
            // 
            // pictureBoxImage
            // 
            pictureBoxImage.BorderStyle = BorderStyle.FixedSingle;
            pictureBoxImage.Dock = DockStyle.Top;
            pictureBoxImage.Location = new Point(0, 0);
            pictureBoxImage.Margin = new Padding(4, 3, 4, 3);
            pictureBoxImage.Name = "pictureBoxImage";
            pictureBoxImage.Size = new Size(207, 249);
            pictureBoxImage.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBoxImage.TabIndex = 25;
            pictureBoxImage.TabStop = false;
            // 
            // pnlPictureBox
            // 
            pnlPictureBox.Controls.Add(btnLoadImage);
            pnlPictureBox.Controls.Add(pictureBoxImage);
            pnlPictureBox.Location = new Point(551, 78);
            pnlPictureBox.Margin = new Padding(4, 3, 4, 3);
            pnlPictureBox.Name = "pnlPictureBox";
            pnlPictureBox.Size = new Size(207, 295);
            pnlPictureBox.TabIndex = 26;
            pnlPictureBox.Visible = false;
            // 
            // btnLoadImage
            // 
            btnLoadImage.Location = new Point(0, 250);
            btnLoadImage.Margin = new Padding(4, 3, 4, 3);
            btnLoadImage.Name = "btnLoadImage";
            btnLoadImage.Size = new Size(207, 37);
            btnLoadImage.TabIndex = 26;
            btnLoadImage.Text = "загрузить рисунок";
            btnLoadImage.UseVisualStyleBackColor = true;
            // 
            // Win7_LinkObjectEditor
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(784, 952);
            ControlBox = false;
            Controls.Add(pnlPictureBox);
            Controls.Add(cbxIsReleased);
            Controls.Add(cbxCategory);
            Controls.Add(lblCategory);
            Controls.Add(rtxtManufacturer);
            Controls.Add(lblManufacturer);
            Controls.Add(btnEditLink);
            Controls.Add(btnDeleteLink);
            Controls.Add(btnAddLink);
            Controls.Add(lblPrice2);
            Controls.Add(txtPrice);
            Controls.Add(lblPrice);
            Controls.Add(btnClose);
            Controls.Add(btnSave);
            Controls.Add(lblLinks);
            Controls.Add(dgvLinks);
            Controls.Add(rtxtDescription);
            Controls.Add(cbxUnit);
            Controls.Add(lblDescription);
            Controls.Add(txtClassifierCode);
            Controls.Add(lblClassifierCode);
            Controls.Add(lblUnit);
            Controls.Add(txtType);
            Controls.Add(lblType);
            Controls.Add(txtName);
            Controls.Add(lblName);
            MaximumSize = new Size(806, 1229);
            MinimumSize = new Size(799, 951);
            Name = "Win7_LinkObjectEditor";
            Text = "Win7_LinkObjectEditor";
            Load += Win7_LinkObjectEditor_Load;
            ((System.ComponentModel.ISupportInitialize)dgvLinks).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBoxImage).EndInit();
            pnlPictureBox.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label lblName;
        private TextBox txtName;
        private TextBox txtType;
        private Label lblType;
        private Label lblUnit;
        private TextBox txtClassifierCode;
        private Label lblClassifierCode;
        private Label lblDescription;
        private ComboBox cbxUnit;
        private RichTextBox rtxtDescription;
        private DataGridView dgvLinks;
        private Label lblLinks;
        private Button btnSave;
        private Button btnClose;
        private TextBox txtPrice;
        private Label lblPrice;
        private Label lblPrice2;
        private Button btnAddLink;
        private Button btnDeleteLink;
        private Button btnEditLink;
        private RichTextBox rtxtManufacturer;
        private Label lblManufacturer;
        private ComboBox cbxCategory;
        private Label lblCategory;
        private CheckBox cbxIsReleased;
        private PictureBox pictureBoxImage;
        private Panel pnlPictureBox;
        private Button btnLoadImage;
    }
}