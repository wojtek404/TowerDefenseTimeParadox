using UnityEngine;
using System.Collections;

public class RootMotionOff : StateMachineBehaviour 
{
	public float ColliderTestTime;

	override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) 
	{
		animator.applyRootMotion = false;
	}

	override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) 
	{
		animator.applyRootMotion = true;
	}
}
