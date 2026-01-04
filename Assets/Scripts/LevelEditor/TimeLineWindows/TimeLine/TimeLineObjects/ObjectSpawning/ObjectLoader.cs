using System.Collections.Generic;
using System.Linq;
using TimeLine.Components;
using TimeLine.Keyframe;
using TimeLine.Parent;
using UnityEngine;
using Zenject;

namespace TimeLine.LevelEditor.TimeLineWindows.TimeLine.TimeLineObjects.ObjectSpawning
{
    public class ObjectLoader
    {
        private ObjectFactory _objectFactory;
        private TrackStorage _trackStorage;
        private BranchCollection _branchCollection;
        private TrackObjectStorage _trackObjectStorage;
        private KeyframeTrackStorage _keyframeTrackStorage;
        private Main _main;
        private DiContainer _container;
        private SaveComposition _saveComposition;
        private ParentLinkRestorer _parentLinkRestorer;

        public ObjectLoader(ObjectFactory objectFactory, TrackStorage trackStorage,
            BranchCollection branchCollection, TrackObjectStorage trackObjectStorage,
            KeyframeTrackStorage keyframeTrackStorage, Main main, SaveComposition saveComposition,
            DiContainer container, ParentLinkRestorer parentLinkRestorer)
        {
            _objectFactory = objectFactory;
            _trackStorage = trackStorage;
            _branchCollection = branchCollection;
            _trackObjectStorage = trackObjectStorage;
            _keyframeTrackStorage = keyframeTrackStorage;
            _saveComposition = saveComposition;
            _main = main;
            _container = container;
            _parentLinkRestorer = parentLinkRestorer;
        }

        public (TrackObjectData, GameObject, Branch) LoadObject(GameObjectSaveData data,
            bool addToStorage = true)
        {
            string id = UniqueIDGenerator.GenerateUniqueID();

            // Создаем сценный объект
            GameObject sceneObject = _objectFactory.CreateSceneObject();

            // Создаем трек-объект
            TrackObject trackObject;
            trackObject = _objectFactory.CreateTrackObject(data.duractionTime, data.gameObjectName,
                _trackStorage.GetTrackLineByIndex(data.lineIndex), data.startTime);

            // Создаем ветку
            Branch branch = _branchCollection.AddBranch(id, data.branch.Name);

            // Добавляем ноды в ветку
            foreach (var node in data.branch.Nodes)
            {
                branch.AddNode(node.Path);
            }

            TrackObjectData trackObjectData;

            //Условие где мы добовляем в хранилище трекобжект или нет.
            //Не добовление может использоваться только в случае если мы сохдаём трек обжект который будет внутри группы
            if (addToStorage)
            {
                // Добавляем в хранилище
                trackObjectData =
                    _trackObjectStorage.Add(sceneObject.gameObject, trackObject, branch, data.sceneObjectID);
            }
            else
            {
                trackObjectData = new TrackObjectData(sceneObject.gameObject, trackObject, branch, data.sceneObjectID);
            }

            //Добавляем необходимые компоненты
            foreach (var component in data.Components)
            {
                IParameterComponent parameterComponent =
                    (IParameterComponent)ComponentRules.GetOrAddComponentSafely(component.ComponentType, sceneObject,
                        _container);
                parameterComponent.SetParameterData(component.Parameters);
            }

            // Добавляем трек и ключевые кадры
            foreach (var track in data.tracks)
            {
                Track trackm = new Track(sceneObject, track.branchPath, track.animationColor);
                _keyframeTrackStorage.AddTrack(branch.FindNode(track.branchPath), trackm, trackObjectData.trackObject);

                foreach (var saveData in track.keyframeSaveData)
                {
                    Keyframe.Keyframe keyframe = Keyframe.Keyframe.FromSaveData(saveData);
                    trackm.AddKeyframe(keyframe);
                }
            }

            sceneObject.GetComponent<NameComponent>().Name.Value = branch.Name;

            trackObjectData.trackObject._parentID = data.parentObjectID;

            return (trackObjectData, sceneObject, branch);
        }

        /// <summary>
        /// Позволяет загружать композицию из сохранения
        /// </summary>
        /// <param name="data">Сам объект сохранения</param>
        /// <param name="compositionID">Id загружаемой композиции</param>
        /// <param name="compositionData">Сохранение композиции из хранилища композиций, нужен потому что в основном объекте сохранения дочерние объекты не сохраняются</param>
        /// <param name="addToStorage">Флаг обозначающи добовлять ли композицию в обжее хранилище трекобжектов, отключение этой фунции нужно для вложенных композиций друг в друге</param>
        /// <param name="lastEditID">Id последнего редактирования</param>
        /// <returns></returns>
        internal (TrackObjectData, GameObject, Branch) LoadComposition(GroupGameObjectSaveData data,
            string compositionID,
            GroupGameObjectSaveData compositionData = null, bool addToStorage = true, string lastEditID = null)
        {
            var (groupTrackObject, groupGameObject, groupBranch) = LoadObject(data, false); //Загружаем объект группы
            _main.SetTimeInTicks(groupTrackObject.trackObject
                .StartTimeInTicks); //Ставим время тамйлайна на старт группы что бы ничего не сьехало
            List<GameObjectSaveData> children; //Создаём список дочерных обьъектов
            if (compositionData == null)
                children = data.children; //Если один объект сохранения то загружаем из основного
            else
                children = compositionData.children; //Если нет то загружаем из файла композиции

            List<TrackObjectData>
                trackObjectDatas = new List<TrackObjectData>(); //Пустой список дочерних объектов композиции
            foreach (var childData in children.ToList()) //Перебираем детей из списка в сохранении
            {
                //Инициализируем пустые поля
                TrackObjectData childTrackObject = null;
                GameObject childSceneObject = null;
                Branch childBranch = null;

                if (childData is GroupGameObjectSaveData childGroupData) //Если дочерний объект тоже композиция
                {
                    GroupGameObjectSaveData groupChildData = _saveComposition.FindCompositionDataById(childGroupData
                        .compositionID); //Ищем по Id в галереии композиций файл сохранения где лежат сохраннёные дочерние объекты

                    if (groupChildData != null) //Если ничего не нашли, загружаем из основного файла
                    {
                        (childTrackObject, childSceneObject, childBranch) = LoadComposition(childGroupData,
                            childGroupData.compositionID,
                            groupChildData); //Рекурсивная загружка
                    }
                }
                else //Если дочерний объект не композиция
                {
                    (childTrackObject, childSceneObject, childBranch) =
                        LoadObject(childData); //Загружаем обычный объект
                }

                if (childTrackObject != null) //Если дочерний объект не пустой
                {
                    trackObjectDatas.Add(childTrackObject); //Добовляем в список дочерних объектов

                    //Сохраняем трансформ дочернего объекта
                    Vector3 pos = childSceneObject.transform.localPosition;
                    Quaternion rot = childSceneObject.transform.localRotation;
                    Vector3 sca = childSceneObject.transform.localScale;

                    //Делаем парент дочернего объекта
                    childSceneObject.transform.SetParent(groupGameObject.gameObject.transform);

                    //Востанавливаем прежние значения
                    childSceneObject.transform.localPosition = pos;
                    childSceneObject.transform.localRotation = rot;
                    childSceneObject.transform.localScale = sca;

                    foreach (var node in childBranch.Nodes) //Перебираем ноды в ветке анимации
                    {
                        foreach (var node2 in node.Children) //Перебираем ноды в ветке анимации
                        {
                            _keyframeTrackStorage.GetTrack(node2)?.SetParent(groupTrackObject.trackObject);
                        }
                    }
                }
            }

            _parentLinkRestorer.Restor(trackObjectDatas);
            // foreach (var track in trackObjectDatas)
            // {
            //     if(!string.IsNullOrEmpty(track.trackObject._parentID))
            //         track.sceneObject.gameObject.transform.SetParent(trackObjectDatas
            //             .Find(x => x.sceneObjectID == track.trackObject._parentID).sceneObject.transform);
            // }

            if (compositionData == null)
            {
                groupTrackObject.trackObject.Setup((float)data.duractionTime, data.gameObjectName,
                    _trackStorage.GetTrackLineByIndex(data.lineIndex),
                    data.parentObjectID, data.startTime, true);
            }
            else
            {
                groupTrackObject.trackObject.Setup((float)data.duractionTime, compositionData.gameObjectName,
                    _trackStorage.GetTrackLineByIndex(data.lineIndex),
                    data.parentObjectID, data.startTime, true);
                groupTrackObject.trackObject.UpdateDuraction(compositionData.duractionTime);
            }


            string sceneObjectID = UniqueIDGenerator.GenerateUniqueID(); //

            TrackObjectGroup trackObjectGroup =
                _trackObjectStorage.AddGroup(groupGameObject.gameObject, groupTrackObject.trackObject,
                    groupTrackObject.branch, trackObjectDatas,
                    sceneObjectID, compositionID, data.lastEditID, addToStorage); // ???????

            groupGameObject.GetComponent<NameComponent>().Name.Value = groupTrackObject.branch.Name;


            var currentTime = _main.TicksCurrentTime();
            _main.SetTimeInTicks(currentTime);


            return (trackObjectGroup, groupGameObject, groupTrackObject.branch);
        }
    }
}