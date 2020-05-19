/*
 * This sample program is ported by C# from examples\webcam_face_pose_ex.cpp.
 * 来源  https://github.com/takuya-takeuchi/DlibDotNet/tree/master/examples/WebcamFacePose
 * LEX 2020-5-19修改
*/

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using DlibDotNet; //DLIB识别库
using OpenCvSharp; //OPENCV机器视觉库

namespace WebcamFacePose
{

    internal class Program
    {

        private static void Main()
        {
            try
            {
                // 定义图像捕捉方式 从摄像头 ， 注意 Windows下需要选择 VideoCaptureAPIs.DSHOW
                var cap = new VideoCapture(0,VideoCaptureAPIs.DSHOW);

                // 定义图像捕捉方式 从摄像头 视频文件
                //var cap = new VideoCapture("video.webm");

                //判断捕捉设备是否打开
                if (!cap.IsOpened())
                {
                    Console.WriteLine("Unable to connect to camera");
                    return;
                }

                Mat temp = null;

                //定义显示窗口
                using (var win = new ImageWindow())
                {
                    //读取人脸检测和标注模型
                    using (var detector = Dlib.GetFrontalFaceDetector())
                    using (var poseModel = ShapePredictor.Deserialize("shape_predictor_68_face_landmarks.dat"))
                    {
                        // 主窗口是否关闭
                        while (!win.IsClosed())
                        {
                            //System.Threading.Thread.Sleep(100);
                            //获得1帧图片
                              temp = cap.RetrieveMat();// new Mat();
                          

                            if ( temp==null )
                            {
                                break;
                            }

                             
                            //将 OPENCV 图像数据 转换为 DILB 图像格式
                            var array = new byte[temp.Width * temp.Height * temp.ElemSize()];
                            Marshal.Copy(temp.Data, array, 0 , array.Length);
                            using (var cimg = Dlib.LoadImageData<BgrPixel>(array, (uint)temp.Height, (uint)temp.Width, (uint)(temp.Width * temp.ElemSize())))
                            {
                                // 人脸检测
                                var faces = detector.Operator(cimg);
                                //标注人脸
                                var shapes = new List<FullObjectDetection>();
                                for (var i = 0; i < faces.Length; ++i)
                                {
                                    var det = poseModel.Detect(cimg, faces[i]);
                                    shapes.Add(det);
                                }

                                //显示
                                win.ClearOverlay();
                                win.SetImage(cimg);
                                var lines = Dlib.RenderFaceDetections(shapes);
                                win.AddOverlay(lines);

                                foreach (var line in lines)
                                    line.Dispose();
                            } 
                        }
                    }
                }
            }
            //catch (serialization_error&e)
            //{
  
            //    cout << "需要下载识别模型   http://dlib.net/files/shape_predictor_68_face_landmarks.dat.bz2" << endl;
            //    cout << endl << e.what() << endl;
            //}
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

    }

}