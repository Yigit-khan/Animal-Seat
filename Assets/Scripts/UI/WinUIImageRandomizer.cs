using UnityEngine;
using UnityEngine.UI;

public class WinUIImageRandomizer : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] private Image imageAnimal;
    [SerializeField] private Sprite[] levelUpSprites;
    void Start()
    {
        SetRandomImage();
    }

    public void SetRandomImage()
    {
        if (levelUpSprites.Length == 0 || imageAnimal == null) return;

        int index = Random.Range(0, levelUpSprites.Length);
        imageAnimal.sprite = levelUpSprites[index];
    }
}
