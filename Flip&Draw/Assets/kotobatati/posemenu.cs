using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class posemenu : MonoBehaviour
{
    public void change_button() //change_buttonという名前にします
    {
        SceneManager.LoadScene("Pose");//secondを呼び出します
    }
}
