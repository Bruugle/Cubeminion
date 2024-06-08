using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace UIIS
{
    public class InteractionPrompt : MonoBehaviour, Iinteractable
    {
        public bool ignorePretext;
        public string promptText = string.Empty;

        public void OnInteract(GameObject sender)
        {
            
        }
    }
}

