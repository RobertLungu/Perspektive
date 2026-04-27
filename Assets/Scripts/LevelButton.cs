using UnityEngine;
using UnityEngine.UI;

public class LevelButton : MonoBehaviour
{
    public string sceneName;

    void Start()
    {
        GetComponent<Button>().onClick.AddListener(() => GameManager.Instance.LoadLevel(sceneName));
    }
}
