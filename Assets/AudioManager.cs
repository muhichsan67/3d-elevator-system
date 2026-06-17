using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [Header("Audio Sources")]
    public AudioSource effectSource;
    public AudioSource machineSource;

    [Header("Audio Clips")]
    public AudioClip clickSound;
    public AudioClip deniedSound;
    public AudioClip hummingSound;
    public AudioClip dingSound;

    // --- FUNGSI UNTUK MEMUTAR SUARA ---

    public void PlayClick()
    {
        effectSource.PlayOneShot(clickSound);
    }

    public void PlayDenied()
    {
        effectSource.PlayOneShot(deniedSound);
    }

    public void PlayDing()
    {
        effectSource.PlayOneShot(dingSound);
    }

    public void StartHumming()
    {
        machineSource.clip = hummingSound;
        machineSource.loop = true; // Membuat suara mesin/dengungan berulang (loop)
        machineSource.Play();
    }

    public void StopHumming()
    {
        machineSource.Stop();
    }
}