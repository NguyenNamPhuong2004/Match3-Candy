using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelGame : MonoBehaviour
{
    [SerializeField] private Button[] levelBtns;
    [SerializeField] private Image[] locks;

    private void Awake()
    {
        for (int i = 0; i < 5; i++)
        {
            int index = i;
            levelBtns[i] = transform.GetChild(i).GetChild(0).GetComponent<Button>();
            levelBtns[i].onClick.AddListener(delegate { PlayGame(index + 1); });
            locks[i] = transform.GetChild(i).GetChild(1).GetComponent<Image>();
        }  
    }
    private void Update()
    {
        for (int i = 0; i < DataPlayer.GetUnlockLevelGame(); i++)
        {
            locks[i].gameObject.SetActive(false);
        }
    }
    public void PlayGame(int level)
    {
        SoundManager.Ins.ButtonSound();
        SceneManager.LoadScene(1);
        DataPlayer.SetLevelGame(level);
    }
}
