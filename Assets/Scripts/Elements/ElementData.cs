using UnityEngine;

[CreateAssetMenu(fileName = "DefaultElementData", menuName = "ScriptableObjects/ElementData")]
public class ElementData : ScriptableObject
{
    public ElementalTypes type;
    public int elementalDmg;
    public Color elementColor;
    public float bulletSpeed;
}
