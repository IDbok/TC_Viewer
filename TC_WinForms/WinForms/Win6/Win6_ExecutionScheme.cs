using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TC_WinForms.DataProcessing;
using TC_WinForms.Interfaces;
using TcModels.Models;
using TcModels.Models.Interfaces;

namespace TC_WinForms.WinForms
{
    public partial class Win6_ExecutionScheme : Form, IViewModeable, ISaveEventForm
    {
        private TechnologicalCard _tc;
        private bool _isViewMode = true;

        public bool HasChanges { get; private set; } = false;

        public bool CloseFormsNoSave { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public Win6_ExecutionScheme(TechnologicalCard tc, bool viewerMode = false)
        {
            _tc = tc;
            _isViewMode = viewerMode;

            InitializeComponent();
        }

        private void Win6_ExecutionScheme_Load(object sender, EventArgs e)
        {
            SetViewMode(_isViewMode);

            if (_tc.ExecutionScheme != null)
            {
                DisplayImage(_tc.ExecutionScheme, pictureBoxExecutionScheme);
            }

            
        }
        public void SetViewMode(bool? isViewMode)
        {
            if (isViewMode != null)
            {
                _isViewMode = (bool)isViewMode;
            }

            if (_isViewMode)
            {
                btnUploadExecutionScheme.Visible = false;
            }
        }
        void DisplayImage(byte[] imageBytes, PictureBox pictureBox)
        {
            using (var ms = new MemoryStream(imageBytes))
            {
                pictureBox.Image = Image.FromStream(ms);
                pictureBox.SizeMode = PictureBoxSizeMode.Zoom;
            }
        }
        private void btnUploadExecutionScheme_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp";
                openFileDialog.Title = "Выберите изображение";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    _tc.ExecutionScheme = File.ReadAllBytes(openFileDialog.FileName);

                    DisplayImage(_tc.ExecutionScheme, pictureBoxExecutionScheme);

                    HasChanges = true;
                }
            }
        }

        public bool GetDontSaveData()
        {
            throw new NotImplementedException();
        }

        public async Task SaveChanges()
        {
            var dbCon = new DbConnector();
            await dbCon.UpdateTcExecutionScheme(_tc.Id, _tc.ExecutionScheme!);
        }
    }
}
