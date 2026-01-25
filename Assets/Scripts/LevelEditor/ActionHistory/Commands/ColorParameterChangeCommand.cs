using System.Linq;
using TimeLine.CustomInspector.Logic.Parameter;
using UnityEngine;

namespace TimeLine.LevelEditor.ActionHistory.Commands
{
    /// <summary>
    /// Команда смены цвета
    /// </summary>
    public class ColorParameterChangeCommand : ICommand
    {
        private readonly ColorParameter _colorParameter;
        
        private readonly Color _valueBeforeChange;
        private readonly Color _valueAfterChange;

        private readonly string _objectId;

        private readonly string _description;

        private readonly TrackObjectStorage _objectStorage;

        /// <summary>
        /// Конструктор команды изменения цветового параметра
        /// </summary>
        /// <param name="objectStorage">Хранилище объектов для восстановления ссылок</param>
        /// <param name="colorParameter">Изменяемый цветовой параметр</param>
        /// <param name="description">Описание изменения для истории</param>
        /// <param name="objectID">Уникальный идентификатор связанного объекта</param>
        /// <param name="valueBeforeChange">Значение параметра до изменения</param>
        /// <param name="valueAfterChange">Значение параметра после изменения</param>
        public ColorParameterChangeCommand(TrackObjectStorage objectStorage, ColorParameter colorParameter, string description,
            string objectID, Color valueBeforeChange, Color valueAfterChange)
        {
            _colorParameter = colorParameter;
            _description = description;
            _valueBeforeChange = valueBeforeChange;
            _valueAfterChange = valueAfterChange;
            _objectId = objectID;
            _objectStorage = objectStorage;
        }

        /// <summary>
        /// Возвращает описание команды
        /// </summary>
        public string Description() => _description;
        
        public void Execute()
        {
            _colorParameter.Value = _valueAfterChange;
        }

        /// <summary>FloatParameterChangeCommand 
        /// Отменяет выполненную команду
        /// </summary>
        public void Undo()
        {
            // Если ссылка на параметр все еще действительна, просто отменяем изменение
            if (_colorParameter != null)
                _colorParameter.Value = _valueBeforeChange;
            else
            {
                // Если ссылка утеряна
                // находим параметр через хранилище объектов
                var components = _objectStorage.FindObjectByID(_objectId).sceneObject
                    .GetComponents<BaseParameterComponent>();

                // Ищем параметр с соответствующим ID среди всех компонентов
                foreach (var component in components)
                {
                    component.GetParameterData().ToList().Find(x => x.Value.Id == _objectId).Value.Value =
                        _valueAfterChange; 
                }
            }
        }
    }
}