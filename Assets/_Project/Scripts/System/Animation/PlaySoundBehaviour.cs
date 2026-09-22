using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaySoundBehaviour : StateMachineBehaviour
{
    private AudioSource audioSource;
    public AudioClip audioSound;
    public bool loop = false;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        audioSource = animator.transform.GetComponent<AudioSource>();
        audioSource.clip = audioSound;
        audioSource.loop = loop;
        audioSource.Play();
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        audioSource.Stop();
        audioSource.loop = false;
    }
}
