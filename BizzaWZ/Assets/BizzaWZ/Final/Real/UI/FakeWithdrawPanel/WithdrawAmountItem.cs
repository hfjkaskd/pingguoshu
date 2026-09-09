#if BIZZA_REAL_WITHDRAW
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

 
public class WithdrawAmountItem : MonoBehaviour
{
    public TMP_Text amountTxt;

    public GameObject getObj;
    public GameObject getedObj;

    public GameObject selectObj;
    public BizzaButton btn;
    private int _index;

    public void Init(FakeWithdrawPanel panel, int index, string amount, bool canGet)
    {
        _index = index;
        amountTxt.text = amount;
        Refresh(canGet);
        btn.onClick.AddListener(() =>
        {
            if (_index == 0 && panel.isReward)
            {
                return;
            }
            panel.SetSelectIndex(_index);
            panel.OnRefresh();
        });
    }
    
    public void Refresh(bool canGet)
    {
        getObj.SetActive(canGet);
        getedObj.SetActive(!canGet && _index == 0);
    }

    public void OnSelectState(bool isSelect)
    {
        selectObj.SetActive(isSelect);
    }

    public void SetSelectState(bool isSelect)
    {
        selectObj.SetActive(isSelect);
    }
}
#endif