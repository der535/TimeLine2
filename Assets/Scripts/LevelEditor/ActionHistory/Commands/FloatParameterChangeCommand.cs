// using System.Linq;
// using TimeLine.CustomInspector.Logic.Parameter;
//
// namespace TimeLine.LevelEditor.ActionHistory.Commands
// {
//     /// <summary>
//     /// Команда изменений Float
//     /// </summary>
//     public class FloatParameterChangeCommand : ICommand
//     {
//         private readonly FloatParameter _floatParameter;
//
//         private readonly float _valueBeforeChange;
//         private readonly float _valueAfterChange;
//
//         private readonly string _objectId;
//
//         private readonly string _description;
//
//         private readonly TrackObjectStorage _objectStorage;
//
//         /// <summary>
//         /// Конструктор команды изменения float параметра
//         /// </summary>
//         /// <param name="objectStorage">Хранилище объектов для восстановления ссылок</param>
//         /// <param name="floatParameter">Изменяемый float параметр</param>
//         /// <param name="description">Описание изменения для истории</param>
//         /// <param name="objectID">Уникальный идентификатор связанного объекта</param>
//         /// <param name="valueBeforeChange">Значение параметра до изменения</param>
//         /// <param name="valueAfterChange">Значение параметра после изменения</param>
//         public FloatParameterChangeCommand(TrackObjectStorage objectStorage, FloatParameter floatParameter, string description,
//             string objectID, float valueBeforeChange, float valueAfterChange)
//         {
//             _floatParameter = floatParameter;
//             _description = description;
//             _valueBeforeChange = valueBeforeChange;
//             _valueAfterChange = valueAfterChange;
//             _objectId = objectID;
//             _objectStorage = objectStorage;
//         }
//
//         /// <summary>
//         /// Возвращает описание команды для отображения в UI истории
//         /// </summary>
//         public string Description() => _description;
//         
//         public void Execute()
//         {
//             _floatParameter.Value = _valueAfterChange;
//         }
//         
//         public void Undo()
//         {
//             // Если ссылка на параметр все еще действительна, просто отменяем изменение
//             if (_floatParameter != null)
//                 _floatParameter.Value = _valueBeforeChange;
//             else
//             {
//                 // Если ссылка утеряна 
//                 // находим параметр через хранилище объектов
//                 var components = _objectStorage.FindObjectByID(_objectId).sceneObject
//                     .GetComponents<BaseParameterComponent>();
//
//                 // Ищем параметр с соответствующим ID среди всех компонентов
//                 foreach (var component in components)
//                 {
//                     component.GetParameterData().ToList().Find(x => x.Value.Id == _objectId).Value.Value =
//                         _valueAfterChange;
//                 }
//             }
//         }
//     }
// }