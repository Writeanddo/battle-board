using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class StatDisplayer : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public StatManager.Stat displayedStat;
    public int statIndex;

    Image statIcon;
    Image statValueIcon;
    TipPopupWindow popup;
    RectTransform rect;
    StatManager sm;
    GameManager gm;

    // Start is called before the first frame update
    void Start()
    {
        rect = GetComponent<RectTransform>();
        sm = FindObjectOfType<StatManager>();
        gm = FindObjectOfType<GameManager>();
        popup = FindObjectOfType<TipPopupWindow>();
        statIcon = GetComponent<Image>();
        statValueIcon = transform.GetChild(0).GetComponent<Image>();
        ClearDisplayedStat();
    }



    //Detect if the Cursor starts to pass over the GameObject
    public void OnPointerEnter(PointerEventData pointerEventData)
    {
        if (displayedStat != null && !popup.activeStatId.Equals(displayedStat.info.id))
        {
            rect.sizeDelta = Vector2.one * 72;
            popup.Activate(displayedStat, rect);
        }
    }

    //Detect when Cursor leaves the GameObject
    public void OnPointerExit(PointerEventData pointerEventData)
    {
        if (displayedStat != null && popup.activeStatId.Equals(displayedStat.info.id))
        {
            rect.sizeDelta = Vector2.one * 60;
            popup.Deactivate();
        }
    }

    public void OnPointerClick(PointerEventData pointerEventData)
    {
        if(gm.rerollDepth == 1 && displayedStat != null && statIndex > -1)
        {
            gm.displayerToReroll = this;
            gm.SetDiceAttributeSprites();
            gm.RerollAdvance();
        }
    }

    public void ClearDisplayedStat()
    {
        displayedStat = null;
        statIcon.color = Color.clear;
        statValueIcon.color = Color.clear;
    }

    public void UpdateDisplayedStat(StatManager.Stat newStat, int index)
    {
        statIcon.color = Color.white;
        displayedStat = newStat;
        statIcon.sprite = newStat.info.icon;
        if (!newStat.info.ignoreNumber)
        {
            statValueIcon.color = Color.white;
            statValueIcon.sprite = sm.numbers[newStat.numericalValue - 1];
        }
        else
            statValueIcon.color = Color.clear;

        statIndex = index;
    }
}
