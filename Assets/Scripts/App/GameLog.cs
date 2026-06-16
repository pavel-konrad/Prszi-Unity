using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// Debug match log: records each game action with the rule context at that moment
/// (top discard, forced suit, draw penalty, ace-pending, active player) and writes
/// it to JSON. Lets us replay a match's sequence and compare it against what the
/// screen actually showed — to pin down rule/visual mismatches.
/// </summary>
public static class GameLog
{
    [System.Serializable]
    class Entry
    {
        public int seq;
        public string ev;          // DEAL, PLAY, DRAW, STAND, ACTIVE, ROUND_END
        public string player;
        public string card;
        public string top;         // top discard after the action
        public string forcedSuit;  // "" = none
        public int pendingDraw;
        public bool acePending;
        public int activeIndex;
    }

    [System.Serializable]
    class Log { public List<Entry> entries = new List<Entry>(); }

    static Log _log = new Log();
    static int _seq = 0;

    /// <summary>Set by CardManager so the log can read the current top discard.</summary>
    public static CardManager Cards;

    public static void Record(string ev, string player = "", string card = "")
    {
        var gs = GameSession.I;
        var r = gs != null ? gs.Rules : null;
        var top = Cards != null ? Cards.GetTopDiscardCard() : null;

        _log.entries.Add(new Entry
        {
            seq = ++_seq,
            ev = ev,
            player = player,
            card = card,
            top = top != null ? top.ToString() : "",
            forcedSuit = r != null && r.ForcedSuit != null ? r.ForcedSuit.ToString() : "",
            pendingDraw = r != null ? r.PendingDrawCount : 0,
            acePending = r != null && r.AcePending,
            activeIndex = gs != null ? gs.ActiveIndex : -1
        });
    }

    /// <summary>Clears the log for a new round.</summary>
    public static void Clear()
    {
        _log = new Log();
        _seq = 0;
    }

    /// <summary>Writes the log to persistentDataPath/prsi_log.json and prints the path.</summary>
    public static void Flush()
    {
        string path = Path.Combine(Application.persistentDataPath, "prsi_log.json");
        File.WriteAllText(path, JsonUtility.ToJson(_log, true));
        Debug.Log($"[GameLog] {_log.entries.Count} záznamů → {path}");
    }
}
