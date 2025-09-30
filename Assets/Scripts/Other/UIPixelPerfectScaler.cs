using UnityEngine;
using UnityEngine.UI;

public class UIPixelPerfectScaler : MonoBehaviour
{
    [Header("Настройки")]
    [SerializeField] private float referencePPU = 100f; // PPU для reference разрешения
    [SerializeField] private Vector2 referenceResolution = new Vector2(1920, 1080);
    
    [Header("Компоненты")]
    [SerializeField] private Image targetImage;
    [SerializeField] private CanvasScaler canvasScaler;
    
    void Start()
    {
        // Автоматически находим компоненты если не установлены
        if (targetImage == null)
            targetImage = GetComponent<Image>();
        
        if (canvasScaler == null)
            canvasScaler = GetComponentInParent<CanvasScaler>();
        
        UpdatePixelPerUnitMultiplier();
    }
    
    void UpdatePixelPerUnitMultiplier()
    {
        if (targetImage == null) return;
        
        // Получаем текущее разрешение экрана
        float currentScreenWidth = Screen.width;
        float currentScreenHeight = Screen.height;
        
        // Рассчитываем коэффициент масштабирования относительно reference разрешения
        float scaleFactorX = currentScreenWidth / referenceResolution.x;
        float scaleFactorY = currentScreenHeight / referenceResolution.y;
        
        // Используем среднее значение или минимальное/максимальное в зависимости от потребностей
        float scaleFactor = (scaleFactorX + scaleFactorY) / 2f;
        
        // Для сохранения пиксельной четкости - используем целые числа масштаба
        if (scaleFactor < 1f)
        {
            // Для уменьшения масштаба - используем обратное значение
            scaleFactor = 1f / Mathf.Round(1f / scaleFactor);
        }
        else
        {
            // Для увеличения масштаба - округляем до целого
            scaleFactor = Mathf.Round(scaleFactor);
        }
        
        // Устанавливаем множитель PPU
        targetImage.pixelsPerUnitMultiplier = referencePPU * scaleFactor;
        
        Debug.Log($"PPU Multiplier установлен: {targetImage.pixelsPerUnitMultiplier} " +
                 $"(Scale: {scaleFactor}, Resolution: {currentScreenWidth}x{currentScreenHeight})");
    }
    
    void Update()
    {
        // Обновляем при изменении размера окна (для десктопных приложений)
        if (Screen.width != lastScreenWidth || Screen.height != lastScreenHeight)
        {
            lastScreenWidth = Screen.width;
            lastScreenHeight = Screen.height;
            UpdatePixelPerUnitMultiplier();
        }
    }
    
    private int lastScreenWidth;
    private int lastScreenHeight;
}