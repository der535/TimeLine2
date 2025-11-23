using System;
using TimeLine;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CompositionCard : MonoBehaviour
{
    [SerializeField] private Button spawnButton;
    [SerializeField] private Button editButton;
    [SerializeField] private Button renameButton;
    [SerializeField] private Button deleteButton;
    [SerializeField] private Button duplicateButton;
    [SerializeField] private TextMeshProUGUI cardText;
    
    private SaveComposition _saveComposition;
    private string _id;
    
    internal string GetID() => _id;

    public void Setup(
        SaveComposition saveComposition,
        Action spawnAction,
        Action editAction,
        Action renameAction,
        Action deleteAction,
        Action duplicateAction,
        string compositionID)
    {
        _id = compositionID;
        
        _saveComposition = saveComposition;

        UpdateText();
        
        spawnButton.onClick.AddListener(spawnAction.Invoke);
        
        if (editAction != null) 
            editButton.onClick.AddListener(editAction.Invoke);
        
        renameButton.onClick.AddListener(renameAction.Invoke);
        
        if (deleteAction != null) 
            deleteButton.onClick.AddListener(deleteAction.Invoke);
        
        if (duplicateAction != null) 
            duplicateButton.onClick.AddListener(duplicateAction.Invoke);
    }

    internal void LockSpawn()
    {
        spawnButton.interactable = false;
    }

    internal void UnlockSpawn()
    {
        spawnButton.interactable = true;
    }

    internal void LockEditButton()
    {
        editButton.interactable = false;
        renameButton.interactable = false;
        deleteButton.interactable = false;
        duplicateButton.interactable = false;
    }

    internal void UnlockEditButton()
    {
        editButton.interactable = true;
        renameButton.interactable = true;
        deleteButton.interactable = true;
        duplicateButton.interactable = true;
    }

    internal void UpdateText()
    {
        cardText.text = _saveComposition.FindCompositionDataById(_id).gameObjectName;
    }
}