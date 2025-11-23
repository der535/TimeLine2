using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

namespace TimeLine
{
    public class ChangeLogController : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI textMeshProComponent;
        private string url = "https://drive.google.com/uc?export=download&id=1mVI9CneDEUxASYgY8pKpmC8j1snIdqoX";

        void Start()
        {
            // Если компонент не назначен в инспекторе, попробуем найти его автоматически
            if (textMeshProComponent == null)
            {
                textMeshProComponent = GetComponent<TextMeshProUGUI>();
            }
        
            // Запускаем загрузку текста
            StartCoroutine(LoadTextFromURL());
        }


        IEnumerator LoadTextFromURL()
        {
            using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
            {
                // Отправляем запрос и ждем завершения
                yield return webRequest.SendWebRequest();

                // Проверяем результат запроса
                if (webRequest.result == UnityWebRequest.Result.Success)
                {
                    // Если запрос успешен, устанавливаем текст в TextMeshPro
                    if (textMeshProComponent != null)
                    {
                        textMeshProComponent.text = webRequest.downloadHandler.text;
                        Debug.Log("Текст успешно загружен и установлен в TextMeshPro");
                    }
                    else
                    {
                        Debug.LogError("TextMeshPro компонент не найден!");
                    }
                }
                else
                {
                    // Обрабатываем ошибки
                    Debug.LogError("Ошибка при загрузке текста: " + webRequest.error);
                
                    if (textMeshProComponent != null)
                    {
                        textMeshProComponent.text = "Ошибка загрузки текста: " + webRequest.error;
                    }
                }
            }
        }

        // Метод для перезагрузки текста (можно вызвать из UI кнопки)
        public void ReloadText()
        {
            StartCoroutine(LoadTextFromURL());
        }
    }
}
