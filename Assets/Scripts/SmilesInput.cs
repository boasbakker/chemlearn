using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using static Definities;
using static NaamGever;
using static TekenScript;

public class SmilesInput : MonoBehaviour
{
    void NieuwAtoom(AtoomSoort soort)
    {
        n++;
        atoomsoort.Add(soort);
        aantalBindingen.Add(0);
        aantalWaterstof.Add(0);
        lading.Add(0);
        fixeerdeOctetten.Add(-1);
        fixeerdeWaterstof.Add(-1);
        bindingen.Add(new());
        OctetRegel(n - 1);
    }

    void LeesAnderAtoom(string s, ref int vorige, int type)
    {
        string nrLading = "", nrH = "";
        int atoomlading = 0, waterstof = 0;
        bool alLading = false;
        for (int i = 0; i < s.Length; i++)
        {
            char c = s[i];
            if (c == '+')
            {
                alLading = true;
                atoomlading++;
            }
            else if (c == '-')
            {
                alLading = true;
                atoomlading--;
            }
            else if (char.IsDigit(c))
            {
                if (alLading) nrLading += c;
                else nrH += c;
            }
            else if (c == 'H')
            {
                waterstof = 1;
            }
            else if (char.IsUpper(c))
            {
                string atoomSoort = "" + c;
                if (i + 1 < s.Length && char.IsLower(s[i + 1])) // uitkijken voor aromtische atomen
                {
                    atoomSoort += s[i + 1];
                    i++;
                }
                NieuwAtoom((AtoomSoort)Enum.Parse(typeof(AtoomSoort), atoomSoort));
                if (vorige != -1)
                {
                    bindingen[vorige].Add((n - 1, type));
                    bindingen.Back().Add((vorige, type));
                    aantalBindingen[n - 1] += type;
                    aantalBindingen[vorige] += type;
                    OctetRegel(vorige);
                    OctetRegel(n - 1);
                }
                vorige = n - 1;
                type = 1;
            }
            else
                throw new Exception();
        }
        if (nrLading != "") atoomlading *= int.Parse(nrLading);
        if (nrH != "")
        {
            waterstof = int.Parse(nrH);
        }
        lading[vorige] = atoomlading;
        fixeerdeWaterstof[vorige] = waterstof;
        aantalWaterstof[vorige] = waterstof;
        OctetRegel(vorige);
    }

    Dictionary<int, (int, int)> ringNummers = new();

    void LeesSmiles(in string s, int l, int r, int nrVoorHaakjes = -1)
    {
        if (string.IsNullOrEmpty(s)) return;
        int type = 1;
        int vorige = nrVoorHaakjes;
        string nr = "";
        bool procentGehad = false;
        void VerwerkRing(int ringNr)
        {
            procentGehad = false;
            nr = "";
            if (!ringNummers.ContainsKey(ringNr))
            {
                ringNummers[ringNr] = (vorige, type);
            }
            else
            {
                var (verbondenNr, type2) = ringNummers[ringNr];
                if (type != type2) throw new Exception();
                bindingen[vorige].Add((verbondenNr, type));
                bindingen[verbondenNr].Add((vorige, type));
                aantalBindingen[verbondenNr] += type;
                aantalBindingen[vorige] += type;
                OctetRegel(vorige);
                OctetRegel(verbondenNr);
                ringNummers.Remove(ringNr);
            }
            type = 1;
        }
        for (int i = l; i <= r; i++)
        {
            char c = s[i];
            if (!char.IsDigit(c) && nr != "")
            {
                VerwerkRing(int.Parse(nr));
            }
            if (c == ' ') break;
            if (c == '%')
            {
                procentGehad = true;
            }
            else if (c == '=')
            {
                type = 2;
            }
            else if (c == '#')
            {
                type = 3;
            }
            else if (char.IsUpper(c))
            {
                string atoomSoort = "" + c;
                if (i + 1 < s.Length && char.IsLower(s[i + 1])) // uitkijken voor aromtische atomen
                {
                    atoomSoort += s[i + 1];
                    i++;
                }
                NieuwAtoom((AtoomSoort)Enum.Parse(typeof(AtoomSoort), atoomSoort));
                if (vorige != -1)
                {
                    bindingen[vorige].Add((n - 1, type));
                    bindingen.Back().Add((vorige, type));
                    aantalBindingen[n - 1] += type;
                    aantalBindingen[vorige] += type;
                    OctetRegel(vorige);
                    OctetRegel(n - 1);
                }
                vorige = n - 1;
                type = 1;
            }
            else if (char.IsDigit(c))
            {
                if (procentGehad)
                    nr += c;
                else VerwerkRing(c - '0');
            }
            else
            {
                if (c == '(')
                {
                    int l2 = i + 1, r2 = i;
                    int haakjesBalans = 1;
                    for (i++; i < s.Length; i++)
                    {
                        if (s[i] == '(') haakjesBalans++;
                        if (s[i] == ')') haakjesBalans--;
                        if (haakjesBalans == 0) break;
                        r2++;
                    }
                    LeesSmiles(in s, l2, r2, vorige);
                }
                else if (c == '[')
                {
                    int haakjesBalans = 1;
                    string tussenStuk = "";
                    for (i++; i < s.Length; i++)
                    {
                        if (s[i] == '[') haakjesBalans++;
                        if (s[i] == ']') haakjesBalans--;
                        if (haakjesBalans == 0) break;
                        tussenStuk += s[i];
                    }
                    LeesAnderAtoom(tussenStuk, ref vorige, type);
                }
                else throw new Exception();
            }
        }
        if (nr != "")
        {
            VerwerkRing(int.Parse(nr));
        }
    }

    [SerializeField] TMP_InputField input;
    [SerializeField] NaamGever naamGever;

    void VerwijderStereo(ref string text)
    {
        text = text.Replace("[C@@H]", "C");
        text = text.Replace("[C@H]", "C");
        text = text.Replace("[C@]", "C");
        text = text.Replace("[C@@]", "C");
        text = text.Replace("@", "");
        text = text.Replace("/", "");
        text = text.Replace(@"\", "");
    }

    public void Submit()
    {
        naamGever.ResetMolecuul();
        try
        {
            string text = input.text;
            VerwijderStereo(ref text);
            LeesSmiles(in text, 0, text.Length - 1);
            naamGever.GeefNaamGetekendeStructuur();
        }
        catch (Exception e)
        {
            Debug.LogWarning(e);
            naamGever.output.text = "Fout bij het lezen van de SMILES.";
        }
    }
}
