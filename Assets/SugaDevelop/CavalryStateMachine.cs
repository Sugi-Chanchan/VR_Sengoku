using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CavalryStateMachine : StateMachineBehaviour
{

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        int random = Random.Range(0, 3);
        animator.SetInteger("AttackNumber", random);
    }
}
