using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
	[SerializeField] Image image;

    void Start()
    {
        
    }

    void Update()
    {
		this.transform.LookAt(Camera.main.transform.position, Vector3.up);
    }

	public void SetAmount(float amount)
	{
		this.gameObject.SetActive(amount != 1);
		this.image.fillAmount = amount;
	}
}
