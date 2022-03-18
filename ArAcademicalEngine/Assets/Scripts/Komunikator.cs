using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace Assets.Scripts
{
    public class Komunikator
    {
        public Komunikator(DostepnoscKamery dostepnoscKamery) => 
            DostepnoscKamery = dostepnoscKamery;
        public DostepnoscKamery DostepnoscKamery { get; set; }
        public string Sciezka { get; set; }
        public string Uri { get; set; }
        public IEnumerator PobieraniePliku()
        {
            Debug.Log("Rozpoczęcie pobierania");
            while (!DostepnoscKamery.CzyWyslano)
            {
                Debug.Log("Czekanie, na wysłanie");
                yield return new WaitForSeconds(5);
            }
            Debug.Log("Zakończenie czekania");
            DostepnoscKamery.CzyWyslano = false;
            using var zadanieWeb = new UnityWebRequest(Uri);
            zadanieWeb.method = UnityWebRequest.kHttpVerbGET;
            var obslugaPobieraniaPliku = new DownloadHandlerFile(Sciezka)
            {
                removeFileOnAbort = true
            };
            zadanieWeb.downloadHandler = obslugaPobieraniaPliku;
            yield return zadanieWeb.SendWebRequest();
            switch (zadanieWeb.result)
            {
                case UnityWebRequest.Result.ConnectionError:
                case UnityWebRequest.Result.DataProcessingError:
                case UnityWebRequest.Result.ProtocolError:
                    Debug.Log("Wystąpił błąd komunikacji");
                    break;
                case UnityWebRequest.Result.Success:
                    Debug.Log("Zakończono żądanie pobierania!");
                    DostepnoscKamery.CzyPobrano = true;
                    break;
            }
            Debug.Log($"Kod odpowiedzi: {zadanieWeb.responseCode}");        
        }
        public IEnumerator WysylaniePlikow(Guid guid, byte[] analizaBajty, byte[] wzorzecBajty)
        {
            var sekcjaFormularzowa = new List<IMultipartFormSection>
            {
                new MultipartFormDataSection("guid", $"{guid}"),
                new MultipartFormFileSection("analiza", analizaBajty, $"{guid}.png", "image/png"),
                new MultipartFormFileSection("wzorzec", wzorzecBajty, "wzorzec.png", "image/png")
            };
            var zadanieWeb = UnityWebRequest.Post(Uri, sekcjaFormularzowa);
            zadanieWeb.timeout = 20;
            Debug.Log("Rozpoczęto żądanie wysyłania!");
            DostepnoscKamery.CzyPobrano = false;
            yield return zadanieWeb.SendWebRequest();
            switch (zadanieWeb.result)
            {
                case UnityWebRequest.Result.ConnectionError:
                case UnityWebRequest.Result.DataProcessingError:
                case UnityWebRequest.Result.ProtocolError:
                    break;
                case UnityWebRequest.Result.Success:
                    DostepnoscKamery.CzyWyslano = true;
                    Debug.Log("Zakończono żądanie wysyłania!");
                    break;
            }
        }
    }
}
