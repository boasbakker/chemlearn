using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using static Definities.AtoomSoort;
using static Definities.KarakteristiekeGroep;
using static Definities;
using static Extensions;
using static TekenScript;
using TMPro;

public class NaamGever : MonoBehaviour
{
    // aantal niet-H atomen
    public static int n;
    public static List<AtoomSoort> atoomsoort = new();
    public static List<List<(int, int)>> bindingen = new();
    public static List<int> aantalWaterstof = new();
    public static List<int> lading = new();
    public static List<int> fixeerdeOctetten = new();
    public static List<int> fixeerdeWaterstof = new();

    public static List<int> aantalBindingen = new();

    KarakteristiekeGroep hoofdGroep = Fout;

    [SerializeField] TekenScript tekenScript;

    public int aantalGroepen = 0;
    public bool naamSucces = true;

    void KarakteristiekeGroepFout()
    {
        throw new Exception("Karakteristieke groep niet ondersteund.");
    }

    void LadingFout()
    {
        throw new Exception("Lading wordt alleen ondersteund in nitrogroepen.");
    }

    KarakteristiekeGroep VindTypeZwavel(int nr)
    {
        if (lading[nr] != 0) LadingFout();
        int enkeleO = 0, dubbeleO = 0, koolstof = 0;
        foreach (var (buur, type) in bindingen[nr])
        {
            switch (atoomsoort[buur])
            {
                case H:
                    break;
                case C:
                    koolstof++;
                    break;
                case O:
                    if (type == 1) enkeleO++;
                    else if (type == 2) dubbeleO++;
                    else KarakteristiekeGroepFout();
                    break;
                default:
                    KarakteristiekeGroepFout();
                    break;
            }
        }
        if (koolstof != 1) KarakteristiekeGroepFout();
        if (dubbeleO == 2 && enkeleO == 1) return Sulfonzuur;
        if (dubbeleO == 0 && enkeleO == 0) return Thiol;
        KarakteristiekeGroepFout();
        return Fout;
    }

    void CheckGroepPuur(int nr)
    {
        if (lading[nr] != 0) LadingFout();
        int koolstof = 0;
        foreach (var (buur, type) in bindingen[nr])
        {
            switch (atoomsoort[buur])
            {
                case H:
                    break;
                case C:
                    koolstof++;
                    break;
                default:
                    KarakteristiekeGroepFout();
                    break;
            }
        }
        if (koolstof > 1) KarakteristiekeGroepFout();
    }

    KarakteristiekeGroep VindTypeZuurstof(int nr)
    {
        if (lading[nr] != 0) LadingFout();
        int koolstof = 0;
        foreach (var (buur, type) in bindingen[nr])
        {
            switch (atoomsoort[buur])
            {
                case H:
                    break;
                case C:
                    koolstof++;
                    break;
                default:
                    KarakteristiekeGroepFout();
                    break;
            }
        }
        if (koolstof == 2) return Ether;
        if (koolstof == 1) return Hydroxylgroep;
        KarakteristiekeGroepFout();
        return Fout;
    }

    int LadingZuurstof(int nr)
    {
        int stikstof = 0;
        foreach (var (buur, type) in bindingen[nr])
        {
            switch (atoomsoort[buur])
            {
                case N:
                    if (type == 1) stikstof++;
                    break;
                case H:
                    break;
                default:
                    KarakteristiekeGroepFout();
                    break;
            }
        }
        if (stikstof != 1) KarakteristiekeGroepFout();
        if (lading[nr] == -1) return -1;
        if (lading[nr] == 0) return 0;
        KarakteristiekeGroepFout();
        return 0;
    }

    KarakteristiekeGroep VindTypeStikstof(int nr)
    {
        int koolstof = 0, geladenO = 0, dubbelO = 0, waterstof = aantalWaterstof[nr];
        foreach (var (buur, type) in bindingen[nr])
        {
            switch (atoomsoort[buur])
            {
                case H:
                    break;
                case C:
                    koolstof++;
                    break;
                case O:
                    if (type == 1)
                    {
                        if (LadingZuurstof(buur) == -1) geladenO++;
                        else KarakteristiekeGroepFout();
                    }
                    else if (type == 2) dubbelO++;
                    else KarakteristiekeGroepFout();
                    break;
                default:
                    KarakteristiekeGroepFout();
                    break;
            }
        }
        if (koolstof != 1) KarakteristiekeGroepFout();
        if (geladenO == 1 && dubbelO == 1 && waterstof == 0) return Nitrogroep;
        if (geladenO == 0 && dubbelO == 0 && waterstof == 2) return Amine;
        KarakteristiekeGroepFout();
        return Fout;
    }

    List<(KarakteristiekeGroep, int)> GroepenOpPlek(int nr)
    {
        if (lading[nr] != 0) LadingFout();
        List<int> enkeleO = new(), ethers = new(), dubbeleO = new();
        int waterstof = aantalWaterstof[nr];
        List<(KarakteristiekeGroep, int)> groepen = new();
        foreach (var (buur, type) in bindingen[nr])
        {
            switch (atoomsoort[buur])
            {
                case H:
                    break;
                case C:
                    break;
                case N:
                    if (type == 1) groepen.Add((VindTypeStikstof(buur), buur));
                    else if (type == 2) groepen.Add((Imine, buur));
                    else if (type == 3) groepen.Add((Nitril, buur));
                    break;
                case O:
                    if (type == 1)
                    {
                        if (VindTypeZuurstof(buur) == Ether) ethers.Add(buur);
                        else enkeleO.Add(buur);
                    }
                    else if (type == 2) dubbeleO.Add(buur);
                    else KarakteristiekeGroepFout();
                    break;
                case F:
                    groepen.Add((halogeenGroepen[atoomsoort[buur]], buur));
                    break;
                case S:
                    if (type == 1) groepen.Add((VindTypeZwavel(buur), buur));
                    else if (type == 2)
                    {
                        CheckGroepPuur(buur);
                        if (aantalWaterstof[nr] == 1)
                        {
                            groepen.Add((Thialdehyde, buur));
                        }
                        else groepen.Add((Thion, buur));
                    }
                    else KarakteristiekeGroepFout();
                    break;
                case Cl:
                    groepen.Add((halogeenGroepen[atoomsoort[buur]], buur));
                    break;
                case Br:
                    groepen.Add((halogeenGroepen[atoomsoort[buur]], buur));
                    break;
                case I:
                    groepen.Add((halogeenGroepen[atoomsoort[buur]], buur));
                    break;
                default:
                    KarakteristiekeGroepFout();
                    break;
            }
        }
        if (enkeleO.Count >= 1 && dubbeleO.Count >= 1)
        {
            int buur = dubbeleO.Back();
            groepen.Add((Carbonzuur, buur));
            enkeleO.PopBack();
            dubbeleO.PopBack();
        }
        if (ethers.Count >= 1 && dubbeleO.Count >= 1)
        {
            int buur = ethers.Back();
            groepen.Add((Ester, buur));
            dubbeleO.PopBack();
            ethers.PopBack();
        }
        while (enkeleO.Count > 0)
        {
            int buur = enkeleO.Back();
            groepen.Add((Hydroxylgroep, buur));
            enkeleO.PopBack();
        }
        while (ethers.Count > 0)
        {
            int buur = ethers.Back();
            groepen.Add((Ether, buur));
            ethers.PopBack();
        }
        while (dubbeleO.Count > 0 && waterstof > 0)
        {
            int buur = dubbeleO.Back();
            groepen.Add((Aldehyde, buur));
            dubbeleO.PopBack();
            waterstof--;
        }
        while (dubbeleO.Count > 0)
        {
            int buur = dubbeleO.Back();
            groepen.Add((Keton, buur));
            dubbeleO.PopBack();
        }
        return groepen;
    }

    // stopNr == aanhechtingsPunt
    void VindRingen(int nr, List<int> pad, bool[] bezocht, bool[] alInRing, List<List<int>> ringen, int stopNr)
    {
        pad.Add(nr);
        bezocht[nr] = true;
        foreach (var (buur, _) in bindingen[nr])
        {
            if (atoomsoort[buur] != C) continue;
            if (buur == stopNr) continue;
            if (pad.Count > 1 && buur == pad[^2]) continue;
            if (bezocht[buur])
            {
                bool wasAlInRing = alInRing[buur];
                List<int> ring = new();
                bool inRing = false;
                foreach (int atoom in pad)
                {
                    if (atoom == buur)
                    {
                        inRing = true;
                    }
                    if (inRing)
                    {
                        alInRing[atoom] = true;
                        ring.Add(atoom);
                    }
                    if (atoom == nr)
                    {
                        inRing = false;
                        break;
                    }
                }
                if (!ring.Empty())
                {
                    if (wasAlInRing)
                    {
                        throw new Exception("Ringsystemen worden niet ondersteund.");
                    }
                    ringen.Add(ring); // als ring al gevonden en terug bij begin in DFS wordt het opnieuw toegevoegd
                }
                continue;
            }
            VindRingen(buur, pad, bezocht, alInRing, ringen, stopNr);
        }
        pad.Remove(nr);
    }

    [Serializable]
    class StamVerbinding
    {
        public bool hoofdGroepCarbo = false; // als hoofdgroep carbaldehyde, carbonzuur of carbonitril is
        public List<int> atomen = new();
        public List<int> enen = new(), ynen = new();

        public bool cyclisch = false;
        // plaatsaanduidingen
        public List<int> hoofdGroepen = new();
        public List<int> aanhechtingsPunten = new();
        public List<int> eenYnAchtervoegsels = new();
        // vanaf hier gaat het om PIN regels
        public List<int> voorvoegsels = new();
        // 3-broom-6-fluor < 6-broom-3-fluor
        // public List<Tuple<string, int>> voorvoegselsAlfabetisch;
        // public string name;
        // een aantal PIN regels zijn weggelaten
    }

    void VerwerkAtoom(ref StamVerbinding verbinding, int nr, int plaatsAanduiding, int aanhechtingsPunt, int type, bool toevoegen)
    {
        // Gebruik de juiste bewerking (Add of Remove)
        Action<List<int>, int> bewerking = toevoegen ? (list, item) => list.Add(item) : (list, item) => list.Remove(item);

        bewerking(verbinding.atomen, nr);

        if (type > 1)
        {
            bewerking(verbinding.eenYnAchtervoegsels, plaatsAanduiding);
            if (type == 2)
            {
                bewerking(verbinding.enen, plaatsAanduiding);
            }
            else
            {
                bewerking(verbinding.ynen, plaatsAanduiding);
            }
        }

        foreach (var (buur, _) in bindingen[nr])
            if (buur == aanhechtingsPunt) bewerking(verbinding.aanhechtingsPunten, plaatsAanduiding + 1);

        foreach (var (groep, buur) in GroepenOpPlek(nr))
        {
            if (buur == aanhechtingsPunt) continue;
            if (groep == hoofdGroep && aanhechtingsPunt == -1)
            {
                bewerking(verbinding.hoofdGroepen, plaatsAanduiding + 1);
            }
            else
            {
                bewerking(verbinding.voorvoegsels, plaatsAanduiding + 1);
            }
        }
    }

    // kandidaten voor stamverbinding
    void GenereerKandidaten(int nr, int plaatsAanduiding, int aanhechtingsPunt, bool[] bezocht, bool[] inRing, ref StamVerbinding verbinding, List<StamVerbinding> kandidaten)
    {
        bezocht[nr] = true;
        foreach (var (buur, type) in bindingen[nr])
        {
            if (atoomsoort[buur] != C) continue;
            if (inRing[buur]) continue;
            if (bezocht[buur]) continue;
            VerwerkAtoom(ref verbinding, buur, plaatsAanduiding, aanhechtingsPunt, type, true);
            GenereerKandidaten(buur, plaatsAanduiding + 1, aanhechtingsPunt, bezocht, inRing, ref verbinding, kandidaten);
            VerwerkAtoom(ref verbinding, buur, plaatsAanduiding, aanhechtingsPunt, type, false);
        }
        kandidaten.Add(verbinding.DeepCopy());
    }

    void GenereerRingKandidaten(int nr, int plaatsAanduiding, int aanhechtingsPunt, bool[] inRing, int vorige, int start, ref StamVerbinding verbinding, List<StamVerbinding> kandidaten)
    {
        int sluitendBindingsType = 1, buren = 0;
        foreach (var (buur, type) in bindingen[nr])
        {
            if (atoomsoort[buur] != C) continue;
            if (!inRing[buur]) continue;
            if (buur == vorige) continue;
            if (buur == start)
            {
                sluitendBindingsType = type;
                continue;
            }
            buren++;
            VerwerkAtoom(ref verbinding, buur, plaatsAanduiding, aanhechtingsPunt, type, true);
            GenereerRingKandidaten(buur, plaatsAanduiding + 1, aanhechtingsPunt, inRing, nr, start, ref verbinding, kandidaten);
            VerwerkAtoom(ref verbinding, buur, plaatsAanduiding, aanhechtingsPunt, type, false);
        }
        if (buren == 0)
        {
            StamVerbinding toevoeging = verbinding.DeepCopy();
            if (sluitendBindingsType >= 2) toevoeging.eenYnAchtervoegsels.Add(plaatsAanduiding);
            if (sluitendBindingsType == 2) toevoeging.enen.Add(plaatsAanduiding);
            else if (sluitendBindingsType == 3) toevoeging.ynen.Add(plaatsAanduiding);
            kandidaten.Add(toevoeging);
        }
    }

    class StructuurNaam : IComparable<StructuurNaam>
    {
        public string naamAlfabetisch = "";
        public string naam = "";
        public bool moetHaakes = false;
        public SortedSet<char> haakjes = new();

        // constructor
        public StructuurNaam(string x, string y, bool z, SortedSet<char> w)
        {
            naamAlfabetisch = x;
            naam = y;
            moetHaakes = z;
            haakjes = w;
        }
        public StructuurNaam() { }

        public int CompareTo(StructuurNaam andere)
        {
            return naamAlfabetisch.CompareTo(andere.naamAlfabetisch);
        }
        public static StructuurNaam operator +(StructuurNaam x, StructuurNaam y)
        {
            SortedSet<char> vereniging = new();
            vereniging.UnionWith(x.haakjes);
            vereniging.UnionWith(y.haakjes);
            return new StructuurNaam(new string((x.naam + y.naam).Where(char.IsLetter).ToArray()), x.naam + "-" + y.naam, x.moetHaakes | y.moetHaakes, vereniging);
        }
        public static StructuurNaam operator +(StructuurNaam x, string y)
        {
            return new StructuurNaam(x.naamAlfabetisch + new string(y.Where(char.IsLetter).ToArray()), x.naam + y, x.moetHaakes, x.haakjes);
        }
    }

    bool IsCarbaldehyde(int nr, int aanhechtingspunt)
    {
        int dubbelO = 0, enkelO = 0, waterstof = 0, anders = 0;
        foreach (var (buur, type) in bindingen[nr])
        {
            if (buur == aanhechtingspunt) continue;
            if (atoomsoort[buur] == O)
            {
                if (type == 1) enkelO++;
                else if (type == 2) dubbelO++;
                else anders++;
            }
            else if (atoomsoort[buur] == H) waterstof++;
            else anders++;
        }
        if (anders == 0 && dubbelO == 1 && enkelO == 0) return true;
        return false;
    }
    bool IsCarbonzuur(int nr, int aanhechtingspunt)
    {
        int dubbelO = 0, enkelO = 0, waterstof = 0, anders = 0;
        foreach (var (buur, type) in bindingen[nr])
        {
            if (buur == aanhechtingspunt) continue;
            if (atoomsoort[buur] == O)
            {
                if (type == 1) enkelO++;
                else if (type == 2) dubbelO++;
                else anders++;
            }
            else if (atoomsoort[buur] == H) waterstof++;
            else anders++;
        }
        if (anders == 0 && dubbelO == 1 && enkelO == 1) return true;
        return false;
    }
    bool IsCarbonitril(int nr, int aanhechtingspunt)
    {
        int nitril = 0, waterstof = 0, anders = 0;
        foreach (var (buur, type) in bindingen[nr])
        {
            if (buur == aanhechtingspunt) continue;
            if (atoomsoort[buur] == N && type == 3) nitril++;
            else if (atoomsoort[buur] == H) waterstof++;
            else anders++;
        }
        if (anders == 0 && nitril == 1) return true;
        return false;
    }

    // aanhechtingsType is -1 voor ether
    StructuurNaam GenereerNaamSubBoom(int aanhechtingsPunt, KarakteristiekeGroep hoofdGroep, int startPlek, int aanhechtingsType)
    {
        Stack<int> s = new();
        s.Push(startPlek);
        bool[] inZijtak = new bool[n];
        int aantalInZijtak = 0;
        // kleine DFS om atomen die deel maken van zijtak te identificeren
        while (s.Count > 0)
        {
            int nr = s.Peek(); s.Pop();
            if (inZijtak[nr]) continue;
            inZijtak[nr] = true;
            aantalInZijtak++;
            foreach (var (buur, _) in bindingen[nr])
            {
                if (buur == aanhechtingsPunt) continue;
                if (inZijtak[buur]) continue;
                s.Push(buur);
            }
        }
        if (aanhechtingsPunt == -1 && aantalInZijtak != n && OefenModusScript.huidigeModus != Modus.NaamgevingAndersom)
        {
            throw new Exception("Niet alle atomen zijn aan elkaar verbonden!");
        }
        List<StamVerbinding> kandidaten = new();
        List<List<int>> ringen = new();
        bool[] bezocht = new bool[n];
        for (int i = 0; i < n; i++)
        {
            if (atoomsoort[i] == C && !bezocht[i] && inZijtak[i])
                VindRingen(startPlek, new(), bezocht, new bool[n], ringen, aanhechtingsPunt);
        }
        bool[] inEenRing = new bool[n];
        foreach (var ring in ringen)
        {
            bool[] inRing = new bool[n];
            foreach (int nr in ring) inRing[nr] = true;
            foreach (int nr in ring) inEenRing[nr] = true;
            for (int i = 0; i < ring.Count; i++)
            {
                int vorigeIndex = (i + ring.Count - 1) % ring.Count;
                int volgendeIndex = (i + 1) % ring.Count;
                StamVerbinding verbinding = new()
                {
                    cyclisch = true
                };
                VerwerkAtoom(ref verbinding, ring[i], 0, aanhechtingsPunt, 1, true);
                GenereerRingKandidaten(ring[i], 1, aanhechtingsPunt, inRing, ring[vorigeIndex], ring[i], ref verbinding, kandidaten);
                VerwerkAtoom(ref verbinding, ring[i], 0, aanhechtingsPunt, 1, false);
                Debug.Assert(verbinding.atomen.Empty());
                VerwerkAtoom(ref verbinding, ring[i], 0, aanhechtingsPunt, 1, true);
                GenereerRingKandidaten(ring[i], 1, aanhechtingsPunt, inRing, ring[volgendeIndex], ring[i], ref verbinding, kandidaten);
            }
        }
        for (int start = 0; start < n; start++)
        {
            if (atoomsoort[start] != C) continue;
            if (!inZijtak[start]) continue;
            bezocht = new bool[n];
            for (int i = 0; i < n; i++) bezocht[i] = !inZijtak[i];
            StamVerbinding verbinding = new();
            VerwerkAtoom(ref verbinding, start, 0, aanhechtingsPunt, 1, true);
            GenereerKandidaten(start, 1, aanhechtingsPunt, bezocht, inEenRing, ref verbinding, kandidaten);
        }
        for (int i = 0; i < kandidaten.Count; i++)
        {
            if (kandidaten[i].cyclisch) continue;
            foreach (int nr in kandidaten[i].atomen)
            {
                if (inEenRing[nr])
                {
                    kandidaten.RemoveAt(i);
                    i--;
                    break;
                }
            }
        }
        if (InstellingenScript.oudeNaamGeving && aanhechtingsPunt != -1)
        {
            for (int i = 0; i < kandidaten.Count; i++)
            {
                if (kandidaten[i].atomen[0] != startPlek)
                {
                    kandidaten.RemoveAt(i);
                    i--;
                }
            }
        }
        for (int i = 0; i < kandidaten.Count; i++)
        {
            bool verwijder = false;
            foreach (int nr in kandidaten[i].atomen)
            {
                if (verwijder)
                {
                    i--;
                    break;
                }
                foreach (var (groep, _) in GroepenOpPlek(nr))
                {
                    if ((groep == Nitril || groep == Carbonzuur) &&
                        (hoofdGroep != groep || aanhechtingsPunt != -1)) // als het als voorvoegsel komt
                    {
                        kandidaten.RemoveAt(i);
                        verwijder = true;
                        break;
                    }
                }
            }
        }
        // voeg voorvoegsels voor zijtakken toe aan kandidaten voor het vergelijken
        int plaatsAanduiding = 1;
        for (int i = 0; i < kandidaten.Count; i++)
        {
            bool[] inStamVerbindingKandidaat = new bool[n];
            foreach (int nr in kandidaten[i].atomen) inStamVerbindingKandidaat[nr] = true;
            plaatsAanduiding = 1;
            foreach (int nr in kandidaten[i].atomen)
            {
                foreach (var (buur, _) in bindingen[nr])
                {
                    if (!inStamVerbindingKandidaat[buur] && atoomsoort[buur] == C)
                    {
                        if (buur != aanhechtingsPunt)
                            kandidaten[i].voorvoegsels.Add(plaatsAanduiding);
                    }
                }
                plaatsAanduiding++;
            }
        }
        plaatsAanduiding = 1;
        if (aanhechtingsPunt == -1)
        {
            // check voor carbonitrillen, carbonzuren, carbaldehydes
            for (int i = 0; i < kandidaten.Count; i++)
            {
                StamVerbinding verbinding = kandidaten[i];
                bool[] inVerbinding = new bool[n];
                foreach (int nr in verbinding.atomen) inVerbinding[nr] = true;
                List<int> carboHoofdgroepen = new();
                for (plaatsAanduiding = 1; plaatsAanduiding <= verbinding.atomen.Count; plaatsAanduiding++)
                {
                    int nr = verbinding.atomen[plaatsAanduiding - 1];
                    foreach (var (buur, type) in bindingen[nr])
                    {
                        if (atoomsoort[buur] != C) continue;
                        if (!inVerbinding[buur] && inZijtak[buur])
                        {
                            bool carbaldeHyde = IsCarbaldehyde(buur, nr) && hoofdGroep == Aldehyde;
                            bool carbonZuur = IsCarbonzuur(buur, nr) && hoofdGroep == Carbonzuur;
                            bool carboNitril = IsCarbonitril(buur, nr) && hoofdGroep == Nitril;
                            if (carbaldeHyde || carbonZuur || carboNitril)
                            {
                                if (!verbinding.hoofdGroepCarbo && verbinding.hoofdGroepen.Count > 0)
                                {
                                    if (!verbinding.cyclisch)
                                    {
                                        bool aanHetBegin = verbinding.hoofdGroepen[0] == 1;
                                        if (aanHetBegin)
                                        {
                                            verbinding.hoofdGroepen.DecrementValues();
                                            verbinding.enen.DecrementValues();
                                            verbinding.ynen.DecrementValues();
                                            verbinding.eenYnAchtervoegsels.DecrementValues();
                                            verbinding.voorvoegsels.DecrementValues();
                                            verbinding.aanhechtingsPunten.DecrementValues();
                                            verbinding.hoofdGroepen[0]++;
                                            verbinding.atomen.RemoveAt(0);
                                            plaatsAanduiding--;
                                        }
                                        else
                                        {
                                            if (verbinding.hoofdGroepen.Count == 2) throw new Exception("Onbekende fout! (2)");
                                            verbinding.atomen.PopBack();
                                        }
                                        if (verbinding.hoofdGroepen.Count == 2)
                                        {
                                            verbinding.atomen.PopBack();
                                            verbinding.hoofdGroepen[^1]--; // .back()
                                        }
                                    }
                                }
                                verbinding.hoofdGroepCarbo = true;
                                verbinding.hoofdGroepen.Add(plaatsAanduiding);
                            }
                        }
                    }
                }
            }
        }
        kandidaten.RemoveAll(k => k.hoofdGroepCarbo &&
            !k.cyclisch &&  // P-65.1.2.2.2, P-66.6.1.1.3, P-66.5.1.1.3
            k.hoofdGroepen.Count <= 2); // P-65.1.2.2.1, P-66.6.1.1.2, P-66.5.1.1.2
        plaatsAanduiding = 1;
        if (InstellingenScript.oudeNaamGeving)
        {
            kandidaten = kandidaten
                .OrderByDescending(v => v.hoofdGroepen.Count)
                .ThenByDescending(v => v.aanhechtingsPunten.Count)
                .ThenByDescending(v => v.cyclisch)
                .ThenByDescending(v => v.eenYnAchtervoegsels.Count)
                .ThenByDescending(v => v.enen.Count)
                .ThenByDescending(v => v.atomen.Count)
                .ThenBy(v => v.hoofdGroepen, new ListComparer())
                .ThenBy(v => v.aanhechtingsPunten, new ListComparer())
                .ThenBy(v => v.eenYnAchtervoegsels, new ListComparer())
                .ThenByDescending(v => v.voorvoegsels.Count)
                .ThenBy(v => v.voorvoegsels, new ListComparer()).ToList();
        }
        else kandidaten = kandidaten
            .OrderByDescending(v => v.hoofdGroepen.Count) // P-44.1.1
            .ThenByDescending(v => v.aanhechtingsPunten.Count) // hier ga ik van uit, staat niet heel duidelijk beschreven
            .ThenByDescending(v => v.cyclisch) // P-44.1.2.2
            .ThenByDescending(v => v.atomen.Count) // P-44.3.2
            .ThenByDescending(v => v.eenYnAchtervoegsels.Count) // P-44.4.1.1
            .ThenByDescending(v => v.enen.Count) // P-44.4.1.2
            .ThenBy(v => v.hoofdGroepen, new ListComparer()) // P-44.4.1.8
            .ThenBy(v => v.aanhechtingsPunten, new ListComparer()) // P-44.4.1.9
            .ThenBy(v => v.eenYnAchtervoegsels, new ListComparer()) // P-44.4.1.10
            .ThenByDescending(v => v.voorvoegsels.Count) // P-45.2.1
            .ThenBy(v => v.voorvoegsels, new ListComparer()) // P-45.2.2
            .ToList();
        var stamVerbinding = kandidaten[0];
        List<int> benzeen1 = new() { 1, 3, 5 };
        List<int> benzeen2 = new() { 2, 4, 6 };
        bool isBenzeen = stamVerbinding.cyclisch && stamVerbinding.atomen.Count == 6 && stamVerbinding.enen.Count == 3 &&
            stamVerbinding.ynen.Count == 0 &&
            (stamVerbinding.enen.SequenceEqual(benzeen1) || stamVerbinding.enen.SequenceEqual(benzeen2));
        if (isBenzeen)
        {
            stamVerbinding.eenYnAchtervoegsels.Clear();
            stamVerbinding.enen.Clear();
        }
        bool[] inStamVerbinding = new bool[n];
        foreach (int nr2 in stamVerbinding.atomen)
        {
            inStamVerbinding[nr2] = true;
        }
        SortedDictionary<StructuurNaam, List<int>> zijgroepen = new();
        StructuurNaam naam = new();
        foreach (int nr in stamVerbinding.atomen)
        {
            foreach (var (groep, buur) in GroepenOpPlek(nr))
            {
                if (buur == aanhechtingsPunt) continue;
                if (groep == Fout)
                {
                    throw new Exception("Onbekende fout! (1)");
                }
                if (groep == Ester && hoofdGroep == Ester)
                {
                    int nieuwNr = bindingen[buur].Where(b => b.Item1 != nr).ToList()[0].Item1;
                    naam += GenereerNaamSubBoom(buur, hoofdGroep, nieuwNr, 1); // esters moeten altijd vooraan
                    continue;
                }
                if (groep == hoofdGroep && aanhechtingsPunt == -1) continue;
                StructuurNaam voorvoegsel = new();
                if (groep == Ether)
                {
                    int nieuwNr = bindingen[buur].Where(b => b.Item1 != nr).ToList()[0].Item1;
                    voorvoegsel = GenereerNaamSubBoom(buur, hoofdGroep, nieuwNr, -1);
                }
                else if (groep == Ester) // Ester is geen hoofdgroep -> opgesplitst in keton + ether
                {
                    StructuurNaam oxo = new();
                    oxo += "oxo";
                    if (!zijgroepen.ContainsKey(oxo))
                    {
                        zijgroepen[oxo] = new();
                    }
                    zijgroepen[oxo].Add(plaatsAanduiding);

                    int nieuwNr = bindingen[buur].Where(b => b.Item1 != nr).ToList()[0].Item1;
                    voorvoegsel = GenereerNaamSubBoom(buur, hoofdGroep, nieuwNr, -1);
                }
                else if (groep == Aldehyde)
                {
                    voorvoegsel += voorvoegsels[Keton];
                }
                else
                {
                    voorvoegsel += voorvoegsels[groep];
                }
                if (!zijgroepen.ContainsKey(voorvoegsel))
                {
                    zijgroepen[voorvoegsel] = new();
                }
                zijgroepen[voorvoegsel].Add(plaatsAanduiding);
            }
            foreach (var (buur, type) in bindingen[nr])
            {
                if (atoomsoort[buur] != C) continue;
                if (!inStamVerbinding[buur] && inZijtak[buur])
                {
                    StructuurNaam voorvoegsel = null;
                    if (stamVerbinding.hoofdGroepCarbo)
                    {
                        if (hoofdGroep == Aldehyde && IsCarbaldehyde(buur, nr))
                        {
                            continue;
                        }
                        if (hoofdGroep == Carbonzuur && IsCarbonzuur(buur, nr))
                        {
                            continue;
                        }
                        if (hoofdGroep == Nitril && IsCarbonitril(buur, nr))
                        {
                            continue;
                        }
                    }
                    else
                    {
                        if (IsCarbaldehyde(buur, nr))
                        {
                            voorvoegsel = new();
                            voorvoegsel += voorvoegsels[Aldehyde];
                        }
                        if (IsCarbonzuur(buur, nr))
                        {
                            voorvoegsel = new();
                            voorvoegsel += voorvoegsels[Carbonzuur];
                        }
                        if (IsCarbonitril(buur, nr))
                        {
                            voorvoegsel = new();
                            voorvoegsel += voorvoegsels[Nitril];
                        }
                    }
                    voorvoegsel ??= GenereerNaamSubBoom(nr, hoofdGroep, buur, type);
                    if (!zijgroepen.ContainsKey(voorvoegsel))
                    {
                        zijgroepen[voorvoegsel] = new();
                    }
                    zijgroepen[voorvoegsel].Add(plaatsAanduiding);
                }
            }
            plaatsAanduiding++;
        }
        foreach (var (voorvoegsel, locaties) in zijgroepen)
        {
            if (voorvoegsel.moetHaakes)
                naam.moetHaakes = true;
            StructuurNaam toevoeging = new();
            if (stamVerbinding.atomen.Count > 1
                 && (stamVerbinding.voorvoegsels.Count > 1 ||
                !stamVerbinding.eenYnAchtervoegsels.Empty() ||
                !stamVerbinding.hoofdGroepen.Empty() ||
                aanhechtingsPunt != -1
                || stamVerbinding.voorvoegsels[0] > 1
                || !stamVerbinding.cyclisch)) // wanneer plaatsaanduidingen nodig zijn
            {
                toevoeging += string.Join(",", locaties);
                toevoeging += "-";
            }
            bool gebruikHaakjes = voorvoegsel.moetHaakes;
            if (locaties.Count > 1)
            {
                naam.moetHaakes = true;
                /*if (gebruikHaakjes && locaties.Count == 2)
                    toevoeging.naam += "bis";
                else if (gebruikHaakjes && locaties.Count == 3)
                    toevoeging.naam += "tris";*/
                toevoeging.naam += ToGreek(locaties.Count);
            }
            if (gebruikHaakjes)
            {
                toevoeging += "" + VolgendHaakje(voorvoegsel.haakjes);
                naam.haakjes.Add(VolgendHaakje(voorvoegsel.haakjes));
            }
            toevoeging.naam += voorvoegsel.naam;
            toevoeging.naamAlfabetisch += voorvoegsel.naamAlfabetisch;
            if (gebruikHaakjes)
            {
                toevoeging += "" + SluitendHaakje(VolgendHaakje(voorvoegsel.haakjes));
            }
            naam += toevoeging;
        }
        bool gebruikPlaatsAanduiding = true; // voor achtervoegsel
        string achtervoegsel = "";
        if ((hoofdGroep == Carbonzuur || hoofdGroep == Aldehyde || hoofdGroep == Thialdehyde) && stamVerbinding.hoofdGroepen.Count <= 2 && !stamVerbinding.hoofdGroepCarbo)
        {
            gebruikPlaatsAanduiding = false;
        }
        if (stamVerbinding.atomen.Count == 1)
        {
            gebruikPlaatsAanduiding = false;
        }
        if (stamVerbinding.atomen.Count == 2 && stamVerbinding.voorvoegsels.Count == 0)
        {
            gebruikPlaatsAanduiding = false;
        }
        if (stamVerbinding.hoofdGroepen.Count == 1 && stamVerbinding.hoofdGroepen[0] == 1)
        {
            gebruikPlaatsAanduiding = false;
        }
        if (isBenzeen)
        {
            if (aanhechtingsPunt == -1)
            {
                if (hoofdGroep != Fout && !gebruikPlaatsAanduiding && klinkerWeg[hoofdGroep] && !stamVerbinding.hoofdGroepCarbo) achtervoegsel += "benzen";
                else achtervoegsel += "benzeen";
            }
            else
            {
                achtervoegsel += "fen";
            }
        }
        else
        {
            if (stamVerbinding.cyclisch) naam += "cyclo";
            naam += Alkaan(stamVerbinding.atomen.Count);
            if (stamVerbinding.enen.Count > 0)
            {
                string enen = "";
                if (stamVerbinding.atomen.Count > 2)
                {
                    naam.moetHaakes = true;
                    enen += "-" + string.Join(",", stamVerbinding.enen) + "-";
                }
                if (stamVerbinding.enen.Count > 1) enen += ToGreek(stamVerbinding.enen.Count);
                enen += "een";
                achtervoegsel += enen;
            }
            if (stamVerbinding.ynen.Count > 0)
            {
                string ynen = "";
                if (stamVerbinding.atomen.Count > 2)
                {
                    naam.moetHaakes = true;
                    ynen += "-" + string.Join(",", stamVerbinding.ynen) + "-";
                }
                if (stamVerbinding.ynen.Count > 1) ynen += ToGreek(stamVerbinding.ynen.Count);
                ynen += "yn";
                achtervoegsel += ynen;
            }
            if (aanhechtingsPunt == -1 && stamVerbinding.eenYnAchtervoegsels.Count == 0)
            {
                if (gebruikPlaatsAanduiding == false && klinkerWeg[hoofdGroep] && !stamVerbinding.hoofdGroepCarbo) achtervoegsel += "an";
                else achtervoegsel += "aan";
            }
        }
        if (aanhechtingsPunt == -1) // is geen zijtak
        {
            if (hoofdGroep != Fout)
            {
                // heeft een hoofdgroep
                if (gebruikPlaatsAanduiding)
                {
                    naam.moetHaakes = true;
                    achtervoegsel += "-" + string.Join(",", stamVerbinding.hoofdGroepen) + "-";
                }
                if (stamVerbinding.hoofdGroepen.Count > 1) achtervoegsel += ToGreek(stamVerbinding.hoofdGroepen.Count);
                if (stamVerbinding.hoofdGroepCarbo)
                    achtervoegsel += carboAchtervoegsels[hoofdGroep];
                else achtervoegsel += achtervoegsels[hoofdGroep];
            }
        }
        else
        {
            bool ylPlaatsaanduiding = stamVerbinding.aanhechtingsPunten[0] > 1 && !isBenzeen && stamVerbinding.atomen.Count > 1;

            void genereerAchtervoegsel()
            {
                if (ylPlaatsaanduiding)
                {
                    achtervoegsel += "aan";
                    naam.moetHaakes = true;
                    achtervoegsel += "-" + string.Join(",", stamVerbinding.aanhechtingsPunten) + "-";
                    if (stamVerbinding.aanhechtingsPunten.Count > 1) achtervoegsel += ToGreek(stamVerbinding.aanhechtingsPunten.Count);
                }
                achtervoegsel += "yl";
                if (aanhechtingsType == 2) achtervoegsel += "ideen";
                else if (aanhechtingsType == 3) achtervoegsel += "idyn";
            }

            if (aanhechtingsType == -1) // ether
            {
                if (ylPlaatsaanduiding)
                {
                    genereerAchtervoegsel();
                    naam += achtervoegsel;
                    naam.naam = naam.naam.TrimStart('-');
                    achtervoegsel = "";
                    naam.naam = VolgendHaakje(naam.haakjes) + naam.naam + SluitendHaakje(VolgendHaakje(naam.haakjes));
                    naam.haakjes.Add(VolgendHaakje(naam.haakjes));
                    naam.moetHaakes = true;
                    naam += "oxy";
                }
                else if (stamVerbinding.atomen.Count > 4 ||
                    stamVerbinding.eenYnAchtervoegsels.Count > 0 ||
                    stamVerbinding.cyclisch) // geen methoxy, ethoxy, propoxy of butoxy
                {
                    achtervoegsel += "yloxy";
                }
                else achtervoegsel += "oxy";
            }
            else
            {
                genereerAchtervoegsel();
            }
        }
        naam += achtervoegsel;
        naam.naam = naam.naam.TrimStart('-');
        if (stamVerbinding.voorvoegsels.Count > 0)
            naam.moetHaakes = true;
        return naam;
    }

    void CheckRingen(int nr, List<int> pad, bool[] bezocht, bool[] alInRing)
    {
        pad.Add(nr);
        bezocht[nr] = true;
        foreach (var (buur, _) in bindingen[nr])
        {
            if (pad.Count > 1 && buur == pad[^2]) continue;
            if (bezocht[buur])
            {
                bool wasAlInRing = alInRing[buur];
                List<int> ring = new();
                bool inRing = false;
                foreach (int atoom in pad)
                {
                    if (atoom == buur)
                    {
                        inRing = true;
                    }
                    if (inRing)
                    {
                        alInRing[atoom] = true;
                        ring.Add(atoom);
                    }
                    if (atoom == nr)
                    {
                        inRing = false;
                        break;
                    }
                }
                if (!ring.Empty())
                {
                    foreach (int nr2 in ring)
                        if (atoomsoort[nr2] != C)
                            throw new Exception("Ringen met heteroatomen worden niet ondersteund.");
                    if (wasAlInRing)
                    {
                        throw new Exception("Ringsystemen worden niet ondersteund.");
                    }
                }
                continue;
            }
            CheckRingen(buur, pad, bezocht, alInRing);
        }
        pad.Remove(nr);
    }

    public string GenereerNaam(int start = 0)
    {
        hoofdGroep = Fout;
        naamSucces = false;
        if (!OefenModusScript.AanHetOefenen && InstellingenScript.skipNaam)
        {
            return "Naamgeving staat uit!";
        }
        if (!fouteValenties.Empty()) return "Er zijn onmogelijke valenties in dit molecuul!";
        if (n == 0) return "Naam:";
        aantalGroepen = 0;
        try
        {
            CheckRingen(0, new(), new bool[n], new bool[n]);
            int aantalEsters = 0;
            for (int i = start; i < n; i++)
            {
                if (atoomsoort[i] == C)
                {
                    foreach (var (groep, _) in GroepenOpPlek(i))
                    {
                        if (groep == Ester) aantalEsters++;
                        aantalGroepen++;
                        hoofdGroep = Min(hoofdGroep, groep);
                    }
                }
            }
            if (aantalEsters > 1) throw new Exception("Er is maximaal 1 ester toegestaan!");
            if (hoofdGroep >= Ether) hoofdGroep = Fout;
            for (int i = start; i < n; i++)
            {
                if (atoomsoort[i] == C)
                {
                    string naam = GenereerNaamSubBoom(-1, hoofdGroep, i, -1).naam;
                    naamSucces = true;
                    if (!OefenModusScript.AanHetOefenen)
                        GUIUtility.systemCopyBuffer = naam;
                    else return naam;
                    return "Naam: " + naam;
                }
            }
            return "Geen koolstofatomen in molecuul!";
        }
        catch (Exception e)
        {
            return e.Message;
        }
    }

    public void ResetMolecuul()
    {
        tekenScript.ResetMolecuul();
        n = 0;
        atoomsoort.Clear();
        aantalBindingen.Clear();
        aantalWaterstof.Clear();
        lading.Clear();
        fixeerdeOctetten.Clear();
        fixeerdeWaterstof.Clear();
        tekenScript.ResetMolecuul();
        bindingen = Enumerable.Repeat(new List<(int, int)>(), n).ToList();
    }

    public TMP_Text output;

    public void GeefNaamGetekendeStructuur()
    {
        if (OefenModusScript.AanHetOefenen) return;
        output.text = GenereerNaam();
    }
}
