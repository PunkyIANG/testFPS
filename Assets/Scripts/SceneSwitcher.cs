using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSwitcher : MonoBehaviour
{
    void Update() {
        if (Input.GetKeyDown(KeyCode.R))
            SceneManager.LoadScene("MovementTestScene", LoadSceneMode.Single);
    }
}