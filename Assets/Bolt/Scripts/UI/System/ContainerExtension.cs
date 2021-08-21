using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Sheed.UI.Internal
{
    public static class ContainerExtension
    {
        // 현재 화면을 newChild로 대치
        public static T Replace<T>(this RectTransform container, T newChild) where T : UIScene
        {
            // Replace에 사용하는 container은 언제나 자식이 하나뿐임을 가정
            if (container.childCount > 1)
            {
                Debug.LogWarning("Replace used with children count more than 1");
            }

            // children 배열이 중간에 바뀔 수도 있으므로 로컬에 미리 저장
            var children = container.Children().Reverse().ToList();

            // OnRemove 호출
            foreach (var child in children)
            {
                var childScene = child.GetComponent<UIScene>();
                if (childScene != null)
                {
                    childScene.OnRemove();
                }
            }

            // 정리 및 추가
            foreach (var child in children)
            {
                GameObject.Destroy(child.gameObject);
            }

            if (newChild == null)
            {
                Debug.LogWarning("null child");
                return null;
            }

            var newInstance = GameObject.Instantiate<T>(newChild, container);
            newInstance.OnAdd();

            return newInstance;
        }

        // 현재 컨테이너의 최상단에 newChild를 배치
        public static T Push<T>(this RectTransform container, T newChild) where T : UIScene
        {
            // 중복 체크
            foreach (var child in container.Children())
            {
                if (child == newChild)
                {
                    Debug.LogWarning($"already added children '{newChild.name}'");
                    return newChild;
                }
            }

            // 추가
            if (newChild == null)
            {
                Debug.LogWarning("null child");
                return null;

            }

            var newInstance = GameObject.Instantiate<T>(newChild, container);
            newInstance.OnAdd();

            return newInstance;
        }

        // 가장 마지막에 추가된 자식을 제거
        public static void Pop(this RectTransform container)
        {
            if (container.childCount == 0)
            {
                Debug.LogWarning($"container is already empty");
                return;
            }

            // 제거 콜백
            var lastChild = container.GetChild(container.childCount - 1);
            var lastScene = lastChild.GetComponent<UIScene>();
            if (lastScene != null)
            {
                lastScene.OnRemove();
            }

            // 실제 화면에서 제거
            GameObject.Destroy(lastChild.gameObject);
        }

        public static void Remove(this RectTransform container, UIScene childToRemove)
        {
            foreach (var child in container.Children())
            {
                var childScene = child.GetComponent<UIScene>();
                if (childScene == childToRemove)
                {
                    childScene.OnRemove();
                    GameObject.Destroy(childScene.gameObject);
                    return;
                }
            }

            Debug.LogWarning($"cannot find child to remove({(childToRemove ? childToRemove.name : "null")})");
        }

        // shortcut
        static IEnumerable<RectTransform> Children(this RectTransform container)
        {
            for (int i = 0; i < container.childCount; ++i)
            {
                var Transform = container.GetChild(i);
                if (Transform is RectTransform)
                {
                    yield return Transform as RectTransform;
                }
                else
                {
                    throw new Exception("child is not RectTransform");
                }
            }
        }
    }
}
