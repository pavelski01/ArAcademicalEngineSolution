using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Features2D;
using Emgu.CV.OCR;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;

namespace RozpoznawatorApi.Recognizer
{
    public class SilnikRozpoznawania
    {
        public void Procesuj(string sciezkaWzorca, string sciezkaObrazu, string sciezkaObrobione)
        {
            using (var wzorzec = new Image<Gray, byte>(sciezkaWzorca))
            {
                var detektor = new SURFDetector(395, false);
                var wzorzecCechy = detektor.DetectFeatures(wzorzec, null);
                using (var wzorzecOcechowany = new Image<Gray, byte>(wzorzec.Bitmap))
                {
                    var font = new MCvFont(FONT.CV_FONT_HERSHEY_SIMPLEX, 1, 1);
                    Array.ForEach(
                        wzorzecCechy, 
                        e =>
                        {
                            wzorzecOcechowany.Draw(
                                ".",
                                ref font,
                                new Point(
                                    (int)e.KeyPoint.Point.X,
                                    (int)e.KeyPoint.Point.Y
                                ),
                                new Gray()
                            );
                        }
                    );
                    using (var obraz = new Image<Gray, byte>(sciezkaObrazu))
                    {
                        var wzorzecPunktyKluczowe = new VectorOfKeyPoint();
                        var punktyKluczowe = wzorzecCechy.Select(e => e.KeyPoint).ToArray();
                        wzorzecPunktyKluczowe.Push(punktyKluczowe);
                        var wzorzecDeskryptory = detektor.DetectAndCompute(wzorzec, null, wzorzecPunktyKluczowe);
                        var cechyObraz = detektor.DetectFeatures(obraz, null);
                        var obrazPunktyKluczowe = new VectorOfKeyPoint();
                        punktyKluczowe = cechyObraz.Select(e => e.KeyPoint).ToArray();
                        obrazPunktyKluczowe.Push(punktyKluczowe);
                        var obrazDeskryptory = detektor.DetectAndCompute(obraz, null, obrazPunktyKluczowe);
                        if (obrazDeskryptory == null)
                        {
                            Debug.WriteLine("Brak punktów kluczowych");
                            return;
                        }
                        var dobieracz = new BruteForceMatcher<float>(DistanceType.L1);
                        dobieracz.Add(wzorzecDeskryptory);
                        var treningowaMacierz = new Matrix<int>(obrazDeskryptory.Rows, 2);
                        Matrix<byte> maska;
                        using (var odleglosciowaMacierz = new Matrix<float>(obrazDeskryptory.Rows, 2))
                        {
                            dobieracz.KnnMatch(obrazDeskryptory, treningowaMacierz, odleglosciowaMacierz, 5, null);
                            maska = new Matrix<byte>(odleglosciowaMacierz.Rows, 1);
                            maska.SetValue(255);
                        }
                        Features2DToolbox.VoteForSizeAndOrientation(
                            wzorzecPunktyKluczowe, obrazPunktyKluczowe, treningowaMacierz, maska, 5, 200
                        );
                        var homografia =
                            Features2DToolbox.GetHomographyMatrixFromMatchedFeatures(
                                wzorzecPunktyKluczowe,
                                obrazPunktyKluczowe, 
                                treningowaMacierz, 
                                maska, 
                                2
                            );
                        if (homografia != null)
                        {
                            var prostokat = wzorzec.ROI;
                            var prostokatWiezcholki = new[]
                            {
                                new PointF(prostokat.Left, prostokat.Bottom),
                                new PointF(prostokat.Right, prostokat.Bottom),
                                new PointF(prostokat.Right, prostokat.Top),
                                new PointF(prostokat.Left, prostokat.Top)
                            };
                            homografia.ProjectPoints(prostokatWiezcholki);
                            using (var obrazRozpoznany = new Image<Bgr, byte>(obraz.Bitmap))
                            {
                                obrazRozpoznany.DrawPolyline(
                                    Array.ConvertAll(prostokatWiezcholki, Point.Round), 
                                    true,
                                    new Bgr(Color.Red), 
                                    10
                                );
                                obrazRozpoznany.Save(sciezkaObrobione);
                            }
                        }
                    }
                }
            }
        }
        public void Rozpoznawaj(string sciezkaObrazu, string sciezkaObrobione) 
        {
            using (var image = new Image<Bgr, byte>(sciezkaObrazu))
            {
                using 
                (
                    var tesseractOcrProvider = 
                        new Tesseract(
                            string.Empty, 
                            "eng", 
                            Tesseract.OcrEngineMode.OEM_TESSERACT_CUBE_COMBINED
                        )
                )
                {
                    tesseractOcrProvider.Recognize(image);
                    var text = tesseractOcrProvider.GetText().TrimEnd();
                }
            }
        }
    }
}