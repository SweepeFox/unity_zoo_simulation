using System.Collections.Generic;

using UnityEngine;

namespace Constants
{
    public enum AnimalType
    {
        PREY,
        PREDATOR
    }

    public struct AnimalData
    {
        public string Name;
        public AnimalType Type;
        public string PrefabAddress;

        public float Speed;

        public float JumpInterval;
        public float JumpDistance;
    }

    public static class AnimalsData
    {
        public static List<AnimalData> Animals = new() {
            new() {
                Name = "Snake",
                Type = AnimalType.PREDATOR,
                PrefabAddress = "Assets/Project/Prefabs/Animals/Snake.prefab",
                Speed = 10f,
                JumpInterval = 0f,
                JumpDistance = 0f
            },
            new() {
                Name = "Frog",
                Type = AnimalType.PREY,
                PrefabAddress = "Assets/Project/Prefabs/Animals/Frog.prefab",
                Speed = 0f,
                JumpInterval = 2f,
                JumpDistance = 5f
            }
        };

        public static AnimalData GetRandomAnimalData() => Animals[Random.Range(0, Animals.Count)];
    }
}
