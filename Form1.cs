using Emgu.CV.Structure;
using System;
using System.Drawing;
using System.Web.UI.MobileControls;
using System.Web.UI.WebControls;
using System.Windows.Controls;
using System.Windows.Forms;
using Form = System.Windows.Forms.Form;
using Emgu.CV;
using Emgu.CV.CvEnum;

namespace Lab7
{
    public partial class Form1 : Form
    {

        Image<Bgr, byte> inputImage = null;

        public Form1()
        {
            InitializeComponent();
        }

        private void btnReview_Click_1(object sender, EventArgs e)
        {
            
        }

        private void btnReview_Click(object sender, EventArgs e)
        {
            DialogResult result = openFileDialog1.ShowDialog();

            if (result == DialogResult.OK)
            {
                inputImage = new Image<Bgr, byte>(openFileDialog1.FileName);
                tbPath.Text = openFileDialog1.FileName;
                btnCalculate_Click(this, null);
            }
            else
                MessageBox.Show("Файл не выбран", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void btnCalculate_Click(object sender, EventArgs e)
        {
            inputImage = new Image<Bgr, byte>(tbPath.Text);
            //Исходное изображение
            Image<Gray, byte> imageGray = inputImage.Convert<Gray, byte>();
            pictureBox1.Image = imageGray.ToBitmap();
            int imageWidth = inputImage.Cols, imageHeight = inputImage.Rows;

            //Градиент
            Mat kernel = CvInvoke.GetStructuringElement(ElementShape.Ellipse, new Size(25, 25), new Point(-1, -1));
            Image<Gray, byte> imageGrad = imageGray.MorphologyEx(MorphOp.Gradient, kernel, new Point(-1, -1), 1, BorderType.Default, new MCvScalar());
            pictureBox2.Image = imageGrad.ToBitmap();

            Image<Gray, byte> binary = imageGrad.Convert<Gray, byte>();
            CvInvoke.Threshold(imageGrad, binary, 32, 255, ThresholdType.Binary);

            var mask = binary.ThresholdBinaryInv(new Gray(32), new Gray(255));
            Mat distanceTransofrm = new Mat();
            CvInvoke.DistanceTransform(mask, distanceTransofrm, null, DistType.L2, 3);
            Image<Gray, byte> markers = distanceTransofrm.ToImage<Gray, byte>();// Дистанция до 0-вого пикселя

            CvInvoke.ConnectedComponents(markers, markers);
            Image<Gray, int> finalMarkers = markers.Convert<Gray, int>();
            //CvInvoke.Normalize(finalMarkers, finalMarkers, 0, 255, NormType.MinMax);
            //pictureBox3.Image = finalMarkers.ToBitmap();

            CvInvoke.Watershed(inputImage, finalMarkers);

            Image<Gray, byte> boundaries = finalMarkers.Convert(delegate (int x)
            {
                return (byte)(x == -1 ? 255 : 0);
            });

            imageGrad.SetValue(new Gray(255), boundaries);
            pictureBox3.Image = imageGrad.ToBitmap();

            inputImage.SetValue(new Bgr(255, 255, 255), boundaries);
            pictureBox4.Image = inputImage.ToBitmap();
        }
    }
}
