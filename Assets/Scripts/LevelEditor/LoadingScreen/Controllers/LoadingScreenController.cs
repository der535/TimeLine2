using System;
using System.Collections;
using EventBus;
using TimeLine.LevelEditor.LoadingScreen.Model;
using TimeLine.LevelEditor.LoadingScreen.View;
using UnityEngine;
using Zenject;

namespace TimeLine.LevelEditor.LoadingScreen.Controllers
{
    public class LoadingScreenController : MonoBehaviour
    {
        [SerializeField] private LoadingViewBase _viewBase;
        [SerializeField] private LoadingStepsStorage storage = new();

        private GameEventBus _eventBus;
        
        internal void Clear()
        {
            _viewBase.UpdateUI(0, String.Empty);
            storage.Value.Clear();
        }

        [Inject]
        private void Construct(GameEventBus gameEventBus)
        {
            _eventBus = gameEventBus;
        }

        internal void AddStep(string desc, Action action) => storage.AddStep(desc, action);

        private void AddStep(string desc, Action action, Func<bool> condition) 
            => storage.AddStep(desc, action, condition);
        
        public void AddStep(string desc, Action<Action> task)
        {
            bool isDone = false;
            // Мы вызываем task и передаем ей "рычаг", который переключит isDone
            AddStep(desc, () =>
            {
                task(() =>
                {
                    isDone = true;
                });
            }, () => isDone);
        }
        
        
        internal void StartLoading()
        {
            StartCoroutine(ExecuteLoading());
        }

        private IEnumerator ExecuteLoading()
        {
            _viewBase.ShowLoadingScreen();
            
            float totalSteps = storage.GetCountSteps();

            for (int i = 0; i < storage.GetCountSteps(); i++)
            {
                var step = storage.GetStep(i);

                // 1. Обновляем текст и прогресс
                _viewBase.UpdateUI((float)i / totalSteps, step.Description);

                // 2. Ждем один кадр, чтобы Unity отрисовала изменения на экране
                yield return null;

                // 3. Выполняем ваш обычный метод
                step.Action.Invoke();
            }

            _viewBase.UpdateUI(1, "Загрузка завершена!");
            
            _viewBase.HideLoadingScreen();
            _eventBus.Raise(new LevelLoadedEvent());
        }
    }


}