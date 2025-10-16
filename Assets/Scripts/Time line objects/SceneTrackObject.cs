// using UnityEngine;
//
// namespace TimeLine
// {
//     public class SceneTrackObject : MonoBehaviour
//     {
//         [SerializeField] private SpriteRenderer spriteRenderer;
//         public Sprite Sprite { get => spriteRenderer.sprite; set => spriteRenderer.sprite = value; }
//         public string name { get => gameObject.name; set => gameObject.name = value; }
//
//         internal void Setup(Sprite sprite, string name)
//         {
//             spriteRenderer.sprite = sprite;
//             gameObject.name = name;
//         }
//
//         public TrackObjectSO Copy() => _trackObjectSO;
//     }
// }
