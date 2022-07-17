using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TipPopupWindow : MonoBehaviour
{
    public string activeStatId = null;
    public RectTransform focus;
    TextMeshProUGUI title;
    TextMeshProUGUI value;
    TextMeshProUGUI description;
    TextMeshProUGUI percentage;
    RectTransform rect;

    bool active;
    float xMultiplier;
    float yMultiplier;

    // Start is called before the first frame update
    void Start()
    {
        rect = GetComponent<RectTransform>();
        title = transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        value = transform.GetChild(1).GetComponent<TextMeshProUGUI>();
        description = transform.GetChild(2).GetComponent<TextMeshProUGUI>();
        percentage = transform.GetChild(3).GetComponent<TextMeshProUGUI>();
    }

    private void FixedUpdate()
    {
        if (active)
        {
            Vector2 pos = focus.anchoredPosition + Vector2.right * 162;
            pos = new Vector2(Mathf.RoundToInt(pos.x), Mathf.RoundToInt(pos.y));
            rect.anchoredPosition = pos;
        }
        else
        {
            rect.anchoredPosition = new Vector2(512, -1140);
        }
    }

    public void Activate(StatManager.Stat stat, RectTransform rect)
    {
        active = true;
        activeStatId = stat.info.id;
        focus = rect;
        title.text = stat.info.displayName;
        description.text = stat.info.description;

        if (stat.info.ignoreNumber)
        {
            value.text = "";
            percentage.text = "";
        }
        else
        {
            value.text = stat.numericalValue.ToString();
            int val = Mathf.RoundToInt(stat.numericalValue / 18f * 100);
            if(!stat.info.hidePercentage)
                percentage.text = val.ToString() + "% chance";
            else
                percentage.text = "";
        }
    }

    public void Deactivate()
    {
        active = false;
        activeStatId = "";
    }
}
