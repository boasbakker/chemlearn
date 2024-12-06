using TMPro;
using UnityEngine;

public class UitlegScript : MonoBehaviour
{
    [SerializeField] GameObject info;
    void Start()
    {
        if (!PlayerPrefs.HasKey("infoGezien"))
        {
            PlayerPrefs.SetInt("infoGezien", 1);
            info.SetActive(true);
        }
    }

    public void Sluit()
    {
        info.SetActive(false);
    }
}
