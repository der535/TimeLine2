using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CompositionCard : MonoBehaviour
{
    [SerializeField] private Button spawnButton;
    [SerializeField] private Button editButton;
    [SerializeField] private Button renameButton;
    [SerializeField] private Button deleteButton;
    [SerializeField] private TextMeshProUGUI cardText;

    public void Setup(
        Action spawnAction,
        Action editAction,
        Action renameAction,
        Action deleteAction,
        string cardName)
    {
        cardText.text = cardName;
        
        spawnButton.onClick.AddListener(spawnAction.Invoke);
        
        if (editAction != null) 
            editButton.onClick.AddListener(editAction.Invoke);
        
        renameButton.onClick.AddListener(renameAction.Invoke);
        
        if (deleteAction != null) 
            deleteButton.onClick.AddListener(deleteAction.Invoke);
    }
}