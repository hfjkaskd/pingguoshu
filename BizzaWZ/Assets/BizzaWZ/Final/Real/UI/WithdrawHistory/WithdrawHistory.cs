#if BIZZA_REAL_WITHDRAW
using System;
using System.Collections;
using System.Collections.Generic;
using Bizza;
using UnityEngine;
using UnityEngine.UI;

public partial class UIPageIds
{
    public static readonly PageId WithdrawHistory = "WithdrawHistory";
}


public class WithdrawHistory : UIPageBase
{
    public WithdrawHistoryItem item;
    public Transform root;
    public GameObject emptyHint;

    private List<WithdrawHistoryItem> items = new List<WithdrawHistoryItem>();

    public RectTransform rectTransform;

    [SerializeField] private BizzaButton closeButton;
    protected override void OnAwake()
    {
        base.OnAwake();
        closeButton.onClick.AddListener(() => { CloseSelf(); });
    }

    protected override void OnOpen()
    {
        OnRefresh();
    }

    private void OnRefresh()
    {
        AccountModule.Instance.Request_WithdrawalRecordRequest(Refresh);
    }

    private void Refresh(FailHttpResponse<List<AccountModule.OceanShineWithdrawalRecord>> response)
    {
        if (!this)
        {
            return;
        }
        if (response.success && response.data != null)
        {

            int count = 0;
            if (response.success && response.data != null)
            {
                count = response.data.Count;
            }

            items.SetCmptListCount(item, root, count);
            for (int i = 0; i < count; i++)
            {
                items[i].Init(response.data[i]);
            }
        }
        // else
        // {
        //     UIModule.Instance.ClosePage(UIPageIds.WithdrawHistory);
        // }

        emptyHint.SetActive(response.data == null || response.data.Count == 0);

        LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
    }

    protected override void OnClose()
    {
        emptyHint.SetActive(false);
    }

}
#endif