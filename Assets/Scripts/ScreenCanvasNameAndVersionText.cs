using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_Text))]
public class ScreenCanvasNameAndVersionText : MonoBehaviour
{
    private void Awake()
    {
        var tmp = GetComponent<TMP_Text>();
        tmp.text = $"{Application.productName} {Application.version} - {Application.companyName}";
    }
}
