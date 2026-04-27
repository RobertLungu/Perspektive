using UnityEngine;

public class MenuController : MonoBehaviour
{
    [Header("Panels")]
    public GameObject mainMenuPanel;
    public GameObject levelSelectPanel;

    void Start()    => Show(mainMenuPanel);
    void OnEnable() => Show(mainMenuPanel);

    public void OnPlay() => Show(levelSelectPanel);
    public void OnBack() => Show(mainMenuPanel);
    public void OnExit() => Application.Quit();

    void Show(GameObject target)
    {
        mainMenuPanel.SetActive(target == mainMenuPanel);
        levelSelectPanel.SetActive(target == levelSelectPanel);
    }
}
