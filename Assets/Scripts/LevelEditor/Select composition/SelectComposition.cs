using System.Collections.Generic;
using NaughtyAttributes;
using TimeLine.CustomInspector.Logic.Parameter;
using UnityEngine;

namespace TimeLine.LevelEditor.Select_composition
{
    public class SelectComposition : MonoBehaviour
    {
        [SerializeField] private SaveComposition saveComposition;
        [SerializeField] private RectTransform rectTransform;
        [SerializeField] private RectTransform rootCard;
        [SerializeField] private SelectCompositionCard prefabCard;

        private readonly List<SelectCompositionCard> _cards = new();

        [Button]
        public void Setup(CompositionParameter parameter) // передать параметр
        {
            foreach (var card in _cards)
            {
                Destroy(card.gameObject);
            }
            
            foreach (var data in saveComposition.GetCompositionData())
            {
                var card = Instantiate(prefabCard, rootCard);
                card.Setup(data, () =>
                {
                    parameter.Value = data;
                });
                _cards.Add(card);
            }
            rectTransform.gameObject.SetActive(true);
        }
    }
}