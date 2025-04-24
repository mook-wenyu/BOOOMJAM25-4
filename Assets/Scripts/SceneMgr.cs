using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneMgr : MonoSingleton<SceneMgr>
{
    // 当前加载场景的异步操作
    private AsyncOperation _currentLoadOperation;

    private List<GameObject> _mainSceneObjs = new List<GameObject>();

    void Awake()
    {
        Scene mainScene = SceneManager.GetActiveScene();
        _mainSceneObjs = mainScene.GetRootGameObjects().ToList();
    }

    public void ShowMainSceneObjects()
    {
        foreach (GameObject obj in _mainSceneObjs)
        {
            if (obj == null) continue;
            if (obj.name != "Main Camera" && obj.name != "Virtual Camera" && obj.name != "World") continue;
            obj.SetActive(true);
        }
    }

    public void HideMainSceneObjects()
    {
        foreach (GameObject obj in _mainSceneObjs)
        {
            if (obj == null) continue;
            if (obj.name != "Main Camera" && obj.name != "Virtual Camera" && obj.name != "World") continue;
            obj.SetActive(false);
        }
    }

    /// <summary>
    /// 加载场景
    /// </summary>
    /// <param name="sceneName">场景名称</param>
    /// <param name="loadSceneMode">加载模式，默认为单一模式</param>
    /// <param name="showLoadingUI">是否显示加载界面</param>
    public void LoadScene(string sceneName, LoadSceneMode loadSceneMode = LoadSceneMode.Single, bool showLoadingUI = true)
    {
        StartCoroutine(LoadSceneAsync(sceneName, loadSceneMode, showLoadingUI));
    }

    /// <summary>
    /// 异步加载场景
    /// </summary>
    private IEnumerator LoadSceneAsync(string sceneName, LoadSceneMode loadSceneMode, bool showLoadingUI)
    {
        if (showLoadingUI)
        {
            // TODO: 显示加载界面
            Debug.Log("显示加载界面");
        }
        GlobalUIMgr.Instance.ShowLoadingMask(true);
        HideMainSceneObjects();

        _currentLoadOperation = SceneManager.LoadSceneAsync(sceneName, loadSceneMode);
        _currentLoadOperation.allowSceneActivation = false;

        while (!_currentLoadOperation.isDone)
        {
            float progress = Mathf.Clamp01(_currentLoadOperation.progress / 0.9f);
            Debug.Log($"加载进度: {progress * 100}%");
            // TODO: 更新加载进度界面

            if (_currentLoadOperation.progress >= 0.9f)
            {
                _currentLoadOperation.allowSceneActivation = true;
                yield return null;
            }
            yield return null;
        }

        yield return null;
        yield return SetActiveScene(sceneName);
        GlobalUIMgr.Instance.ShowLoadingMask(false);
        if (showLoadingUI)
        {
            // TODO: 隐藏加载界面
            Debug.Log("隐藏加载界面");
        }
    }

    public IEnumerator SetActiveScene(string sceneName)
    {
        Scene scene = SceneManager.GetSceneByName(sceneName);
        if (scene.isLoaded)
        {
            SceneManager.SetActiveScene(scene);
        }
        yield return null;
    }

    /// <summary>
    /// 卸载场景
    /// </summary>
    /// <param name="sceneName">要卸载的场景名称</param>
    public void UnloadScene(string sceneName)
    {
        StartCoroutine(UnloadSceneAsync(sceneName));
    }

    /// <summary>
    /// 异步卸载场景
    /// </summary>
    private IEnumerator UnloadSceneAsync(string sceneName)
    {
        AsyncOperation unloadOperation = SceneManager.UnloadSceneAsync(sceneName);

        while (!unloadOperation.isDone)
        {
            yield return null;
        }

        // 卸载完成后，执行资源回收
        Resources.UnloadUnusedAssets();

        SceneManager.SetActiveScene(SceneManager.GetSceneByName("MainScene"));
        ShowMainSceneObjects();
        GlobalUIMgr.Instance.ShowLoadingMask(false);
    }
}
