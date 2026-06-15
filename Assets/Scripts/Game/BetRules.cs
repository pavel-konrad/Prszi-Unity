using System.Linq;
using UnityEngine;

public static class BetRules
{
    public static readonly int[] Presets = {25,50,100,150,200,300,500,1000};

    public static int RandomAffordable(int cash){
        var opts = Presets.Where(p=>p<=cash).ToArray();
        if (opts.Length==0) return 0;
        return opts[Random.Range(0, opts.Length)];
    }

    public static int HighestAffordable(int cash){
        var opts = Presets.Where(p=>p<=cash);
        return opts.Any() ? opts.Max() : 0;
    }

    public static int ClampToPreset(int value){
        if (value <= 0) return 0;
        return Presets.OrderBy(p => Mathf.Abs(p - value)).First();
    }
}
