using System;
using System.Collections.Generic;
using TimeLine.CustomInspector.Logic.Parameter;
using UnityEngine;

namespace TimeLine
{
    public class SelectSpriteController : MonoBehaviour
    {
        [SerializeField] private RectTransform windows;
        [SerializeField] private SpriteCardSO[] baseCollection;
        [SerializeField] private SpriteCard prefab;
        [SerializeField] private RectTransform content;

        private List<SpriteCard> _spriteCards = new List<SpriteCard>();
        private bool _isInitialized = false;

        private void InitializeCards()
        {
            if (_isInitialized) return;

            foreach (var card in baseCollection)
            {
                SpriteCard spriteCard = Instantiate(prefab, content);
                spriteCard.Setup(card, null); // Изначально без действия
                _spriteCards.Add(spriteCard);
            }

            _isInitialized = true;
        }

        internal void Setup(SpriteParameter spriteParameter)
        {
            InitializeCards(); // Создаём карточки при первом вызове

            windows.gameObject.SetActive(true);

            // Теперь просто обновляем action для каждой карточки
            for (int i = 0; i < _spriteCards.Count; i++)
            {
                int index = i; // Замыкание для правильного захвата индекса
                _spriteCards[index].Setup(baseCollection[index], () =>
                {
                    spriteParameter.Value = baseCollection[index].sprite;
                    windows.gameObject.SetActive(false);
                });
            }
        }
    }
}