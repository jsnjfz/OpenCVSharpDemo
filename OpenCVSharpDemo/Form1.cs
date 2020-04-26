using OpenCvSharp;
using OpenCvSharp.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace OpenCVSharpDemo
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        string fileName;
        Mat mat;

        private void Form1_Load(object sender, EventArgs e)
        {
            fileName = @Application.StartupPath + @"\test.png";
            mat = new Mat(fileName);
            pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox1.Image = mat.ToBitmap();
        }

        public void findTextRegion(Mat dilation)
        {
            // 1. 查找轮廓
            OpenCvSharp.Point[][] contours;
            HierarchyIndex[] hierarchly;
            Rect biggestContourRect = new Rect();

            Cv2.FindContours(dilation, out contours, out hierarchly, RetrievalModes.Tree, ContourApproximationModes.ApproxSimple);

            // 2. 筛选那些面积小的
            int i = 0;
            foreach (OpenCvSharp.Point[] contour in contours)
            {
                double area = Cv2.ContourArea(contour);

                //面积小的都筛选掉
                if (area < 1000)
                {
                    continue;
                }

                //轮廓近似，作用很小
                double epsilon = 0.001 * Cv2.ArcLength(contour, true);

                //找到最小的矩形
                biggestContourRect = Cv2.BoundingRect(contour);

                if (biggestContourRect.Height > (biggestContourRect.Width * 1.2))
                {
                    continue;
                }

                //画线
                mat.Rectangle(biggestContourRect,new Scalar(0, 255, 0), 2);
            }

            pictureBox1.Image = mat.ToBitmap();
            //Cv2.ImShow("img", mat);
        }


        public Mat preprocess(string imgPath)
        {
            Mat dilation2 = new Mat();

            //读取灰度图
            using (Mat src = new Mat(imgPath, ImreadModes.Grayscale))
            {
                //1.Sobel算子，x方向求梯度
                Mat sobel = new Mat();
                Cv2.Sobel(src, sobel, MatType.CV_8U, 1, 0, 3);

                //2.二值化
                Mat binary = new Mat();
                Cv2.Threshold(sobel, binary, 0, 255, ThresholdTypes.Otsu | ThresholdTypes.Binary);

                //3. 膨胀和腐蚀操作的核函数
                Mat element1 = new Mat();
                Mat element2 = new Mat();
                OpenCvSharp.Size size1 = new OpenCvSharp.Size(30, 9);
                OpenCvSharp.Size size2 = new OpenCvSharp.Size(24, 6);

                element1 = Cv2.GetStructuringElement(MorphShapes.Rect, size1);
                element2 = Cv2.GetStructuringElement(MorphShapes.Rect, size2);

                //4. 膨胀一次，让轮廓突出
                Mat dilation = new Mat();
                Cv2.Dilate(binary, dilation, element2);

                //5. 腐蚀一次，去掉细节，如表格线等。注意这里去掉的是竖直的线
                Mat erosion = new Mat();
                Cv2.Erode(dilation, erosion, element1);

                //6. 再次膨胀，让轮廓明显一些
                Cv2.Dilate(erosion, dilation2, element2, null, 3);
            }
            return dilation2;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Mat test = preprocess(fileName);
            findTextRegion(test);
        }
    }
}
