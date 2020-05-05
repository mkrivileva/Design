using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ChangeScene : MonoBehaviour
{
    private Dictionary<string, int> SceneCode = new Dictionary<string, int> 
                { { "Services Scene", 1 }, { "Navigation Scene", 2 }, { "Timetable Scene", 3 } };
    private Image image;
    public void ChangeToScene(string sceneToChangeTo) {
        image = GameObject.Find("BottomNav").transform.Find("Image").gameObject.GetComponent<Image>();
        Animator animator = image.GetComponent<Animator>();
        string currentScene = SceneManager.GetActiveScene().name;
        //UnityEngine.Debug.Log(SceneCode[currentScene] * 10 + SceneCode[sceneToChangeTo]);
        animator.SetInteger("State", SceneCode[currentScene] * 10 + SceneCode[sceneToChangeTo]);
        UnityEngine.Debug.Log(animator.GetInteger("State"));
        SceneManager.LoadScene(sceneToChangeTo);
    }

}
