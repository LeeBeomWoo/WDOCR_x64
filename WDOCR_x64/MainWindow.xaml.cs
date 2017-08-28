
using Microsoft.Win32;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace WDOCR_x64
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : System.Windows.Window
    {
        string data = @"C:\Program Files\WD_OCR\tessdata";
        string img, filename;
        BitmapImage src_image;
        System.Drawing.Point HP, WP;
        double w, h;

        Mat rsz, gray, bw, src, joints, mask, result;
        public MainWindow()
        {
            InitializeComponent();
            filename = @"/tempimg.png";
        }

        public List<Line> HeLines = new List<Line>();
        public List<Line> WiLines = new List<Line>();
        public List<Line> Lines = new List<Line>();
        System.Drawing.Point? dragStart = null;
        Line SelectedLine = null;
        private Bitmap BitmapFromWriteableBitmap(WriteableBitmap writeBmp)
        {
            Bitmap bmp;
            using (MemoryStream outStream = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(writeBmp));
                enc.Save(outStream);
                bmp = new Bitmap(outStream);
            }
            return bmp;
        }
        public WriteableBitmap SaveAsWriteableBitmap(Canvas surface)
        {
            if (surface == null) return null;

            // Save current canvas transform
            Transform transform = surface.LayoutTransform;
            // reset current transform (in case it is scaled or rotated)
            surface.LayoutTransform = null;

            // Get the size of canvas
            System.Windows.Size size = new System.Windows.Size(surface.ActualWidth, surface.ActualHeight);
            // Measure and arrange the surface
            // VERY IMPORTANT
            surface.Measure(size);
            surface.Arrange(new System.Windows.Rect(size));

            // Create a render bitmap and push the surface to it
            RenderTargetBitmap renderBitmap = new RenderTargetBitmap(
              (int)size.Width,
              (int)size.Height,
              96d,
              96d,
              PixelFormats.Pbgra32);
            renderBitmap.Render(surface);


            //Restore previously saved layout
            surface.LayoutTransform = transform;

            //create and return a new WriteableBitmap using the RenderTargetBitmap
            return new WriteableBitmap(renderBitmap);

        }
        private BitmapImage convertImage(string path)
        {
            OpenCvSharp.Size size = new OpenCvSharp.Size(800, 900);
            src = new Mat();
            src = Cv2.ImRead(path,ImreadModes.GrayScale);
            rsz = new Mat();
            Cv2.Resize(src, rsz, size);            
            // Transform source image to gray if it is not
            gray = new Mat();;

            if (rsz.Channels() == 3)
            {
                Cv2.CvtColor(rsz, gray, ColorConversionCodes.BGR2GRAY);
            }
            else
            {
                gray = rsz;
            }
            // Apply adaptiveThreshold at the bitwise_not of gray, notice the ~ symbol
            
            bw = new Mat(); ;
            Cv2.AdaptiveThreshold(gray, bw, 255, AdaptiveThresholdTypes.MeanC, ThresholdTypes.Binary, 15, -2);
            /**
           
            */
            return ConvertBit(gray.ToBitmap());
        }
        private Mat Dep_1(Mat src)
        {
            Mat result = new Mat();
            int[,] _b = new int[,] { { 0, -1, 0 }, { -1, 5, -1 }, { 0, -1, 0 } };
            Mat kernel = new Mat(3, 3, MatType.CV_32F, Scalar.Black);
            kernel.At<int>(_b[0, 0]);
            kernel.At<int>(_b[0, 1]);
            kernel.At<int>(_b[0, 2]);
            kernel.At<int>(_b[1, 0]);
            kernel.At<int>(_b[1, 1]);
            kernel.At<int>(_b[1, 2]);
            kernel.At<int>(_b[2, 0]);
            kernel.At<int>(_b[2, 1]);
            kernel.At<int>(_b[2, 2]);
            Cv2.Filter2D(src, result, src.Depth(), kernel);
            return result;
        }
        private Mat Dep_2(Mat src)
        {
            Mat result = new Mat();
            float[,] _b = new float[,] { { 0.0f/0.5f, -1.0f/0.5f, 0.0f / 0.5f }, { -1.0f / 0.5f, 9.0f / 0.5f
                    , -1.0f/0.5f }, { 0.0f/0.5f, -1.0f/0.5f, 0.0f/0.5f } };
            Mat kernel = new Mat(3, 3, MatType.CV_32F, Scalar.Black);
            kernel.At<float>((int)_b[0, 0]);
            kernel.At<float>((int)_b[0, 1]);
            kernel.At<float>((int)_b[0, 2]);
            kernel.At<float>((int)_b[1, 0]);
            kernel.At<float>((int)_b[1, 1]);
            kernel.At<float>((int)_b[1, 2]);
            kernel.At<float>((int)_b[2, 0]);
            kernel.At<float>((int)_b[2, 1]);
            kernel.At<float>((int)_b[2, 2]);
            Cv2.Filter2D(src, result, src.Depth(), kernel);
            return result;
        }
        private Mat Dep_3(Mat src)
        {
            Mat result = new Mat();
            int[,] _b = new int[,] { { -1, -1, -1 }, { -1, 9, -1 }, { -1, -1, -1 } };
            Mat kernel = new Mat(3, 3, MatType.CV_32F, Scalar.Black);
            kernel.At<int>(_b[0, 0]);
            kernel.At<int>(_b[0, 1]);
            kernel.At<int>(_b[0, 2]);
            kernel.At<int>(_b[1, 0]);
            kernel.At<int>(_b[1, 1]);
            kernel.At<int>(_b[1, 2]);
            kernel.At<int>(_b[2, 0]);
            kernel.At<int>(_b[2, 1]);
            kernel.At<int>(_b[2, 2]);
            Cv2.Filter2D(src, result, src.Depth(), kernel);
            return result;
        }
        private Mat V_Edge(Mat src)
        {
            Mat result = new Mat();
            int[,] _b = new int[,] { { -1, 0, 1 }, { -1, 0, 1 }, { -1, 0, 1 } };
            Mat kernel = new Mat(3, 3, MatType.CV_32F, Scalar.Black);
            kernel.At<int>(_b[0, 0]);
            kernel.At<int>(_b[0, 1]);
            kernel.At<int>(_b[0, 2]);
            kernel.At<int>(_b[1, 0]);
            kernel.At<int>(_b[1, 1]);
            kernel.At<int>(_b[1, 2]);
            kernel.At<int>(_b[2, 0]);
            kernel.At<int>(_b[2, 1]);
            kernel.At<int>(_b[2, 2]);
            Cv2.Filter2D(src, result, src.Depth(), kernel);
            Cv2.Threshold(result, result, 80, 255, ThresholdTypes.BinaryInv);
            return result;
        }
        private Mat H_Edge(Mat src)
        {
            Mat result = new Mat();
            int[,] _b = new int[,] { { 1, 1, 1 }, { 0, 0, 0 }, { -1, -1, -1 } };
            Mat kernel = new Mat(3, 3, MatType.CV_32F, Scalar.Black);
            kernel.At<int>(_b[0, 0]);
            kernel.At<int>(_b[0, 1]);
            kernel.At<int>(_b[0, 2]);
            kernel.At<int>(_b[1, 0]);
            kernel.At<int>(_b[1, 1]);
            kernel.At<int>(_b[1, 2]);
            kernel.At<int>(_b[2, 0]);
            kernel.At<int>(_b[2, 1]);
            kernel.At<int>(_b[2, 2]);
            Cv2.Filter2D(src, result, src.Depth(), kernel);
            Cv2.Threshold(result, result, 80, 255, ThresholdTypes.BinaryInv);
            return result;
        }
        private void ImageToText()
        {
            ExportToPng(filename, pic_imgB);            
            Mat horizontal, vertical;
            // Create the images that will use to extract the horizonta and vertical lines
            horizontal = new Mat(filename);
            Cv2.BitwiseNot(horizontal, horizontal);
            vertical = horizontal.Clone();

        
            int scale = 3; // play with this variable in order to increase/decrease the amount of lines to be detected

            // Specify size on horizontal axis
            int horizontalsize = horizontal.Cols / scale;

            // Create structure element for extracting horizontal lines through morphology operations
            Mat horizontalStructure = Cv2.GetStructuringElement(MorphShapes.Rect, new OpenCvSharp.Size(horizontalsize, 1));

            // Apply morphology operations
            Cv2.Erode(horizontal, horizontal, horizontalStructure, new OpenCvSharp.Point(-1, -1));
            Cv2.Dilate(horizontal, horizontal, horizontalStructure, new OpenCvSharp.Point(-1, -1));
            
            // Specify size on vertical axis
            int verticalsize = vertical.Rows / scale;

            // Create structure element for extracting vertical lines through morphology operations
            Mat verticalStructure = Cv2.GetStructuringElement(MorphShapes.Rect, new OpenCvSharp.Size(1, verticalsize));

            // Apply morphology operations
            Cv2.Erode(vertical, vertical, verticalStructure, new OpenCvSharp.Point(-1, -1));
            Cv2.Dilate(vertical, vertical, verticalStructure, new OpenCvSharp.Point(-1, -1));
            //    dilate(vertical, vertical, verticalStructure, Point(-1, -1)); // expand vertical lines
            
            mask = horizontal + vertical;
            joints = new Mat();
            Cv2.GaussianBlur(mask, joints, new OpenCvSharp.Size(mask.Width -1, mask.Height -1), 1.8);
            Cv2.AddWeighted(mask, 1.8, joints, -1.8, 0, joints);
            Cv2.GaussianBlur(mask, joints, new OpenCvSharp.Size(mask.Width + 1, mask.Height + 1), 1.8);
            Cv2.AddWeighted(mask, 1.8, joints, -1.8, 0, joints);
            ImageBrush ib = new ImageBrush();
            Lines.Clear();
            pic_imgB.Children.Clear();
            ib.ImageSource = ConvertBit(joints.ToBitmap());
            pic_imgB.Background = ib;
            File.Delete(filename);
        }

        public void ExportToPng(string path, Canvas diagram)
        {

            Transform transform = diagram.LayoutTransform;

            diagram.LayoutTransform = null;

            System.Windows.Size size = new System.Windows.Size(diagram.ActualWidth, diagram.ActualHeight);

            diagram.Measure(size);
            diagram.Arrange(new System.Windows.Rect(size));


            RenderTargetBitmap renderBitmap =
              new RenderTargetBitmap(
                (int)size.Width,
                (int)size.Height,
                96d,
                96d,
                PixelFormats.Pbgra32);
            renderBitmap.Render(diagram);


            using (FileStream outStream = new FileStream(path, FileMode.Create))
            {

                PngBitmapEncoder encoder = new PngBitmapEncoder();

                encoder.Frames.Add(BitmapFrame.Create(renderBitmap));
                encoder.Save(outStream);
            }
        }
            public BitmapImage ConvertBit(Bitmap src)
        {
            MemoryStream ms = new MemoryStream();
            src.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
            BitmapImage image = new BitmapImage();
            image.BeginInit();
            ms.Seek(0, SeekOrigin.Begin);
            image.StreamSource = ms;
            image.EndInit();
            return image;
        }
        private void Convert_Btn_MouseDown(object sender, EventArgs e)
        {/**
            Rect bounds = VisualTreeHelper.GetDescendantBounds(pic_imgB);
            double dpi = 96d;
            
            RenderTargetBitmap rtb = new RenderTargetBitmap((int)bounds.Width, (int)bounds.Height, dpi, dpi, PixelFormats.Default);


            DrawingVisual dv = new DrawingVisual();
            using (DrawingContext dc = dv.RenderOpen())
            {
                VisualBrush vb = new VisualBrush(pic_imgB);
                dc.DrawRectangle(vb, null, new Rect(new System.Windows.Point(), bounds.Size));
            }

            rtb.Render(dv);
            BitmapEncoder pngEncoder = new PngBitmapEncoder();
            pngEncoder.Frames.Add(BitmapFrame.Create(rtb));

            try
            {
                MemoryStream ms = new MemoryStream();

                pngEncoder.Save(ms);
                ms.Close();

                File.WriteAllBytes(filename, ms.ToArray());
            }
            catch (Exception err)
            {
                MessageBox.Show(err.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }*/
        }

        private void Load_Btn_Click(object sender, EventArgs e)
        {
            w = pic_imgB.ActualWidth;
            h = pic_imgB.ActualHeight;
            HP.X = 0;
            HP.Y = Convert.ToInt32(w);
            WP.X = Convert.ToInt32(h);
            WP.Y = 0;
            OpenFileDialog op = new OpenFileDialog()
            {
                Filter = "Image files (*.jpg, *.jpeg, *.jpe, *.jfif, *.png) | *.jpg; *.jpeg; *.jpe; *.jfif; *.png;"
            };
            bool? result = op.ShowDialog();

            // Get the selected file name and display in a TextBox
            if (result == true)
            {
                img = op.FileName;
                filepath_txtB.Content = op.FileName;
                loadImage();
            }

        }
        private void drawcell_W(int sel)
        {
            WiLines.Clear();
            Lines.Clear();
            double k = pic_imgB.ActualWidth / sel;
            if (sel > 1)
            {
                for (int i = 1; i < sel; i++)
                {
                    Line WLine = new Line();
                    WLine.X1 = Convert.ToInt32(k * i);
                    WLine.Y1 = 10;
                    WLine.X2 = Convert.ToInt32(k * i);
                    WLine.Y2 = Convert.ToInt32(h)-10;
                    WLine.Tag = "W";
                    WLine.Stroke = System.Windows.Media.Brushes.Black;
                    WLine.HorizontalAlignment = HorizontalAlignment.Left;
                    WLine.VerticalAlignment = VerticalAlignment.Center;
                    WLine.MouseLeftButtonDown += new MouseButtonEventHandler(Select);
                    WLine.MouseMove += new MouseEventHandler(Move);
                    WLine.MouseLeftButtonUp += new MouseButtonEventHandler(complete);
                    WLine.MouseEnter += new MouseEventHandler(Enter);
                    WLine.MouseLeave += new MouseEventHandler(Leave);
                    WLine.MouseRightButtonUp += new MouseButtonEventHandler(Delete);
                    WLine.StrokeThickness = 1;
                    WiLines.Add(WLine);
                }
            }
            Lines = SetList();
            foreach (var line in Lines)//라인집합을 캔버스에 그리는 작업
            {
                if (pic_imgB.Children.Contains(line))
                {
                    pic_imgB.Children.Remove(line);
                }
                pic_imgB.Children.Add(line);
            }
        }
        private void drawcell_H(int sel)
        {
            HeLines.Clear(); // 세로라인을 초기화
            Lines.Clear();// 라인전체 집합을 초기화
            for (int i = 1; i < 30; i++)
            {
                Line HLine = new Line();
                HLine.X1 =10;
                HLine.Y1 = i * sel;
                HLine.X2 = Convert.ToInt32(w)-10;
                HLine.Y2 = i * sel;
                HLine.Tag = "H";
                HLine.Stroke = System.Windows.Media.Brushes.Black;
                HLine.HorizontalAlignment = HorizontalAlignment.Left;
                HLine.VerticalAlignment = VerticalAlignment.Center;
                HLine.MouseLeftButtonDown += new MouseButtonEventHandler(Select);
                HLine.MouseMove += new MouseEventHandler(Move);
                HLine.MouseLeftButtonUp += new MouseButtonEventHandler(complete);
                HLine.MouseEnter += new MouseEventHandler(Enter);
                HLine.MouseLeave += new MouseEventHandler(Leave);
                HLine.MouseRightButtonUp += new MouseButtonEventHandler(Delete);
                HLine.StrokeThickness = 1;
                HeLines.Add(HLine);
            }
            Lines = SetList();//저장되 있던 가로라인과 세로 만들어진 세로 라인을 라인으로 합치기 
            foreach (var line in Lines)//라인집합을 캔버스에 그리는 작업
            {
                if (pic_imgB.Children.Contains(line))
                {
                    pic_imgB.Children.Remove(line);
                }
                pic_imgB.Children.Add(line);
            }

        }


        private void height_comB_SelectedIndexChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBoxItem typeItem = (ComboBoxItem)height_comB.SelectedItem;
            string value = typeItem.Content.ToString();
            drawcell_H(int.Parse(value));
        }
        //가로 선 콤보박스를 변경시 작동
        private void width_comB_SelectedIndexChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBoxItem typeItem = (ComboBoxItem)width_comB.SelectedItem;
            string value = typeItem.Content.ToString();
            drawcell_W(int.Parse(value));
        }
        //이미지를 배경으로 물러오는 작업
        private void loadImage()
        {
            ImageBrush ib = new ImageBrush();
            Lines.Clear();
            pic_imgB.Children.Clear();
            ib.ImageSource = ConvertBit(new Bitmap(img));
            pic_imgB.Background = ib;
        }

        private void pic_imgB_KeyDown(object sender, KeyEventArgs e)
        {
        }

        private void pic_imgB_MouseEnter(object sender, MouseEventArgs e)
        {
            FocusManager.SetFocusedElement(this, pic_imgB);
        }

        //가로와 세로선을 취합하여 Line으로 통일하는 작업 
        private List<Line> SetList()
        {
            if (pic_imgB.Children.Count > 0)
            {
                pic_imgB.Children.Clear();
            }
            if (HeLines.Count > 0)
            {
                foreach (var line in HeLines)
                {
                    Lines.Add(line);
                }
            }
            if (WiLines.Count > 0)
            {
                foreach (var line in WiLines)
                {
                    Lines.Add(line);
                }
            }
            return Lines;
        }
        private void Select(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                SelectedLine = ((Line)sender);
                System.Windows.Point wpfPoint = e.GetPosition(SelectedLine);
                System.Drawing.Point p = new System.Drawing.Point(Convert.ToInt32(wpfPoint.X), Convert.ToInt32(wpfPoint.Y));
                
                dragStart = p;
                if (Mouse.Captured != null)
                {
                    SelectedLine.ReleaseMouseCapture();
                }
                SelectedLine.CaptureMouse();
                SelectedLine.Stroke = System.Windows.Media.Brushes.BlueViolet;
            }
        }
        private void Delete(object sender, MouseButtonEventArgs e)
        {
            if(e.RightButton == MouseButtonState.Released)
            {
                if (SelectedLine != null)
                {
                    SelectedLine = null;
                }
                SelectedLine = ((Line)sender);
                
                    if (SelectedLine.Tag.ToString() == "H")
                    {
                        HeLines.Remove(SelectedLine);
                    }
                    else if (SelectedLine.Tag.ToString() == "W")
                    {
                        WiLines.Remove(SelectedLine);
                    }
                    Lines.Remove(SelectedLine);

                if (pic_imgB.Children.Contains(SelectedLine))
                {
                    pic_imgB.Children.Remove(SelectedLine);
                }
                SelectedLine = null;
            }
        }
        private void Enter(object sender, MouseEventArgs e)
        {
                if (((Line)sender).Tag.ToString() == "H")
                {
                    Mouse.OverrideCursor = Cursors.SizeNS;
                }
                else if (((Line)sender).Tag.ToString() == "W")
                {
                    Mouse.OverrideCursor = Cursors.SizeWE;
                }
                ((Line)sender).Stroke = System.Windows.Media.Brushes.Red;
        }
        private void Leave(object sender, MouseEventArgs e)
        {
            Mouse.OverrideCursor = Cursors.Arrow;
            ((Line)sender).Stroke = System.Windows.Media.Brushes.Black;
        }

        private void result_Btn_Click(object sender, RoutedEventArgs e)
        {
            ImageToText();
        }
        
        private void Move(object sender, MouseEventArgs e)
        {
            if (dragStart != null && e.LeftButton == MouseButtonState.Pressed)
            {
                System.Windows.Point wpfPoint = e.GetPosition(((Line)sender));
                System.Drawing.Point p = new System.Drawing.Point(Convert.ToInt32(wpfPoint.X), Convert.ToInt32(wpfPoint.Y));
                var p2 = e.GetPosition(pic_imgB);
                if (SelectedLine.Tag.ToString() == "H")
                {
                    Canvas.SetTop(SelectedLine, p2.Y - dragStart.Value.Y);
                    Mouse.OverrideCursor = Cursors.SizeNS;
                }
                else if (SelectedLine.Tag.ToString() == "W")
                {
                    Canvas.SetLeft(SelectedLine, p2.X - dragStart.Value.X);
                    Mouse.OverrideCursor = Cursors.SizeWE;
                }
            }
        }        

        private void complete(object sender, MouseButtonEventArgs e)
        {
            dragStart = null;
            if (Mouse.Captured != null)
            {
                SelectedLine.ReleaseMouseCapture();
            }
            SelectedLine.Stroke = System.Windows.Media.Brushes.Black;
            SelectedLine = null;
            Mouse.OverrideCursor = Cursors.Arrow;
        }
    }
}

/**
public void Main(string args)
{
    try {
        using (var engine = new TesseractEngine(@"C:\Program Files\WD_OCR\tessdata", "eng+kor", EngineMode.Default))
        {
            using (var img = Pix.LoadFromFile(args))
            {
                using (var page = engine.Process(img))
                {
                    var text = page.GetText();
                    Console.WriteLine("Mean confidence: {0}", page.GetMeanConfidence());
                    Console.WriteLine("Text (GetText): \r\n{0}", text);
                    result_rtxtB.Document.Blocks.Clear();
                    result_rtxtB.Document.Blocks.Add(new Paragraph(new Run(text)));
                    Console.WriteLine("Text (iterator):");
                    using (var iter = page.GetIterator())
                    {
                        iter.Begin();
                        do{
                            do{
                                do{
                                    do{
                                        if (iter.IsAtBeginningOf(PageIteratorLevel.Block)){
                                            Console.WriteLine("<BLOCK>");
                                        }
                                        Console.Write(iter.GetText(PageIteratorLevel.Word));
                                        Console.Write(" ");
                                        if (iter.IsAtFinalOf(PageIteratorLevel.TextLine, PageIteratorLevel.Word))
                                        {
                                            Console.WriteLine();
                                        }
                                    } while (iter.Next(PageIteratorLevel.TextLine, PageIteratorLevel.Word));
                                    if (iter.IsAtFinalOf(PageIteratorLevel.Para, PageIteratorLevel.TextLine)) {
                                        Console.WriteLine();
                                    }
                                } while (iter.Next(PageIteratorLevel.Para, PageIteratorLevel.TextLine));
                            } while (iter.Next(PageIteratorLevel.Block, PageIteratorLevel.Para));
                        } while (iter.Next(PageIteratorLevel.Block));
                    }
                }
            }
        }
    }
    catch (Exception e)
    {
        MessageBox.Show(e.ToString(), "관리자에게 에러 내용을 전달해 주세요");
    }
}
*/
