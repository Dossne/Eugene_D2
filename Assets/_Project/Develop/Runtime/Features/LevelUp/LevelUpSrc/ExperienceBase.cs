using System;
using System.Collections.Generic;
using UnityEngine;

namespace Features.LevelUp
{
    public class ExperienceBase
    {
        private class State
        {
            public int level = 1;
            public int currentExp = 0;
        }

        public event Action OnLevelUp;
        public event Action OnExpAdded;

        private readonly List<int> config;

        private State currentState;

        private int nextExpTarget;
        private int prevExpTarget;


        public ExperienceBase(List<int> config)
        {
            currentState = new State();
            this.config = config;
        }


        public int Level => currentState.level;
        public int CurrentExp => currentState.currentExp;
        public int NextExp => nextExpTarget;
        public float CurrenProgress => GetCurrentProgress();


        public void Initialize()
        {
            InitExpTargets();
        }


        public void Add(int value)
        {
            if (value < 0)
            {
                Debug.LogError("GAME DESIGN Exp value cannot be less than zero");
                return;
            }

            currentState.currentExp += value;

            if (CanLevelUp())
            {
                InitNextLevel();
                InitExpTargets();
                OnLevelUp?.Invoke();
            }
            else
            {
                OnExpAdded?.Invoke();
            }
        }


        public int GetNextLevelExperience()
        {
            return nextExpTarget - currentState.currentExp;
        }


        public void Reset()
        {
            currentState = new State();
            InitExpTargets();
        }


        private bool IsLastLevel()
        {
            return Level >= config.Count;
        }


        private void InitNextLevel()
        {
            int levelByConfig = GetLevelByConfig();

            if (levelByConfig > currentState.level)
                currentState.level = levelByConfig;
        }


        private void InitExpTargets()
        {
            if (IsLastLevel())
            {
                nextExpTarget = 0;
                prevExpTarget = 0;
                return;
            }

            SetExpTargets();
        }


        private bool CanLevelUp()
        {
            return !IsLastLevel() && currentState.currentExp >= nextExpTarget;
        }


        private float GetCurrentProgress()
        {
            if (nextExpTarget == 0)
            {
                return 1;
            }

            return (currentState.currentExp - prevExpTarget) / (float)(nextExpTarget - prevExpTarget);
        }


        private void SetExpTargets()
        {
            int nextIndex = Level;
            int currentIndex = Level - 1;
            int maxIndex = config.Count - 1;

            prevExpTarget = 0;
            nextExpTarget = 0;

            if (maxIndex <= 0)
            {
                return;
            }

            for (int i = 0; i <= currentIndex; i++)
            {
                prevExpTarget += config[i];
            }

            if (nextIndex >= maxIndex)
            {
                nextExpTarget = prevExpTarget + config[maxIndex];
                return;
            }

            nextExpTarget = prevExpTarget + config[nextIndex];
        }


        private int GetLevelByConfig()
        {
            if (currentState.currentExp == 0)
                return 1;

            int totalConfigExp = 0;
            for (int i = 0; i < config.Count; i++)
            {
                totalConfigExp += config[i];
                if (totalConfigExp > currentState.currentExp)
                {
                    return i;
                }
            }

            return config.Count;
        }
    }
}