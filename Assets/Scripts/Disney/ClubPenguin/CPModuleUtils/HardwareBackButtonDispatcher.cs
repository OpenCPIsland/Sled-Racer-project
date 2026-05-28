using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Disney.ClubPenguin.CPModuleUtils
{
    [RequireComponent(typeof(EventSystem))]
    public class HardwareBackButtonDispatcher : MonoBehaviour
    {
        private static Stack<Button> pointerHandlerStack = new Stack<Button>();

        private static bool attachedToEventSystem = false;

        public static bool ListenForInput = true;

        public static void SetTargetClickHandler(Button pointerClickHandler, bool visible = true)
        {
            if (!attachedToEventSystem)
            {
                throw new Exception("HardwareBackButtonDispatcher script has not been attached to an EventSystem.");
            }
            if (pointerClickHandler.gameObject == null)
            {
                Debug.LogWarning("Attempting to add the hardware back button to an missing game object");
                return;
            }
            pointerHandlerStack.Push(pointerClickHandler);
            if (!(pointerClickHandler != null) || visible)
            {
                return;
            }
            RectTransform component = pointerClickHandler.GetComponent<RectTransform>();
            if (component != null)
            {
                component.localScale = new Vector3(0f, 0f, 0f);
                Image component2 = component.GetComponent<Image>();
                if ((bool)component2)
                {
                    component2.enabled = false;
                }
                Image[] componentsInChildren = component.GetComponentsInChildren<Image>();
                Image[] array = componentsInChildren;
                foreach (Image image in array)
                {
                    image.enabled = false;
                }
            }
        }

        private void Awake()
        {
            attachedToEventSystem = true;
        }

        private void Update()
        {
            while (pointerHandlerStack.Count > 0 && (pointerHandlerStack.Peek() == null || pointerHandlerStack.Peek().gameObject == null))
            {
                pointerHandlerStack.Pop();
            }
            if (pointerHandlerStack.Count != 0 && ListenForInput && Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                IPointerClickHandler pointerClickHandler = pointerHandlerStack.Pop();
                PointerEventData pointerEventData = new PointerEventData(null);
                pointerEventData.button = PointerEventData.InputButton.Left;
                pointerClickHandler.OnPointerClick(pointerEventData);
            }
        }
    }
}