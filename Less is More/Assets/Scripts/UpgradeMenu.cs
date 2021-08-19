using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UpgradeMenu : MonoBehaviour
{
    public static bool IsPaused;

    [SerializeField] private GameObject upgradeMenu;
    [SerializeField] private GameObject upgradeSpellOne, upgradeSpellTwo, upgradeSpellThree, doNotUpgrade;

    public void PauseUnpause()
    {
        if (!upgradeMenu.activeInHierarchy)
        {
            IsPaused = true;

            upgradeMenu.SetActive(true);
            Time.timeScale = 0f;

            // Clear selected object
            EventSystem.current.SetSelectedGameObject(null);

            // Set new selected object
            EventSystem.current.SetSelectedGameObject(upgradeSpellOne);
        }
        else
        {
            StartCoroutine(Unpause());

            upgradeMenu.SetActive(false);
            Time.timeScale = 1f;
        }
    }

    private IEnumerator Unpause()
    {
        yield return new WaitForSeconds(0.5f);
        IsPaused = false;
    }
}
