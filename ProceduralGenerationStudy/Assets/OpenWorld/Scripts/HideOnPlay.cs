﻿using UnityEngine;

namespace OpenWorld
{
	public class HideOnPlay : MonoBehaviour {

		// Use this for initialization
		void Start () {
			gameObject.SetActive (false);
		}
	}
}
