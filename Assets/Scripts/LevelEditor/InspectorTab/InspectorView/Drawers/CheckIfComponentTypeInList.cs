using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Entities;

namespace TimeLine.LevelEditor.InspectorTab.InspectorView.Drawers
{
    public static class CheckIfComponentTypeInList
    {
        /// <summary>
        /// Проверяет есть ли в списке iterableList компонент verifiable
        /// </summary>
        /// <param name="iterableList">Проверяемый список</param>
        /// <param name="verifiable">Значение которое ищется в списке</param>
        /// <returns>Результат проверки</returns>
        internal static bool Check(List<ComponentType> iterableList, ComponentType verifiable)
        {
            return iterableList.Exists(x => x.GetManagedType() == verifiable);
        }

        internal static bool Check(Entity entity, ComponentType verifiable)
        {
            EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            using NativeArray<ComponentType> types = entityManager.GetComponentTypes(entity);

            return Check(types.ToList(), verifiable);
        }

        internal static bool Check(Entity entity, List<ComponentType> verifiable)
        {
            EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            using NativeArray<ComponentType> types = entityManager.GetComponentTypes(entity);

            return Check(types.ToList(), verifiable);
        }


        /// <summary>
        ///  Проверяет есть ли в списке iterableList компоненты из списка verifiableList
        /// </summary>
        /// <param name="iterableList">Проверяемый список</param>
        /// <param name="verifiableList">Компоненты которые должны быть в iterableList</param>
        /// <returns>Результат проверки</returns>
        internal static bool Check(List<ComponentType> iterableList, List<ComponentType> verifiableList)
        {
            if (verifiableList == null) return false;

            foreach (var verifiable in verifiableList)
            {
                if (!Check(iterableList, verifiable)) return false;
            }

            return true;
        }
    }
}