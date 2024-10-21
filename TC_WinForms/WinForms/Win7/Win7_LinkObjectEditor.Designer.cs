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
            lblName.Location = new Point(21, 18);
            lblName.Margin = new Padding(2, 0, 2, 0);
            lblName.Name = "lblName";
            lblName.Size = new Size(93, 15);
            lblName.TabIndex = 0;
            lblName.Text = "Наименование:";
            // 
            // txtName
            // 
            txtName.Location = new Point(156, 16);
            txtName.Margin = new Padding(2);
            txtName.Name = "txtName";
            txtName.Size = new Size(223, 23);
            txtName.TabIndex = 1;
            // 
            // txtType
            // 
            txtType.Location = new Point(156, 54);
            txtType.Margin = new Padding(2);
            txtType.Name = "txtType";
            txtType.Size = new Size(223, 23);
            txtType.TabIndex = 3;
            // 
            // lblType
            // 
            lblType.AutoSize = true;
            lblType.Location = new Point(21, 54);
            lblType.Margin = new Padding(2, 0, 2, 0);
            lblType.Name = "lblType";
            lblType.Size = new Size(108, 15);
            lblType.TabIndex = 2;
            lblType.Text = "Тип (исполнение):";
            // 
            // lblUnit
            // 
            lblUnit.AutoSize = true;
            lblUnit.Location = new Point(21, 90);
            lblUnit.Margin = new Padding(2, 0, 2, 0);
            lblUnit.Name = "lblUnit";
            lblUnit.Size = new Size(49, 15);
            lblUnit.TabIndex = 4;
            lblUnit.Text = "Ед. изм.";
            // 
            // txtClassifierCode
            // 
            txtClassifierCode.Location = new Point(156, 162);
            txtClassifierCode.Margin = new Padding(2);
            txtClassifierCode.Name = "txtClassifierCode";
            txtClassifierCode.Size = new Size(223, 23);
            txtClassifierCode.TabIndex = 6;
            // 
            // lblClassifierCode
            // 
            lblClassifierCode.AutoSize = true;
            lblClassifierCode.Location = new Point(21, 162);
            lblClassifierCode.Margin = new Padding(2, 0, 2, 0);
            lblClassifierCode.Name = "lblClassifierCode";
            lblClassifierCode.Size = new Size(87, 15);
            lblClassifierCode.TabIndex = 5;
            lblClassifierCode.Text = "Код в classifier:";
            // 
            // lblDescription
            // 
            lblDescription.AutoSize = true;
            lblDescription.Location = new Point(21, 228);
            lblDescription.Margin = new Padding(2, 0, 2, 0);
            lblDescription.Name = "lblDescription";
            lblDescription.Size = new Size(65, 15);
            lblDescription.TabIndex = 7;
            lblDescription.Text = "Описание:";
            // 
            // cbxUnit
            // 
            cbxUnit.FormattingEnabled = true;
            cbxUnit.Location = new Point(156, 90);
            cbxUnit.Margin = new Padding(2);
            cbxUnit.Name = "cbxUnit";
            cbxUnit.Size = new Size(154, 23);
            cbxUnit.TabIndex = 8;
            // 
            // rtxtDescription
            // 
            rtxtDescription.Location = new Point(156, 228);
            rtxtDescription.Margin = new Padding(2);
            rtxtDescription.Name = "rtxtDescription";
            rtxtDescription.Size = new Size(376, 56);
            rtxtDescription.TabIndex = 9;
            rtxtDescription.Text = "";
            // 
            // dgvLinks
            // 
            dgvLinks.AllowUserToAddRows = false;
            dgvLinks.AllowUserToDeleteRows = false;
            dgvLinks.AllowUserToOrderColumns = true;
            dgvLinks.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvLinks.Location = new Point(156, 372);
            dgvLinks.Margin = new Padding(2);
            dgvLinks.Name = "dgvLinks";
            dgvLinks.RowHeadersWidth = 62;
            dgvLinks.RowTemplate.Height = 33;
            dgvLinks.Size = new Size(374, 114);
            dgvLinks.TabIndex = 10;
            // 
            // lblLinks
            // 
            lblLinks.AutoSize = true;
            lblLinks.Location = new Point(21, 372);
            lblLinks.Margin = new Padding(2, 0, 2, 0);
            lblLinks.Name = "lblLinks";
            lblLinks.Size = new Size(93, 30);
            lblLinks.TabIndex = 11;
            lblLinks.Text = "Ссылки на \r\nпроизводителя:";
            // 
            // btnSave
            // 
            btnSave.Location = new Point(21, 496);
            btnSave.Margin = new Padding(2);
            btnSave.Name = "btnSave";
            btnSave.Size = new Size(121, 40);
            btnSave.TabIndex = 12;
            btnSave.Text = "Сохранить";
            btnSave.UseVisualStyleBackColor = true;
            btnSave.Click += btnSave_Click;
            // 
            // btnClose
            // 
            btnClose.Location = new Point(410, 496);
            btnClose.Margin = new Padding(2);
            btnClose.Name = "btnClose";
            btnClose.Size = new Size(121, 40);
            btnClose.TabIndex = 13;
            btnClose.Text = "Закрыть";
            btnClose.UseVisualStyleBackColor = true;
            btnClose.Click += btnClose_Click;
            // 
            // txtPrice
            // 
            txtPrice.Location = new Point(156, 192);
            txtPrice.Margin = new Padding(2);
            txtPrice.Name = "txtPrice";
            txtPrice.Size = new Size(140, 23);
            txtPrice.TabIndex = 15;
            // 
            // lblPrice
            // 
            lblPrice.AutoSize = true;
            lblPrice.Location = new Point(21, 192);
            lblPrice.Margin = new Padding(2, 0, 2, 0);
            lblPrice.Name = "lblPrice";
            lblPrice.Size = new Size(70, 15);
            lblPrice.TabIndex = 14;
            lblPrice.Text = "Стоимость:";
            // 
            // lblPrice2
            // 
            lblPrice2.AutoSize = true;
            lblPrice2.Location = new Point(298, 197);
            lblPrice2.Margin = new Padding(2, 0, 2, 0);
            lblPrice2.Name = "lblPrice2";
            lblPrice2.Size = new Size(76, 15);
            lblPrice2.TabIndex = 16;
            lblPrice2.Text = "руб.без НДС";
            // 
            // btnAddLink
            // 
            btnAddLink.Location = new Point(21, 406);
            btnAddLink.Margin = new Padding(2);
            btnAddLink.Name = "btnAddLink";
            btnAddLink.Size = new Size(119, 24);
            btnAddLink.TabIndex = 17;
            btnAddLink.Text = "Добавить";
            btnAddLink.UseVisualStyleBackColor = true;
            btnAddLink.Click += btnAddLink_Click;
            // 
            // btnDeleteLink
            // 
            btnDeleteLink.Location = new Point(21, 462);
            btnDeleteLink.Margin = new Padding(2);
            btnDeleteLink.Name = "btnDeleteLink";
            btnDeleteLink.Size = new Size(119, 24);
            btnDeleteLink.TabIndex = 18;
            btnDeleteLink.Text = "Удалить";
            btnDeleteLink.UseVisualStyleBackColor = true;
            btnDeleteLink.Click += btnDeleteLink_Click;
            // 
            // btnEditLink
            // 
            btnEditLink.Location = new Point(21, 434);
            btnEditLink.Margin = new Padding(2);
            btnEditLink.Name = "btnEditLink";
            btnEditLink.Size = new Size(119, 24);
            btnEditLink.TabIndex = 19;
            btnEditLink.Text = "Изменить";
            btnEditLink.UseVisualStyleBackColor = true;
            btnEditLink.Click += btnEditLink_Click;
            // 
            // rtxtManufacturer
            // 
            rtxtManufacturer.Location = new Point(156, 300);
            rtxtManufacturer.Margin = new Padding(2);
            rtxtManufacturer.Name = "rtxtManufacturer";
            rtxtManufacturer.Size = new Size(376, 56);
            rtxtManufacturer.TabIndex = 21;
            rtxtManufacturer.Text = "";
            // 
            // lblManufacturer
            // 
            lblManufacturer.AutoSize = true;
            lblManufacturer.Location = new Point(21, 300);
            lblManufacturer.Margin = new Padding(2, 0, 2, 0);
            lblManufacturer.Name = "lblManufacturer";
            lblManufacturer.Size = new Size(96, 30);
            lblManufacturer.TabIndex = 20;
            lblManufacturer.Text = "Производители:\r\n(поставщики)";
            // 
            // cbxCategory
            // 
            cbxCategory.FormattingEnabled = true;
            cbxCategory.Location = new Point(156, 126);
            cbxCategory.Margin = new Padding(2);
            cbxCategory.Name = "cbxCategory";
            cbxCategory.Size = new Size(223, 23);
            cbxCategory.TabIndex = 23;
            // 
            // lblCategory
            // 
            lblCategory.AutoSize = true;
            lblCategory.Location = new Point(21, 126);
            lblCategory.Margin = new Padding(2, 0, 2, 0);
            lblCategory.Name = "lblCategory";
            lblCategory.Size = new Size(66, 15);
            lblCategory.TabIndex = 22;
            lblCategory.Text = "Категория:";
            // 
            // cbxIsReleased
            // 
            cbxIsReleased.AutoSize = true;
            cbxIsReleased.Location = new Point(402, 17);
            cbxIsReleased.Margin = new Padding(2);
            cbxIsReleased.Name = "cbxIsReleased";
            cbxIsReleased.Size = new Size(101, 19);
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
            pictureBoxImage.Margin = new Padding(3, 2, 3, 2);
            pictureBoxImage.Name = "pictureBoxImage";
            pictureBoxImage.Size = new Size(145, 150);
            pictureBoxImage.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBoxImage.TabIndex = 25;
            pictureBoxImage.TabStop = false;
            // 
            // pnlPictureBox
            // 
            pnlPictureBox.Controls.Add(btnLoadImage);
            pnlPictureBox.Controls.Add(pictureBoxImage);
            pnlPictureBox.Location = new Point(386, 47);
            pnlPictureBox.Margin = new Padding(3, 2, 3, 2);
            pnlPictureBox.Name = "pnlPictureBox";
            pnlPictureBox.Size = new Size(145, 177);
            pnlPictureBox.TabIndex = 26;
            pnlPictureBox.Visible = false;
            // 
            // btnLoadImage
            // 
            btnLoadImage.Location = new Point(0, 150);
            btnLoadImage.Margin = new Padding(3, 2, 3, 2);
            btnLoadImage.Name = "btnLoadImage";
            btnLoadImage.Size = new Size(145, 22);
            btnLoadImage.TabIndex = 26;
            btnLoadImage.Text = "загрузить рисунок";
            btnLoadImage.UseVisualStyleBackColor = true;
            // 
            // Win7_LinkObjectEditor
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(550, 554);
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
            Margin = new Padding(2);
            MaximumSize = new Size(571, 760);
            MinimumSize = new Size(566, 593);
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