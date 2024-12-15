using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Mathf;
using static Extensions;
using static TekenScript;
using static NaamGever;
using static Definities;
using static Definities.AtoomSoort;
using System;
using System.Linq;
using TMPro;

public class TekenScript3D : MonoBehaviour
{
    public static List<List<Vector3>> bindingsVectoren = new()
    {
        // al bestaande binding is (0, 0, 1) = Vector3.forward

        // Volgorde is belangrijk hier: de laatste plekken worden gebruikt door vrije elektronenparen

        new () // voor bijvoorbeeld H2 of B#C, eerst atoom kunnen renderen
        {
            Vector3.forward,
            Vector3.forward,
        },

        // Linear molecular geometry
        new ()
        {
            Vector3.forward,
            new Vector3(0, 0, -1),
        },
        // Trigonal planar molecular geometry
        new ()
        {
            Vector3.forward,
            new Vector3(Sqrt(3f)/2f, 0f, -0.5f),
            new Vector3(-Sqrt(3f)/2f, 0f, -0.5f),
        },
        // Tetrahral molecular geometry
        new ()
        {
            Vector3.forward,
            new Vector3(-Sqrt(2/9f), Sqrt(6/9f), -1/3f),
            new Vector3(-Sqrt(2/9f), -Sqrt(6/9f), -1/3f),
            new Vector3(Sqrt(8/9f), 0f, -1/3f),
        },
        // Trigonal bipyramidal molecular geometry
        new ()
        {
            Vector3.forward,
            new Vector3(0f, 0f, -1f), // 3 elektronenparen -> linear
            new Vector3(1f, 0f, 0f), // 2 elektronenparen -> t-shaped
            new Vector3(-0.5f, Sqrt(3f)/2f, 0f), // 1 elektronenpaar -> Seesaw
            new Vector3(-0.5f, -Sqrt(3f)/2f, 0f),
        },
        // Octahedral molecular geometry
        new ()
        {
            Vector3.forward,
            new Vector3(0f, 0f, -1f),
            new Vector3(0f, 1f, 0f),
            new Vector3(0f, -1f, 0f), // 2 elektronenparen -> square planar 
            new Vector3(1f, 0f, 0f),
            new Vector3(-1f, 0f, 0f),
        },
        // Pentagonal bipyramidal molecular geometry
        new ()
        {
            Vector3.forward,
            new Vector3(0f, SinDegrees(72f), CosDegrees(72f)),
            new Vector3(0f, SinDegrees(72f*2), CosDegrees(72f*2)),
            new Vector3(0f, SinDegrees(72f*3), CosDegrees(72f*3)),
            new Vector3(0f, SinDegrees(72f*4), CosDegrees(72f*4)),
            new Vector3(1f, 0f, 0f),
            new Vector3(-1f, 0f, 0f),
        },
        // Square antiprismatic molecular geometry
        // skip ik voor nu omdat ik zo snel geen exacte coordinaten kon vinden
    };
    [Serializable]
    public struct AtoomKleur
    {
        public AtoomSoort atoomSoort;
        public Color kleur;
    }

    [SerializeField] NaamGever naamGever;
    [SerializeField] GameObject canvas;
    [SerializeField] GameObject canvas3D;
    [SerializeField] GameObject atoom;
    [SerializeField] GameObject bindingPrefab;
    [SerializeField] List<AtoomKleur> atoomKleuren = new(); // om in de inspector in te stellen
    [SerializeField] Color fallbackKleur = new();
    Dictionary<AtoomSoort, Color> kleurVoorAtoom = new();
    List<float> xs = new(), ys = new(), zs = new();
    List<GameObject> atoomObjecten = new(), bindingsObjecten = new();
    [SerializeField] float afstand = 0.1f;
    [SerializeField] ProgressBar progressBar;
    [SerializeField] GameObject genoeg;
    [SerializeField] GameObject waarschuwing;
    [SerializeField] OefenModusScript oefenModusScript;
    int[] benzeenNr = new int[0];
    int aantalBenzeen = 0;
    Vector3 startVec = new(0.01745240643f, 0f, 0.99984769515f);
    Structuur besteStructuur = new();
    DateTime eindTijd = DateTime.MaxValue;
    bool klaar = true;
    bool rendered = true;
    int pogingen = 0;
    float afstandsCategorien = 10f;
    int uiteinde1 = -1, uiteinde2 = -1;
    public SortedDictionary<int, Vector3> positie = new();
    public SortedDictionary<int, Stack<Vector3>> waterstof = new();
    int startPunt = 0;

    private void Awake()
    {
        foreach (var paar in atoomKleuren) kleurVoorAtoom[paar.atoomSoort] = paar.kleur;
        foreach (AtoomSoort atoom in Enum.GetValues(typeof(AtoomSoort)))
        {
            if (!kleurVoorAtoom.ContainsKey(atoom)) kleurVoorAtoom[atoom] = fallbackKleur;
        }
    }

    void MaakAtoom(Vector3 positie, int nr = -1)
    {
        GameObject huidigAtoom = Instantiate(atoom);
        huidigAtoom.name = "AtoomNr: " + nr;
        atoomObjecten.Add(huidigAtoom);
        AtoomSoort atoomSoort = nr < 0 ? H : atoomsoort[nr];
        if (atoomSoort == H)
        {
            huidigAtoom.transform.localScale = new Vector3(0.28f, 0.28f, 0.28f);
        }
        huidigAtoom.GetComponent<MeshRenderer>().material.color = kleurVoorAtoom[atoomSoort];
        huidigAtoom.transform.position = positie;
        xs.Add(positie.x);
        ys.Add(positie.y);
        zs.Add(positie.z);
        huidigAtoom.SetActive(true);
    }

    void TekenLijn(Vector3 positie, Quaternion rotatie, float offSet)
    {
        GameObject bindingsObject = Instantiate(bindingPrefab);
        bindingsObjecten.Add(bindingsObject);
        bindingsObject.SetActive(true);
        positie += rotatie * Vector3.forward * offSet;
        bindingsObject.transform.SetPositionAndRotation(positie, rotatie);
    }

    class Structuur
    {
        public float minimaleAfstand = 10f;
        public List<(Vector3, int)> atomenTeMaken = new();
        public List<(Vector3, Quaternion, float)> bindingenTeMaken = new();
        public List<Vector3> posities = new();
        public Vector3 pos1, pos2;
        public float uiteindenAfstand = Infinity;
    }

    Vector3 Draai(Vector3 binding, Quaternion rotatie)
    {
        var (x, y, z) = (rotatie.eulerAngles.x, rotatie.eulerAngles.y, rotatie.eulerAngles.z);
        if ((x == 180f || y == 180f || z == 180f) && x + y + z == 180f)
        {
            return -binding;
        }
        return rotatie * binding;
    }

    void MaakBinding(ref Structuur structuur, Quaternion rotatie, Vector3 positie, int omringingsgetal, ref int index, out Vector3 nieuweBinding, out Vector3 nieuwePositie, int vorige, int type = 1)
    {
        if (vorige == -1 && index > 2)
        {
            nieuweBinding = Draai(bindingsVectoren[omringingsgetal - 1][index - 1], rotatie);
        }
        else if (vorige == -1 && index == 2)
        {
            nieuweBinding = Draai(bindingsVectoren[omringingsgetal - 1][0], rotatie);
        }
        else nieuweBinding = Draai(bindingsVectoren[omringingsgetal - 1][index], rotatie);
        nieuwePositie = positie + nieuweBinding;
        index++;
        Quaternion bindingsRotatie = Quaternion.LookRotation(nieuweBinding) * Quaternion.LookRotation(Vector3.up);
        if (type != 2)
        {
            structuur.bindingenTeMaken.Add(((positie + nieuwePositie) / 2, bindingsRotatie, 0f));
        }
        else
        {
            structuur.bindingenTeMaken.Add(((positie + nieuwePositie) / 2, bindingsRotatie, afstand / 2f));
            structuur.bindingenTeMaken.Add(((positie + nieuwePositie) / 2, bindingsRotatie, -afstand / 2f));
        }
        if (type == 3)
        {
            structuur.bindingenTeMaken.Add(((positie + nieuwePositie) / 2, bindingsRotatie, afstand));
            structuur.bindingenTeMaken.Add(((positie + nieuwePositie) / 2, bindingsRotatie, -afstand));
        }
    }

    public int OmringingsGetal(int nr)
    {
        int vrijeElektronenParen = OctetRegel(nr, 0, true).vrijeElektronenParen;
        int omringingsgetal = vrijeElektronenParen + bindingen[nr].Count;
        omringingsgetal += aantalWaterstof[nr];
        return omringingsgetal;
    }

    void Maak3DModel(ref Structuur structuur, int random)
    {
        Stack<(int nr, int vorige, Vector3 positie, Vector3 binding, int cnt)> stack = new();
        // Stack ipv recursie om StackOverflow te voorkomenv voor grote moleculen
        stack.Push((startPunt, -1, new Vector3(), startVec, 0));
        bool[] bezocht = new bool[n];

        while (stack.Count > 0)
        {
            var (nr, vorige, positie, binding, cnt) = stack.Pop();

            if (nr == uiteinde1) structuur.pos1 = positie;
            if (nr == uiteinde2) structuur.pos2 = positie;
            structuur.atomenTeMaken.Add((positie, nr));
            structuur.posities.Add(positie);
            bezocht[nr] = true;
            int omringingsgetal = OmringingsGetal(nr);
            Vector3 richting = -binding;
            Quaternion rotatie = Quaternion.FromToRotation(Vector3.forward, richting);
            int index = 1;
            List<(int nr, int vorige, Vector3 positie, Vector3 binding, int cnt)> calls = new();
            foreach (var (buur, type) in bindingen[nr])
            {
                if (buur == vorige) continue;
                if (benzeenNr[buur] != benzeenNr[nr] || benzeenNr[nr] == 0) continue;
                if (index == 1)
                {
                    MaakBinding(ref structuur, rotatie, positie, omringingsgetal, ref index, out Vector3 nieuweBinding, out Vector3 nieuwePositie, vorige, type);
                    if (!bezocht[buur])
                    {
                        calls.Add((buur, nr, nieuwePositie, nieuweBinding, cnt + 1));
                    }
                }
                else index++;
            }
            List<(int, int)> buren = new();
            foreach (var (buur, type) in bindingen[nr])
            {
                if (buur == vorige) continue;
                if (benzeenNr[buur] == benzeenNr[nr] && benzeenNr[nr] != 0) continue;
                buren.Add((buur, type));
            }
            for (int i = 0; i < aantalWaterstof[nr]; i++)
            {
                buren.Add((-1, -1));
            }
            if (random >= 1)
            {
                if (random == 1 && bindingen[nr].Count == 2 && buren.Count >= 2)
                {
                    if (cnt % 2 == 1)
                    {
                        Swap(buren, 0, 1);
                    }
                }
                else buren.Shuffle();
            }
            else if (cnt % 2 == 1 && buren.Count >= 2)
            {
                Swap(buren, 0, 1);
            }
            foreach (var (buur, type) in buren)
            {
                MaakBinding(ref structuur, rotatie, positie, omringingsgetal, ref index, out Vector3 nieuweBinding, out Vector3 nieuwePositie, vorige, type);
                if (type == -1)
                {
                    structuur.atomenTeMaken.Add((nieuwePositie, -nr - 1));
                    structuur.posities.Add(nieuwePositie);
                }
                else
                {
                    if (!bezocht[buur])
                    {
                        calls.Add((buur, nr, nieuwePositie, nieuweBinding, cnt + 1));
                    }
                }
            }
            for (int i = calls.Count - 1; i >= 0; i--)
            {
                stack.Push(calls[i]);
            }
        }
    }

    void VindBenzeen(int nr, List<int> pad, bool[] bezocht, bool[] alInRing)
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
                    if (wasAlInRing || ring.Count != 6)
                    {
                        throw new Exception();
                    }
                    for (int i = 0; i < 6; i++)
                    {
                        if (OmringingsGetal(ring[i]) != 3 && OmringingsGetal(ring[i]) != 5)
                            throw new Exception();
                    }
                    aantalBenzeen++;
                    if (aantalBenzeen > 1) throw new();
                    for (int i = 0; i < 6; i++)
                    {
                        startPunt = ring[0];
                        benzeenNr[ring[i]] = aantalBenzeen;
                    }
                }
                continue;
            }
            VindBenzeen(buur, pad, bezocht, alInRing);
        }
        pad.Remove(nr);
    }

    public void Genoeg()
    {
        klaar = true;
        progressBar.gameObject.SetActive(false);
        genoeg.SetActive(false);
        RenderStructuur(true);
    }

    float RoundZoNodig(float f)
    {
        if (InstellingenScript.selecteerOpUiteindeAfstand) return Round(f);
        else return f;
    }

    bool veranderd = true;

    void GenereerStructuren()
    {
        double aantal = InstellingenScript.operatiesPerFrame / ((10 * n + 10) * Log(10 * n + 10, 2));
        if (aantal < 1) aantal = 1f;
        for (int i = 0; i < (int)aantal; i++)
        {
            if (DateTime.Now > eindTijd) break;
            Structuur structuur = new();
            int randomModus = 1;
            if (i % 5 == 0) randomModus = 2;
            Maak3DModel(ref structuur, randomModus);
            ClosestPair3D closestPair3D = new();
            structuur.minimaleAfstand = InstellingenScript.selecteerOpMinimaleAfstand
                 ? RoundZoNodig(closestPair3D.FindClosestPair(structuur.posities) * afstandsCategorien)
                 : -1;
            structuur.uiteindenAfstand = InstellingenScript.selecteerOpMinimaleAfstand
                ? Vector3.Distance(structuur.pos1, structuur.pos2)
                : -1f;
            if (structuur.minimaleAfstand > besteStructuur.minimaleAfstand)
            {
                veranderd = true;
                besteStructuur = structuur;
            }
            else if (structuur.minimaleAfstand == besteStructuur.minimaleAfstand && structuur.uiteindenAfstand > besteStructuur.uiteindenAfstand)
            {
                veranderd = true;
                besteStructuur = structuur;
            }
            pogingen++;
        }
    }

    void RenderStructuur(bool stop = true)
    {
        if (stop)
        {
            eindTijd = DateTime.MaxValue;
            rendered = true;
            Debug.Log($"Pogingen: {pogingen}");
            Debug.Log($"minimaleAfstand: {besteStructuur.minimaleAfstand}");
            Debug.Log($"UiteindeAfstand: {besteStructuur.uiteindenAfstand}");
        }
        if (!veranderd) return;
        waterstof.Clear();
        positie.Clear();
        veranderd = false;
        foreach (var obj in atoomObjecten) Destroy(obj);
        foreach (var obj in bindingsObjecten) Destroy(obj);
        atoomObjecten.Clear();
        bindingsObjecten.Clear();
        xs.Clear(); ys.Clear(); zs.Clear();
        waarschuwing.SetActive(besteStructuur.minimaleAfstand < 10);
        foreach (var (a, b, c) in besteStructuur.bindingenTeMaken)
        {
            TekenLijn(a, b, c);
        }
        foreach (var (a, b) in besteStructuur.atomenTeMaken)
        {
            if (b < 0)
            {
                if (!waterstof.ContainsKey(-b - 1))
                {
                    waterstof[-b - 1] = new();
                }
                waterstof[-b - 1].Push(a);
            }
            else positie[b] = a;
            MaakAtoom(a, b);
        }

        Vector3 middle = new(xs.Average(), ys.Average(), zs.Average());
        foreach (var obj in atoomObjecten) obj.transform.position -= middle;
        foreach (var obj in bindingsObjecten) obj.transform.position -= middle;
        //FindAnyObjectByType<OefenModusScript>().Polair();
        if (oefenModusScript.laadInfoScherm)
            oefenModusScript.OpenInfoScherm();
    }

    private void Update()
    {
        if (rendered) return;
        if (DateTime.Now >= eindTijd && tijd < 2 && besteStructuur.minimaleAfstand < 10)
        {
            eindTijd = eindTijd.AddSeconds(2 - tijd);
            tijd = 2;
        }
        TimeSpan diff = eindTijd - DateTime.Now;
        progressBar.SetProgress(1.0 - (diff.TotalSeconds / tijd));
        if (DateTime.Now < eindTijd)
        {
            GenereerStructuren();
        }
        else
        {
            klaar = true;
        }
        if (klaar)
        {
            Genoeg();
        }
        if (Time.frameCount % 90 == 0)
        {
            RenderStructuur(false);
        }
    }

    int VindUiteinde(int start)
    {
        int laatste = start;
        Queue<int> q = new(); // BFS
        q.Enqueue(start);
        bool[] bezocht = new bool[n];
        while (q.Count > 0)
        {
            int nr = q.Peek(); q.Dequeue();
            if (bezocht[nr]) continue;
            bezocht[nr] = true;
            laatste = nr;
            foreach (var (buur, _) in bindingen[nr])
            {
                if (bezocht[buur]) continue;
                q.Enqueue(buur);
            }
        }
        return laatste;
    }

    [SerializeField] GameObject foutPopUp;
    [SerializeField] TMP_Text foutText;

    double tijd = 0.01;

    void Fout(string text)
    {
        foutPopUp.SetActive(true);
        foutText.text = text;
    }

    public void Render3D()
    {
        Render3D(false);
    }

    public bool Render3D(bool oefenModus = false)
    {
        if (fouteValenties.Count > 0)
        {
            Fout("Foute valenties worden ook niet ondersteund in 3D!");
            return false;
        }
        foreach (var obj in atoomObjecten) Destroy(obj);
        foreach (var obj in bindingsObjecten) Destroy(obj);
        atoomObjecten.Clear();
        bindingsObjecten.Clear();
        xs.Clear(); ys.Clear(); zs.Clear();
        benzeenNr = new int[n];
        aantalBenzeen = 0;
        startPunt = 0;
        try
        {
            if (n != 0 && !InstellingenScript.negeerUnsupported)
                VindBenzeen(0, new(), new bool[n], new bool[n]);
        }
        catch
        {
            if (!oefenModus) Fout("Alleen één benzeen-achtige ring wordt ondersteund in 3D!");
            return false;
        }
        for (int i = 0; i < n; i++)
        {
            if (OmringingsGetal(i) >= 8)
            {
                Fout("Omringingsgetal 8 of hoger wordt niet ondersteund in 3D!");
                return false;
            }
        }
        veranderd = true;
        if (!oefenModus)
        {
            canvas.SetActive(false);
            canvas3D.SetActive(true);
        }
        if (n == 0) return false;
        if (!oefenModus)
        {
            progressBar.gameObject.SetActive(true);
            genoeg.SetActive(true);
            progressBar.SetProgress(0.0);
        }
        klaar = false;
        rendered = false;
        pogingen = 0;
        tijd = InstellingenScript.genereerTijd;
        if (double.IsNaN(tijd))
        {
            tijd = Math.Clamp(Math.Pow(3, n) / 1e9, 0.05, 2);
        }
        if (oefenModus)
        {
            tijd = 0.01f;
        }
        eindTijd = DateTime.Now.AddSeconds(tijd);
        besteStructuur = new();
        uiteinde1 = VindUiteinde(0);
        uiteinde2 = VindUiteinde(uiteinde1);
        Maak3DModel(ref besteStructuur, 0);
        ClosestPair3D closestPair3D = new();
        besteStructuur.minimaleAfstand = InstellingenScript.selecteerOpMinimaleAfstand
            ? RoundZoNodig(closestPair3D.FindClosestPair(besteStructuur.posities) * afstandsCategorien)
            : -1;
        besteStructuur.uiteindenAfstand = InstellingenScript.selecteerOpUiteindeAfstand
            ? Vector3.Distance(besteStructuur.pos1, besteStructuur.pos2)
            : -1f;
        GenereerStructuren();
        return true;
    }
    public void Terug()
    {
        rendered = true;
        canvas.SetActive(true);
        canvas3D.SetActive(false);
    }
}