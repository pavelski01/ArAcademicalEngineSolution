using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using Assets.Scripts;

public class NadzorcaZdarzen : MonoBehaviour
{   
    private static NadzorcaZdarzen kierownikZdarzen;

    private Dictionary<string, ZdarzenieRr> SlownikZdarzen { get; set; }
    public DostepnoscKamery DostepnoscKamery { get; set; }
    public static NadzorcaZdarzen Instancja 
    {
        get 
        {
            if (!kierownikZdarzen) 
            {
                kierownikZdarzen = (NadzorcaZdarzen)FindObjectOfType(typeof(NadzorcaZdarzen));
                if (!kierownikZdarzen) 
                    Debug.Log("Musi być co najmniej jeden KierownikZdarzen");
                else kierownikZdarzen.Inicjalizuj();                
            }
            return kierownikZdarzen;
        }
    }
    
    void Inicjalizuj()
    {
        if (SlownikZdarzen == null)
            SlownikZdarzen = new Dictionary<string, ZdarzenieRr>();
        if (DostepnoscKamery == null)
            DostepnoscKamery = gameObject.AddComponent<DostepnoscKamery>();
    }
    public static void RozpocznijNasluch(string nazwaZdarzenia, UnityAction<ZdarzenieRrDto> nasluchiwacz) 
    {
        if (Instancja.SlownikZdarzen.TryGetValue(nazwaZdarzenia, out ZdarzenieRr zdarzenie))
            zdarzenie?.AddListener(nasluchiwacz);
        else
        {
            zdarzenie = new ZdarzenieRr();
            zdarzenie.AddListener(nasluchiwacz);
            Instancja.SlownikZdarzen.Add(nazwaZdarzenia, zdarzenie);
        }
    }
    public static void ZakonczNasluch(string nazwaZdarzenia, UnityAction<ZdarzenieRrDto> nasluchiwacz)
    {
        if (Instancja == null || nasluchiwacz == null)
            return;
        if (Instancja.SlownikZdarzen.TryGetValue(nazwaZdarzenia, out ZdarzenieRr zdarzenie))
            zdarzenie?.RemoveListener(nasluchiwacz);
    }
    public static void WyzwolZdarzenie(ZdarzenieRrDto zdarzenie) 
    {
        OdpluskwiaczDziennik.WpisDziennika("Zdarzenie Rozgloszone: " + zdarzenie.nazwa);
        if (Instancja.SlownikZdarzen.TryGetValue(zdarzenie.nazwa, out ZdarzenieRr zarejestrowanaAkcja))
            zarejestrowanaAkcja?.Invoke(zdarzenie);
    }    
}