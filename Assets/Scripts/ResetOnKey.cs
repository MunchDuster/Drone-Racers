using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ResetOnKey : MonoBehaviour
{
    public KeyCode key = KeyCode.R;

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(key))
		{
			Reset();
		}
    }

	public void Reset()
	{
		int curScene = SceneManager.GetActiveScene().buildIndex;
		SceneManager.LoadScene(curScene);
	}
}
