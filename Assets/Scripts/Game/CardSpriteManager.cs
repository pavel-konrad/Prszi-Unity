using UnityEngine;
using System.Collections.Generic;
using Prsi.Core.Cards;

public class CardSpriteManager : MonoBehaviour
{
    [Header("Card Sprites")]
    [SerializeField] private Sprite[] heartSprites;    // 7, 8, 9, 10, J, Q, K, A
    [SerializeField] private Sprite[] diamondSprites;  // 7, 8, 9, 10, J, Q, K, A
    [SerializeField] private Sprite[] clubSprites;     // 7, 8, 9, 10, J, Q, K, A
    [SerializeField] private Sprite[] spadeSprites;    // 7, 8, 9, 10, J, Q, K, A
    
    [Header("Alternative: Individual Symbols")]
    [SerializeField] private Sprite[] symbolSprites;   // Všechny symboly v pořadí: 7,8,9,10,J,Q,K,A
    [SerializeField] private Sprite heartSymbol;
    [SerializeField] private Sprite diamondSymbol;
    [SerializeField] private Sprite clubSymbol;
    [SerializeField] private Sprite spadeSymbol;
    
    [Header("Card Back")]
    [SerializeField] private Sprite cardBackSprite;
    
    private Dictionary<string, Sprite> spriteCache = new Dictionary<string, Sprite>();
    
    public static CardSpriteManager Instance { get; private set; }
    
    // Statická metoda pro zajištění existence instance
    public static CardSpriteManager EnsureInstance()
    {
        if (Instance == null)
        {
            // Hledat existující instanci
            Instance = FindObjectOfType<CardSpriteManager>();
            
            // Pokud neexistuje, vytvořit novou
            if (Instance == null)
            {
                GameObject go = new GameObject("CardSpriteManager");
                Instance = go.AddComponent<CardSpriteManager>();
                DontDestroyOnLoad(go); // Zachovat mezi scénami
            }
        }
        
        return Instance;
    }
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            InitializeSpriteCache();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void InitializeSpriteCache()
    {
        
        // Načíst sprites pro každou barvu
        LoadSpritesForSuit(Suit.Hearts, heartSprites);
        LoadSpritesForSuit(Suit.Diamonds, diamondSprites);
        LoadSpritesForSuit(Suit.Clubs, clubSprites);
        LoadSpritesForSuit(Suit.Spades, spadeSprites);
        
        // Pokud nemáme žádné sprites, zkusit flexibilní načítání
        if (spriteCache.Count == 0)
        {
            LoadSpritesFlexibly();
        }
        
        // Pokud stále nemáme sprites, vytvořit fallback sprites
        if (spriteCache.Count == 0)
        {
            CreateFallbackSprites();
        }
        
        
        // Vypsat všechny načtené sprites
    }
    
    void LoadSpritesForSuit(Suit suit, Sprite[] sprites)
    {
        
        if (sprites == null || sprites.Length == 0)
        {
            Debug.LogWarning($"[CardSpriteManager] Žádné sprites pro {suit}");
            return;
        }
        
        Rank[] ranks = { Rank.Seven, Rank.Eight, Rank.Nine, Rank.Ten, 
                             Rank.Jack, Rank.Queen, Rank.King, Rank.Ace };
        
        for (int i = 0; i < ranks.Length && i < sprites.Length; i++)
        {
            string key = GetSpriteKey(suit, ranks[i]);
            spriteCache[key] = sprites[i];
        }
    }
    
    string GetSpriteKey(Suit suit, Rank rank)
    {
        return $"{suit}_{rank}";
    }
    
    public Sprite GetCardSprite(Suit suit, Rank rank)
    {
        string key = GetSpriteKey(suit, rank);
        
        if (spriteCache.TryGetValue(key, out Sprite sprite))
        {
            return sprite;
        }
        
        Debug.LogWarning($"[CardSpriteManager] Sprite nenalezen: {key}");
        Debug.LogWarning($"[CardSpriteManager] Dostupné klíče: {string.Join(", ", spriteCache.Keys)}");
        
        // Pokud je cache prázdný, zkusit flexibilní načítání
        if (spriteCache.Count == 0)
        {
            LoadSpritesFlexibly();
            
            // Zkusit znovu najít sprite
            if (spriteCache.TryGetValue(key, out sprite))
            {
                return sprite;
            }
        }
        
        // Pokud nemáme sprite, vytvořit fallback
        sprite = CreateFallbackSprite(suit, rank);
        
        // Uložit do cache pro příště
        spriteCache[key] = sprite;
        
        return sprite;
    }
    
    public Sprite GetCardBackSprite()
    {
        return cardBackSprite;
    }
    
    // Vytvoří sprite karty z jednotlivých symbolů
    public Sprite CreateCardSpriteFromSymbols(Suit suit, Rank rank)
    {
        if (symbolSprites == null || symbolSprites.Length == 0)
        {
            Debug.LogWarning("[CardSpriteManager] Symbol sprites nejsou nastaveny!");
            return null;
        }
        
        // Najít správný symbol pro rank
        int rankIndex = GetRankIndex(rank);
        if (rankIndex < 0 || rankIndex >= symbolSprites.Length)
        {
            Debug.LogWarning($"[CardSpriteManager] Neplatný rank index: {rankIndex}");
            return null;
        }
        
        // Najít symbol pro suit
        Sprite suitSprite = GetSuitSymbol(suit);
        if (suitSprite == null)
        {
            Debug.LogWarning($"[CardSpriteManager] Suit symbol nenalezen: {suit}");
            return null;
        }
        
        // Zde byste vytvořili kombinovaný sprite
        // Pro jednoduchost vrátíme symbol ranku
        return symbolSprites[rankIndex];
    }
    
    int GetRankIndex(Rank rank)
    {
        switch (rank)
        {
            case Rank.Seven: return 0;
            case Rank.Eight: return 1;
            case Rank.Nine: return 2;
            case Rank.Ten: return 3;
            case Rank.Jack: return 4;
            case Rank.Queen: return 5;
            case Rank.King: return 6;
            case Rank.Ace: return 7;
            default: return -1;
        }
    }
    
    Sprite GetSuitSymbol(Suit suit)
    {
        switch (suit)
        {
            case Suit.Hearts: return heartSymbol;
            case Suit.Diamonds: return diamondSymbol;
            case Suit.Clubs: return clubSymbol;
            case Suit.Spades: return spadeSymbol;
            default: return null;
        }
    }
    
    // Automatické načítání spriteů z Resources složky (alternativa)
    [ContextMenu("Load Sprites from Resources")]
    public void LoadSpritesFromResources()
    {
        
        // Příklad struktury: Resources/Cards/Hearts/7.png, Resources/Cards/Hearts/8.png, atd.
        Suit[] suits = { Suit.Hearts, Suit.Diamonds, Suit.Clubs, Suit.Spades };
        Rank[] ranks = { Rank.Seven, Rank.Eight, Rank.Nine, Rank.Ten, 
                             Rank.Jack, Rank.Queen, Rank.King, Rank.Ace };
        
        int loadedCount = 0;
        
        foreach (var suit in suits)
        {
            foreach (var rank in ranks)
            {
                string path = $"Cards/{suit}/{rank}";
                
                Sprite sprite = Resources.Load<Sprite>(path);
                
                if (sprite != null)
                {
                    string key = GetSpriteKey(suit, rank);
                    spriteCache[key] = sprite;
                    loadedCount++;
                }
                else
                {
                    Debug.LogWarning($"[CardSpriteManager] ❌ Sprite nenalezen v Resources: {path}");
                }
            }
        }
        
    }

    // Fallback systém pro vytvoření jednoduchých spriteů
    public Sprite CreateFallbackSprite(Suit suit, Rank rank)
    {
        
        // Vytvořit jednoduchý sprite s textem
        Texture2D texture = new Texture2D(100, 140);
        Color[] pixels = new Color[100 * 140];
        
        // Barva pozadí (bílá)
        Color backgroundColor = Color.white;
        // Barva textu (černá)
        Color textColor = Color.black;
        
        // Barva pro různé barvy karet
        switch (suit)
        {
            case Suit.Hearts:
            case Suit.Diamonds:
                textColor = Color.red;
                break;
            case Suit.Clubs:
            case Suit.Spades:
                textColor = Color.black;
                break;
        }
        
        // Vyplnit pozadí
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = backgroundColor;
        }
        
        
        
        // Aplikovat pixely
        texture.SetPixels(pixels);
        texture.Apply();
        
        // Vytvořit sprite
        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, 100, 140), new Vector2(0.5f, 0.5f));

        
        return sprite;
    }

    // Vytvořit všechny fallback sprites
    void CreateFallbackSprites()
    {
        
        Suit[] suits = { Suit.Hearts, Suit.Diamonds, Suit.Clubs, Suit.Spades };
        Rank[] ranks = { Rank.Seven, Rank.Eight, Rank.Nine, Rank.Ten, 
                             Rank.Jack, Rank.Queen, Rank.King, Rank.Ace };
        
        foreach (var suit in suits)
        {
            foreach (var rank in ranks)
            {
                string key = GetSpriteKey(suit, rank);
                if (!spriteCache.ContainsKey(key))
                {
                    Sprite fallbackSprite = CreateFallbackSprite(suit, rank);
                    spriteCache[key] = fallbackSprite;
                }
            }
        }
        
    }

    // Debug metoda pro kontrolu nastavení
    [ContextMenu("Debug Settings")]
    public void DebugSettings()
    {
    }

    // Kontrola Resources složky
    [ContextMenu("Check Resources Folder")]
    public void CheckResourcesFolder()
    {
        
        // Zkontrolovat existenci Resources složky
        Object[] allResources = Resources.LoadAll("");
        
        // Najít všechny sprites
        Sprite[] allSprites = Resources.LoadAll<Sprite>("");
        
        // Najít sprites v Cards složce
        Sprite[] cardSprites = Resources.LoadAll<Sprite>("Cards");
        
        // Vypsat všechny sprites
        
        // Zkontrolovat strukturu složek
        Suit[] suits = { Suit.Hearts, Suit.Diamonds, Suit.Clubs, Suit.Spades };
        foreach (var suit in suits)
        {
            Sprite[] suitSprites = Resources.LoadAll<Sprite>($"Cards/{suit}");
        }
        
    }

    // Flexibilní načítání spriteů z různých cest
    [ContextMenu("Load Sprites Flexibly")]
    public void LoadSpritesFlexibly()
    {
        
        // Nejdříve zkontrolovat co je v Resources
        Sprite[] allSprites = Resources.LoadAll<Sprite>("");
        
        if (allSprites.Length == 0)
        {
            Debug.LogWarning("[CardSpriteManager] ❌ Žádné sprites v Resources složce!");
            return;
        }
        
        // Vypsat všechny sprites
        
        Suit[] suits = { Suit.Hearts, Suit.Diamonds, Suit.Clubs, Suit.Spades };
        Rank[] ranks = { Rank.Seven, Rank.Eight, Rank.Nine, Rank.Ten, 
                             Rank.Jack, Rank.Queen, Rank.King, Rank.Ace };
        
        int loadedCount = 0;
        
        // Možné cesty pro načítání (upravené pro váš formát)
        string[] possiblePaths = {
            "Cards/{suit}_{rank}",              // Cards/clubs_7, Cards/diamonds_Q
            "Cards/{suit}_{rank}.png",          // Cards/clubs_7.png, Cards/diamonds_Q.png
            "Cards/{rank}_{suit}",              // Cards/7_clubs, Cards/Q_diamonds
            "Cards/{rank}_{suit}.png",          // Cards/7_clubs.png, Cards/Q_diamonds.png
            "Cards/{suit}/{rank}",              // Cards/clubs/7, Cards/diamonds/Q
            "Cards/{suit}/{rank}.png",          // Cards/clubs/7.png, Cards/diamonds/Q.png
            "Sprites/Cards/{suit}_{rank}",      // Sprites/Cards/clubs_7
            "Sprites/Cards/{suit}_{rank}.png",  // Sprites/Cards/clubs_7.png
            "Assets/Cards/{suit}_{rank}",       // Assets/Cards/clubs_7
            "Assets/Cards/{suit}_{rank}.png"    // Assets/Cards/clubs_7.png
        };
        
        foreach (var suit in suits)
        {
            foreach (var rank in ranks)
            {
                string key = GetSpriteKey(suit, rank);
                
                // Pokud už máme sprite, přeskočit
                if (spriteCache.ContainsKey(key))
                {
                    continue;
                }
                
                
                // Zkusit různé cesty
                foreach (var pathTemplate in possiblePaths)
                {
                    string path = pathTemplate
                        .Replace("{suit}", suit.ToString().ToLower())
                        .Replace("{rank}", GetRankShortName(rank));
                    
                    
                    Sprite sprite = Resources.Load<Sprite>(path);
                    if (sprite != null)
                    {
                        spriteCache[key] = sprite;
                        loadedCount++;
                        break; // Našli jsme sprite, pokračovat na další kartu
                    }
                }
                
                // Pokud jsme nenašli sprite, zkusit načíst všechny sprites a najít podle jména
                if (!spriteCache.ContainsKey(key))
                {
                    Sprite foundSprite = FindSpriteByName(suit, rank);
                    if (foundSprite != null)
                    {
                        spriteCache[key] = foundSprite;
                        loadedCount++;
                    }
                }
            }
        }
        
    }
    
    // Najít sprite podle jména
    Sprite FindSpriteByName(Suit suit, Rank rank)
    {
        // Načíst všechny sprites z Resources
        Sprite[] allSprites = Resources.LoadAll<Sprite>("");
        
        string suitName = suit.ToString().ToLower();
        string rankName = GetRankShortName(rank).ToLower();
        
        // Možné názvy pro váš formát
        string[] possibleNames = {
            $"{suitName}_{rankName}",           // clubs_7, clubs_a, diamonds_q
            $"{suitName}_{rankName}.png",       // clubs_7.png, clubs_a.png
            $"{rankName}_{suitName}",           // 7_clubs, a_clubs, q_diamonds
            $"{rankName}_{suitName}.png",       // 7_clubs.png, a_clubs.png
            $"{suitName}{rankName}",            // clubs7, clubsa, diamondsq
            $"{rankName}{suitName}",            // 7clubs, aclubs, qdiamonds
            $"{suitName}_{rankName}.png",       // clubs_7.png, clubs_a.png
            $"{rankName}_{suitName}.png"        // 7_clubs.png, a_clubs.png
        };
        
        
        foreach (var sprite in allSprites)
        {
            string spriteName = sprite.name.ToLower();
            
            foreach (var possibleName in possibleNames)
            {
                if (spriteName == possibleName)
                {
                    return sprite;
                }
            }
        }
        
        Debug.LogWarning($"[CardSpriteManager] ❌ Sprite nenalezen pro {suit}_{rank}");
        return null;
    }
    
    // Převést rank na krátký název (7, 8, 9, 10, J, Q, K, A)
    string GetRankShortName(Rank rank)
    {
        switch (rank)
        {
            case Rank.Seven: return "7";
            case Rank.Eight: return "8";
            case Rank.Nine: return "9";
            case Rank.Ten: return "10";
            case Rank.Jack: return "J";
            case Rank.Queen: return "Q";
            case Rank.King: return "K";
            case Rank.Ace: return "A";
            default: return rank.ToString();
        }
    }

    // Test formátu názvů
    [ContextMenu("Test Name Format")]
    public void TestNameFormat()
    {
        
        Suit[] suits = { Suit.Hearts, Suit.Diamonds, Suit.Clubs, Suit.Spades };
        Rank[] ranks = { Rank.Seven, Rank.Eight, Rank.Nine, Rank.Ten, 
                             Rank.Jack, Rank.Queen, Rank.King, Rank.Ace };
        
        foreach (var suit in suits)
        {
            foreach (var rank in ranks)
            {
                string expectedName = $"{suit.ToString().ToLower()}_{GetRankShortName(rank)}";
            }
        }
        
        Sprite[] allSprites = Resources.LoadAll<Sprite>("");
        
    }
    
    // Zobrazit všechny sprites v Resources
    [ContextMenu("Show All Resources Sprites")]
    public void ShowAllResourcesSprites()
    {
        
        Sprite[] allSprites = Resources.LoadAll<Sprite>("");
        
        
    }
}
