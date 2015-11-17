using UnityEngine;
using System.Collections;

public class AssetManager : MonoBehaviour {

    public static AssetManager Main;
    public AudioSource audioSource;
    public AudioClip[] music;
    public AudioClip[] uiSFX;

    void Awake()
    {
        Main = this;
        audioSource = GetComponent<AudioSource>();
    }
    // Use this for initialization
    void Start () {
        audioSource.clip = music[0];
        audioSource.loop = true;
        audioSource.Play();
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
