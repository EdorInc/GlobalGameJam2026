using UnityEngine;
using UnityEngine.SceneManagement;

public class AdditiveSceneTrigger : MonoBehaviour
{
    [SerializeField] private string sceneToLoad;
    [SerializeField] private bool loadOnlyOnce = true;

    private bool hasLoaded = false;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (loadOnlyOnce && hasLoaded)
            return;

        hasLoaded = true;
        LoadScene();
    }

    private void LoadScene()
    {
        if (!SceneManager.GetSceneByName(sceneToLoad).isLoaded)
        {
            SceneManager.LoadSceneAsync(sceneToLoad, LoadSceneMode.Additive);
        }
    }
}
