using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public GameObject[] UIPogoTip;
    private Image[] pogoTipOutlines;

    private void Start()
    {
        pogoTipOutlines = new Image[UIPogoTip.Length];
        
        for (int i = 0; i < UIPogoTip.Length; i++)
        {
            UIPogoTip[i].SetActive(false);
            pogoTipOutlines[i] = UIPogoTip[i].transform.Find("Outline").GetComponent<Image>();
        }
    }

    private void Update()
    {
        UpdatePogoTipColor();
    }

    public void UpdatePogoTipColor()
    {
        for (int i = 0; i < ColorManager.Instance.TotalColors; i++)
        {
            UIPogoTip[i].GetComponent<Image>().color = ColorManager.Instance.Colors[i];
            UIPogoTip[i].SetActive(true);

            pogoTipOutlines[i].enabled = (i == (int)ColorManager.Instance.CurrentColorIndex);
        }
    }
}
