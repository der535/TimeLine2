using System;
using System.Collections.Generic;
using UnityEngine;

namespace TimeLine
{
    public class TabStorage : MonoBehaviour
    {
       [SerializeField] private List<TabData> tabs = new();

       private TabData _currentTab;
       
       private void Start()
       {
           foreach (var tab in tabs)
           {
               Setup(tab);
           }
           
           foreach (var tab in tabs)
           {
               tab.tabButton.Deselected();
               tab.tabPanel.SetActive(false);
           }
           tabs[0].tabButton.Selected();
           tabs[0].tabPanel.SetActive(true);
           _currentTab = tabs[0];
       }

       private void Setup(TabData tabData)
       {
           tabData.tabButton.Setup(() =>
           {
               foreach (var tab in tabs)
               {
                   tab.tabButton.Deselected();
                   tab.tabPanel.SetActive(false);
               }
               tabData.tabButton.Selected();
               tabData.tabPanel.SetActive(true);
               _currentTab = tabData;
           });
       }

       public TabData GetActiveTab()
       {
           return _currentTab;
       }
    }

    [Serializable]
    public class TabData
    {
        public TabButton tabButton;
        public GameObject tabPanel;
    }
}
