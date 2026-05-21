using Disney.ClubPenguin.CPModuleUtils;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Disney.ClubPenguin.SledRacer.Tests
{
    public class MainMenuButtonsExist : UIIntegrationTest
    {
        public string[] ExpectedButtonNames;

        public string[] ExcludedButtonContainerNames;

        public IList<GameObject> ExcludedButtonContainers = new List<GameObject>();

        protected override void RunTest()
        {
            string[] excludedButtonContainerNames = ExcludedButtonContainerNames;
            foreach (string name in excludedButtonContainerNames)
            {
                ExcludedButtonContainers.Add(GameObject.Find(name));
            }
            Button[] array = Object.FindObjectsByType<Button>(FindObjectsSortMode.None);
            foreach (Button button in array)
            {
                UnityEngine.Debug.Log("Found Button: " + button.GetPath());
                if (button.gameObject.transform.IsChildOf(panel.transform))
                {
                    bool flag = true;
                    foreach (GameObject excludedButtonContainer in ExcludedButtonContainers)
                    {
                        if (button.gameObject.transform.IsChildOf(excludedButtonContainer.transform))
                        {
                            UnityEngine.Debug.Log("Skipping button: " + button.name + " in the container: " + excludedButtonContainer.name);
                            flag = false;
                            break;
                        }
                    }
                    if (flag)
                    {
                        IntegrationTest.Assert(ExpectedButtonNames.Contains(button.name));
                    }
                }
            }
        }
    }
}