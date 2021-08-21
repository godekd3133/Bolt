using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Sheed.UI.Internal;
using Cysharp.Threading.Tasks;

namespace Sheed.UI
{
    public class RootCanvas : MonoBehaviour
    {
        private static RootCanvas _instance = null;
        public static RootCanvas instance => _instance; // 싱글톤 접근자

        [SerializeField]
        private RectTransform Scene;

        [SerializeField]
        private RectTransform Popup;

        [SerializeField]
        private RectTransform Overlay;

        [SerializeField]
        AssetReferenceGameObject EntryScene; // 최초로 등장시킬 장면 프리팹

        UIScene _currentScene;
        GameObject _transitionPrefab;

        // 현재 scene 클래스를 가져올 수 있다
        public T GetCurrentScene<T>() where T : UIScene
        {
            return _currentScene as T;
        }

        void Awake()
        {
            if (_instance != null)
            {
                Debug.LogWarning("RootCanvas instance is already registered");
                return;
            }

            _instance = this;
        }

        // Start is called before the first frame update
        void Start()
        {
            Bootup().Catch(e => Debug.LogException(e));
        }

        void OnDestroy()
        {
            // 씬 전환될 때 호출됨
            if (_instance == this)
            {
                _instance = null;
            }
        }

        async Task Bootup()
        {
            if (EntryScene == null)
                return;

            await ResourceManager.instance.PrepareLoading();

            // 트랜지션
            _transitionPrefab = await ResourceManager.instance.LoadPrefab("UI/System/CharacterTransition");

            // 최초 씬
            _ = SetScene(EntryScene, false);
        }

        /** SCENE **/
        public Task SetScene(string scenePath, bool transition = true)
            => setSceneInternal(ResourceManager.instance.LoadPrefab(scenePath), transition);

        public Task SetScene(AssetReferenceT<GameObject> sceneRef, bool transition = true)
            => setSceneInternal(ResourceManager.instance.LoadPrefab(sceneRef), transition);

        /** POPUP **/
        public Task<T> AddPopup<T>(string popupPath) where T : UIScene => addPopupInternal<T>(ResourceManager.instance.LoadPrefab(popupPath));
        public Task<T> AddPopup<T>(AssetReferenceT<GameObject> popupRef) where T : UIScene => addPopupInternal<T>(ResourceManager.instance.LoadPrefab(popupRef));
        public void RemovePopup(UIScene popup) => Popup.Remove(popup);
        public void RemoveLastPopup() => Popup.Pop();

        /** OVERLAY **/
        public Task<T> AddOverlay<T>(string overlayPath) where T : UIScene => addOverlayInternal<T>(ResourceManager.instance.LoadPrefab(overlayPath));
        public Task<T> AddOverlay<T>(AssetReferenceT<GameObject> overlayRef) where T : UIScene => addOverlayInternal<T>(ResourceManager.instance.LoadPrefab(overlayRef));
        public T AddOverlay<T>(T overlay) where T : UIScene => Overlay.Push(overlay);
        public void RemoveOverlay(UIScene overlay) => Overlay.Remove(overlay);

        async Task setSceneInternal(Task<GameObject> task, bool transition)
        {
            // 트랜지션하면서 로딩
            await Task.WhenAll(new[] {
                task,
                transition ? Transition(false) : Task.CompletedTask, // 페이드 인
            });

            try
            {
                var prefab = task.GetAwaiter().GetResult();
                if (prefab != null)
                {
                    Debug.Log($"SetScene from prefab {prefab.name}");
                    _currentScene = Scene.Replace(prefab.GetComponent<UIScene>());
                }
                else
                {
                    Debug.LogError("failed to load scene");
                }
            }
            finally
            {
                // 페이드 아웃
                if (transition)
                {
                    await Transition(true);
                }
            }
        }

        Task<T> addPopupInternal<T>(Task<GameObject> task) where T : UIScene => task.Then(prefab =>
        {
            if (prefab != null)
            {
                var popup = prefab.GetComponent<T>();
                Debug.Assert(popup);
                return Popup.Push(popup);
            }
            else
            {
                Debug.LogError("failed to load popup");
                return null;
            }
        });

        Task<T> addOverlayInternal<T>(Task<GameObject> task) where T : UIScene => task.Then(prefab =>
        {
            if (prefab != null)
            {
                var overlay = prefab.GetComponent<T>();
                Debug.Assert(overlay);
                return Overlay.Push(overlay);
            }
            else
            {
                Debug.LogError("failed to load overlay");
                return null;
            }
        });

        public Task Transition(bool fadeout)
        {
            var inst = GameObject.Instantiate(_transitionPrefab, Overlay.transform);
            var anim = inst.GetComponent<Animator>();
            anim.SetFloat("Direction", fadeout ? 1 : -1);
            anim.Play("FadeIn", -1, fadeout ? 0 : 0.99f);

            return (fadeout ? track() : trackInverse()).Then(() => Destroy(inst));

            async Task track()
            {
                await UniTask.NextFrame();
                await UniTask.WaitWhile(() => this && anim.GetCurrentAnimatorStateInfo(0).normalizedTime < 1);
            }

            async Task trackInverse()
            {
                await UniTask.NextFrame();
                await UniTask.WaitWhile(() => this && anim.GetCurrentAnimatorStateInfo(0).normalizedTime > 0);
            }
        }
    }
}
