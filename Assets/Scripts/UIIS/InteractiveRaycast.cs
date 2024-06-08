using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UIIS
{
    public class InteractiveRaycast : MonoBehaviour
    {
        public float range;

        public bool isPromptInteractable = false;
        public string promptMessage = string.Empty;
        GameObject promptObject = null;

        private void Update()
        {
            PromptCast(transform.position, transform.forward, range);

            

            if (isPromptInteractable)
            {
                if (promptObject.TryGetComponent<InteractionPrompt>(out var interactionPrompt))
                {
                    promptMessage = "E to " + interactionPrompt.promptText;
                    if (interactionPrompt.ignorePretext) { promptMessage = interactionPrompt.promptText; }
                }
                else
                {
                    promptMessage = string.Empty;
                }
            }
            else
            {
                promptMessage = string.Empty;
            }
        }

        void PromptCast(Vector3 origin, Vector3 direction, float distance)
        {
            var ray = new Ray(origin, direction);
            var hitInfo = new RaycastHit();
            if (Physics.Raycast(ray, out hitInfo, distance))
            {
                var otherObject = hitInfo.collider.gameObject;

                if (otherObject == promptObject)
                    return;

                if (otherObject.GetComponent<Iinteractable>() == null)
                {
                    isPromptInteractable = false;
                }
                else
                {
                    isPromptInteractable = true;
                }

                promptObject = otherObject;

            }
            else
            {
                promptObject = null;
                isPromptInteractable = false;
            }
        }

        public void TryInteract()
        {
            if (promptObject == null || !isPromptInteractable)
                return;

            var interactables = promptObject.GetComponents<Iinteractable>();
            foreach (var i in interactables)
            {
                i.OnInteract(this.gameObject);
            }

        }
    }
}


