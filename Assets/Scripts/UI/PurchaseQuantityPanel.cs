using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PurchaseQuantityPanel : MonoBehaviour
{
    Button cancelButton;

    private void Awake()
    {
        Transform child = transform.GetChild(3);
        cancelButton = child.GetComponent<Button>();
        cancelButton.onClick.AddListener(Cancel);
    }

    private void Cancel()
    {
        this.gameObject.SetActive(false);
    }
}
