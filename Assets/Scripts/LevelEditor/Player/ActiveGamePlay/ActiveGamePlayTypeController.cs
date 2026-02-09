using System;
using NaughtyAttributes;
using TimeLine.LevelEditor.Player.PlayerMove.PlayerFreeMove;
using UnityEngine;
using Zenject;

namespace TimeLine.LevelEditor.Player.ActiveGamePlay
{
    public class ActiveGamePlayTypeController : MonoBehaviour
     {
        [SerializeField] private PlayerPlatformerModel _platformerModel;
        [SerializeField] private PlayerFreeMoveModel _freeMoveModel;
        
        private PlayerFreeMoveController _freeMove;
        private PlayerPlatformerController _platformer;

        private void Start()
        {
            FreeMove();
        }

        [Inject]
        private void Construct(PlayerFreeMoveController freeMove, PlayerPlatformerController platformer)
        {
            _freeMove = freeMove;
            _platformer = platformer;
        }

        [Button]
        private void FreeMove()
        {
            _platformer.Disable();
            _freeMove.Enable(_freeMoveModel);
        }
        
        [Button]
        private void Platformer()
        {
            _freeMove.Disable();
            _platformer.Enable(_platformerModel);
        }
    }
}