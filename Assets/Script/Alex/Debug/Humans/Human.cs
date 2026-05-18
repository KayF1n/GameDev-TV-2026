using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class Human : MonoBehaviour {
    [SerializeField] public PersonDataSO personDataSO;

    public string Name => personDataSO != null ? personDataSO.Name : string.Empty;
    public int Happiness = 4;

    private List<Human> friends = new List<Human>();

    public void AddFriend(Human friend) {
        if (!friends.Contains(friend)) {
            friends.Add(friend);
        }
    }

    public int GetFriendCount() {
        return friends.Count;
    }

    public List<Human> GetFriends() {
        return new List<Human>(friends);
    }

    public void ChangeHappiness(int delta) {
        Happiness += delta;
        Happiness = Mathf.Clamp(Happiness, 0, 10);
    }
}