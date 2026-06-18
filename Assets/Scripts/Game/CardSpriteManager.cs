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
    [SerializeField] private Sprite[] symbolSprites;   // All symbols in order: 7,8,9,10,J,Q,K,A
    [SerializeField] private Sprite heartSymbol;
    [SerializeField] private Sprite diamondSymbol;
    [SerializeField] private Sprite clubSymbol;
    [SerializeField] private Sprite spadeSymbol;
    
    [Header("Card Back")]
    [SerializeField] private Sprite cardBackSprite;
    
    private Dictionary<string, Sprite> spriteCache = new Dictionary<string, Sprite>();
    
    public static CardSpriteManager Instance { get; private set; }
    
    // Static method to ensure instance exists
    public static CardSpriteManager EnsureInstance()
    {
        if (Instance == null)
        {
            // Look for existing instance
            Instance = FindObjectOfType<CardSpriteManager>();
            
            // If none exists, create a new one
            if (Instance == null)
            {
                GameObject go = new GameObject("CardSpriteManager");
                Instance = go.AddComponent<CardSpriteManager>();
                DontDestroyOnLoad(go); // Keep across scenes
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
        
        // Load sprites for each suit
        LoadSpritesForSuit(Suit.Hearts, heartSprites);
        LoadSpritesForSuit(Suit.Diamonds, diamondSprites);
        LoadSpritesForSuit(Suit.Clubs, clubSprites);
        LoadSpritesForSuit(Suit.Spades, spadeSprites);
        
        // If we have no sprites, try flexible loading
        if (spriteCache.Count == 0)
        {
            LoadSpritesFlexibly();
        }
        
        // If still no sprites, create fallback sprites
        if (spriteCache.Count == 0)
        {
            CreateFallbackSprites();
        }
        
        
        // Log all loaded sprites
    }
    
    void LoadSpritesForSuit(Suit suit, Sprite[] sprites)
    {
        
        if (sprites == null || sprites.Length == 0)
        {
            Debug.LogWarning($"[CardSpriteManager] No sprites for {suit}");
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
        
        Debug.LogWarning($"[CardSpriteManager] Sprite not found: {key}");
        Debug.LogWarning($"[CardSpriteManager] Available keys: {string.Join(", ", spriteCache.Keys)}");
        
        // If cache is empty, try flexible loading
        if (spriteCache.Count == 0)
        {
            LoadSpritesFlexibly();
            
            // Try finding sprite again
            if (spriteCache.TryGetValue(key, out sprite))
            {
                return sprite;
            }
        }
        
        // If no sprite, create fallback
        sprite = CreateFallbackSprite(suit, rank);
        
        // Store in cache for next time
        spriteCache[key] = sprite;
        
        return sprite;
    }
    
    public Sprite GetCardBackSprite()
    {
        return cardBackSprite;
    }
    
    // Builds card sprite from individual symbols
    public Sprite CreateCardSpriteFromSymbols(Suit suit, Rank rank)
    {
        if (symbolSprites == null || symbolSprites.Length == 0)
        {
            Debug.LogWarning("[CardSpriteManager] Symbol sprites are not set!");
            return null;
        }
        
        // Find correct symbol for rank
        int rankIndex = GetRankIndex(rank);
        if (rankIndex < 0 || rankIndex >= symbolSprites.Length)
        {
            Debug.LogWarning($"[CardSpriteManager] Invalid rank index: {rankIndex}");
            return null;
        }
        
        // Find symbol for suit
        Sprite suitSprite = GetSuitSymbol(suit);
        if (suitSprite == null)
        {
            Debug.LogWarning($"[CardSpriteManager] Suit symbol not found: {suit}");
            return null;
        }
        
        // Here you would build a combined sprite
        // For simplicity return the rank symbol
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
    
    // Automatic sprite loading from Resources folder (alternative)
    [ContextMenu("Load Sprites from Resources")]
    public void LoadSpritesFromResources()
    {
        
        // Example structure: Resources/Cards/Hearts/7.png, Resources/Cards/Hearts/8.png, etc.
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
                    Debug.LogWarning($"[CardSpriteManager] ❌ Sprite not found in Resources: {path}");
                }
            }
        }
        
    }

    // Fallback system for creating simple sprites
    public Sprite CreateFallbackSprite(Suit suit, Rank rank)
    {
        
        // Create simple sprite with text
        Texture2D texture = new Texture2D(100, 140);
        Color[] pixels = new Color[100 * 140];
        
        // Background colour (white)
        Color backgroundColor = Color.white;
        // Text colour (black)
        Color textColor = Color.black;
        
        // Colour for different card suits
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
        
        // Fill background
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = backgroundColor;
        }
        
        
        
        // Apply pixels
        texture.SetPixels(pixels);
        texture.Apply();
        
        // Create sprite
        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, 100, 140), new Vector2(0.5f, 0.5f));

        
        return sprite;
    }

    // Create all fallback sprites
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

    // Debug method to check setup
    [ContextMenu("Debug Settings")]
    public void DebugSettings()
    {
    }

    // Check Resources folder
    [ContextMenu("Check Resources Folder")]
    public void CheckResourcesFolder()
    {
        
        // Check Resources folder exists
        Object[] allResources = Resources.LoadAll("");
        
        // Find all sprites
        Sprite[] allSprites = Resources.LoadAll<Sprite>("");
        
        // Find sprites in Cards folder
        Sprite[] cardSprites = Resources.LoadAll<Sprite>("Cards");
        
        // Log all sprites
        
        // Check folder structure
        Suit[] suits = { Suit.Hearts, Suit.Diamonds, Suit.Clubs, Suit.Spades };
        foreach (var suit in suits)
        {
            Sprite[] suitSprites = Resources.LoadAll<Sprite>($"Cards/{suit}");
        }
        
    }

    // Flexible sprite loading from various paths
    [ContextMenu("Load Sprites Flexibly")]
    public void LoadSpritesFlexibly()
    {
        
        // First check what is in Resources
        Sprite[] allSprites = Resources.LoadAll<Sprite>("");
        
        if (allSprites.Length == 0)
        {
            Debug.LogWarning("[CardSpriteManager] ❌ No sprites in the Resources folder!");
            return;
        }
        
        // Log all sprites
        
        Suit[] suits = { Suit.Hearts, Suit.Diamonds, Suit.Clubs, Suit.Spades };
        Rank[] ranks = { Rank.Seven, Rank.Eight, Rank.Nine, Rank.Ten, 
                             Rank.Jack, Rank.Queen, Rank.King, Rank.Ace };
        
        int loadedCount = 0;
        
        // Possible load paths (adjusted for your format)
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
                
                // If we already have the sprite, skip
                if (spriteCache.ContainsKey(key))
                {
                    continue;
                }
                
                
                // Try various paths
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
                        break; // Found the sprite, continue to the next card
                    }
                }
                
                // If sprite not found, load all sprites and find by name
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
    
    // Find sprite by name
    Sprite FindSpriteByName(Suit suit, Rank rank)
    {
        // Load all sprites from Resources
        Sprite[] allSprites = Resources.LoadAll<Sprite>("");
        
        string suitName = suit.ToString().ToLower();
        string rankName = GetRankShortName(rank).ToLower();
        
        // Possible names for your format
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
        
        Debug.LogWarning($"[CardSpriteManager] ❌ Sprite not found for {suit}_{rank}");
        return null;
    }
    
    // Convert rank to short name (7, 8, 9, 10, J, Q, K, A)
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

    // Test name format
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
    
    // Show all sprites in Resources
    [ContextMenu("Show All Resources Sprites")]
    public void ShowAllResourcesSprites()
    {
        
        Sprite[] allSprites = Resources.LoadAll<Sprite>("");
        
        
    }
}
