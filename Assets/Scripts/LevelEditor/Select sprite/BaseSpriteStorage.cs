using UnityEngine;

namespace TimeLine
{
    public class BaseSpriteStorage : MonoBehaviour
    {
        private System.Collections.Generic.Dictionary<string, Sprite> _spriteCache;
        private static BaseSpriteStorage _instance;
        public static BaseSpriteStorage Instance
        {
            get
            {
                if (_instance == null)
                {
                    // Пытаемся найти существующий объект на сцене
                    _instance = FindFirstObjectByType<BaseSpriteStorage>();

                    // Если не найден — создаём временный объект (только в рантайме!)
                    if (_instance == null && Application.isPlaying)
                    {
                        GameObject obj = new GameObject("BaseSpriteStorage");
                        _instance = obj.AddComponent<BaseSpriteStorage>();
                    }
                }
                return _instance;
            }
        }

        [SerializeField] private Sprite[] sprites;
        public Sprite[] Sprites => sprites;

        private void Awake()
        {
            // Защита от дубликатов
            if (_instance == null)
            {
                _instance = this;
            }
            else if (_instance != this)
            {
                // Уничтожаем дубликат
                Destroy(gameObject);
            }
            
            BuildCache();
        }
        
        private void BuildCache()
        {
            _spriteCache = new System.Collections.Generic.Dictionary<string, Sprite>();
            foreach (var sprite in sprites)
            {
                if (sprite != null)
                {
                    _spriteCache[sprite.name] = sprite;
                }
            }
        }
    
        public Sprite GetSprite(string name)
        {
            if (_spriteCache != null && _spriteCache.TryGetValue(name, out Sprite sprite))
            {
                return sprite;
            }
            return null;
        }
    }
    
    
}