using UnityEngine;

// Директивы препроцессора для разделения кода на Editor и Runtime части
#if UNITY_EDITOR
using UnityEditor;
#endif


[System.Serializable]
public class SceneField
{
    // Приватное поле для хранения ссылки на SceneAsset (только в Editor)
    [SerializeField]
    private Object m_SceneAsset; // В редакторе это SceneAsset, в runtime - null

    // Приватное поле для хранения имени сцены (работает в runtime)
    [SerializeField]
    private string m_SceneName = "";

    /// Публичное свойство только для чтения, возвращающее имя сцены.
    public string SceneName
    {
        get { return m_SceneName; }
    }


    /// Неявный оператор преобразования SceneField в string.
    /// Позволяет использовать объект SceneField как строку с именем сцены.
 
    /// <param name="sceneField">Экземпляр SceneField для преобразования</param>
    /// <returns>Имя сцены в виде строки</returns>
    public static implicit operator string(SceneField sceneField)
    {
        return sceneField.SceneName;
    }
}

// Вся логика PropertyDrawer находится внутри директивы UNITY_EDITOR
// Это гарантирует, что код не попадет в билд игры
#if UNITY_EDITOR


/// Кастомный PropertyDrawer для отображения SceneField в Unity Inspector.
/// Заменяет стандартное текстовое поле на поле для выбора SceneAsset.

[CustomPropertyDrawer(typeof(SceneField))]
public class SceneFieldPropertyDrawer : PropertyDrawer 
{

    /// Метод отрисовки кастомного поля в Inspector.
 
    /// <param name="_position">Позиция и размеры поля в Inspector</param>
    /// <param name="_property">Сериализованное свойство для отрисовки</param>
    /// <param name="_label">Заголовок поля</param>
    public override void OnGUI(Rect _position, SerializedProperty _property, GUIContent _label)
    {
        // Начинаем блок свойств для корректной обработки Undo/Redo
        EditorGUI.BeginProperty(_position, GUIContent.none, _property);
        
        // Находим дочерние свойства класса SceneField
        SerializedProperty sceneAsset = _property.FindPropertyRelative("m_SceneAsset");
        SerializedProperty sceneName = _property.FindPropertyRelative("m_SceneName");
        
        // Создаем префикс с меткой поля
        _position = EditorGUI.PrefixLabel(_position, GUIUtility.GetControlID(FocusType.Passive), _label);
        
        // Проверяем, что свойство найдено
        if (sceneAsset != null)
        {
            // Создаем ObjectField для выбора SceneAsset
            // Позволяет выбирать только объекты типа SceneAsset
            sceneAsset.objectReferenceValue = EditorGUI.ObjectField(
                _position,                         // Позиция поля
                sceneAsset.objectReferenceValue,   // Текущее значение
                typeof(SceneAsset),                // Разрешенный тип
                false                              // Не разрешать выбор объектов сцены
            ); 

            // Если выбран SceneAsset, обновляем имя сцены
            if (sceneAsset.objectReferenceValue != null)
            {
                // Безопасное приведение типа с использованием null-conditional оператора
                sceneName.stringValue = (sceneAsset.objectReferenceValue as SceneAsset)?.name;
            }
            else
            {
                // Если SceneAsset не выбран, очищаем имя сцены
                sceneName.stringValue = "";
            }
        }
        
        // Завершаем блок свойств
        EditorGUI.EndProperty();
    }
}

#endif