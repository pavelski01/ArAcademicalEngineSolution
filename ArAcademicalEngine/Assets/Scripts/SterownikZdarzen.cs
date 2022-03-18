using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SterownikZdarzen : MonoBehaviour
{
    private readonly Dictionary<string, UnityAction<ZdarzenieRrDto>> slownikZdarzen = 
        new Dictionary<string, UnityAction<ZdarzenieRrDto>>();
    private void OnEnable()
    {
        foreach (var wpis in slownikZdarzen)
            NadzorcaZdarzen.RozpocznijNasluch(wpis.Key, wpis.Value);
    }
    private void OnDisable()
    {
        foreach (var wpis in slownikZdarzen)
            NadzorcaZdarzen.ZakonczNasluch(wpis.Key, wpis.Value);
    }
    protected void ZarejestrujZdarzenie(string nazwaZdarzenia, Action<ZdarzenieRrDto> action) =>
        slownikZdarzen.Add(nazwaZdarzenia, new UnityAction<ZdarzenieRrDto>(action));
}
