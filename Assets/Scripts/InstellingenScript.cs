using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static Definities.KarakteristiekeGroep;
using static Definities;

public class InstellingenScript : MonoBehaviour
{
    [SerializeField] TMP_Text text;
    [SerializeField] TMP_InputField ladingVeld;
    [SerializeField] TMP_InputField octetVeld;
    [SerializeField] TMP_Text tijdText;
    [SerializeField] TMP_Text operatiesText;
    public static int? octet = null;
    public static int? lading = null;
    public static double genereerTijd = double.NaN;
    public static bool negeerUnsupported = false;
    public static bool skipNaam = false;
    public static bool selecteerOpMinimaleAfstand = true;
    public static bool selecteerOpUiteindeAfstand = true;
    public static double operatiesPerFrame = 2e6;
    public static bool oudeNaamGeving = false;

    // random molecuul
    public static float cyclischKans = 0.4f;
    public static float bindingDubbelKans = 0.15f; // 56% op >=1 bij lengte 6
    public static float bindingDrieDubbelKans = 0.02f; // 9.6% op >=1 bij lengte 6
    public static float benzeenKans = 0.05f;
    public static float zijTakKans = 0.2f;
    public static float groepKans = 0.12f;
    public static float uiteindeGroepKans = 0.15f;
    public static int maxKoolstof = 10;

    [SerializeField] GameObject orthoGraphic;
    [SerializeField] GameObject perspective;

    public void VeranderRenderModus(int optie)
    {
        selecteerOpUiteindeAfstand = optie >= 1;
        selecteerOpMinimaleAfstand = optie != 1;
    }

    public void VeranderCamera(int optie)
    {
        orthoGraphic.SetActive(optie == 0);
        perspective.SetActive(optie == 1);
    }

    public void VeranderNegeerUnsupported(bool v)
    {
        negeerUnsupported = v;
    }
    public void VeranderOudeNaamgeving(bool v)
    {
        oudeNaamGeving = v;
    }
    public void VeranderSkipNaam(bool v)
    {
        skipNaam = v;
    }
    public void SetTijd(float value)
    {
        genereerTijd = Mathf.Pow(10f, value);
        tijdText.text = genereerTijd.ToString("F5") + "s";
    }
    public void SetOperaties(float value)
    {
        operatiesPerFrame = Mathf.Pow(10f, value);
        operatiesText.text = $"Operaties per frame: 10^{value:F2}\r\nZet lager bij lag tijdens het renderen.";
    }

    public void VerderOctet()
    {
        try
        {
            octet = int.Parse(octetVeld.text);
            if (octet < 0) throw new InvalidOperationException();
        }
        catch
        {
            octet = null;
            text.text = "Ongeldig aantal!";
            return;
        }
        text.text = "Klik op het atoom waar je het uitgebreid octet wil instellen!";
    }

    public void VerderLading()
    {
        try
        {
            lading = int.Parse(ladingVeld.text);
            if (lading < -2) throw new InvalidOperationException();
            if (lading > 8) throw new InvalidOperationException();
        }
        catch
        {
            lading = null;
            text.text = "Ongeldige lading!";
            return;
        }
        text.text = "Klik op het atoom waar je de lading wil instellen!";
    }
}
