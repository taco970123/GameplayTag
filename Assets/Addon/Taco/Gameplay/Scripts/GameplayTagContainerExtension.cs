#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Taco.Gameplay
{
    public partial class GameplayTagContainer
    {
        public void AddTag(GameplayTagInfo tagToAdd)
        {
            if (Application.isPlaying)
            {
                AddTagRuntime(tagToAdd.Name);
            }
            else if (!ContainsTag(tagToAdd.Name))
            {
                for (int i = Tags.Count - 1; i >= 0; i--)
                {
                    GameplayTagInfo tag = m_GameplayTagData.NameToInfo(Tags[i]);
                    if (tagToAdd.Name.StartTagIs(tag.Name))
                    {
                        Tags.RemoveAt(i);
                        TagGuids.Remove(tag.Guid);
                        tag.Reference.Remove(ReferencePath);
                    }
                }
                Tags.Add(tagToAdd.Name);
                TagGuids.Add(tagToAdd.Guid);
                tagToAdd.Reference.Add(ReferencePath);
                Tags = Tags.OrderBy(tag => tag).ToList();
                OnValueChanged?.Invoke();
                m_GameplayTagData.ChangeReference();
            }
        }
        public void RemoveTag(GameplayTagInfo tagToRemove)
        {
            if (Application.isPlaying)
            {
                RemoveTagRuntime(tagToRemove.Name);
            }
            else if (Tags.Contains(tagToRemove.Name))
            {
                Tags.Remove(tagToRemove.Name);
                TagGuids.Remove(tagToRemove.Guid);
                tagToRemove.Reference.Remove(ReferencePath);
                OnValueChanged?.Invoke();
                m_GameplayTagData.ChangeReference();
            }
        }
        public void RemoveTagWithChild(GameplayTagInfo tagToRemove)
        {
            if (Application.isPlaying)
            {
                RemoveTagWithChildRuntime(tagToRemove.Name);
            }
            else
            {
                for (int i = Tags.Count - 1; i >= 0; i--)
                {
                    if (Tags[i].StartTagIs(tagToRemove.Name))
                        RemoveTag(m_GameplayTagData.NameToInfo(Tags[i]));
                }
            }
        }
        public void ClearTags()
        {
            if (Application.isPlaying)
            {
                ClearTagRuntime();
            }
            else
            {
                foreach (var tag in Tags)
                {
                    m_GameplayTagData.NameToInfo(tag).Reference.Remove(ReferencePath);
                }
                Tags.Clear();
                TagGuids.Clear();
                OnValueChanged?.Invoke();
                m_GameplayTagData.ChangeReference();
            }
        }
    }
}
#endif