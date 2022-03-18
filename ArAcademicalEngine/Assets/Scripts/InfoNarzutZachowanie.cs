using UnityEngine;

public class InfoNarzutZachowanie : SterownikZdarzen
{
    private GameObject narzut;
    void Awake()
    {
        narzut = transform.Find("InfoNarzut").gameObject;
        narzut.SetActive(false);
        ZarejestrujZdarzenie("nacisnietoZdjecieDesc", OtworzNarzut);
    }
    private void OtworzNarzut(ZdarzenieRrDto eventData) => 
        narzut.SetActive(true);
    public void ZamknijNarzut() => narzut.SetActive(false);
}
