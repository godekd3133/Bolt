using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Sheed.UI
{
    [DisallowMultipleComponent]
    public abstract class UIScene : UIBehaviour
    {
        // ui hierarchy에 추가될 때 실행
        public virtual void OnAdd() { }

        // ui hierarchy에서 제거될 때 실행
        public virtual void OnRemove() { }
    }
}