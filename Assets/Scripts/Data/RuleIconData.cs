using UnityEngine;

public enum InteractionType { Likes, Dislikes, Neutral }
public enum RuleSpecificity { SingleType, AnyType } // Kuralýn belirli bir türe mi yoksa herkese mi uygulandýðýný belirtir.

[CreateAssetMenu(fileName = "YeniKuralIkonu", menuName = "Animal Jam/Kural Ýkon Verisi")]
public class RuleIconData : ScriptableObject
{
    [Tooltip("Bu kural hangi hayvan türünden kaynaklanýyor?")]
    public AnimalType SourceAnimal;

    [Tooltip("Bu kural herkese mi yoksa belirli bir türe mi uygulanýyor?")]
    public RuleSpecificity Specificity;

    [Tooltip("Eðer Specificity 'SingleType' ise, kuralýn hedefi olan hayvan türü.")]
    public AnimalType TargetAnimal; // Sadece Specificity == SingleType ise kullanýlýr.

    [Tooltip("Etkileþim türü (olumlu, olumsuz).")]
    public InteractionType Interaction;

    [Tooltip("Düþünce balonunda gösterilecek ikon.")]
    public Sprite Icon;

    [TextArea(3, 5)] // Inspector'da daha geniþ bir metin alaný saðlar.
    [Tooltip("Baloncuða týklandýðýnda gösterilecek açýklama metni.")]
    public string Description;
}