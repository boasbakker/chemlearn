using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Definities
{
    public static int maxBindingen = 3;
    // public static string[] openendeHaakjes = new string[3] { "(", "[", "{" };
    // public static string[] sluitendeHaakjes = new string[3] { ")", "]", "}" };
    public enum AtoomSoort
    {
        H = 1, He, Li, Be, B, C, N, O, F, Ne,
        Na, Mg, Al, Si, P, S, Cl, Ar,
        K, Ca, Sc, Ti, V, Cr, Mn, Fe, Co, Ni, Cu, Zn, Ga, Ge, As, Se, Br, Kr,
        Rb, Sr, Y, Zr, Nb, Mo, Tc, Ru, Rh, Pd, Ag, Cd, In, Sn, Sb, Te, I, Xe
    }
    [System.Serializable]
    public enum Modus
    {
        Algemeen,
        Naamgeving,
        NaamgevingAndersom,
        Massaspectrometrie,
        Molmassa,
        Waterstofbruggen,
        Polariteit,
        PolaireBindingen,
        Oplosbaarheid,
        Lewisstructuren,
        //Stereochemie,
    };
    public static SortedDictionary<AtoomSoort, int> valentieElektronen = new()
    {
        { AtoomSoort.H, 1 },
        { AtoomSoort.He, 2 },
        { AtoomSoort.B, 3 },
        { AtoomSoort.C, 4 },
        { AtoomSoort.N, 5 },
        { AtoomSoort.O, 6 },
        { AtoomSoort.F, 7 },
        { AtoomSoort.Ne, 8 },
        { AtoomSoort.Si, 4 },
        { AtoomSoort.P, 5 },
        { AtoomSoort.S, 6 },
        { AtoomSoort.Cl, 7 },
        { AtoomSoort.Ar, 8 },
        { AtoomSoort.As, 5 },
        { AtoomSoort.Se, 6 },
        { AtoomSoort.Br, 7 },
        { AtoomSoort.Kr, 8 },
        { AtoomSoort.Te, 6 },
        { AtoomSoort.I, 7 },
        { AtoomSoort.Xe, 8 },
    };
    public static SortedDictionary<AtoomSoort, int> atoomMassa = new()
    {
        { AtoomSoort.H, 1 },
        { AtoomSoort.He, 4 },
        { AtoomSoort.B, 11 },
        { AtoomSoort.C, 12 },
        { AtoomSoort.N, 14 },
        { AtoomSoort.O, 16 },
        { AtoomSoort.F, 19 },
        { AtoomSoort.Ne, 20 },
        { AtoomSoort.Si, 28 },
        { AtoomSoort.P, 31 },
        { AtoomSoort.S, 32 },
        { AtoomSoort.Cl, 35 }, // 75%
        { AtoomSoort.Ar, 40 },
        { AtoomSoort.As, 75 },
        { AtoomSoort.Kr, 84 },
        { AtoomSoort.I, 127 }
    };
    public static SortedDictionary<AtoomSoort, double> gemiddeldeAtoomMassa = new()
    {
        { AtoomSoort.H, 1.008 },
        { AtoomSoort.He, 4.003 },
        { AtoomSoort.B, 10.81 },
        { AtoomSoort.C, 12.01 },
        { AtoomSoort.N, 14.01 },
        { AtoomSoort.O, 16.00 },
        { AtoomSoort.F, 19.00 },
        { AtoomSoort.Ne, 20.18 },
        { AtoomSoort.Si, 28.09 },
        { AtoomSoort.P, 30.97 },
        { AtoomSoort.S, 32.06 },
        { AtoomSoort.Cl, 35.45 },
        { AtoomSoort.Ar, 39.95 },
        { AtoomSoort.As, 74.92 },
        { AtoomSoort.Se, 78.96 },
        { AtoomSoort.Br, 79.90 },
        { AtoomSoort.Kr, 83.80 },
        { AtoomSoort.Te, 127.6 },
        { AtoomSoort.I, 126.9 },
        { AtoomSoort.Xe, 131.3 },
    };

    public static SortedDictionary<AtoomSoort, double> elektronegativiteit = new()
    {
        { AtoomSoort.H, 2.1 },
        { AtoomSoort.B, 2.0 },
        { AtoomSoort.C, 2.5 },
        { AtoomSoort.N, 3.0 },
        { AtoomSoort.O, 3.5 },
        { AtoomSoort.F, 4.0 },
        { AtoomSoort.Si, 1.9 },
        { AtoomSoort.P, 2.2 },
        { AtoomSoort.S, 2.6 },
        { AtoomSoort.Cl, 3.2 },
        { AtoomSoort.As, 2.2 },
        { AtoomSoort.Se, 2.5 },
        { AtoomSoort.Br, 3.0 },
        { AtoomSoort.Te, 2.1 },
        { AtoomSoort.I, 2.7 },
    };

    // geeft het aantal elektronen in de mogelijke (uitgebreide octetten) voor de gegeven atoomsoort.
    public static List<int> Octetten(AtoomSoort atoomSoort)
    {
        return atoomSoort switch
        {
            AtoomSoort.H => new() { 2 },
            AtoomSoort.He => new() { 2 },
            AtoomSoort.Li => new() { 2 },
            AtoomSoort.Be => new() { 4 },
            AtoomSoort.B => new() { 6 },
            AtoomSoort.P => new() { 8, 10 },// vb: PCl5
            AtoomSoort.As => new() { 8, 10 },
            AtoomSoort.S => new() { 8, 10, 12 },// vb: SO2, SF6
            AtoomSoort.Se => new() { 8, 10, 12 },
            AtoomSoort.Te => new() { 8, 10, 12 },
            AtoomSoort.Cl => new() { 8, 10, 12, 14 },// vb: ClF3, ClF5, [ClO4]-
            AtoomSoort.Br => new() { 8, 10, 12, 14 },// vb: BrF3, BrF5, BrO3F
            AtoomSoort.I => new() { 8, 10, 12, 14 },// vb: IF3, IF5, IF7
            AtoomSoort.Kr => new() { 8, 10 },// KrF2, misschien ook 12?
            AtoomSoort.Xe => new() { 8, 10, 12, 14, 16 },// Xe, XeF2, XeF4, XeF6, XeO4
            _ => new() { 8 },
        };
    }
    public enum KarakteristiekeGroep
    {
        Carbonzuur,
        Sulfonzuur,
        Ester,
        Nitril,
        Aldehyde,
        Thialdehyde,
        Keton,
        Thion,
        Hydroxylgroep,
        Thiol,
        Amine,
        Imine,
        Ether,
        Halogeengroep,
        Fluorgroep,
        Chloorgroep,
        Broomgroep,
        Joodgroep,
        Nitrogroep,
        Fout
    }
    public static SortedDictionary<KarakteristiekeGroep, bool> klinkerWeg = new()
    {
        { KarakteristiekeGroep.Carbonzuur, false },
        { KarakteristiekeGroep.Sulfonzuur, false },
        { KarakteristiekeGroep.Ester, true },
        { KarakteristiekeGroep.Aldehyde, true },
        { KarakteristiekeGroep.Thialdehyde, false },
        { KarakteristiekeGroep.Keton, true },
        { KarakteristiekeGroep.Thion, false },
        { KarakteristiekeGroep.Hydroxylgroep, true },
        { KarakteristiekeGroep.Thiol, false },
        { KarakteristiekeGroep.Amine, false },
        { KarakteristiekeGroep.Imine, false },
        { KarakteristiekeGroep.Nitril, false },
        { KarakteristiekeGroep.Fout, false },
    };
    public static SortedDictionary<AtoomSoort, KarakteristiekeGroep> halogeenGroepen = new()
    {
        { AtoomSoort.F, KarakteristiekeGroep.Fluorgroep },
        { AtoomSoort.Cl, KarakteristiekeGroep.Chloorgroep },
        { AtoomSoort.Br, KarakteristiekeGroep.Broomgroep },
        { AtoomSoort.I, KarakteristiekeGroep.Joodgroep },
    };
    public static SortedDictionary<KarakteristiekeGroep, string> voorvoegsels = new()
    {
        { KarakteristiekeGroep.Carbonzuur, "carboxy" },
        { KarakteristiekeGroep.Sulfonzuur, "sulfo" },
        // Ester heeft geen voorvoegsel, wordt keton+ether
        { KarakteristiekeGroep.Aldehyde, "formyl" }, // oxo is keton?
        { KarakteristiekeGroep.Thialdehyde, "sulfanylideen" },
        { KarakteristiekeGroep.Keton, "oxo" },
        { KarakteristiekeGroep.Thion, "sulfanylideen" },
        { KarakteristiekeGroep.Hydroxylgroep, "hydroxy" },
        { KarakteristiekeGroep.Thiol, "sulfanyl" },
        { KarakteristiekeGroep.Amine, "amino" },
        { KarakteristiekeGroep.Imine, "imino" },
        { KarakteristiekeGroep.Fluorgroep, "fluor" },
        { KarakteristiekeGroep.Chloorgroep, "chloor" },
        { KarakteristiekeGroep.Broomgroep, "broom" },
        { KarakteristiekeGroep.Joodgroep, "jood" },
        { KarakteristiekeGroep.Nitrogroep, "nitro" },
        { KarakteristiekeGroep.Nitril, "cyaan" },
    };

    public static SortedDictionary<KarakteristiekeGroep, string> achtervoegsels = new()
    {
        { KarakteristiekeGroep.Carbonzuur, "zuur" },
        { KarakteristiekeGroep.Sulfonzuur, "sulfonzuur" },
        { KarakteristiekeGroep.Ester, "oaat" },
        { KarakteristiekeGroep.Aldehyde, "al" },
        { KarakteristiekeGroep.Thion, "thion" },
        { KarakteristiekeGroep.Keton, "on" },
        { KarakteristiekeGroep.Hydroxylgroep, "ol" },
        { KarakteristiekeGroep.Thiol, "thiol" },
        { KarakteristiekeGroep.Amine, "amine" },
        { KarakteristiekeGroep.Imine, "imine" },
        { KarakteristiekeGroep.Nitril, "nitril" },
        { KarakteristiekeGroep.Thialdehyde, "thial"},
    };
    public static SortedDictionary<KarakteristiekeGroep, string> carboAchtervoegsels = new()
    {
        { KarakteristiekeGroep.Carbonzuur, "carbonzuur" },
        { KarakteristiekeGroep.Aldehyde, "carbaldehyde" },
        { KarakteristiekeGroep.Nitril, "carbonitril" }
    };
    public static SortedDictionary<char, string> cijfers = new()
    {
        { '0', "" },
        { '1', "hen" },
        { '2', "di" },
        { '3', "tri" },
        { '4', "tetra" },
        { '5', "penta" },
        { '6', "hexa" },
        { '7', "hepta" },
        { '8', "octa" },
        { '9', "nona" }
    };

    public static char VolgendHaakje(SortedSet<char> haakjes)
    {
        if (haakjes.Count == 3) return '(';
        if (haakjes.Count == 2)
        {
            if (!haakjes.Contains('[')) return '[';
            if (!haakjes.Contains('{')) return '{';
        }
        else
        {
            if (haakjes.Contains('(')) return '[';
            if (haakjes.Contains('[')) return '{';
        }
        return '(';
    }

    public static char SluitendHaakje(char haakje)
    {
        if (haakje == '(') return ')';
        else return (char)(haakje + 2);
    }

    public static string ToGreek(int n)
    {
        if (n >= 10000)
        {
            throw new Exception();
            // return "";
        }
        if (n == 0) return "";
        string resultaat = "";
        string s = new(n.ToString().ToCharArray().Reverse().ToArray());
        int length = s.Length;
        int p = 0; // tienmacht

        foreach (char c in s)
        {
            string toevoeging = cijfers[c];
            if (c == '1')
            {
                if (p == 0 && length > 1 && s[p + 1] == '1') toevoeging = "un"; // elf: undeca
                else if (p == 1) toevoeging = "deca"; // tien
                else if (p == 2) toevoeging = "he"; // honderd
                else if (p == 3) toevoeging = "ki"; // duizend
            }
            else if (c == '2')
            {
                if (p == 0 && length > 1) toevoeging = "do";
                else if (p == 1)
                {
                    if (s[p - 1] < '2') toevoeging = "i"; // uitzondering henicosa
                    else toevoeging = "";
                    toevoeging += "cosa"; // twintigtallen
                }
            }
            if (p > 1 || (c != '2' && c != '1'))
            {
                if (c != '0')
                {
                    if (p == 1 && c == '3') toevoeging += "a"; // uitzondering triaconta
                    if (p == 1) toevoeging += "conta"; // tientallen
                    else if (p == 2) toevoeging += "cta"; // hondertallen
                    else if (p == 3) toevoeging += "lia"; // duizentallen
                }
            }
            p++;
            resultaat += toevoeging;
        }
        return resultaat;
    }

    public static string Alkaan(int n)
    {
        if (n == 0) return "";
        else if (n == 1) return "meth";
        else if (n == 2) return "eth";
        else if (n == 3) return "prop";
        else if (n == 4) return "but";
        else
        {
            string greek = ToGreek(n);
            return greek.Remove(greek.Length - 1); // verwijder laatste 'a'
        }
    }
}

public static class Extensions
{
    public static float SinDegrees(float degrees)
    {
        return Mathf.Sin(degrees * Mathf.PI / 180f);
    }
    public static float CosDegrees(float degrees)
    {
        return Mathf.Cos(degrees * Mathf.PI / 180f);
    }
    public static void DecrementValues(this List<int> numbers)
    {
        for (int i = 0; i < numbers.Count; i++)
        {
            numbers[i] = numbers[i] - 1;
        }
    }
    // Net zoals in C++
    public static T Back<T>(this IList<T> list)
    {
        if (list == null || list.Count == 0)
        {
            throw new InvalidOperationException("Cannot access the last element of an empty or null list.");
        }
        return list[^1];
    }
    // Net zoals in C++
    public static void PopBack<T>(this IList<T> list)
    {
        if (list == null || list.Count == 0)
        {
            throw new InvalidOperationException("Cannot access the last element of an empty or null list.");
        }
        list.RemoveAt(list.Count - 1);
    }
    // Net zoals in C++
    public static T Min<T>(T a, T b)
    {
        return Convert.ToInt32(a) < Convert.ToInt32(b) ? a : b;
    }
    // Net zoals in C++
    public static bool Empty<T>(this ICollection<T> collection)
    {
        return collection == null || collection.Count == 0;
    }
    public static T DeepCopy<T>(this T obj)
    {
        // Serialize and then deserialize to create a deep copy
        return JsonUtility.FromJson<T>(JsonUtility.ToJson(obj));
    }

    public static bool IsInteger(this double number)
    {
        return Math.Abs(number % 1) < double.Epsilon;
    }
    public static void Shuffle<T>(this List<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = UnityEngine.Random.Range(0, n + 1);
            (list[n], list[k]) = (list[k], list[n]);
        }
    }
    public static void Swap<T>(ref T a, ref T b)
    {
        (b, a) = (a, b);
    }
    public static void Swap<T>(List<T> list, int index1, int index2)
    {
        (list[index2], list[index1]) = (list[index1], list[index2]);
    }
    public static void TryIncrement<TKey>(this Dictionary<TKey, int> dictionary, TKey key, int n = 1)
    {
        if (dictionary.ContainsKey(key))
        {
            dictionary[key] += n;
        }
        else
        {
            dictionary[key] = n; // Initialize to 1 if the key does not exist
        }
    }
}

public class ListComparer : IComparer<List<int>>
{
    public int Compare(List<int> x, List<int> y)
    {
        int result = 0;
        for (int i = 0; i < x.Count && i < y.Count; i++)
        {
            result = x[i].CompareTo(y[i]);
            if (result != 0) return result;
        }
        return result;
    }
}
