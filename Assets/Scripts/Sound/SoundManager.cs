using UnityEngine.Audio;
using System;
using UnityEngine;

// Bu script, oyundaki tüm ses ve müzik yönetiminden sorumludur.
public class SoundManager : MonoBehaviour
{

    public bool sfxEnabled = true; // settings panel için, baþlangýçta sesler açýk olacak.

    // Singleton (Tekil Nesne) yapýsý için statik referans.
    public static SoundManager Instance;

    [Header("Ses Listeleri")]
    [Tooltip("Arkaplan müzikleri bu listeye eklenmelidir.")]
    public Sound[] musicTracks;

    [Tooltip("Kýsa ses efektleri (buton týklama, hayvan sesi vb.) bu listeye eklenmelidir.")]
    public Sound[] sfxSounds;

    void Awake()
    {
        #region Singleton Deseni
        // Eðer baþka bir SoundManager yoksa, bunu ana SoundManager yap ve sahneler arasýnda koru.
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        // Eðer zaten bir SoundManager varsa ve bu o deðilse, bu kopyayý yok et.
        // Bu, ana menüye geri dönüldüðünde yeni bir SoundManager oluþmasýný engeller.
        else if (Instance != this)
        {
            Destroy(gameObject);
            return; // Kalan kodun çalýþmasýný engelle.
        }
        #endregion

        // Müzikler için AudioSource bileþenlerini oluþtur.
        foreach (Sound s in musicTracks)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;
            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.loop = s.loop;
        }

        // SFX'ler için AudioSource bileþenlerini oluþtur.
        foreach (Sound s in sfxSounds)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;
            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.loop = s.loop;
        }

        ApplySFXState();
    }

    /// <summary>
    /// Belirtilen isimdeki müziði çalar. Çalmadan önce diðer tüm müzikleri durdurur.
    /// </summary>
    /// <param name="name">Çalýnacak müziðin Inspector'da verilen ismi.</param>
    public void PlayMusic(string name)
    {
        // Ýsimle eþleþen sesi listede bul.
        Sound s = Array.Find(musicTracks, sound => sound.name == name);
        if (s == null)
        {
            Debug.LogWarning("SoundManager: '" + name + "' isimli MÜZÝK bulunamadý!");
            return;
        }

        // Diðer müzikleri durdur.
        foreach (var music in musicTracks)
        {
            if (music.source.isPlaying)
                music.source.Stop();
        }

        // Ýstenen müziði çal.
        s.source.Play();
    }


    /// <summary>
    /// Belirtilen isimdeki ses efektini çalar.
    /// </summary>
    /// <param name="name">Çalýnacak ses efektinin Inspector'da verilen ismi.</param>
    public void PlaySFX(string name)
    {
        Debug.Log("SFX Enabled? " + sfxEnabled);
        if (!sfxEnabled)
        {
            Debug.Log("SFX kapalý, ses çalmýyor.");
            return; //ayarlardan sound kapatýlmýþ ise çalmasýn.
        }
           
        // Ýsimle eþleþen sesi listede bul.
        Sound s = Array.Find(sfxSounds, sound => sound.name == name);
        if (s == null)
        {
            Debug.LogWarning("SoundManager: '" + name + "' isimli SFX bulunamadý!");
            return;
        }
        s.source.Play();
    }

    public void ApplySFXState()
    {
        // sfxEnabled false ise hepsini sustur ve gerekirse durdur.
        foreach (var s in sfxSounds)
        {
            if (s?.source == null) continue;

            s.source.mute = !sfxEnabled;

            if (!sfxEnabled && s.source.isPlaying)
                s.source.Stop();
        }
    }

}


// Bu sýnýf, ses dosyalarýný ve ayarlarýný bir arada tutmamýzý saðlar.
// [System.Serializable] sayesinde Inspector'da görünebilir.
[System.Serializable]
public class Sound
{
    [Tooltip("Bu sesi koddan çaðýrmak için kullanýlacak benzersiz isim.")]
    public string name;

    [Tooltip("Çalýnacak ses dosyasý.")]
    public AudioClip clip;

    [Range(0f, 1f)]
    [Tooltip("Sesin varsayýlan ses yüksekliði.")]
    public float volume = 1f;

    [Range(.1f, 3f)]
    [Tooltip("Sesin varsayýlan perdesi.")]
    public float pitch = 1f;

    [Tooltip("Sesin döngüye girip girmeyeceði (genellikle müzikler için true).")]
    public bool loop = false;

    // Oluþturulacak AudioSource bileþenini referans olarak tutar. Inspector'da gizlidir.
    [HideInInspector]
    public AudioSource source;
}