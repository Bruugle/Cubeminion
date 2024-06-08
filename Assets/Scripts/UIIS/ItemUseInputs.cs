using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine;
using System;

namespace UIIS
{
    [Serializable]
    public struct KeyBind
    {
        public Key key;
        [SerializeField]
        [RequireInterface(typeof(Iusable))]
        private UnityEngine.Object _usable;
        public Iusable usable => _usable as Iusable;
    }

    public class ItemUseInputs : MonoBehaviour
    {
        public UseSlot slot;

        [SerializeField]
        [RequireInterface(typeof(Iusable))]
        private UnityEngine.Object _leftClickBind;
        public Iusable leftClickBind => _leftClickBind as Iusable;

        [SerializeField]
        [RequireInterface(typeof(Iusable))]
        private UnityEngine.Object _rightClickBind;
        public Iusable rightClickBind => _leftClickBind as Iusable;

        public List<KeyBind> keyBindings;

    }
}

