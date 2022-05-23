using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class NarzutObrazuZachowanie : MonoBehaviour
{
    private GameObject narzut;
    private Image obraz;
    private UnityAction<ZdarzenieRrDto> onPhotoTapped;
    void Awake()
    {
        narzut = transform.Find("ObrazNarzutu").gameObject;
        obraz = narzut.transform.Find("KontenerObrazu").gameObject.GetComponent<Image>();
        narzut.SetActive(false);
        onPhotoTapped = new UnityAction<ZdarzenieRrDto>(WyswietlNarzut);
    }
    private void WyswietlNarzut(ZdarzenieRrDto eventData)
    {
        var nacisnietyObjekt = eventData.objekt;
        var res = eventData.sciezka;
        var nacisnietyKomponentObrazu = nacisnietyObjekt.GetComponent<Image>();
        var tag = nacisnietyObjekt.tag;
        Sprite imageSprite;
        if (string.IsNullOrEmpty(res))
            imageSprite = nacisnietyKomponentObrazu.sprite;
        else imageSprite = Resources.Load<Sprite>(res);
        obraz.sprite = imageSprite;
        if (tag == "Photo")
            narzut.SetActive(true);
    }  
    public void ZamknijNarzut() => narzut.SetActive(false);
    private void OnEnable() =>
        NadzorcaZdarzen.RozpocznijNasluch("nacisnietoZdjeciePhoto", onPhotoTapped);
    private void OnDisable() =>
        NadzorcaZdarzen.ZakonczNasluch("nacisnietoZdjeciePhoto", onPhotoTapped);
}
