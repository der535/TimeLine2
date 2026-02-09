using UnityEngine;

namespace TimeLine
{
    public class GroundCheckController : MonoBehaviour
    {
        public Transform playerTransform;
        public Vector2 boxSize = new Vector2(0.5f, 0.1f);
        // Расстояние от центра игрока до зоны проверки
        [SerializeField] private float _checkDistance = 1f; 
        public LayerMask groundLayer;

        // Вспомогательный метод для получения оффсета на основе направления
        private Vector3 GetOffset(GravitationDirection direction)
        {
            return direction switch
            {
                GravitationDirection.Down  => new Vector3(0, -_checkDistance, 0),
                GravitationDirection.Up    => new Vector3(0, _checkDistance, 0),
                GravitationDirection.Left  => new Vector3(-_checkDistance, 0, 0),
                GravitationDirection.Right => new Vector3(_checkDistance, 0, 0),
                _ => Vector3.down
            };
        }

        public bool IsGrounded(GravitationDirection gravitationDirection)
        {
            Vector3 currentOffset = GetOffset(gravitationDirection);
            
            // Если гравитация горизонтальная (Left/Right), нам нужно поменять местами 
            // ширину и высоту бокса, чтобы проверка оставалась плоской вдоль стены
            Vector2 finalBoxSize = boxSize;
            if (gravitationDirection == GravitationDirection.Left || gravitationDirection == GravitationDirection.Right)
            {
                finalBoxSize = new Vector2(boxSize.y, boxSize.x);
            }

            return Physics2D.OverlapBox(playerTransform.position + currentOffset, finalBoxSize, 0f, groundLayer);
        }

        private void OnDrawGizmos()
        {
            if (playerTransform == null) return;
            
            Gizmos.color = Color.cyan;
            // Рисуем для всех направлений или только для Down по умолчанию для дебага
            // Для красоты можно вызывать GetOffset(GravitationDirection.Down)
            Gizmos.DrawWireCube(playerTransform.position + GetOffset(GravitationDirection.Down), boxSize);
        }
    }
}