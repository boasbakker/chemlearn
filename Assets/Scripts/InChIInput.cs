using UnityEngine;
using TMPro;
using static Definities;
using static NaamGever;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System;

public class InChIInput : MonoBehaviour
{
    void LeesBindingen(string s, int nrVoorHaakjes = -1)
    {
        if (string.IsNullOrEmpty(s)) return;
        s += '-'; // dummy karakter
        string nr = "";
        int nr2 = nrVoorHaakjes;
        for (int i = 0; i < s.Length; i++)
        {
            char c = s[i];
            if (char.IsDigit(c))
            {
                nr += c;
            }
            else
            {
                int nr1 = nr2;
                nr2 = int.Parse(nr) - 1;
                nr = "";
                if (nr1 != -1)
                {
                    aantalBindingen[nr1]++;
                    aantalBindingen[nr2]++;
                    bindingen[nr1].Add((nr2, 1));
                    bindingen[nr2].Add((nr1, 1));
                }
                if (c == '(')
                {
                    int haakjesBalans = 1;
                    List<string> tussenStukken = new();
                    string tussenStuk = "";
                    for (i++; i < s.Length; i++)
                    {
                        if (s[i] == '(') haakjesBalans++;
                        if (s[i] == ')') haakjesBalans--;
                        if (haakjesBalans == 0) break;
                        if (s[i] == ',' && haakjesBalans == 1)
                        {
                            tussenStukken.Add(tussenStuk);
                            tussenStuk = "";
                        }
                        else tussenStuk += s[i];
                    }
                    tussenStukken.Add(tussenStuk);
                    foreach (string stuk in tussenStukken) LeesBindingen(stuk, nr2);
                }
            }
        }
    }

    void LeesWaterstof(string waterstof)
    {
        List<string> delen = new();
        string cur = "";
        bool Hgehad = false;
        waterstof += ','; // dummy karakter
        foreach (char c in waterstof)
        {
            if (c == ',' && Hgehad)
            {
                Hgehad = false;
                delen.Add(cur);
                cur = "";
            }
            else
                cur += c;
            if (c == 'H') Hgehad = true;
        }
        foreach (string s in delen)
        {
            string nr = "";
            List<int> nrs = new();
            int nr1 = -1, nr2 = -1;
            for (int ix = 0; ix < s.Length; ix++)
            {
                char c = s[ix];
                if (char.IsDigit(c))
                {
                    nr += c;
                }
                else
                {
                    if (nr1 == -1)
                    {
                        nr1 = int.Parse(nr);
                    }
                    else
                    {
                        if (c == ',')
                        {
                            nrs.Add(nr1);
                            nr1 = int.Parse(nr);
                        }
                        else if (c == 'H')
                        {
                            nr2 = int.Parse(nr);
                            for (int i = nr1; i <= nr2; i++)
                                nrs.Add(i);
                            nr1 = -1; nr2 = -1;
                        }
                        else throw new Exception();
                    }
                    nr = "";
                }
            }
            int aantal = nr == "" ? 1 : int.Parse(nr);
            foreach (int i in nrs) aantalWaterstof[i - 1] = aantal;
        }
    }

    bool LeesInChI(string InChiI)
    {
        naamGever.ResetMolecuul();
        try
        {
            if (InChiI.StartsWith("InChI="))
            {
                InChiI = InChiI[6..];
            }
            if (InChiI.StartsWith("1S/"))
            {
                InChiI = InChiI[3..];
            }
            if (InChiI.StartsWith("1/"))
            {
                InChiI = InChiI[2..];
            }
            string[] lagen = InChiI.Split('/');
            if (lagen.Length == 0)
                return false;
            string molecuulformule = lagen[0];
            string currentAtom = "";
            string currentNumber = "";
            molecuulformule += 'A'; // Voeg een dummy karakter toe om de laatste atoomsoort te verwerken
            foreach (char c in molecuulformule)
            {
                if (char.IsLetter(c))
                {
                    if (char.IsUpper(c))
                    {
                        if (!string.IsNullOrEmpty(currentAtom))
                        {
                            int aantal = 1;
                            if (!string.IsNullOrEmpty(currentNumber))
                            {
                                aantal = int.Parse(currentNumber);
                            }
                            if (currentAtom != "H")
                            {
                                n += aantal;
                                for (int i = 0; i < aantal; i++)
                                {
                                    atoomsoort.Add((AtoomSoort)Enum.Parse(typeof(AtoomSoort), currentAtom));
                                }
                            }
                        }
                        currentAtom = c.ToString();
                        currentNumber = "";
                    }
                    else if (char.IsLower(c))
                    {
                        currentAtom += c;
                    }
                    else return false;
                }
                else if (char.IsDigit(c))
                {
                    currentNumber += c;
                }
                else return false;
            }
            string bindingLaag = "", waterstof = "";
            for (int i = 1; i < lagen.Length; i++)
            {
                if (lagen[i][0] == 'c') bindingLaag = lagen[i][1..];
                if (lagen[i][0] == 'h') waterstof = lagen[i][1..];
            }
            bindingLaag.Replace(",", ")(");
            bindingLaag = Regex.Replace(bindingLaag, @"\s+", "");
            for (int i = 0; i < n; i++) bindingen.Add(new());
            aantalBindingen = Enumerable.Repeat(0, n).ToList();
            aantalWaterstof = Enumerable.Repeat(0, n).ToList();
            lading = Enumerable.Repeat(0, n).ToList();
            fixeerdeOctetten = Enumerable.Repeat(-1, n).ToList();
            fixeerdeWaterstof = Enumerable.Repeat(-1, n).ToList();
            LeesBindingen(bindingLaag);
            LeesWaterstof(waterstof);
        }
        catch (Exception ex)
        {
            Debug.LogWarning(ex);
            return false;
        }
        return true;
    }

    [SerializeField] TMP_InputField input;
    [SerializeField] NaamGever naamGever;
    public void OnSubmit()
    {
        bool succes = LeesInChI(input.text);
        if (!succes)
            naamGever.output.text = "Fout bij het lezen van de InChI.";
        naamGever.GeefNaamGetekendeStructuur();
    }
}
