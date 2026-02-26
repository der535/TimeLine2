// using System.Collections.Generic;
// using EventBus;
// using TimeLine;
// using TimeLine.EventBus.Events.TrackObject;
// using TMPro;
// using UnityEngine;
// using Zenject;
//
// public class ParentMain : MonoBehaviour
// {
//     [SerializeField] private TMP_Dropdown _allObjects;
//     [SerializeField] private GameObject _panel;
//
//     private GameEventBus _gameEventBus;
//     private TrackObjectStorage _trackObjectStorage;
//     private SelectObjectController _selectObjectController;
//
//     // Список для сопоставления индекса Dropdown с данными объекта
//     private List<TrackObjectPacket> _dropdownReferenceData = new();
//
//     // Словарь для быстрого доступа (имя/id -> данные)
//     private Dictionary<string, TrackObjectPacket> _parentObjects = new();
//
//     [Inject]
//     private void Constructor(GameEventBus gameEventBus,
//         TrackObjectStorage trackObjectStorage,
//         SelectObjectController selectObjectController)
//     {
//         _gameEventBus = gameEventBus;
//         _trackObjectStorage = trackObjectStorage;
//         _selectObjectController = selectObjectController;
//     }
//
//     // private void Start()
//     // {
//     //     _panel.SetActive(false);
//     //     
//     //     // Подписка на событие шины
//     //     _gameEventBus.SubscribeTo((ref SelectObjectEvent data) =>
//     //     {
//     //         UpdateDropdown(data.Tracks);
//     //         _panel.SetActive(true);
//     //     });
//     //
//     //     _gameEventBus.SubscribeTo((ref DeselectAllObjectEvent eventData) => { _panel.SetActive(false); });
//     //     _gameEventBus.SubscribeTo((ref DeselectObjectEvent eventData) =>
//     //     {
//     //         if (eventData.SelectedObjects.Count <= 0)
//     //             _panel.SetActive(false);
//     //     });
//     //
//     //     // Подписка на изменение значения в самом UI
//     //     _allObjects.onValueChanged.AddListener(OnDropdownValueChanged);
//     // }
//
//     private void UpdateDropdown(List<TrackObjectPacket> selectedTracks)
//     {
//         // 1. Определяем общий ID среди выделенных объектов
//         string generalID = string.Empty;
//         bool isFirst = true;
//
//         foreach (var track in selectedTracks)
//         {
//             string currentID = track.trackObject.trackObject._parentID;
//             if (isFirst)
//             {
//                 generalID = currentID;
//                 isFirst = false;
//             }
//             else if (generalID != currentID)
//             {
//                 generalID = "---";
//                 break;
//             }
//         }
//
//         // 2. Очищаем старые данные
//         // Отключаем листенер, чтобы программная смена значения не вызывала OnDropdownValueChanged
//         _allObjects.onValueChanged.RemoveListener(OnDropdownValueChanged);
//
//         _allObjects.ClearOptions();
//         _dropdownReferenceData.Clear();
//         _parentObjects.Clear();
//
//         List<string> options = new List<string>();
//
//         // 3. Добавляем системные опции
//         options.Add("Empty");
//         _dropdownReferenceData.Add(null);
//
//         if (generalID == "---")
//         {
//             options.Add("---");
//             _dropdownReferenceData.Add(null);
//         }
//
//         // 4. Фильтруем и добавляем доступные объекты
//         var allActiveTracks = _trackObjectStorage.GetAllActiveTrackData();
//         // Используем HashSet для быстрой проверки исключений
//         var selectedSet = new HashSet<TrackObjectPacket>(_selectObjectController.SelectObjects);
//
//         foreach (var trackData in allActiveTracks)
//         {
//             // Убираем те, что сейчас выделены (нельзя быть родителем самому себе)
//             if (selectedSet.Contains(trackData))
//                 continue;
//
//             string objName = trackData.sceneObject.name;
//             options.Add(objName);
//             _dropdownReferenceData.Add(trackData);
//             _parentObjects[objName] = trackData;
//         }
//
//         _allObjects.AddOptions(options);
//
//         // 5. Устанавливаем текущий выбранный индекс
//         int targetIndex = 0; // По умолчанию Empty
//         if (generalID == "---")
//         {
//             targetIndex = 1;
//         }
//         else if (!string.IsNullOrEmpty(generalID))
//         {
//             // Ищем индекс объекта в созданном списке по сохраненному ID
//             for (int i = 0; i < _dropdownReferenceData.Count; i++)
//             {
//                 var refData = _dropdownReferenceData[i];
//                 if (refData != null && refData.sceneObjectID == generalID)
//                 {
//                     targetIndex = i;
//                     break;
//                 }
//             }
//         }
//
//         _allObjects.value = targetIndex;
//         _allObjects.RefreshShownValue();
//
//         // Включаем листенер обратно
//         _allObjects.onValueChanged.AddListener(OnDropdownValueChanged);
//     }
//
//     private void OnDropdownValueChanged(int index)
//     {
//         if (index < 0 || index >= _dropdownReferenceData.Count) return;
//
//         TrackObjectPacket selectedPacket = _dropdownReferenceData[index];
//
//         if (selectedPacket != null)
//         {
//             // ВЫБРАН ОБЪЕКТ
//             Debug.Log($"Родитель изменен на: {selectedPacket.sceneObject.name}");
//             foreach (var track in _selectObjectController.SelectObjects)
//             {
//                 print(selectedPacket.sceneObjectID);
//                 track.sceneObject.transform.SetParent(selectedPacket.sceneObject.transform);
//                 track.trackObject.trackObject._parentID = selectedPacket.sceneObjectID;
//             }
//             // Здесь можно отправить событие в шину о смене родителя для всех выделенных
//         }
//         else
//         {
//             string text = _allObjects.options[index].text;
//             if (text == "Empty")
//             {
//                 Debug.Log("Родитель сброшен (Empty)");
//                 foreach (var track in _selectObjectController.SelectObjects)
//                 {
//                     track.sceneObject.transform.SetParent(null);
//                     track.trackObject.trackObject._parentID = string.Empty;
//                 }
//             }
//             else if (text == "---")
//             {
//                 Debug.Log("Смешанное состояние, действие не требуется");
//             }
//         }
//     }
//
//     private void OnDestroy()
//     {
//         _allObjects.onValueChanged.RemoveListener(OnDropdownValueChanged);
//     }
// }