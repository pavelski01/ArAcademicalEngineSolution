using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts
{
    public class ZrzutEkranu : MonoBehaviour
    {
        public AudioClip dzwiekPstrykniecia;
        public AudioSource zrodloDzwieku;
        public Toggle czyZrzut;
        private DostepnoscKamery DostepnoscKamery { get; set; }
        void Awake() =>
            DostepnoscKamery = NadzorcaZdarzen.Instancja.DostepnoscKamery;
        public void ZaznaczPrzechwycanie()
        {
            zrodloDzwieku.PlayOneShot(dzwiekPstrykniecia);
            DostepnoscKamery.PrzechwycObraz = true;
            DostepnoscKamery.CzyZrzut = czyZrzut.isOn;
        }
    }
}