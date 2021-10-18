using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshProUGUI))]
public class ScreenCanvasFPS : MonoBehaviour
{
  private TextMeshProUGUI tmp;

  private readonly string[] fpsAsStrings = new string[]
  {
    "00 fps", "01 fps", "02 fps", "03 fps", "04 fps", "05 fps", "06 fps", "07 fps", "08 fps", "09 fps",
    "10 fps", "11 fps", "12 fps", "13 fps", "14 fps", "15 fps", "16 fps", "17 fps", "18 fps", "19 fps",
    "20 fps", "21 fps", "22 fps", "23 fps", "24 fps", "25 fps", "26 fps", "27 fps", "28 fps", "29 fps",
    "30 fps", "31 fps", "32 fps", "33 fps", "34 fps", "35 fps", "36 fps", "37 fps", "38 fps", "39 fps",
    "40 fps", "41 fps", "42 fps", "43 fps", "44 fps", "45 fps", "46 fps", "47 fps", "48 fps", "49 fps",
    "50 fps", "51 fps", "52 fps", "53 fps", "54 fps", "55 fps", "56 fps", "57 fps", "58 fps", "59 fps",
    "60 fps", "61 fps", "62 fps", "63 fps", "64 fps", "65 fps", "66 fps", "67 fps", "68 fps", "69 fps",
    "70 fps", "71 fps", "72 fps", "73 fps", "74 fps", "75 fps", "76 fps", "77 fps", "78 fps", "79 fps",
    "80 fps", "81 fps", "82 fps", "83 fps", "84 fps", "85 fps", "86 fps", "87 fps", "88 fps", "89 fps",
    "90 fps", "91 fps", "92 fps", "93 fps", "94 fps", "95 fps", "96 fps", "97 fps", "98 fps", "99 fps"
  };

  private void Awake()
  {
    tmp = GetComponent<TextMeshProUGUI>();
  }

  private void Update()
  {
    int fps = Mathf.RoundToInt((1 / Time.smoothDeltaTime) * Time.timeScale);
    tmp.text = fpsAsStrings[Mathf.Clamp(fps, 0, fpsAsStrings.Length - 1)];
  }
}