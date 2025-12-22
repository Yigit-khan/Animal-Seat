using UnityEngine;
using UnityEngine.UI;

public class BottomNavController : MonoBehaviour
{
    [System.Serializable]
    public class NavItem
    {
        public RectTransform button;  // Butonun tamamý
        public GameObject label;      // Altýndaki yazý
    }

    public NavItem[] navItems;
    private float selectedY = 150f;   // Týklanan butonun çýkacaðý yükseklik
    private float normalY = 120f;      // Normal konum

    private int selectedIndex = 0;

    void Start()
    {
        UpdateNavUI();
    }

    public void OnNavItemClicked(int index)
    {
        selectedIndex = index;
        UpdateNavUI();
    }

    //alt kýsýmdaki butonlardan hangisine týklanýyosa o buton yukarý çýkýyor ve text'i açýlýyor
    void UpdateNavUI()
    {
        for (int i = 0; i < navItems.Length; i++)
        {
            if (i == selectedIndex)
            {
                navItems[i].button.localPosition = new Vector3(
                    navItems[i].button.localPosition.x,
                    selectedY,
                    navItems[i].button.localPosition.z
                );
                navItems[i].label.SetActive(true);
            }
            else
            {
                navItems[i].button.localPosition = new Vector3(
                    navItems[i].button.localPosition.x,
                    normalY,
                    navItems[i].button.localPosition.z
                );
                navItems[i].label.SetActive(false);
            }
        }
    }
}
