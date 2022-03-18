using UnityEngine;

public class ZdarzenieRrDto
{
    public string nazwa;
    public GameObject objekt;
    public string sciezka;
    public ZdarzenieRrDto() {}
    public ZdarzenieRrDto(string nazwa, GameObject objekt) 
    {
        this.nazwa = nazwa;
        this.objekt = objekt;
    }
    public ZdarzenieRrDto(string nazwa, GameObject objekt, string sciezka)
    {
        this.nazwa = nazwa;
        this.objekt = objekt;
        this.sciezka = sciezka;
    }
}
