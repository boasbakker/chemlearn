using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;
using static Definities;
using static Definities.AtoomSoort;
using static NaamGever;

public class TekenScript : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] Canvas canvas;
    [SerializeField] GameObject textPrefab;
    [SerializeField] UILineRendererList lijnPrefab;
    public NaamGever naamGever;
    //[SerializeField] Button undoButton;
    public Vector2? klikPositie = null;
    public int klikNr = -1;
    string atoom = "C";
    public const float afstand = 7;
    public List<Actie> acties = new();
    public static SortedSet<int> fouteValenties = new();
    public List<AtoomScript2D> atomen = new();
    public Dictionary<(int, int), List<UILineRendererList>> lijnObjecten = new();
    [SerializeField] Image geselecteerd;
    [SerializeField] RectTransform rectTransform;
    [SerializeField] RectTransform middle;
    [SerializeField] RectTransform safeArea;

    void Destroy(AtoomScript2D atoom)
    {
        atomen.Remove(atoom);
        OefenModusScript.getekendeAtomen.Remove(atoom);
        OefenModusScript.antwoordAtomen.Remove(atoom);
        Destroy(atoom.gameObject);
    }

    void Destroy(UILineRendererList binding)
    {
        OefenModusScript.getekendeBindingen.Remove(binding);
        OefenModusScript.antwoordBindingen.Remove(binding);
        Destroy(binding.gameObject);
    }

    public class Actie
    {
        public int nr1 = -1;
        public int nr2 = -1;
        public int type = 1;
        public AtoomScript2D atoom;
        public Vector2 a, b;
    }

    public void ZetAtoom(TMP_Text waarde)
    {
        geselecteerd.color = Color.white;
        geselecteerd = waarde.GetComponentInParent<Image>();
        geselecteerd.color = new(0.9f, 0.9f, 0.9f);
        atoom = waarde.text;
    }

    private void Update()
    {
        if (OefenModusScript.AanHetOefenen) return;
        if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
        {
            if (Input.GetKeyDown(KeyCode.Z))
            {
                Undo();
            }
            if (Input.GetKeyDown(KeyCode.R))
            {
                ResetMolecuul();
            }
        }
    }

    public void Undo()
    {
        if (acties.Empty()) return;
        Actie act = acties.Back();
        if (act.nr1 != -1)
        {
            foreach (var obj in lijnObjecten[(act.nr1, act.nr2)])
            {
                if (obj == null) continue;
                obj.ClearPoints();
                Destroy(obj);
            }
            lijnObjecten[(act.nr1, act.nr2)].Clear();
            for (int i = 1; i <= maxBindingen; i++)
            {
                if (bindingen[act.nr1].Contains((act.nr2, i)))
                {
                    aantalBindingen[act.nr1] -= act.type;
                    aantalBindingen[act.nr2] -= act.type;
                    bindingen[act.nr1].Remove((act.nr2, i));
                    bindingen[act.nr2].Remove((act.nr1, i));
                    if (i - act.type >= 1)
                    {
                        bindingen[act.nr1].Add((act.nr2, i - act.type));
                        bindingen[act.nr2].Add((act.nr1, i - act.type));
                        TekenBindng(act.nr1, act.nr2, act.a, act.b, i - act.type);
                    }
                    atomen[act.nr1].SetWaterstof(OctetRegel(act.nr1).aantalWaterstof);
                    atomen[act.nr2].SetWaterstof(OctetRegel(act.nr2).aantalWaterstof);
                }
            }
        }
        if (act.atoom != null)
        {
            if (n - 1 == klikNr)
            {
                klikNr = -1;
                klikPositie = null;
            }
            Destroy(act.atoom.GetComponent<AtoomScript2D>());
            n--;
            bindingen.PopBack();
            atoomsoort.PopBack();
            aantalBindingen.PopBack();
            aantalWaterstof.PopBack();
            lading.PopBack();
            fixeerdeOctetten.PopBack();
        }
        acties.PopBack();
        naamGever.GeefNaamGetekendeStructuur();
    }

    public void ResetMolecuul()
    {
        OefenModusScript.VerwijderAlles();
        foreach (var act in acties)
        {
            if (act.nr1 != -1)
            {
                foreach (var obj in lijnObjecten[(act.nr1, act.nr2)])
                {
                    if (obj == null) continue;
                    obj.ClearPoints();
                    Destroy(obj);
                }
            }
            if (act.atoom != null)
            {
                Destroy(act.atoom);
            }
        }
        klikNr = -1;
        klikPositie = null;
        atomen.Clear();
        n = 0;
        bindingen.Clear();
        atoomsoort.Clear();
        aantalBindingen.Clear();
        aantalWaterstof.Clear();
        lading.Clear();
        fixeerdeOctetten.Clear();
        acties.Clear();
        fouteValenties.Clear();
        naamGever.GeefNaamGetekendeStructuur();
    }


    public void TekenLijn(Vector2 a, Vector2 b, float offSet, int nr1, int nr2, bool extra, bool onzichtbaar = false)
    {
        UILineRendererList lijnRenderer = Instantiate(lijnPrefab, transform);
        lijnRenderer.transform.localScale = Vector3.one;
        if (extra) lijnRenderer.color = Color.red;
        lijnRenderer.transform.SetParent(lijnPrefab.transform.parent);
        lijnRenderer.transform.position = middle.position;
        Vector2 richting = b - a;
        Vector2 loodRecht = new Vector2(-richting.y, richting.x).normalized;
        loodRecht *= offSet;
        lijnRenderer.AddPoint(a + loodRecht);
        lijnRenderer.AddPoint(b + loodRecht);
        if (extra)
        {
            OefenModusScript.lijnen.Add(lijnRenderer);
        }
        else
        {
            lijnObjecten.TryAdd((nr1, nr2), new());
            lijnObjecten[(nr1, nr2)].Add(lijnRenderer);
            lijnObjecten.TryAdd((nr2, nr1), new());
            lijnObjecten[(nr2, nr1)].Add(lijnRenderer);
            if (OefenModusScript.AanHetOefenen)
            {
                if (onzichtbaar)
                {
                    OefenModusScript.antwoordBindingen.Add(lijnRenderer);
                    lijnRenderer.gameObject.SetActive(false);
                }
                else
                {
                    OefenModusScript.getekendeBindingen.Add(lijnRenderer);
                }
            }
        }
    }

    public static (int aantalWaterstof, int vrijeElektronenParen) OctetRegel(AtoomSoort soort, int valentieElektronen, int bindingen, int nr = -1)
    {
        List<int> octetten = Octetten(soort);
        if (nr != -1 && fixeerdeOctetten[nr] != -1) octetten = new() { fixeerdeOctetten[nr] };
        if (nr == -1) octetten.Shuffle();
        foreach (int octet in octetten)
        {
            // oplossing van de vergelijking: valentieElektronen + (aantalBindingen + aantalWaterstof) = 8
            int h = octet - valentieElektronen - bindingen;
            // oplossing van de vergelijking: vrijeElektronenParen * 2 + aantalBindingen + h == valentieElektronen
            double vrijeElektronenParen = (valentieElektronen - h - bindingen) / 2.0d;
            if (h >= 0 && vrijeElektronenParen >= 0 && vrijeElektronenParen.IsInteger())
            {
                return (h, (int)vrijeElektronenParen);
            }
        }
        return (-1, -1);
    }

    public static (int aantalWaterstof, int vrijeElektronenParen) OctetRegel(int nr, int extra = 0, bool readOnly = false)
    {
        fouteValenties.Remove(nr);
        AtoomSoort soort = atoomsoort[nr];
        int valentieElektronen = Definities.valentieElektronen[soort] - lading[nr],
            bindingen = aantalBindingen[nr] + extra;
        if (fixeerdeWaterstof[nr] != -1)
        {
            bindingen += fixeerdeWaterstof[nr];
            if ((valentieElektronen - bindingen) % 2 == 1)
            {
                fouteValenties.Add(nr);
                return (-1, -1);
            }
            if (!readOnly) aantalWaterstof[nr] = fixeerdeWaterstof[nr];
            return (fixeerdeWaterstof[nr], (valentieElektronen - bindingen) / 2);
        }
        (int, int) res = OctetRegel(soort, valentieElektronen, bindingen, nr);
        if (res == (-1, -1))
            fouteValenties.Add(nr);
        if (!readOnly) aantalWaterstof[nr] = res.Item1;
        return res;
    }

    public void TekenBindng(int nr1, int nr2, Vector2 a, Vector2 b, int type, bool extra = false, bool onzichtbaar = false)
    {
        if (type != 2)
        {
            TekenLijn(a, b, 0, nr1, nr2, extra, onzichtbaar);
        }
        else
        {
            TekenLijn(a, b, afstand / 2, nr1, nr2, extra, onzichtbaar);
            TekenLijn(a, b, -afstand / 2, nr1, nr2, extra, onzichtbaar);
        }
        if (type == 3)
        {
            TekenLijn(a, b, afstand, nr1, nr2, extra, onzichtbaar);
            TekenLijn(a, b, -afstand, nr1, nr2, extra, onzichtbaar);
        }
    }

    public void MaakBinding(Vector2 a, Vector2 b, int nr1, int nr2, AtoomScript2D atoomObj, int type = -1, bool onzichtbaar = false)
    {
        if (nr1 == nr2) return;
        if (type == -1)
        {
            type = 1;
            if (Input.GetKey(KeyCode.Alpha2))
            {
                type = 2;
            }
            if (Input.GetKey(KeyCode.Alpha3))
            {
                type = 3;
            }
            if (atoom == "≡") type = 3;
            if (atoom == "=") type = 2;
        }
        for (int i = maxBindingen - type + 1; i <= maxBindingen; i++)
        {
            if (bindingen[nr1].Contains((nr2, i)))
            {
                return; // aantal bindingen wordt te hoog
            }
        }
        atomen[nr1].SetWaterstof(OctetRegel(nr1, type).aantalWaterstof);
        atomen[nr2].SetWaterstof(OctetRegel(nr2, type).aantalWaterstof);
        aantalBindingen[nr1] += type;
        aantalBindingen[nr2] += type;
        Actie toevoeging = new()
        {
            nr1 = nr1,
            nr2 = nr2,
            atoom = atoomObj,
            type = type,
            a = a,
            b = b
        };
        for (int i = 1; i <= maxBindingen - type; i++)
        {
            if (bindingen[nr1].Contains((nr2, i)))
            {
                type += i;
                bindingen[nr1].RemoveAll(b => b == (nr2, i));
                bindingen[nr2].RemoveAll(b => b == (nr1, i));
                foreach (var obj in lijnObjecten[(nr1, nr2)])
                    if (obj != null) obj.ClearPoints();
                foreach (var obj in lijnObjecten[(nr1, nr2)])
                    if (obj != null) Destroy(obj);
                lijnObjecten.Remove((nr1, nr2));
                lijnObjecten.Remove((nr2, nr1));
            }
        }
        bindingen[nr2].Add((nr1, type));
        bindingen[nr1].Add((nr2, type));
        naamGever.GeefNaamGetekendeStructuur();
        TekenBindng(nr1, nr2, a, b, type, false, onzichtbaar);
        klikPositie = null;
        acties.Add(toevoeging);
    }

    public void SpawnAtoom(Vector2 positie, AtoomSoort atoomSoort, bool onzichtbaar = false)
    {
        n++;
        atoomsoort.Add(atoomSoort);
        bindingen.Add(new());
        aantalBindingen.Add(0);
        aantalWaterstof.Add(0);
        lading.Add(0);
        fixeerdeOctetten.Add(-1);
        fixeerdeWaterstof.Add(-1);

        GameObject spawnedText = Instantiate(textPrefab, safeArea.transform);
        spawnedText.transform.SetSiblingIndex(6);
        RectTransform rect = spawnedText.GetComponent<RectTransform>();
        rect.anchoredPosition = positie;
        AtoomScript2D atoomScript = spawnedText.GetComponent<AtoomScript2D>();
        atoomScript.pos = positie;
        atoomScript.nr = n - 1;
        atoomScript.atoomSoort = atoomSoort.ToString();
        atomen.Add(atoomScript);

        spawnedText.SetActive(!onzichtbaar);
        if (OefenModusScript.AanHetOefenen)
        {
            if (onzichtbaar)
            {
                OefenModusScript.antwoordAtomen.Add(atoomScript);
            }
            else OefenModusScript.getekendeAtomen.Add(atoomScript);
        }
        TMP_Text text = spawnedText.GetComponentInChildren<TMP_Text>();
        text.text = atoom;

        if (klikPositie != null)
        {
            MaakBinding((Vector2)klikPositie, rect.anchoredPosition, klikNr, atoomsoort.Count - 1, atoomScript, -1, onzichtbaar);
        }
        else // anders wordt het in MaakBinding gecalled
        {
            atoomScript.SetWaterstof(OctetRegel(atoomScript.nr).aantalWaterstof);
            naamGever.GeefNaamGetekendeStructuur();
            Actie toevoeging = new()
            {
                atoom = atoomScript
            };
            acties.Add(toevoeging);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (OefenModusScript.AanHetOefenen && OefenModusScript.huidigeModus != Modus.NaamgevingAndersom) return;
        if (OefenModusScript.goedAntwoordZichtbaar) return;
        Vector2 screenPos = eventData.position;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas.GetComponent<RectTransform>(), screenPos, canvas.worldCamera, out Vector2 localPos);

        if (!char.IsLetter(atoom[0]))
        {
            return;
        }
        AtoomSoort atoomSoort = (AtoomSoort)Enum.Parse(typeof(AtoomSoort), atoom);
        SpawnAtoom(localPos, atoomSoort);
    }
}
