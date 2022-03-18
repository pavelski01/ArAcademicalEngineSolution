using UnityEngine;
using System.Collections;
using Vuforia;
using System.IO;
using System;
using System.Threading;

namespace Assets.Scripts
{
    public class DostepnoscKamery : MonoBehaviour
    {
        public bool PrzechwycObraz { get; set; }
        public bool CzyZrzut { get; set; } = true;
        public bool CzyWyslano { get; set; }
        public bool CzyPobrano { get; set; }
        private PixelFormat FormatPiksela { get; } = PixelFormat.RGB888;
        private string SciezkaZapisu { get; } = "/storage/emulated/0/DCIM/GaleriaAr";
        private string ApiUrl { get; } = "192.168.100.103/api/recognizer";      
        private bool CzyFormatKlatkiZarejestrowany { get; set; } = false;
        private bool CzyTeksturaLadowna { get; set; } = false;        
        private Texture2D Tekstura { get; set; }

        void Start()
        {
            Tekstura = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);            
            VuforiaApplication.Instance.OnVuforiaStarted += GdyVuforiaWystartowala;
            VuforiaApplication.Instance.OnVuforiaPaused += GdyPauza;
            VuforiaBehaviour.Instance.World.OnStateUpdated += GdySledzenieZaktualizowane;
            VuforiaBehaviour.Instance.SetWorldCenter(WorldCenterMode.DEVICE, null);
        }

        private void GdyVuforiaWystartowala() => ZarejestrujFormat();
        private void GdyPauza(bool czyZapauzowane)
        {
            if (czyZapauzowane) WyrejestrujFormat();
            else ZarejestrujFormat();
        }
        private void GdySledzenieZaktualizowane()
        {       
            if (!CzyFormatKlatkiZarejestrowany || CzyTeksturaLadowna || !PrzechwycObraz) return;
            var obraz = VuforiaBehaviour.Instance.CameraDevice.GetCameraImage(FormatPiksela);
            if (obraz == null) return;
            StartCoroutine(ZapiszZrzut(obraz.Pixels, obraz.Width, obraz.Height, Screen.orientation));
        }        
        private IEnumerator ZapiszZrzut(byte[] piksele, int dlugosc, int wysokosc, ScreenOrientation orientacjaEkranu)
        {
            if (!CzyTeksturaLadowna)
            {
                CzyTeksturaLadowna = true;                
                if (piksele != null && piksele.Length > 0)
                {
                    if (Tekstura.width != dlugosc || Tekstura.height != wysokosc)
                        Tekstura = new Texture2D(dlugosc, wysokosc, TextureFormat.RGB24, false);
                    Tekstura.LoadRawTextureData(piksele);
                    Tekstura.Apply();
                    Tekstura = TransformacjaTekstury(Tekstura, orientacjaEkranu);
                    var bajty = Tekstura.EncodeToPNG();
                    var guid = Guid.NewGuid();
                    var nazwaPliku = 
                        CzyZrzut ? 
                            $"{guid}.png" : 
                            "wzorzec.png";
                    try
                    {
                        if (!Directory.Exists(SciezkaZapisu))
                            Directory.CreateDirectory(SciezkaZapisu);
                        var folderDocelowy = CzyZrzut ? "Analiza" : "Wzorzec";
                        var sciezkaAnalizy = Path.Combine(SciezkaZapisu, folderDocelowy);
                        if (!Directory.Exists(Path.Combine(SciezkaZapisu, "Obrobka")))
                            Directory.CreateDirectory(Path.Combine(SciezkaZapisu, "Obrobka"));
                        if (!Directory.Exists(sciezkaAnalizy))
                            Directory.CreateDirectory(sciezkaAnalizy);
                        var lokalizacjaPliku = Path.Combine(sciezkaAnalizy, nazwaPliku);
                        File.WriteAllBytes(lokalizacjaPliku, bajty);
                        if (CzyZrzut)
                        {
                            var komunikatorNadanie = new Komunikator(this)
                            {
                                Uri = $"{ApiUrl}"                                
                            };
                            var wzorzecPlik = Path.Combine(SciezkaZapisu, "Wzorzec", "wzorzec.png");
                            lokalizacjaPliku = Path.Combine(SciezkaZapisu, "Analiza", $"{guid}.png");
                            var wzorzecBajty = File.ReadAllBytes(wzorzecPlik);
                            var lokalizacjaBajty = File.ReadAllBytes(lokalizacjaPliku);
                            StartCoroutine(komunikatorNadanie.WysylaniePlikow(guid, lokalizacjaBajty, wzorzecBajty));
                            var komunikatorOdbior = new Komunikator(this)
                            {
                                Uri = $"{ApiUrl}/{guid}",
                                Sciezka = Path.Combine(SciezkaZapisu, "Obrobka", $"{guid}.png")
                            };
                            StartCoroutine(komunikatorOdbior.PobieraniePliku());
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.Log("Błąd zapisu do systemu plików: " + e.ToString());
                    }
                }
                yield return null;
                CzyTeksturaLadowna = false;
                PrzechwycObraz = false;
            }
        }
        private void WyrejestrujFormat() =>
            CzyFormatKlatkiZarejestrowany = !VuforiaBehaviour.Instance.CameraDevice.SetFrameFormat(FormatPiksela, false);
        private void ZarejestrujFormat()
        {
            try
            {
                CzyFormatKlatkiZarejestrowany = VuforiaBehaviour.Instance.CameraDevice.SetFrameFormat(FormatPiksela, true);
                Debug.Log($"Udało się zrejestrować format piksela {FormatPiksela}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Nie udało się zrejestrować formatu piksela {FormatPiksela}: " + ex);
                CzyFormatKlatkiZarejestrowany = false;
            }
        }
        private Texture2D TransformacjaTekstury(
            Texture2D tekstura, ScreenOrientation orientacjaEkranu
        )
        {
            Texture2D transformowanaTekstura;
            switch (orientacjaEkranu)
            {
                case ScreenOrientation.Portrait:
                    transformowanaTekstura = new Texture2D(tekstura.height, tekstura.width, tekstura.format, false);
                    for (int i = 0; i < tekstura.width; i++)
                        for (int j = 0; j < tekstura.height; j++)
                            transformowanaTekstura.SetPixel(j, i, tekstura.GetPixel(tekstura.width - i, tekstura.height - j));
                    break;
                case ScreenOrientation.PortraitUpsideDown:
                    transformowanaTekstura = new Texture2D(tekstura.height, tekstura.width, tekstura.format, false);
                    for (int i = 0; i < tekstura.width; i++)
                        for (int j = 0; j < tekstura.height; j++)
                            transformowanaTekstura.SetPixel(j, i, tekstura.GetPixel(i, j));
                    break;
                case ScreenOrientation.LandscapeRight:
                    transformowanaTekstura = new Texture2D(tekstura.width, tekstura.height, tekstura.format, false);
                    for (int i = 0; i < tekstura.width; i++)
                        for (int j = 0; j < tekstura.height; j++)
                            transformowanaTekstura.SetPixel(i, j, tekstura.GetPixel(tekstura.width - i, j));
                    break;
                case ScreenOrientation.LandscapeLeft:
                    transformowanaTekstura = new Texture2D(tekstura.width, tekstura.height, tekstura.format, false);
                    for (int i = 0; i < tekstura.width; i++)
                        for (int j = 0; j < tekstura.height; j++)
                            transformowanaTekstura.SetPixel(i, j, tekstura.GetPixel(i, tekstura.height - j));
                    break;
                default:
                    transformowanaTekstura = tekstura;
                    break;
            }
            transformowanaTekstura.Apply();
            return transformowanaTekstura;
        }        
    }
}
