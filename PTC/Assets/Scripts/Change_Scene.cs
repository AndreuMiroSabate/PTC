using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Change_Scene : MonoBehaviour
{
    public void changeScene(string scenename )
    {
        SceneManager.LoadScene(scenename);
    }

    public void DisableObject(GameObject gameObjectToDisable)
    {
        gameObjectToDisable.SetActive(false);
    }
}
