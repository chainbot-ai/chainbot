using OpenCvSharp;
using OpenCvSharp.Dnn;
using OpenCvSharp.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plugins.Shared.Library.ImageRecognition
{
    public static class ImageRecognitionCV
    {
        private static int memoryPressureBytes = 200 * 1024 * 1024;

        public static Rect Find(string sourceImagePath, Rect searchRect, string templateImagePath, float scoreThreshold)
        {
            Mat sourceMat = null;
            Mat templateMat = null;
            Mat resultMat = null;
            try
            {
                GC.AddMemoryPressure(memoryPressureBytes);

                if (sourceImagePath == null)
                {
                    ScreenCapture sc = new ScreenCapture();
                    var sourceImage = sc.CaptureScreen();
                    sourceMat = BitmapConverter.ToMat((System.Drawing.Bitmap)sourceImage);
                }
                else
                {
                    try
                    {
                        sourceMat = new Mat(sourceImagePath, ImreadModes.AnyColor);
                    }
                    catch (Exception)
                    {
                        throw new Exception("找不到待识别的源图像，请检查！");
                    }
                }

                if (searchRect != Rect.Empty)
                {
                    try
                    {
                        sourceMat = new Mat(sourceMat, searchRect);
                    }
                    catch (Exception)
                    {
                        throw new Exception("图像识别时搜索范围可能超出实际区域，请检查！");
                    }
                }

                templateMat = new Mat(templateImagePath, ImreadModes.AnyColor);

                resultMat = new Mat();

                try
                {
                    resultMat.Create(sourceMat.Rows - templateMat.Rows + 1, sourceMat.Cols - templateMat.Cols + 1, MatType.CV_32FC1);
                }
                catch (Exception)
                {
                    throw new Exception("图像识别时出现异常范围，请检查！");
                }


                Cv2.MatchTemplate(sourceMat, templateMat, resultMat, TemplateMatchModes.CCoeffNormed);

                double minValue, maxValue;
                Point minLocation, maxLocation;
                Cv2.MinMaxLoc(resultMat, out minValue, out maxValue, out minLocation, out maxLocation);

                if (maxValue >= scoreThreshold)
                {
                    Rect resultRect = Rect.Empty;

                    if (searchRect != Rect.Empty)
                    {
                        resultRect = new Rect(searchRect.Add(maxLocation).Location, templateMat.Size());
                    }
                    else
                    {
                        resultRect = new Rect(maxLocation, templateMat.Size());
                    }

                    return resultRect;
                }

                return Rect.Empty;
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                sourceMat?.Dispose();
                templateMat?.Dispose();
                resultMat?.Dispose();

                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();

                GC.RemoveMemoryPressure(memoryPressureBytes);
            }
        }

        
        public static List<Rect> FindAll(string sourceImagePath, Rect searchRect, string templateImagePath, float scoreThreshold, float nmsThreshold, out Rect resultBestRect, out List<Rect> relativeResultRects)
        {
            resultBestRect = Rect.Empty;

            relativeResultRects = new List<Rect>();

            Mat sourceMat = null;
            Mat templateMat = null;
            Mat resultMat = null;
            try
            {
                GC.AddMemoryPressure(memoryPressureBytes);

                if (sourceImagePath == null)
                {
                    ScreenCapture sc = new ScreenCapture();
                    var sourceImage = sc.CaptureScreen();
                    sourceMat = BitmapConverter.ToMat((System.Drawing.Bitmap)sourceImage);
                }
                else
                {
                    try
                    {
                        sourceMat = new Mat(sourceImagePath, ImreadModes.AnyColor);
                    }
                    catch (Exception)
                    {
                        throw new Exception("找不到待识别的源图像，请检查！");
                    }
                }

                if (searchRect != Rect.Empty)
                {
                    try
                    {
                        sourceMat = new Mat(sourceMat, searchRect);//如果searchRect超过实际的范围会报异常
                    }
                    catch (Exception)
                    {
                        throw new Exception("图像识别时搜索范围可能超出实际区域，请检查！");
                    }
                }

                templateMat = new Mat(templateImagePath, ImreadModes.AnyColor);

                resultMat = new Mat();

                try
                {
                    resultMat.Create(sourceMat.Rows - templateMat.Rows + 1, sourceMat.Cols - templateMat.Cols + 1, MatType.CV_32FC1);
                }
                catch (Exception)
                {
                    throw new Exception("图像识别时出现异常范围，请检查！");
                }

                Cv2.MatchTemplate(sourceMat, templateMat, resultMat, TemplateMatchModes.CCoeffNormed);

                double minValue, maxValue;
                Point minLocation, maxLocation;
                Cv2.MinMaxLoc(resultMat, out minValue, out maxValue, out minLocation, out maxLocation);

                if (maxValue >= scoreThreshold)
                {
                    Rect resultRect = Rect.Empty;

                    if (searchRect != Rect.Empty)
                    {
                        resultRect = new Rect(searchRect.Add(maxLocation).Location, templateMat.Size());
                    }
                    else
                    {
                        resultRect = new Rect(maxLocation, templateMat.Size());
                    }

                    resultBestRect = resultRect;
                }


                var bboxes = new List<Rect>();
                var scores = new List<float>();

                int[] indices;

                var indexer = resultMat.GetGenericIndexer<float>();
                for (int r = 0; r < resultMat.Rows; r++)
                {
                    for (int c = 0; c < resultMat.Cols; c++)
                    {
                        float similarity = indexer[r, c];
                        if (similarity >= scoreThreshold)
                        {
                            var rect = new Rect(new Point(c, r), templateMat.Size());
                            bboxes.Add(rect);

                            scores.Add(similarity);
                        }
                    }
                }

                CvDnn.NMSBoxes(bboxes, scores, scoreThreshold, nmsThreshold, out indices);

                var resultRects = new List<Rect>();
                foreach (var idx in indices)
                {
                    var bbox = bboxes[idx];
                    relativeResultRects.Add(bbox);

                    if (searchRect != Rect.Empty)
                    {
                        var resultRect = new Rect(searchRect.Add(bbox.Location).Location, bbox.Size);
                        resultRects.Add(resultRect);
                    }
                    else
                    {
                        resultRects.Add(bbox);
                    }
                }

                return resultRects;
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                sourceMat?.Dispose();
                templateMat?.Dispose();
                resultMat?.Dispose();

                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();

                GC.RemoveMemoryPressure(memoryPressureBytes);
            }
        }

       
        public static bool CheckExists(string sourceImagePath, Rect searchRect, string templateImagePath, float scoreThreshold)
        {
            var findRect = Find(sourceImagePath, searchRect, templateImagePath, scoreThreshold);

            if (findRect == Rect.Empty)
            {
                return false;
            }

            return true;
        }


        public static void ShowMatchRect(string sourceImagePath, Rect searchRect, Rect matchRect, Scalar color, int thickness = 2)
        {
            Mat sourceMat = null;

            try
            {
                GC.AddMemoryPressure(memoryPressureBytes);

                if (sourceImagePath == null)
                {
                    ScreenCapture sc = new ScreenCapture();
                    var sourceImage = sc.CaptureScreen();
                    sourceMat = BitmapConverter.ToMat((System.Drawing.Bitmap)sourceImage);
                }
                else
                {
                    try
                    {
                        sourceMat = new Mat(sourceImagePath, ImreadModes.AnyColor);
                    }
                    catch (Exception)
                    {
                        throw new Exception("找不到待识别的源图像，请检查！");
                    }
                }

                if (searchRect != Rect.Empty)
                {
                    try
                    {
                        sourceMat = new Mat(sourceMat, searchRect);//如果searchRect超过实际的范围会报异常
                    }
                    catch (Exception)
                    {
                        throw new Exception("图像识别时搜索范围可能超出实际区域，请检查！");
                    }
                }


                var bbox = matchRect;
                if (bbox != Rect.Empty)
                {
                    Cv2.Rectangle(sourceMat, bbox.Location, new Point(bbox.X + bbox.Width, bbox.Y + bbox.Height), color, thickness);
                }

                Cv2.ImShow("单个图像匹配结果", sourceMat);
            }
            catch (Exception)
            {

                throw;
            }
            finally
            {
                sourceMat?.Dispose();

                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();

                GC.RemoveMemoryPressure(memoryPressureBytes);
            }
        }


        public static void ShowMatchRects(string sourceImagePath, Rect searchRect, List<Rect> matchRects, Scalar color, int thickness = 2)
        {
            Mat sourceMat = null;

            try
            {
                GC.AddMemoryPressure(memoryPressureBytes);

                if (sourceImagePath == null)
                {
                    ScreenCapture sc = new ScreenCapture();
                    var sourceImage = sc.CaptureScreen();
                    sourceMat = BitmapConverter.ToMat((System.Drawing.Bitmap)sourceImage);
                }
                else
                {
                    try
                    {
                        sourceMat = new Mat(sourceImagePath, ImreadModes.AnyColor);
                    }
                    catch (Exception)
                    {
                        throw new Exception("找不到待识别的源图像，请检查！");
                    }
                }

                if (searchRect != Rect.Empty)
                {
                    try
                    {
                        sourceMat = new Mat(sourceMat, searchRect);//如果searchRect超过实际的范围会报异常
                    }
                    catch (Exception)
                    {
                        throw new Exception("图像识别时搜索范围可能超出实际区域，请检查！");
                    }
                }


                var bboxes = matchRects;
                if (bboxes != null)
                {
                    foreach (var bbox in bboxes)
                    {
                        Cv2.Rectangle(sourceMat, bbox.Location, new Point(bbox.X + bbox.Width, bbox.Y + bbox.Height), color, thickness);
                    }
                }

                var winName = $"所有图像匹配结果(共计{matchRects.Count}个匹配结果)";
                float fScale = 0.9f;
                var psb = System.Windows.Forms.Screen.PrimaryScreen.Bounds;
                Cv2.NamedWindow(winName);
                Cv2.ResizeWindow(winName, new Size(sourceMat.Width * fScale, sourceMat.Height * fScale));
                Cv2.MoveWindow(winName, (psb.Width - (int)(sourceMat.Width * fScale)) / 2, (psb.Height - (int)(sourceMat.Height * fScale)) / 2);

                Cv2.ImShow(winName, sourceMat);
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                sourceMat?.Dispose();

                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();

                GC.RemoveMemoryPressure(memoryPressureBytes);
            }
        }

        public static void ShowMatchRects(System.Drawing.Image sourceImage, Rect searchRect, List<Rect> matchRects, Scalar color, int thickness = 2)
        {
            Mat sourceMat = null;

            try
            {
                GC.AddMemoryPressure(memoryPressureBytes);

                sourceMat = BitmapConverter.ToMat((System.Drawing.Bitmap)sourceImage);

                if (searchRect != Rect.Empty)
                {
                    try
                    {
                        sourceMat = new Mat(sourceMat, searchRect);
                    }
                    catch (Exception)
                    {
                        throw new Exception("图像识别时搜索范围可能超出实际区域，请检查！");
                    }
                }


                var bboxes = matchRects;
                if (bboxes != null)
                {
                    foreach (var bbox in bboxes)
                    {
                        Cv2.Rectangle(sourceMat, bbox.Location, new Point(bbox.X + bbox.Width, bbox.Y + bbox.Height), color, thickness);
                    }
                }

                var winName = $"所有图像匹配结果(共计{matchRects.Count}个匹配结果)";
                float fScale = 0.9f;
                var psb = System.Windows.Forms.Screen.PrimaryScreen.Bounds;
                Cv2.NamedWindow(winName);
                Cv2.ResizeWindow(winName, new Size(sourceMat.Width * fScale, sourceMat.Height * fScale));
                Cv2.MoveWindow(winName, (psb.Width - (int)(sourceMat.Width * fScale)) / 2, (psb.Height - (int)(sourceMat.Height * fScale)) / 2);

                Cv2.ImShow(winName, sourceMat);
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                sourceMat?.Dispose();

                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();

                GC.RemoveMemoryPressure(memoryPressureBytes);
            }
        }

    }
}
