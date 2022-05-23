using UnityEngine;

public class CelObrazuZachowanie : DefaultObserverEventHandler
{
    protected override void OnTrackingFound()
    {
        var eventData = new ZdarzenieRrDto
        {
            objekt = transform.gameObject,
            nazwa = "znalezionoCelObrazu"
        };
        Debug.Log(nameof(OnTrackingFound) + $":{eventData.nazwa}");
        NadzorcaZdarzen.WyzwolZdarzenie(eventData);
    }
    protected override void OnTrackingLost()
    {
        var eventData = new ZdarzenieRrDto
        {
            objekt = transform.gameObject,
            nazwa = "straconoCelObrazu"
        };
        Debug.Log(nameof(OnTrackingLost) + $":{eventData.nazwa}");
        NadzorcaZdarzen.WyzwolZdarzenie(eventData);
    }
}
