using System.Collections.Generic;
using UnityEngine;
using Taco.Gameplay;

public class TestSelector : MonoBehaviour
{
    public List<Test> Units;

    [SerializeField]
    GameplayTagContainer Selector;


    void Start()
    {
        Selector.OnValueChanged += Select;
    }

    void Select()
    {
        Units.ForEach(i => i.Select(false));
        foreach (var unit in Units)
        {
            if (unit.Tag.Contains(Selector))
                unit.Select(true);
        }
    }
}
