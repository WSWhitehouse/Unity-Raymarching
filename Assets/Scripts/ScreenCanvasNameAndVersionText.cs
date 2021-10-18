using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshProUGUI))]
public class ScreenCanvasNameAndVersionText : MonoBehaviour
{
    private void Awake()
    {
        var tmp = GetComponent<TextMeshProUGUI>();
        tmp.text = $"{Application.productName} {Application.version} - © {Application.companyName}";
    }
}
