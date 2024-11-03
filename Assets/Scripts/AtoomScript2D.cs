using TMPro;
using UnityEngine;
using static TekenScript;
using static InstellingenScript;
using System.Linq;
using UnityEngine.UI;
using System.Collections.Generic;
using System;
using Unity.VisualScripting;
using static Definities;

public class AtoomScript2D : MonoBehaviour
{
    [SerializeField] Canvas canvas;
    [SerializeField] RectTransform rectTransform;
    [SerializeField] TekenScript tekenScript;
    [SerializeField] TMP_Text text;
    [SerializeField] GameObject plusLading;
    [SerializeField] GameObject minLading;
    [SerializeField] GameObject andereLading;
    [SerializeField] TMP_Text andereLadingText;
    public Image image;
    public int nr;
    public string atoomSoort;
    Color geselecteerdeKleur = new(0.9f, 0.9f, 0.9f);
    public Vector2 pos;
    public TMP_Text antwoord;

    public void SetWaterstof(int x)
    {
        if (x < 0)
        {
            fouteValenties.Add(nr);
            return;
        }
        text.text = atoomSoort;
        if (x > 0) text.text += 'H';
        if (x > 1) text.text += x;
    }

    public void OnClick()
    {
        if (OefenModusScript.goedAntwoordZichtbaar) return;
        if (OefenModusScript.klikBindingen)
        {
            if (OefenModusScript.selectie.Count % 2 == 1)
            {
                AtoomScript2D vorige = OefenModusScript.selectie.Back();
                if (vorige.nr != nr)
                {
                    int type = 1;
                    foreach (var (buur, t) in NaamGever.bindingen[nr]) if (vorige.nr == buur) type = t;
                    tekenScript.TekenBindng(vorige.nr, nr, vorige.pos, pos, type, true);
                }
            }
            OefenModusScript.selectie.Add(this);
            return;
        }
        if (OefenModusScript.klikMeerdereAtomen)
        {
            OefenModusScript.selectie.Add(this);
            return;
        }
        if (OefenModusScript.klikAtomen)
        {
            GetComponent<Button>().transition = Selectable.Transition.None;
            if (OefenModusScript.selectie.Contains(this))
            {
                image.color = Color.white;
                OefenModusScript.selectie.Remove(this);
                return;
            }
            image.color = geselecteerdeKleur;
            OefenModusScript.selectie.Add(this);
            return;
        }
        if (OefenModusScript.AanHetOefenen && OefenModusScript.huidigeModus != Modus.NaamgevingAndersom) return;
        if (octet != null)
        {
            NaamGever.fixeerdeOctetten[nr] = (int)octet;
            octet = null;
            SetWaterstof(OctetRegel(nr).aantalWaterstof);
            tekenScript.naamGever.GeefNaamGetekendeStructuur();
            return;
        }
        if (lading != null)
        {
            NaamGever.lading[nr] = (int)lading;
            SetWaterstof(OctetRegel(nr).aantalWaterstof);
            tekenScript.naamGever.GeefNaamGetekendeStructuur();
            plusLading.SetActive(lading == 1);
            minLading.SetActive(lading == -1);
            andereLading.SetActive(Mathf.Abs((float)lading) > 1.1);
            string ladingString = lading.ToString();
            if (lading > 0) ladingString.Insert(0, "+");
            andereLadingText.text = lading.ToString();
            lading = null;
            return;
        }
        if (tekenScript.klikPositie != null)
        {
            tekenScript.MaakBinding((Vector2)tekenScript.klikPositie, rectTransform.anchoredPosition, nr, tekenScript.klikNr, null);
        }
        else
        {
            tekenScript.klikPositie = rectTransform.anchoredPosition;
            tekenScript.klikNr = nr;
        }
    }
}
