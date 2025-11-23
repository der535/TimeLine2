using UnityEngine;

namespace TimeLine
{
    [CreateAssetMenu(fileName = "Sprite Card", menuName = "ScriptableObjects/Sprite Card")]
    public class SpriteCardSO: ScriptableObject
    {
        public Sprite sprite;
        public string name;
    }
}