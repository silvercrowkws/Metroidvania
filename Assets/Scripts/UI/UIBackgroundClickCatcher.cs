using UnityEngine;
using UnityEngine.EventSystems;

public class UIBackgroundClickCatcher : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        // 슬롯 외부 아무 곳 클릭 시 모든 슬롯에 알림
        Slot.OnAnyUIClickedOutside?.Invoke();
    }
}
