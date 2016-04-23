
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class CharacterDemoController : MonoBehaviour 
{
	Animator animator;
	public GameObject		floorPlane;
	public int 				WeaponState=0;
	public bool 			wasAttacking;
	float				rotateSpeed = 20.0f;
	public Vector3 		movementTargetPosition;
	public Vector3 		attackPos;
	public Vector3		lookAtPos;
	float				gravity = 0.3f;
	RaycastHit hit;
	Ray ray;
    public bool rightButtonDown = false;

	void Start () 
	{	
		animator = GetComponentInChildren<Animator>();
		movementTargetPosition = transform.position;
	}

	void Update () 
	{
		if ( ! Input.GetKey(KeyCode.LeftAlt))
		{
			if(Input.GetMouseButton(0))
			{
				ray = Camera.main.ScreenPointToRay (Input.mousePosition);
				if(floorPlane.GetComponent<Collider>().Raycast(ray, out hit, 500.0f)) 											
				{
					movementTargetPosition = hit.point;
					wasAttacking = false;
				}
			}
		}
		
		switch(Input.inputString)
		{						 
			case "0":
				WeaponState = 0;
				break;
			case "1":
				WeaponState = 1;
				break;
			case "2":
				WeaponState = 2;
				break;
			case "3":
				WeaponState = 3;
				break;
			case "4":
				WeaponState = 4;
				break;
			case "5":
				WeaponState = 5;
				break;
			case "6":
				WeaponState = 6;
				break;
			case "7":
				WeaponState = 7;
				break;
			case "8":
				WeaponState = 8;
				break;
			
			case "p":
				animator.SetTrigger("Pain");
				break;
			case "a":
				animator.SetInteger("Death", 1);
				break;
			case "b":
				animator.SetInteger("Death", 2);
				break;
			case "c":
				animator.SetInteger("Death", 3);
				break;
			case "n":
				animator.SetBool("NonCombat", true);
				break;
			default:
				break;
		}
		
		animator.SetInteger("WeaponState", WeaponState);
		
		if ( ! Input.GetKey(KeyCode.LeftAlt))
		{
			if(Input.GetMouseButton(1))
			{
				if(rightButtonDown != true)
				{
					
					ray = Camera.main.ScreenPointToRay (Input.mousePosition);
					if(floorPlane.GetComponent<Collider>().Raycast(ray, out hit, 500.0f)) 												
					{
						movementTargetPosition = transform.position; 
						attackPos = hit.point;
						attackPos.y = transform.position.y;
						Vector3 attackDelta = attackPos - transform.position;
						attackPos = transform.position + attackDelta.normalized * 20.0f;
						animator.SetTrigger("Use");
						animator.SetBool("Idling", true);
						rightButtonDown = true;
						wasAttacking =true;
					}
				}
			}
		}
		
		if (Input.GetMouseButtonUp(1))
		{
			if (rightButtonDown == true)
			{
				rightButtonDown = false;
			}
		}
		
		Debug.DrawLine ((movementTargetPosition + transform.up*2), movementTargetPosition);
		Vector3 deltaTarget = movementTargetPosition - transform.position;
		if(!wasAttacking)
		{
			lookAtPos = transform.position + deltaTarget.normalized*2.0f;
			lookAtPos.y = transform.position.y;
		}
		else
		{
			lookAtPos = attackPos;
		}

		Quaternion tempRot = transform.rotation;
		transform.LookAt(lookAtPos);						
		Quaternion hitRot = transform.rotation;
		transform.rotation = Quaternion.Slerp(tempRot, hitRot, Time.deltaTime * rotateSpeed);
		if(Vector3.Distance(movementTargetPosition,transform.position)>0.5f)
		{
			animator.SetBool("Idling", false);
		}
		else
		{
			animator.SetBool("Idling", true);
		}
	}
	
	void OnGUI()
	{
		string tempString = "LMB=move RMB=attack p=pain abc=deaths 12345678 0=change weapons";
		GUI.Label (new Rect (10, 5,1000, 20), tempString);
	}
}
