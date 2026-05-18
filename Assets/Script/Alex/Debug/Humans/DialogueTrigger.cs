using System;
using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{

    public event Action<bool> OnDialoguePossible;

    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Player")) {
            //Debug.Log("Player entered dialogue trigger");
            OnDialoguePossible?.Invoke(true);
        }
    }

    private void OnTriggerExit(Collider other) {
        if (other.CompareTag("Player")) {
            //Debug.Log("Player exited dialogue trigger");
            OnDialoguePossible?.Invoke(false);
        }
    }

    private void OnTriggerStay(Collider other) {
        //Debug.Log("Player inside dialogue trigger");
    }
}
