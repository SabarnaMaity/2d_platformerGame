using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ExitTrigger : MonoBehaviour
{
    private bool hasExited = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!hasExited && collision.gameObject.CompareTag("Player"))
        {
            hasExited = true;
            StartCoroutine(LevelExit());
        }
    }

    IEnumerator LevelExit()
    {
        //anim.SetTrigger("Exit");
        yield return new WaitForSeconds(0.1f);

        if (UIManager.instance != null)
        {
            UIManager.instance.fadeToBlack = true;
            yield return new WaitForSeconds(1.5f);
        }
        else
        {
            yield return new WaitForSeconds(0.5f);
        }

        // Trigger level complete flow
        if (GameManager.instance != null)
        {
            GameManager.instance.LevelComplete();
        }
        else
        {
            Debug.LogError("GameManager.instance not found when completing level!");
        }
    }
}
