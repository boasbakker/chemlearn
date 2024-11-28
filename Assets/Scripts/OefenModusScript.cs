using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using UnityEngine;
using static InstellingenScript;
using static Definities.AtoomSoort;
using static Definities.KarakteristiekeGroep;
using static Definities.Modus;
using static Definities;
using static NaamGever;
using System.Collections;
using TMPro;
using UnityEngine.UI.Extensions;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;
using NUnit.Framework.Constraints;
using UnityEngine.UI;
using Unity.VisualScripting;

public class OefenModusScript : MonoBehaviour
{
    [SerializeField] RectTransform canvasTransform;
    [SerializeField] TekenScript tekenScript;
    [SerializeField] TMP_Text vraagTekst;
    [SerializeField] GameObject antwoordKnop;
    float eenheidsLengte = 30f;
    float edgeThreshold = 50f;

    List<Vector3> posities = new();
    public static bool klikAtomen = false;
    public static bool klikMeerdereAtomen = false;
    public static bool klikBindingen = false;
    public static List<AtoomScript2D> selectie = new();
    public static bool AanHetOefenen = false;
    [SerializeField] GameObject rechterMenuStandaard;
    [SerializeField] GameObject rechterMenuOefenen;
    [SerializeField] TMP_InputField antwoordVeld;
    [SerializeField] NaamGever naamGever;
    [SerializeField] TekenScript3D tekenScript3D;
    [SerializeField] TMP_Dropdown JaNeeDropDown;
    [SerializeField] GameObject Knop3D;
    [SerializeField] TMP_Text infoText;
    [SerializeField] GameObject GeefOpKnop;
    [SerializeField] GameObject undoKnop;
    public static List<UILineRendererList> lijnen = new();

    public static Modus huidigeModus = Algemeen;
    public bool laadInfoScherm = false;
    List<int> massaSpecAntwoord;
    bool MagRing(Modus m)
    {
        return m switch
        {
            Algemeen => true,
            Naamgeving => true,
            Massaspectrometrie => false,
            Polariteit => false,
            Oplosbaarheid => false,
            _ => true,
        };
    }
    float KansAnorganisch(Modus m)
    {
        return m switch
        {
            Naamgeving => 0f,
            Massaspectrometrie => 0.2f,
            Modus.PolaireBindingen => 0.2f,
            Molmassa => 0.2f,
            Polariteit => 1f,
            Lewisstructuren => 0.5f,
            Oplosbaarheid => 0.9f,
            NaamgevingAndersom => 0f,
            _ => 0.1f,
        };
    }

    public int GetRandomWeightedIndex(List<float> weights)
    {
        if (weights == null || weights.Count == 0) return -1;

        float w;
        float t = 0f;
        int i;
        for (i = 0; i < weights.Count; i++)
        {
            w = weights[i];
            if (float.IsPositiveInfinity(w)) return i;
            else if (w >= 0f && !float.IsNaN(w)) t += weights[i];
        }

        float r = Random.value;
        float s = 0f;

        for (i = 0; i < weights.Count; i++)
        {
            w = weights[i];
            if (float.IsNaN(w) || w <= 0f) continue;

            s += w / t;
            if (s >= r) return i;
        }

        return -1;
    }

    int Rng(int mn, int mx) => Random.Range(mn, mx + 1);

    bool Kans(float kans) => Random.value <= kans;

    void SetOefenen(bool b)
    {
        ResetSelectie();
        rechterMenuOefenen.SetActive(b);
        rechterMenuStandaard.SetActive(!b);
        AanHetOefenen = b;
        if (!b)
        {
            if (huidigeModus == Modus.NaamgevingAndersom)
            {
                tekenScript.ResetMolecuul();
            }
            else
            {
                getekendeAtomen.Clear();
                getekendeBindingen.Clear();
                antwoordAtomen.Clear();
                antwoordBindingen.Clear();
            }
            klikAtomen = false;
            klikBindingen = false;
            klikMeerdereAtomen = false;
            antwoordKnop.SetActive(false);
            antwoordVeld.gameObject.SetActive(false);
            huidigeModus = Algemeen;
            JaNeeDropDown.gameObject.SetActive(false);
            naamGever.GeefNaamGetekendeStructuur();
            goedAntwoordZichtbaar = false;
        }
    }

    public void StopOefenModus()
    {
        SetOefenen(false);
    }

    bool IsNearEdge(Vector2 position)
    {
        // Get the size of the canvas
        Rect canvasRect = canvasTransform.rect;

        // Calculate the bounds (left, right, top, bottom)
        float leftEdge = canvasRect.xMin + edgeThreshold;
        float rightEdge = canvasRect.xMax - edgeThreshold;
        float topEdge = canvasRect.yMax - edgeThreshold;
        float bottomEdge = canvasRect.yMin + edgeThreshold;

        // Check if the position is near or over any edge
        if (position.x < leftEdge || position.x > rightEdge || position.y > topEdge || position.y < bottomEdge)
        {
            return true; // Position is near or over the edge
        }

        return false; // Position is within the safe zone
    }

    // voor geavanceerdere random moleculen die soms in de knoop raken
    /*void CheckAfstanden()
    {
        ClosestPair3D closestPair3D = new();
        if (closestPair3D.FindClosestPair(posities) < 0.99f) throw new System.Exception("Afstand <1");
    }*/
    public void SpawnMolecuul()
    {
        tekenScript.ResetMolecuul();
        for (int poging = 0; poging < 1e4; poging++)
        {
            bool succes = false;
            try
            {
                RandomMolecuul();
                succes = true;
            }
            catch
            {
                succes = false;
            }
            if (succes) return;
        }
    }
    public void SpawnMolecuulAnorganisch()
    {
        tekenScript.ResetMolecuul();
        for (int poging = 0; poging < 1e4; poging++)
        {
            bool succes = false;
            try
            {
                RandomMolecuulAnorganisch();
                succes = true;
                for (int i = 0; i < n; i++)
                {
                    if (tekenScript3D.OmringingsGetal(i) > 4)
                    {
                        succes = false;
                        tekenScript.ResetMolecuul();
                        break;
                    }
                }
            }
            catch
            {
                succes = false;
            }
            if (succes) return;
        }
    }

    AtoomSoort RandomSoort(ref List<int> vorigen)
    {
        // halogenen: 2
        // edelgassen/metalen: 0
        // zeldzame atomen: 1
        // veelvoorkomend: 4
        List<float> weights = new()
        {
            0, 0,
            0, 0, 0.5f, 8, 8, 12, 3, 0,
            0, 0, 0, 0.5f, 5, 5,  3, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0.5f, 0.5f, 3, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,    0.5f, 3, 0,
        };
        if (huidigeModus == Massaspectrometrie)
        {
            weights[(int)Se - 1] = 0;
            weights[(int)Br - 1] = 0;
            weights[(int)Te - 1] = 0;
            weights[(int)Xe - 1] = 0;
        }
        foreach (var s in vorigen) weights[s] += 20;
        int atoomNummer = GetRandomWeightedIndex(weights);
        vorigen.Add(atoomNummer);
        return (AtoomSoort)atoomNummer;
    }

    void RandomMolecuulAnorganisch()
    {
        List<int> aantallen = new() { 1, 2, 2, 2, 3, 3, 3, 3, 3, 4, 4, 4, 5 };
        int aantal = aantallen[Rng(0, aantallen.Count - 1)];
        List<(int, int, int, Vector2, Vector2)> bindingen = new();
        List<(Vector2, AtoomSoort)> atomen = new();
        List<int> atoomSoortenTotNuToe = new();
        List<int> bindingenOver = new();
        void maakBinding(Vector2 pos1, Vector2 pos2, int nr1, int nr2, int type)
        {
            bindingen.Add((nr1, nr2, type, pos1, pos2));
            bindingenOver[nr1] -= type;
            bindingenOver[nr2] -= type;
            if (bindingenOver[nr1] < 0)
                throw new();
            if (bindingenOver[nr2] < 0)
                throw new();
        }
        void spawnAtoom(Vector2 pos, Vector2 dxy, AtoomSoort soort, int type, int nr1 = -1)
        {
            if (nr1 == -1) nr1 = atomen.Count - 1;
            Vector2 pos1 = pos;
            pos1 += dxy;
            posities.Add(pos1);
            atomen.Add((pos1, soort));
            bindingenOver.Add(TekenScript.OctetRegel(soort, valentieElektronen[soort], 0).aantalWaterstof);
            if (nr1 >= 0)
            {
                maakBinding(pos, pos1, nr1, atomen.Count - 1, type);
            }
        }
        {
            int lengte = aantal <= 3 ? aantal : 3;
            float afstand = (lengte - 1f) / 2f;
            Vector2 pos = new(-afstand, 0f);
            spawnAtoom(pos, new(), RandomSoort(ref atoomSoortenTotNuToe), 0);
            for (int i = 1; i < lengte; i++)
            {
                int type = Kans(bindingDubbelKans) ? 2 : 1;
                if (Kans(bindingDrieDubbelKans)) type = 3;
                spawnAtoom(pos, new(1, 0), RandomSoort(ref atoomSoortenTotNuToe), type, i - 1);
                pos.x++;
            }
        }
        if (aantal >= 4)
        {
            int type = Kans(bindingDubbelKans) ? 2 : 1;
            if (Kans(bindingDrieDubbelKans)) type = 3;
            spawnAtoom(new(), new(0f, 1f), RandomSoort(ref atoomSoortenTotNuToe), type, 1);
            if (aantal == 5)
            {
                type = Kans(bindingDubbelKans) ? 2 : 1;
                if (Kans(bindingDrieDubbelKans)) type = 3;
                spawnAtoom(new(), new(0f, -1f), RandomSoort(ref atoomSoortenTotNuToe), type, 1);
            }
        }
        int massa = 0;
        foreach (var (pos, soort) in atomen)
        {
            massa += atoomMassa[soort];
            if (IsNearEdge(pos * eenheidsLengte))
                throw new();
        }
        if (huidigeModus == Massaspectrometrie && massa < 48) throw new();
        foreach (var (pos, soort) in atomen)
        {
            tekenScript.SpawnAtoom(pos * eenheidsLengte, soort);
        }
        foreach (var (nr1, nr2, type, a, b) in bindingen)
        {
            tekenScript.MaakBinding(a * eenheidsLengte, b * eenheidsLengte, nr1, nr2, null, type);
        }
    }

    void RandomMolecuul()
    {
        if (AanHetOefenen) maxKoolstof = 10;
        else maxKoolstof = 20;
        if (AanHetOefenen && huidigeModus == Oplosbaarheid) maxKoolstof = 6;
        int lengte = Rng(1, maxKoolstof);
        if (!AanHetOefenen) maxKoolstof = lengte + 10;
        bool cyclisch = Kans(cyclischKans) && MagRing(huidigeModus) && lengte >= 3;
        bool benzeen = Kans(benzeenKans) && (MagRing(huidigeModus) || huidigeModus == Polariteit || huidigeModus == Oplosbaarheid);
        if (benzeen)
        {
            lengte = 6;
            cyclisch = true;
        }
        List<(int, int, int, Vector2, Vector2)> bindingen = new();
        List<(Vector2, AtoomSoort)> atomen = new();
        int aantalC = 0;
        List<int> bindingenOver = new();
        List<Vector2> richtingsVectoren = new();
        void maakBinding(Vector2 pos1, Vector2 pos2, int nr1, int nr2, int type)
        {
            bindingen.Add((nr1, nr2, type, pos1, pos2));
            bindingenOver[nr1] -= type;
            bindingenOver[nr2] -= type;
            if (bindingenOver[nr1] < 0)
                throw new();
            if (bindingenOver[nr2] < 0)
                throw new();
        }
        void spawnAtoom(Vector2 pos, Vector2 dxy, AtoomSoort soort, int type, int nr1 = -1)
        {
            if (nr1 == -1) nr1 = atomen.Count - 1;
            Vector2 pos1 = pos;
            pos1 += dxy;
            posities.Add(pos1);
            atomen.Add((pos1, soort));
            if (soort == C)
            {
                bindingenOver.Add(4);
                aantalC++;
            }
            else if (soort == O) bindingenOver.Add(2);
            else if (soort == N) bindingenOver.Add(3);
            else bindingenOver.Add(1);
            if (nr1 >= 0)
            {
                maakBinding(pos, pos1, nr1, atomen.Count - 1, type);
            }
        }
        void SpawnZijtak(int maxLengte, Vector2 pos, Vector2 richting, int start = -1)
        {
            int lengtezijtak = Rng(1, maxLengte);
            for (int k = 0; k < lengtezijtak; k++)
            {
                spawnAtoom(pos, richting, C, 1, k == 0 ? start : -1);
                pos += richting;
            }
        }
        if (!cyclisch)
        {
            float afstand = (lengte - 1f) / 2f;
            Vector2 pos = new(-afstand, 0f);
            spawnAtoom(pos, new(), C, 0);
            for (int i = 1; i < lengte; i++)
            {
                int type = Kans(bindingDubbelKans) ? 2 : 1;
                if (Kans(bindingDrieDubbelKans)) type = 3;
                spawnAtoom(pos, new(1, 0), C, type, i - 1);
                pos.x++;
            }
        }
        else
        {
            float afstand = 0.5f / Mathf.Sin(Mathf.PI / lengte); // Circumscribed circle radius https://en.wikipedia.org/wiki/Regular_polygon
            float hoekVershil = 360f / lengte; // Hoek tussen elke atoom in graden

            Vector2 beginPos = new(), eindPos = new();
            int type;
            for (int i = 0; i < lengte; i++)
            {
                float hoekInRadialen = i * hoekVershil * Mathf.Deg2Rad; // Omzetten naar radialen voor Mathf trigonometrische functies
                float x = afstand * Mathf.Cos(hoekInRadialen); // X-positie gebaseerd op hoek en straal
                float y = afstand * Mathf.Sin(hoekInRadialen); // Y-positie gebaseerd op hoek en straal

                type = Kans(bindingDubbelKans) ? 2 : 1;
                if (Kans(bindingDrieDubbelKans)) type = 3;
                if (benzeen) type = 2 - (i % 2); // afwisselend dubbel en enkel bij benzeen
                Vector2 pos = new(x, y);
                richtingsVectoren.Add(pos.normalized);
                Vector2 vorige = i == 0 ? new() : atomen.Back().Item1;
                Vector2 dpos = pos - vorige;
                spawnAtoom(vorige, dpos, C, type, i - 1);
                if (i == 0) beginPos = pos;
                else if (i == lengte - 1) eindPos = pos;
            }
            type = Kans(bindingDubbelKans) ? 2 : 1;
            if (Kans(bindingDrieDubbelKans)) type = 3;
            if (benzeen) type = 2;
            maakBinding(beginPos, eindPos, 0, lengte - 1, type);
        }
        for (int ix = 0; ix < 2; ix++)
        {
            int i = ix == 0 ? 0 : lengte - 1;
            if (!cyclisch && Kans(uiteindeGroepKans))
            {
                List<KarakteristiekeGroep> groepen = new()
                {
                    Carbonzuur,
                    Aldehyde,
                    Ester
                };
                if (huidigeModus != Naamgeving && huidigeModus != Modus.NaamgevingAndersom)
                {
                    groepen.Add(Nitril);
                }
                var groep = groepen[Rng(0, groepen.Count - 1)];
                while (aantalC >= maxKoolstof && groep == Ester)
                {
                    groep = groepen[Rng(0, groepen.Count - 1)];
                }
                float x = (lengte - 1) / -2f + i * 1f;
                float richting = ix == 0 ? -1f : 1f;
                Vector2 pos = new(x, 0f);

                if (groep == Nitril)
                {
                    spawnAtoom(pos, new(richting, 0), N, 3, i); // dubbele O is L/R
                }
                else
                {
                    spawnAtoom(pos, new(richting, 0), O, 2, i); // dubbele O is L/R
                    if (groep == Carbonzuur || groep == Ester)
                    {
                        spawnAtoom(pos, new(0, -1), O, 1, i);
                    }
                    if (groep == Ester)
                    {
                        pos.y--;
                        SpawnZijtak(maxKoolstof - aantalC, pos, new(0f, -1f));
                    }
                }
                bindingenOver[i] = 0;
            }
        }
        for (int i = 0; i < lengte; i++)
        {
            int aantalBindingen = 0;
            for (int j = 0; j < bindingenOver[i]; j++)
            {
                if (j > 0 && cyclisch) break;
                if (Kans(groepKans))
                {
                    List<KarakteristiekeGroep> groepen = new()
                    {
                        Hydroxylgroep,
                        Keton,
                        Amine,
                        Ether,
                        Halogeengroep,
                    };
                    if (huidigeModus != Naamgeving && huidigeModus != Modus.NaamgevingAndersom)
                    {
                        groepen.Add(Thiol);
                        groepen.Add(Thialdehyde);
                        groepen.Add(Imine);
                    }
                    var groep = groepen[Rng(0, groepen.Count - 1)];
                    while (aantalC >= maxKoolstof && groep == Ether)
                    {
                        groep = groepen[Rng(0, groepen.Count - 1)];
                    }
                    aantalBindingen++;
                    Vector2 richting = new(0f, j == 0 ? -1f : 1f);
                    Vector2 pos = new((lengte - 1) / -2f + i * 1f, 0f);

                    if (cyclisch)
                    {
                        pos = atomen[i].Item1;
                        richting = richtingsVectoren[i];
                    }

                    if (groep == Keton)
                    {
                        aantalBindingen++;
                        spawnAtoom(pos, richting, O, 2, i);
                    }
                    if (groep == Imine)
                    {
                        aantalBindingen++;
                        spawnAtoom(pos, richting, N, 2, i);
                    }
                    if (groep == Thialdehyde)
                    {
                        aantalBindingen++;
                        spawnAtoom(pos, richting, S, 2, i);
                    }
                    if (groep == Thiol)
                    {
                        spawnAtoom(pos, richting, S, 1, i);
                    }
                    if (groep == Ether || groep == Hydroxylgroep)
                    {
                        spawnAtoom(pos, richting, O, 1, i);
                    }
                    if (groep == Amine)
                    {
                        spawnAtoom(pos, richting, N, 1, i);
                    }
                    if (groep == Ether)
                    {
                        pos += richting;
                        SpawnZijtak(Mathf.Min(maxKoolstof - aantalC, 4), pos, richting);
                    }
                    if (groep == Halogeengroep)
                    {
                        List<AtoomSoort> atoom = new() { Cl, I, F, Br };
                        AtoomSoort soort = atoom[Rng(0, atoom.Count - 1)];
                        while (huidigeModus == Massaspectrometrie && soort == Br) soort = atoom[Rng(0, atoom.Count - 1)];
                        spawnAtoom(pos, richting, soort, 1, i);
                    }
                }
                else if (Kans(zijTakKans) && aantalC < maxKoolstof)
                {
                    aantalBindingen++;
                    Vector2 richting = new(0f, j == 0 ? -1f : 1f);
                    Vector2 pos = new((lengte - 1) / -2f + i * 1f, 0f);
                    if (cyclisch)
                    {
                        pos = atomen[i].Item1;
                        richting = richtingsVectoren[i];
                    }
                    SpawnZijtak(maxKoolstof - aantalC, pos, richting, i);
                }
            }
            bindingenOver[i] -= aantalBindingen;
        }
        int massa = 0;
        foreach (var (pos, soort) in atomen)
        {
            massa += atoomMassa[soort];
            if (IsNearEdge(pos * eenheidsLengte))
                throw new();
        }
        if (huidigeModus == Massaspectrometrie && massa < 48) throw new();
        foreach (var (pos, soort) in atomen)
        {
            tekenScript.SpawnAtoom(pos * eenheidsLengte, soort, huidigeModus == NaamgevingAndersom);
        }
        foreach (var (nr1, nr2, type, a, b) in bindingen)
        {
            tekenScript.MaakBinding(a * eenheidsLengte, b * eenheidsLengte, nr1, nr2, null, type, huidigeModus == NaamgevingAndersom);
        }
    }

    int gevraagdeMassa = 0;

    public void NaamgevingAndersomVraag()
    {
        huidigeModus = NaamgevingAndersom;
        NieuweVraag();
    }

    public void MassaSpecVraag()
    {
        huidigeModus = Massaspectrometrie;
        NieuweVraag();
    }
    public void NaamGevingVraag()
    {
        huidigeModus = Naamgeving;
        NieuweVraag();
    }
    public void MolMassaVraag()
    {
        huidigeModus = Molmassa;
        NieuweVraag();
    }
    public void WaterStofBrugVraag()
    {
        huidigeModus = Waterstofbruggen;
        NieuweVraag();
    }
    public void PolaireBindingenVraag()
    {
        huidigeModus = Modus.PolaireBindingen;
        NieuweVraag();
    }

    public void PolariteitVraag()
    {
        huidigeModus = Polariteit;
        NieuweVraag();
    }
    public void OplosbaarheidVraag()
    {
        huidigeModus = Oplosbaarheid;
        NieuweVraag();
    }
    public void LewisstructuurVraag()
    {
        huidigeModus = Lewisstructuren;
        NieuweVraag();
    }

    public void OpenZoInfoScherm()
    {
        laadInfoScherm = true;
        if (!tekenScript3D.Render3D(true))
        {
            OpenInfoScherm(true);
        }
    }

    void SwitchVisibility()
    {
        goedAntwoordZichtbaar = !goedAntwoordZichtbaar;
        foreach (var a in antwoordAtomen)
        {
            a.gameObject.SetActive(goedAntwoordZichtbaar);
        }
        foreach (var a in antwoordBindingen)
        {
            a.gameObject.SetActive(goedAntwoordZichtbaar);
        }
        foreach (var a in getekendeAtomen)
        {
            a.gameObject.SetActive(!goedAntwoordZichtbaar);
        }
        foreach (var a in getekendeBindingen)
        {
            a.gameObject.SetActive(!goedAntwoordZichtbaar);
        }
    }

    public static void VerwijderAlles()
    {
        foreach (var a in getekendeAtomen)
        {
            Destroy(a.gameObject);
        }
        foreach (var a in getekendeBindingen)
        {
            Destroy(a.gameObject);
        }
        getekendeAtomen.Clear();
        getekendeBindingen.Clear();
        foreach (var a in antwoordAtomen)
        {
            Destroy(a.gameObject);
        }
        foreach (var a in antwoordBindingen)
        {
            Destroy(a.gameObject);
        }
        antwoordAtomen.Clear();
        antwoordBindingen.Clear();
    }

    public static bool goedAntwoordZichtbaar = false;
    [SerializeField] Image geefOpKnop;
    [SerializeField] Sprite zichtbaar;
    [SerializeField] Sprite onzichtbaar;

    public void GeefOp()
    {
        switch (huidigeModus)
        {
            case Naamgeving:
                vraagTekst.text = $"Het goede antwoord was: {naamGever.GenereerNaam()}";
                break;
            case NaamgevingAndersom:
                SwitchVisibility();
                if (goedAntwoordZichtbaar)
                {
                    geefOpKnop.sprite = onzichtbaar;
                    vraagTekst.text = $"Hier zie je een goede structuurformule bij de naam: {naamGevingAntwoord}";
                }
                else
                {
                    geefOpKnop.sprite = zichtbaar;
                    vraagTekst.text = $"Probeer je structuur te verbeteren zodat het overeenkomt met de naam: {naamGevingAntwoord}";
                }
                break;
            case Massaspectrometrie:
                ResetSelectie();
                vraagTekst.text = $"Dit was een goed antwoord:";
                klikAtomen = true;
                foreach (int nr in massaSpecAntwoord)
                {
                    tekenScript.atomen[nr].OnClick();
                }
                break;
            case Molmassa:
                string berekening = "";
                double ans = 0;
                foreach (var (soort, aantal) in MolecuulFormule())
                {
                    ans += aantal * gemiddeldeAtoomMassa[soort];
                    berekening += aantal + "×" + gemiddeldeAtoomMassa[soort] + " + ";
                }
                berekening = berekening[..^2] + "= ";
                vraagTekst.text = $"Het goede antwoord was: {berekening}{ans:F2}";
                break;
            case Waterstofbruggen:
                ResetSelectie();
                vraagTekst.text = $"Dit was het goede antwoord:";
                klikAtomen = true;
                SortedSet<int> hbruggen = antwoordOntvangende ? WaterstofBrugOntvangers() : WaterstofBrugVormers(); klikAtomen = true;
                foreach (int nr in hbruggen)
                {
                    tekenScript.atomen[nr].OnClick();
                }
                break;
            case Modus.PolaireBindingen:
                ResetSelectie();
                SortedSet<(int plus, int min)> goedAntwoord = PolaireBindingen();
                klikBindingen = true;
                vraagTekst.text = $"De polaire bindingen zijn in het rood gemarkeerd. Vergeet de bindingen waterstof niet, die zijn nu niet gemarkeerd.";
                foreach (var (plus, min) in goedAntwoord)
                {
                    tekenScript.atomen[plus].OnClick();
                    tekenScript.atomen[min].OnClick();
                }
                break;
            case Lewisstructuren:
                ResetSelectie();
                SortedDictionary<int, int> elektronenParen = VrijeElektronenParen();
                vraagTekst.text = $"Het aantal elektronenparen is voor elk atoom linksboven weergeven.";
                klikAtomen = true;
                foreach (var (nr, aantal) in elektronenParen)
                {
                    tekenScript.atomen[nr].antwoord.text = aantal.ToString();
                }
                break;
            default:
                break;
        }
    }

    string naamGevingAntwoord;
    public static List<AtoomScript2D> getekendeAtomen = new();
    public static List<AtoomScript2D> antwoordAtomen = new();
    public static List<UILineRendererList> getekendeBindingen = new();
    public static List<UILineRendererList> antwoordBindingen = new();

    public void NieuweVraag()
    {
        goedAntwoordZichtbaar = false;
        geefOpKnop.sprite = zichtbaar;
        SetOefenen(true);
        if (Kans(KansAnorganisch(huidigeModus))) SpawnMolecuulAnorganisch();
        else SpawnMolecuul();
        antwoordKnop.SetActive(true);
        Knop3D.SetActive(huidigeModus != Polariteit && huidigeModus != Lewisstructuren && huidigeModus != Oplosbaarheid && huidigeModus != Modus.NaamgevingAndersom);
        JaNeeDropDown.gameObject.SetActive(huidigeModus == Polariteit || huidigeModus == Oplosbaarheid);
        undoKnop.SetActive(huidigeModus == NaamgevingAndersom);
        antwoordVeld.gameObject.SetActive(huidigeModus == Naamgeving || huidigeModus == Molmassa);
        klikAtomen = false;
        klikBindingen = false;
        switch (huidigeModus)
        {
            case Naamgeving:
                vraagTekst.text = $"Geef de systematische naam van dit molecuul!";
                break;
            case NaamgevingAndersom:
                naamGevingAntwoord = naamGever.GenereerNaam();
                vraagTekst.text = $"Teken de structuurformule van het molecuul met deze naam: {naamGevingAntwoord}";
                getekendeBindingen.Clear();
                getekendeAtomen.Clear();
                tekenScript.acties.Clear();
                break;
            case Massaspectrometrie:
                gevraagdeMassa = RandomSplitsing();
                vraagTekst.text = $"Klik op de atomen die een brokstuk met massa {gevraagdeMassa}u vormen!";
                klikAtomen = true;
                break;
            case Molmassa:
                vraagTekst.text = "Geef de gemiddelde molaire massa van dit molecuul (in g/mol)!";
                break;
            case Waterstofbruggen:
                antwoordOntvangende = true;
                klikAtomen = true;
                vraagTekst.text = "Klik op de waterstofbrug ontvangende atomen! (Mogelijk zijn er geen)";
                break;
            case Modus.PolaireBindingen:
                klikBindingen = true;
                vraagTekst.text = "Klik op de polaire bindingen: klik eerst op het δ+ atoom, en dan op het δ- atoom!\r\n" +
                    "Klik 2x op het atoom voor een polaire binding met waterstof.";
                break;
            case Polariteit:
                tekenScript3D.Render3D(true);
                vraagTekst.text = "Beredeneer aan de hand van de ruimtelijke structuur of dit molecuul polair is.";
                break;
            case Oplosbaarheid:
                tekenScript3D.Render3D(true);
                vraagTekst.text = "Beredeneer of dit molecuul (een beetje) oplosbaar in water is.";
                break;
            case Lewisstructuren:
                klikMeerdereAtomen = true;
                vraagTekst.text = "Klik op alle atomen met een vrij elektronenpaar. Klik 1x voor elk paar dat een atoom heeft";
                break;
        }
        GeefOpKnop.SetActive(!JaNeeDropDown.gameObject.activeSelf);
    }

    // voor waterstofbrug vragen
    bool antwoordOntvangende = true;

    void ResetSelectie()
    {
        foreach (var a in selectie)
        {
            a.image.color = Color.white;
        }
        foreach (var a in lijnen)
        {
            a.ClearPoints();
            Destroy(a);
        }
        lijnen.Clear();
        selectie.Clear();
    }

    bool WaterStofBrugOntvangend(AtoomSoort s)
    {
        return (s == N || s == O || s == F);
    }

    double VerschilEN(int a, int b)
    {
        AtoomSoort soort = a == -1 ? H : atoomsoort[a];
        return elektronegativiteit[soort] - elektronegativiteit[atoomsoort[b]];
    }

    SortedSet<(int plus, int min)> PolaireBindingen()
    {
        SortedSet<(int plus, int min)> lijst = new();
        for (int i = 0; i < n; i++)
        {
            for (int ix = 0; ix < aantalWaterstof[i]; ix++)
            {
                if (System.Math.Abs(VerschilEN(-1, i)) > 0.4001)
                {
                    lijst.Add((i, i));
                }
            }
            foreach (var (buur, _) in NaamGever.bindingen[i])
            {
                if (VerschilEN(buur, i) > 0.4001)
                {
                    lijst.Add((i, buur));
                }
            }
        }
        return lijst;
    }

    public float DipoolMoment()
    {
        Vector3 richting = new();
        foreach (var (plus, min) in PolaireBindingen())
        {
            Vector3 a = tekenScript3D.positie[plus];
            Vector3 b = tekenScript3D.positie[min];
            if (plus == min)
            {
                // waterstof is altijd δ+
                a = tekenScript3D.waterstof[plus].Pop();
            }
            float ΔEN = (float)VerschilEN(plus == min ? -1 : plus, min);
            richting += ΔEN * (a - b);
        }
        Debug.Log("Magnitude: " + richting.magnitude);
        return richting.magnitude;
    }

    public bool Polair()
    {
        return DipoolMoment() > 0.4001;
    }

    SortedSet<int> WaterstofBrugVormers()
    {
        SortedSet<int> antwoord = new();
        for (int i = 0; i < n; i++)
        {
            if (aantalWaterstof[i] > 0 && WaterStofBrugOntvangend(atoomsoort[i]))
            {
                antwoord.Add(i);
            }
        }
        return antwoord;
    }
    SortedSet<int> WaterstofBrugOntvangers()
    {
        SortedSet<int> antwoord = new();
        for (int i = 0; i < n; i++)
        {
            if (WaterStofBrugOntvangend(atoomsoort[i])
                && TekenScript.OctetRegel(i, 0, true).vrijeElektronenParen >= 1)
            {
                antwoord.Add(i);
            }
        }
        return antwoord;
    }

    SortedDictionary<int, int> VrijeElektronenParen()
    {
        SortedDictionary<int, int> goedAntwoord = new();
        for (int i = 0; i < n; i++)
        {
            for (int ix = 0; ix < TekenScript.OctetRegel(i, 0, true).vrijeElektronenParen; ix++)
            {
                if (goedAntwoord.ContainsKey(i))
                    goedAntwoord[i]++;
                else goedAntwoord[i] = 1;
            }
        }
        return goedAntwoord;
    }

    double GetMolMassa()
    {
        double goedAntwoord = 0;
        for (int i = 0; i < n; i++)
        {
            if (atoomsoort[i] == H) continue;
            goedAntwoord += gemiddeldeAtoomMassa[H] * aantalWaterstof[i];
            goedAntwoord += gemiddeldeAtoomMassa[atoomsoort[i]];
        }
        return goedAntwoord;
    }

    Dictionary<AtoomSoort, int> MolecuulFormule()
    {
        Dictionary<AtoomSoort, int> aantal = new();
        for (int i = 0; i < n; i++)
        {
            aantal.TryIncrement(atoomsoort[i]);
            aantal.TryIncrement(H, aantalWaterstof[i]);
        }
        return aantal;
    }

    public void OpenInfoScherm(bool geen3D = false)
    {
        laadInfoScherm = false;
        float dipoolmoment = geen3D ? 0f : DipoolMoment();
        bool polair = dipoolmoment > 0.4001;
        int polaireBindingen = PolaireBindingen().Count;
        int hBrugVormers = WaterstofBrugVormers().Count;
        int hBrugOntvangers = WaterstofBrugOntvangers().Count;
        bool oplosbaar = polair | hBrugVormers > 0 | hBrugOntvangers > 0;
        int vrijeElektronenparen = VrijeElektronenParen().Values.Sum();
        double molmassa = GetMolMassa();
        int aantalKarakteristiekeGroepen = naamGever.aantalGroepen;
        string molecuulformule = "";
        Dictionary<AtoomSoort, int> aantal = MolecuulFormule();
        foreach (var (soort, num) in aantal)
        {
            if (num == 0) continue;
            molecuulformule += soort.ToString();
            if (num > 1) molecuulformule += num.ToString();
        }
        infoText.text = $"Molecuulformule: {molecuulformule}\r\n" +
            $"Molaire massa: {molmassa:F2} g/mol\r\n" +
            $"Aantal karakteristieke groepen: {(naamGever.naamSucces ? aantalKarakteristiekeGroepen : "?")}\r\n" +
            $"Aantal polaire bindingen: {polaireBindingen}\r\n" +
            $"Schatting dipoolmoment: {(geen3D ? "?" : (dipoolmoment.ToString("F3") + " D"))}\r\n" +
            $"Polair: {(geen3D ? "?" : (polair ? "ja" : "nee"))}\r\n" +
            $"H-brug ontvangende atomen: {hBrugOntvangers}\r\n" +
            $"H-brug vormende atomen: {hBrugVormers}\r\n" +
            $"Vrije elektronenparen: {vrijeElektronenparen}\r\n" +
            $"Oplosbaar in water: {(oplosbaar ? "ja/een beetje" : (geen3D ? "?" : "zeer slecht"))}";
    }

    public void Antwoord()
    {
        if (huidigeModus == Molmassa)
        {
            double goedAntwoord = GetMolMassa();
            try
            {
                double antwoord = double.Parse(antwoordVeld.text.Trim());
                if (System.Math.Abs(antwoord - goedAntwoord) < 0.01)
                {
                    vraagTekst.text = $"Goedzo!";
                    antwoordVeld.text = "";
                    antwoordKnop.SetActive(false);
                }
                else
                {
                    double verschilPercentage = 100.0 * System.Math.Abs(1.0 - (antwoord / goedAntwoord));
                    vraagTekst.text = $"Molmassa verschilt {verschilPercentage:G4}%.\r\n" +
                        $"Geef de gemiddelde molaire massa van dit molecuul (in g/mol)!";
                }
            }
            catch
            {
                vraagTekst.text = "Molmassa niet in het goede formaat!";
            }
        }
        else if (huidigeModus == Massaspectrometrie)
        {
            if (selectie.Empty())
                return;
            bool[] inBrokStuk = new bool[n];
            foreach (var i in selectie) inBrokStuk[i.nr] = true;

            int massa(int nr, int vorige)
            {
                int m = aantalWaterstof[nr] + atoomMassa[atoomsoort[nr]];
                foreach (var (buur, _) in bindingen[nr])
                {
                    if (inBrokStuk[buur] && buur != vorige)
                    {
                        m += massa(buur, nr);
                    }
                }
                return m;
            }
            if (massa(selectie[0].nr, -1) == gevraagdeMassa)
            {
                vraagTekst.text = $"Goedzo!";
                klikAtomen = false;
                antwoordKnop.SetActive(false);
            }
            else
            {
                vraagTekst.text = $"Massa komt niet overeen (of de atomen zijn niet onderling verbonden).\r\n" +
                    $"Probeer het opnieuw: klik op de atomen die een brokstuk met massa {gevraagdeMassa}u vormen!";
            }
            ResetSelectie();
        }
        else if (huidigeModus == Naamgeving)
        {
            string antwoord = naamGever.GenereerNaam();
            if (antwoordVeld.text.Trim() == antwoord)
            {
                vraagTekst.text = $"Goedzo!";
                antwoordVeld.text = "";
                klikAtomen = false;
                antwoordKnop.SetActive(false);
                huidigeModus = Naamgeving;
            }
            else
            {
                vraagTekst.text = $"Naam komt niet overeen.\r\n" +
                    $"Probeer het opnieuw: Geef de systematische naam van dit molecuul!";
            }
        }
        else if (huidigeModus == NaamgevingAndersom)
        {
            string antwoord = naamGever.GenereerNaam(antwoordAtomen.Count);
            if (naamGevingAntwoord == antwoord)
            {
                vraagTekst.text = $"Goedzo!";
                antwoordKnop.SetActive(false);
            }
            else
            {
                vraagTekst.text = $"Naam komt niet overeen.\r\n" +
                    $"Probeer het opnieuw:  Teken de structuurformule van het molecuul met deze naam: {naamGevingAntwoord}";
            }
        }
        else if (huidigeModus == Waterstofbruggen)
        {
            SortedSet<int> goedAntwoord = antwoordOntvangende ? WaterstofBrugOntvangers() : WaterstofBrugVormers();
            SortedSet<int> antwoord = new();
            foreach (var a in selectie) antwoord.Add(a.nr);
            if (antwoord.SetEquals(goedAntwoord))
            {
                vraagTekst.text = $"Goedzo!\r\n";
                antwoordVeld.text = "";
                ResetSelectie();
                if (!antwoordOntvangende)
                {
                    antwoordKnop.SetActive(false);
                    klikAtomen = false;
                    return;
                }
                antwoordOntvangende = !antwoordOntvangende;
            }
            else
            {
                vraagTekst.text = "Helaas, niet het goede antwoord!\r\n";
            }
            klikAtomen = true;
            if (antwoordOntvangende) vraagTekst.text += "Klik op de waterstofbrug ontvangende atomen! (Mogelijk zijn er geen)";
            else vraagTekst.text += "Klik op de waterstofbrug vormende atomen! (Mogelijk zijn er geen)";
        }
        else if (huidigeModus == Modus.PolaireBindingen)
        {
            if (selectie.Count % 2 == 1)
            {
                vraagTekst.text = "Klik op een even aantal atomen!";
                return;
            }
            SortedSet<(int plus, int min)> goedAntwoord = PolaireBindingen();
            SortedSet<(int plus, int min)> antwoord = new();

            for (int i = 0; i < selectie.Count; i += 2)
            {
                antwoord.Add((selectie[i].nr, selectie[i + 1].nr));
            }
            if (antwoord.SetEquals(goedAntwoord))
            {
                vraagTekst.text = $"Goedzo!\r\n";
                klikBindingen = false;
            }
            else
            {
                vraagTekst.text = "Helaas: probeer het opnieuw.\r\n" +
                    "Klik op de polaire bindingen: klik eerst op het δ+ atoom, en dan op het δ- atoom!\r\n" +
                    "Klik 2x op het atoom voor een polaire binding met waterstof.";
            }
            ResetSelectie();
        }
        else if (huidigeModus == Polariteit)
        {
            // Eerste optie (optie 0) is Ja
            Knop3D.SetActive(true);
            if ((JaNeeDropDown.value == 0) == Polair())
            {
                vraagTekst.text = $"Goedzo!";
                antwoordKnop.SetActive(false);
                JaNeeDropDown.gameObject.SetActive(false);
            }
            else
            {
                vraagTekst.text = $"Helaas niet het goede antwoord. Klik rechts op 3D om de ruimtelijke structuur te zien.";
            }
        }
        else if (huidigeModus == Oplosbaarheid)
        {
            SortedSet<int> waterstofbruggen = WaterstofBrugOntvangers();
            waterstofbruggen.UnionWith(WaterstofBrugVormers());
            var polaireBindingen = PolaireBindingen();
            bool polair = Polair();
            bool oplosbaar = polair | waterstofbruggen.Count > 0;
            // Eerste optie (optie 0) is Ja
            Knop3D.SetActive(true);
            if ((JaNeeDropDown.value == 0) == oplosbaar)
            {
                vraagTekst.text = $"Goedzo!";
                antwoordKnop.SetActive(false);
                JaNeeDropDown.gameObject.SetActive(false);
            }
            else
            {
                vraagTekst.text = $"Helaas niet het goede antwoord. ";
                if (waterstofbruggen.Count > 0)
                {
                    vraagTekst.text += "Het molecuul is oplosbaar in water omdat het waterstofbruggen kan vormen, je kan daar apart mee oefenen.";
                }
                else if (oplosbaar)
                {
                    vraagTekst.text += "Het molecuul is oplosbaar in water omdat het polair is, je kan daar apart mee oefenen.";
                }
                else if (polaireBindingen.Count > 0)
                {
                    vraagTekst.text += "Hoewel het molecuul polaire bindingen heeft, heffen die elkaar op waardoor het molecuul niet polair is. Zie de 3D structuur.";
                }
            }
        }
        else if (huidigeModus == Lewisstructuren)
        {
            SortedDictionary<int, int> goedAntwoord = VrijeElektronenParen();
            SortedDictionary<int, int> antwoord = new();
            foreach (var a in selectie)
            {
                if (antwoord.ContainsKey(a.nr))
                    antwoord[a.nr]++;
                else antwoord[a.nr] = 1;
            }
            ResetSelectie();
            bool dictionariesEqual = antwoord.Keys.Count == goedAntwoord.Keys.Count && antwoord.Keys.All(k => goedAntwoord.ContainsKey(k) && object.Equals(goedAntwoord[k], antwoord[k]));
            if (dictionariesEqual)
            {
                vraagTekst.text = $"Goedzo!";
                antwoordKnop.SetActive(false);
            }
            else
            {
                vraagTekst.text = "Helaas, probeer het opnieuw. " + vraagTekst.text;
            }
        }
    }

    int RandomSplitsing()
    {
        int molecuulMassa = 0;
        for (int poging = 0; poging < 100 && molecuulMassa < 24; poging++)
        {
            List<float> weights = new();
            List<int> antwoord = new();
            for (int i = 0; i < n; i++)
            {
                weights.Add(bindingen[i].Count);
            }
            int nr = GetRandomWeightedIndex(weights);
            int nr2 = bindingen[nr][Rng(0, bindingen[nr].Count - 1)].Item1;

            int massa(int nr, int vorige)
            {
                antwoord.Add(nr);
                int m = aantalWaterstof[nr] + atoomMassa[atoomsoort[nr]];
                foreach (var (buur, _) in bindingen[nr])
                {
                    if (buur != vorige)
                    {
                        m += massa(buur, nr);
                    }
                }
                return m;
            }
            massaSpecAntwoord = antwoord;
            molecuulMassa = massa(nr, nr2);
        }
        return molecuulMassa;
    }
}
