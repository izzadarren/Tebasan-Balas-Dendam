
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class TabController : MonoBehaviour
{
    public Image[] tabImages;
    public GameObject[] pages;
    void Start()
    {
        ActivateTab(0);
    }

    // Update is called once per frame
    public void ActivateTab(int tabNo)
    {
        for (int i = 0; i < pages.Length; i++)
        {
            pages[i].SetActive(false);
            SetTabColor(tabImages[i], Color.gray);
        }
        pages[tabNo].SetActive(true);
        SetTabColor(tabImages[tabNo], Color.white);
    }

    void SetTabColor(Image tab, Color color)
    {
        // Ubah warna image utama
        tab.color = color;

        // Ubah warna semua child images (misalnya icon)
        foreach (Image childImage in tab.GetComponentsInChildren<Image>())
        {
            childImage.color = color;
        }
    }
}
