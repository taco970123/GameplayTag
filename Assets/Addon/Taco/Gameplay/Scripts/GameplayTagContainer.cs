using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Taco.Gameplay
{
    [Serializable]
    public class GameplayTagContainer
    {
        public List<string> Tags = new List<string>();

        public Action OnValueChanged;
        public void AddTag(string tag)
        {
            if (!Is(tag))
            {
                for (int i = Tags.Count - 1; i >= 0; i--)
                {
                    if (tag.StartTagIs(Tags[i]))
                        Tags.RemoveAt(i);
                }
                Tags.Add(tag);

#if UNITY_EDITOR
                Tags = Tags.OrderBy(tag => tag).ToList();
#endif

                OnValueChanged?.Invoke();
            }
        }
        public void RemoveTag(string tag)
        {
            if (Tags.Contains(tag))
            {
                Tags.Remove(tag);
                OnValueChanged?.Invoke();
            }
        }
        public void RemoveTagWithChild(string tag)
        {
            for (int i = Tags.Count - 1; i >= 0; i--)
            {
                if (Tags[i].StartTagIs(tag))
                    RemoveTag(Tags[i]);
            }
        }
        public void ClearTags()
        {
            Tags.Clear();
            OnValueChanged?.Invoke();
        }

        public bool Is(string tag)
        {
            return Tags.Contains(tag) || HasChild(tag);
        }
        public bool Is(IEnumerable<string> requiredTags)
        {
            if (requiredTags.Count() == 0)
                return false;

            foreach (var requiredTag in requiredTags)
            {
                if (!Is(requiredTag))
                    return false;
            }
            return true;
        }
        public bool Is(GameplayTagContainer selector)
        {
            return Is(selector.Tags);
        }
        public bool HasChild(string parentTag)
        {
            foreach (var tag in Tags)
            {
                if (tag.StartTagIs(parentTag) && tag != parentTag)
                    return true;
            }
            return false;
        }
    }
}