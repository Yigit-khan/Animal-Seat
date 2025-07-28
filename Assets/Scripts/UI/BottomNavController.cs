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
    private float selectedY = 125f;   // Týklanan butonun çýkacaðý yükseklik
    public float normalY = 0f;      // Normal konum

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
