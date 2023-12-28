using UnityEngine;
using TMPro;

public class WarningPanel : MonoBehaviour
{
    public static WarningPanel instance;

    public TMP_Text warningText;
    [SerializeField] private GameObject warningPanel;

    private void Awake()
    {
        if (instance == null) instance = this;
    }

    public void showWarining(string warning)
    {
        warningText.text = warning;
        warningPanel.SetActive(true);
    }
}
