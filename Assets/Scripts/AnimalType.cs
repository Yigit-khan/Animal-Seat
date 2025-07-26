using UnityEngine;

// Bu enum, kodumuzu daha okunaklý ve hatasýz yapar.
public enum AnimalType { Yirtici, Otobur, Notr, Savunmaci, Islak, IslakSevmeyen }

[CreateAssetMenu(fileName = "YeniHayvan", menuName = "Animal Jam/Hayvan Verisi")]
public class AnimalData : ScriptableObject
{
    [Header("Temel Bilgiler")]
    public string hayvanAdi;
    public AnimalType turu;

    [Tooltip("Bu hayvanýn 3D modelini içeren prefab.")]
    public GameObject hayvanModelPrefab; // 2D Sprite yerine 3D Model Prefab'ý

    [Header("Oynanýþ Kurallarý")]
    [Tooltip("Bu hayvan otobüste kaç koltuk kaplar?")]
    public int kapladigiKoltukSayisi = 1;
}