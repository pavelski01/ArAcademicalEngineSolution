using UnityEngine;
using UnityEngine.UI;

public class OdpluskwiaczDziennik : MonoBehaviour 
{
    private static Text Wpis;
    void Start() => Wpis = (Text)gameObject.GetComponent(typeof(Text));
    public static void WpisDziennika(string wiadomosc)
    {
        if (Wpis != null)
            Wpis.text += "\n" + wiadomosc;
    }
}
