using UnityEngine;

namespace TimeLine.LevelEditor.Player
{
    public class PlayerPosition : MonoBehaviour
    {
        [SerializeField] private GameObject player;

        internal Vector3 GetPosition() => player.transform.position;
    }
}