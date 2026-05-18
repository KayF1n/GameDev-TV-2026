using UnityEngine;

[CreateAssetMenu(fileName = "NewPersonData", menuName = "Dialogue/Person Data")]
public class PersonDataSO : ScriptableObject {
    [SerializeField] private string personName;
    public string Name => personName;

    [SerializeField] private Sprite portrait;
    public Sprite Portrait => portrait;
}