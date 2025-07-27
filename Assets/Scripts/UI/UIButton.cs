using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(UIInputReceiver))]
public class UIButton : Button
{
	private InputReceiver reciever;
	protected override void Awake()
	{
		reciever = GetComponent<UIInputReceiver>();
		onClick.AddListener(() => reciever.OnInputRecieved());
	}
}
