using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossDieExplosion : MonoBehaviour
{
    private Animator animator;

    private void Start()
    {
        animator = GetComponent<Animator>();
        StartCoroutine(DestroyOnAnimationEnd());
    }

    IEnumerator DestroyOnAnimationEnd()
    {
        // 현재 애니메이션 정보 가져오기
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);

        // 애니메이션 길이만큼 대기
        yield return new WaitForSeconds(stateInfo.length);

        // 오브젝트 파괴
        Destroy(gameObject);
    }
}
