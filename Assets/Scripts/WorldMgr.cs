using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class WorldMgr : MonoSingleton<WorldMgr>
{
    public CinemachineVirtualCamera virtualCamera;
    public CinemachineConfiner2D confiner2D;
    public Transform followTarget;

    public Light2D globalLight;

    public Transform uiRoot;

    public GameObject blackScreen;

    public GameObject endGameScreen;
    public Button endGameBtn;

    private const float KEYBOARD_MOVE_SPEED = 20f;

    void Awake()
    {
        if (!virtualCamera)
        {
            virtualCamera = FindAnyObjectByType<CinemachineVirtualCamera>();
        }

        if (!confiner2D)
        {
            confiner2D = virtualCamera.GetComponent<CinemachineConfiner2D>();
        }

        if (uiRoot == null)
        {
            uiRoot = GameObject.Find("UIPanelRoot").transform;
        }

        blackScreen.SetActive(false);

        endGameBtn.onClick.AddListener(OnEndGameBtnClicked);
        endGameScreen.SetActive(false);

        AudioMgr.Instance.PlayMusic("bgm室内");
    }

    // Start is called before the first frame update
    void Start()
    {
        if (uiRoot == null)
        {
            uiRoot = GameObject.Find("UIPanelRoot").transform;
        }
    }

    // Update is called once per frame
    void Update()
    {
        HandleCameraMovement();
    }

    /// <summary>
    /// 处理相机移动
    /// </summary>
    private void HandleCameraMovement()
    {
        if (virtualCamera.Follow != null) return;    // 如果有跟随目标，不处理键盘移动

        // 键盘移动
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        if (Mathf.Abs(horizontal) > 0.1f || Mathf.Abs(vertical) > 0.1f)
        {
            Vector3 translate = new Vector3(horizontal, vertical, 0) * KEYBOARD_MOVE_SPEED * Time.deltaTime;

            // 计算预期位置
            Vector3 targetPosition = virtualCamera.transform.position + translate;

            // 检查是否超出边界
            if (confiner2D && confiner2D.enabled)
            {
                Bounds bounds = confiner2D.m_BoundingShape2D.bounds;
                // 获取相机的正交尺寸
                float orthoSize = virtualCamera.m_Lens.OrthographicSize;
                // 计算相机视口的宽度
                float aspectRatio = Screen.width / (float)Screen.height;
                float horizontalSize = orthoSize * aspectRatio;

                // 限制目标位置在边界内
                targetPosition.x = Mathf.Clamp(targetPosition.x,
                    bounds.min.x + horizontalSize,
                    bounds.max.x - horizontalSize);
                targetPosition.y = Mathf.Clamp(targetPosition.y,
                    bounds.min.y + orthoSize,
                    bounds.max.y - orthoSize);
            }
            virtualCamera.transform.position = targetPosition;
        }
    }

    /// <summary>
    /// 停止相机跟随
    /// </summary>
    public void StopCameraFollowing()
    {
        CharacterMgr.Player().SetStatus(CharacterStatus.Busy);
        virtualCamera.Follow = null;
        virtualCamera.m_Lens.OrthographicSize = 25;
        virtualCamera.transform.position = new Vector3(0, -6, virtualCamera.transform.position.z);
    }

    /// <summary>
    /// 恢复相机跟随
    /// </summary>
    public void RestoreCameraFollowing()
    {
        virtualCamera.Follow = followTarget;
        CharacterMgr.Player().SetStatus(CharacterStatus.Idle);
        virtualCamera.m_Lens.OrthographicSize = 8;
    }

    public void OnEndGameBtnClicked()
    {
        AudioMgr.Instance.PlaySound("点击");
        SceneManager.LoadScene("StartScene");
    }


}
