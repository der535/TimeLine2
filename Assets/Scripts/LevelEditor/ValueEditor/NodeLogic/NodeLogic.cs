using System;
using System.Collections.Generic;
using TimeLine.LevelEditor.ValueEditor;

/// <summary>
/// Базовый класс для создания логиги других нод
/// </summary>
public abstract class NodeLogic
{
    public string Id = Guid.NewGuid().ToString();

    public List<(string name, DataType type)> InputDefinitions = new(); //Входы
    public List<(string name, DataType type)> OutputDefinitions = new(); //Выходы
    
    public Dictionary<int, object> ManualValues = new();

    //Хранит все подключенные ноды к этой
    // Храним: Индекс нашего входа -> (Нода-источник, Индекс её выхода)
    public Dictionary<int, (NodeLogic node, int outputIndex)> ConnectedInputs = new();

    // Сигнатуру GetValue тоже нужно обновить, чтобы она знала, какой выход запрашивают
    public abstract object GetValue(int outputIndex = 0);

    //Подключаем ноду
    public virtual void ConnectInput(int inputIndex, NodeLogic sourceNode, int outputIndex)
    {
        var connection = (sourceNode, outputIndex);

        ConnectedInputs[inputIndex] = connection;
    }
    
    public virtual void DisconnectInput(int inputIndex)
    {
        if (ConnectedInputs.ContainsKey(inputIndex))
        {
            ConnectedInputs.Remove(inputIndex);
        }
    }
    
    public virtual void ClearConnections()
    {
        // Очищаем словарь подключенных входов
        ConnectedInputs.Clear();
    
        // ManualValues обычно можно оставить, если ты хочешь 
        // сохранить введенные числа при переподключении, 
        // но если нужно полное "зануление", раскомментируй:
        // ManualValues.Clear();
    }
    
    public void AddInputDefinition()
    {
        // Добавляем новое описание входа, чтобы GetValue видел его в цикле
        InputDefinitions.Add(($"Input {InputDefinitions.Count}", DataType.Float));
    }

    /// <summary>
    /// Метод для получения значение из какогото входящего порта
    /// </summary>
    /// <param name="index">Индес порта из которого мы хотим получить значение</param>
    /// <param name="defaultValue">Если пустое значение выводим это значение</param>
    /// <returns>Возвращает полученный из входной ноды значение</returns>
    protected object GetInputValue(int index, object fallback)
    {
        // 1. Если есть провод — берем значение из провода
        if (ConnectedInputs.TryGetValue(index, out var conn))
        {
            return conn.node.GetValue(conn.outputIndex);
        }

        // 2. Если провода нет — проверяем, ввел ли пользователь что-то вручную
        return ManualValues.GetValueOrDefault(index, fallback);

        // 3. Если и там пусто — возвращаем дефолт (например, 0)
    }
}