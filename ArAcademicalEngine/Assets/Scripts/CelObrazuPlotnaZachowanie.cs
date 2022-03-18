using UnityEngine;
using UnityEngine.EventSystems;

public class CelObrazuPlotnaZachowanie : MonoBehaviour, IPointerClickHandler
{
    public string sciezkaZasobu;
    public void OnPointerClick(PointerEventData daneWskaznika)
    {
        var objekt = daneWskaznika.pointerCurrentRaycast.gameObject;
        if (objekt != null)
        {
            var nazwaZdarzenia = "nacisnietoZdjecie" + objekt.tag;
            var daneZdarzenia = new ZdarzenieRrDto(nazwaZdarzenia, objekt, sciezkaZasobu);
            NadzorcaZdarzen.WyzwolZdarzenie(daneZdarzenia);
        }
    }
}
