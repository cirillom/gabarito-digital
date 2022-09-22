using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ChangeScene : MonoBehaviour
{
    public void ChangeTheScene(int index)
    {
        SceneManager.LoadScene(index);
    }

    public void ResetPrefs()
    {
        PlayerPrefs.DeleteAll();
    }
}
