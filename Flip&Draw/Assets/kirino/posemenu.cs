using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class posemenu : MonoBehaviour
{
    public void change_button() //change_button�Ƃ������O�ɂ��܂�
    {
        SceneManager.LoadScene("Pose");//second���Ăяo���܂�
    }
}
