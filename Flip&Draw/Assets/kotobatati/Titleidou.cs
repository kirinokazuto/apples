using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Titleidou : MonoBehaviour
{
    public void change_button() //change_buttonという名前にします
    {
        SceneManager.LoadScene("Game");//secondを呼び出します
    }
}
