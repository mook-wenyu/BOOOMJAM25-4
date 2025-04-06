using UnityEngine;

public class SingleMono<T> : MonoBehaviour where T : SingleMono<T>
{
    public static T Instance { get; private set; }

    private void Awake()
    {
        Instance = this as T;
        GameObject.DontDestroyOnLoad(gameObject);
        Init();
    }

    private void Start()
    {
        Begin();
    }

    private void Update()
    {
        var delta = (int)(Time.deltaTime * 1000);
        Tick(delta);
    }

    public virtual void Init() 
    {
    }

    public virtual void Begin() 
    {
    }

    public virtual void Tick(int delta) 
    {
    }

}
