using Disney.ClubPenguin.SledRacer.TestHarness;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Disney.ClubPenguin.SledRacer.Tests
{
    public abstract class UIIntegrationTest : MonoBehaviour
    {
        public GameObject HarnessPrefab;

        public string PanelScene;

        protected GameObject gameManagerObject;

        protected UITestHarnessGameManager harness;

        protected UIManager ui;

        protected GameObject panel;

        private bool isPanelLoadStarted;

        private bool eventListening;

        private void OnUIEvent(object sender, UIEvent e)
        {
            UnityEngine.Debug.Log("Receiving UI Event: " + e);
            UIEvent.uiGameEvent type = e.type;
            if (type == UIEvent.uiGameEvent.LoadingComplete)
            {
                UnityEngine.Debug.Log("LoadingComplete");
                OnPanelLoaded(ui.GetCurrentPanel());
            }
        }

        private void OnEnable()
        {
            isPanelLoadStarted = false;
            CheckDrestoyObject("/GameManager");
            CheckDrestoyObject("/EventSystem");
            GameObject gameObject = GameObject.Find("/UIManager");
            if (gameObject != null)
            {
                gameObject.GetComponent<UIManager>().Reset();
            }
            gameManagerObject = (UnityEngine.Object.Instantiate(HarnessPrefab) as GameObject);
            gameManagerObject.name = "GameManager";
            harness = GameManager.GetInstanceAs<UITestHarnessGameManager>();
        }

        private void OnDestroy()
        {
            if (eventListening)
            {
                Service.Get<EventDataService>().OnUIEvent -= OnUIEvent;
            }
        }

        private void CheckDrestoyObject(string name)
        {
            GameObject gameObject = GameObject.Find(name);
            if (gameObject != null)
            {
                UnityEngine.Debug.Log("Destroy old " + name);
                UnityEngine.Object.DestroyImmediate(gameObject);
            }
        }

        private void Update()
        {
            if (!eventListening)
            {
                Service.Get<EventDataService>().OnUIEvent += OnUIEvent;
                eventListening = true;
            }
            if (harness.IsLoaded && !isPanelLoadStarted)
            {
                isPanelLoadStarted = true;
                ui = harness.UIManager;
                UnityEngine.Debug.Log("OnHarnessLoaded ui = " + ui);
                UnityEngine.Debug.Log("scene to load: " + PanelScene);
                OnHarnessLoaded();
                if (!string.IsNullOrEmpty(PanelScene))
                {
                    UnityEngine.Debug.Log("show panel being called");
                    ui.showPanel(PanelScene);
                }
            }
            if (panel != null)
            {
                DoUpdate();
            }
        }

        private void OnPanelLoaded(GameObject panel)
        {
            UnityEngine.Debug.Log("OnPanelLoaded: " + panel);
            this.panel = panel;
            RunTest();
        }

        protected virtual void OnHarnessLoaded()
        {
        }

        protected virtual void RunTest()
        {
        }

        protected virtual void DoUpdate()
        {
        }

        protected void CheckButtonsAreSetup(string[] excludedButtonContainerNames = null)
        {
            List<GameObject> list = new List<GameObject>();
            if (excludedButtonContainerNames != null)
            {
                foreach (string name in excludedButtonContainerNames)
                {
                    list.Add(GameObject.Find(name));
                }
            }
            Button[] array = UnityEngine.Object.FindObjectsByType<Button>(FindObjectsSortMode.None);
            foreach (Button button in array)
            {
                UnityEngine.Debug.Log("Found Button: " + button.name);
                if (button.gameObject.transform.IsChildOf(panel.transform))
                {
                    bool flag = true;
                    foreach (GameObject item in list)
                    {
                        if (button.gameObject.transform.IsChildOf(item.transform))
                        {
                            UnityEngine.Debug.Log("Skipping button: " + button.name + " in the container: " + item.name);
                            flag = false;
                            break;
                        }
                    }
                    if (flag)
                    {
                        IntegrationTest.Assert(button.onClick.GetPersistentEventCount() >= 1);
                    }
                }
            }
        }
    }
}