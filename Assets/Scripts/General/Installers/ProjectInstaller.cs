
using TimeLine.TimeLine; 
using UnityEngine; 
using Zenject; 


namespace TimeLine.General.Installers
{

    /// Установщик зависимостей на уровне всего проекта.
    /// Наследуется от MonoInstaller для работы как компонент в сцене Unity.
    /// Отвечает за настройку глобальных зависимостей, которые используются во всем проекте.
   
    public class ProjectInstaller : MonoInstaller
    {
        // Сериализуемое поле для ссылки на объект FontStorage в инспекторе Unity
        // Позволяет перетащить префаб или объект FontStorage из сцены
        [SerializeField] private FontStorage fontStorage;


        /// Вызывается автоматически Zenject при инициализации контекста.
       
        public override void InstallBindings()
        {
            // Регистрируем FontStorage в DI-контейнере
            // FromInstance(fontStorage) - использует конкретный экземпляр из сцены
            // AsSingle() - гарантирует, что будет создан только один экземпляр (синглтон)
            Container.Bind<FontStorage>().FromInstance(fontStorage).AsSingle();

            // После выполнения этой привязки:
            // 1. Любой класс с [Inject] атрибутом получит этот экземпляр FontStorage
            // 2. FontSetter из предыдущего примера получит эту зависимость
            // 3. Все зависимости разрешаются автоматически при создании объектов
        }

        // Примечания по использованию:
        // 1. Этот установщик должен быть добавлен в сцену как компонент GameObject
        // 2. В инспекторе нужно перетащить объект FontStorage в поле fontStorage
        // 3. Установщик выполняется при запуске сцены, до инициализации других объектов

        // Альтернативные способы привязки для сравнения:
        // 
        // Container.Bind<FontStorage>().To<FontStorage>().AsSingle();
        //   - Создает новый экземпляр через конструктор
        //   
        // Container.Bind<FontStorage>().FromComponentInNewPrefab(fontStoragePrefab).AsSingle();
        //   - Создает из префаба (если fontStorage - префаб)
        //   
        // Container.Bind<FontStorage>().FromComponentInHierarchy().AsSingle();
        //   - Ищет существующий экземпляр в иерархии сцены

        // Преимущества текущего подхода (FromInstance):
        // - Полный контроль над экземпляром из редактора Unity
        // - Возможность настройки через инспектор
        // - Производительность (не создает новый объект)
        // - Поддержка горячей перезагрузки в Editor
    }
}