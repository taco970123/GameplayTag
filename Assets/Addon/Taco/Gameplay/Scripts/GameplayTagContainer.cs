using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Taco.Gameplay
{
    [Serializable]
    public partial class GameplayTagContainer
    {
        public List<string> TagGuids = new List<string>();
        public string ReferencePath = "CustomData";

        List<string> m_Tags;
        public List<string> Tags
        {
            get
            {
                if(m_Tags == null)
                    Init();
                return m_Tags;
            }
            set => m_Tags = value;
        }

        public Action OnValueChanged;
        GameplayTagData m_GameplayTagData => GameplayTagUtility.GameplayTagData;

        public void Init()
        {
            m_Tags = new List<string>();
            for (int i = TagGuids.Count - 1; i >= 0; i--)
            {
                string tagGuid = TagGuids[i];
                string tag = m_GameplayTagData.GuidToName(tagGuid);
                if (!string.IsNullOrEmpty(tag))
                    m_Tags.Add(tag);
#if UNITY_EDITOR
                //else
                //    TagGuids.RemoveAt(i);
#endif
            }
            OnValueChanged?.Invoke();
        }

        public void AddTagRuntime(string tag)
        {
            if (!ContainsTag(tag))
            {
                for (int i = Tags.Count - 1; i >= 0; i--)
                {
                    if (tag.StartTagIs(Tags[i]))
                        Tags.RemoveAt(i);
                }
                Tags.Add(tag);
                OnValueChanged?.Invoke();
            }
        }
        public void RemoveTagRuntime(string tag)
        {
            if (Tags.Contains(tag))
            {
                Tags.Remove(tag);
                OnValueChanged?.Invoke();
            }
        }
        public void RemoveTagWithChildRuntime(string tag)
        {
            for (int i = Tags.Count - 1; i >= 0; i--)
            {
                if (Tags[i].StartTagIs(tag))
                    RemoveTagRuntime(Tags[i]);
            }
        }
        public void ClearTagRuntime()
        {
            Tags.Clear();
            OnValueChanged?.Invoke();
        }

        public bool ContainsTag(string tag)
        {
            return Tags.Contains(tag) || HasChildTag(tag);
        }
        public bool ContainsTags(IEnumerable<string> requiredTags)
        {
            if (requiredTags.Count() == 0)
                return false;

            foreach (var requiredTag in requiredTags)
            {
                if (!ContainsTag(requiredTag))
                    return false;
            }
            return true;
        }
        public bool Contains(GameplayTagContainer selector)
        {
            return ContainsTags(selector.Tags);
        }
        public bool HasChildTag(string parentTag)
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