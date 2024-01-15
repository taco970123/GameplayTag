using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Taco.Gameplay
{
    public class GameplayTagData : ScriptableObject
    {
        [SerializeField]
        List<GameplayTagInfo> m_GameplayTagInfos = new List<GameplayTagInfo>();
        public List<GameplayTagInfo> GameplayTagInfos => m_GameplayTagInfos;

        Dictionary<string, GameplayTagInfo> m_NameTagInfoMap = new Dictionary<string, GameplayTagInfo>();
        Dictionary<string, GameplayTagInfo> m_GuidTagInfoMap = new Dictionary<string, GameplayTagInfo>();

#if UNITY_EDITOR
        TagNode m_RootNode;
        Dictionary<string, TagNode> m_TagNodeMap = new Dictionary<string, TagNode>();
        public Action OnValueChanged;
        public Action OnReferenceChanged;
#endif
        public GameplayTagInfo this[string tag]
        {
            get
            {
                if (m_NameTagInfoMap.TryGetValue(tag,out GameplayTagInfo gameplayTagEditInfo))
                    return gameplayTagEditInfo;
                else
                    return null;
            }
        }


        public void Init()
        {
            m_GameplayTagInfos = m_GameplayTagInfos.OrderBy(i => i.Name).ToList();
            m_NameTagInfoMap.Clear();
            m_GuidTagInfoMap.Clear();

#if UNITY_EDITOR
            m_RootNode = new TagNode(string.Empty, null);
            m_TagNodeMap.Clear();
            m_TagNodeMap.Add(string.Empty, m_RootNode);
#endif

            foreach (var gameplayTagEditInfo in m_GameplayTagInfos)
            {
                m_NameTagInfoMap.Add(gameplayTagEditInfo.Name, gameplayTagEditInfo);
                m_GuidTagInfoMap.Add(gameplayTagEditInfo.Guid, gameplayTagEditInfo);

#if UNITY_EDITOR
                AddNode(gameplayTagEditInfo.Name, gameplayTagEditInfo);
#endif
            }

#if UNITY_EDITOR
            OnValueChanged?.Invoke();
            OnReferenceChanged?.Invoke();
#endif
        }
        public bool Contains(string tag)
        {
            return m_NameTagInfoMap.ContainsKey(tag);
        }

        public GameplayTagInfo NameToInfo(string name)
        {
            if (m_NameTagInfoMap.TryGetValue(name, out GameplayTagInfo gameplayTagEditInfo))
                return gameplayTagEditInfo;
            else
                return null;
        }
        public GameplayTagInfo GuidToInfo(string guid)
        {
            if (m_GuidTagInfoMap.TryGetValue(guid, out GameplayTagInfo gameplayTagEditInfo))
                return gameplayTagEditInfo;
            else
                return null;
        }
        public string NameToGuid(string name)
        {
            return NameToInfo(name)?.Guid ?? string.Empty;
        }
        public string GuidToName(string guid)
        {
            return GuidToInfo(guid)?.Name ?? string.Empty;
        }

#if UNITY_EDITOR

        public void AddTag(string tag)//A.A.A
        {
            var splitStrings = tag.Split('.');
            string parentTag = tag;//A.A.A
            for (int i = splitStrings.Length - 1; i >= 0; i--)
            {
                if (!Contains(parentTag))
                {
                    GameplayTagInfo gameplayTagEditInfo = new GameplayTagInfo(parentTag, true, false);
                    GameplayTagInfos.Add(gameplayTagEditInfo);
                }
                parentTag = parentTag.Substring(0, Mathf.Max(parentTag.Length - splitStrings[i].Length - 1, 0));//A.A.A=>A.A
            }
            Init();
        }
        public void RemoveTagWithoutChildren(string tag)
        {
            TagNode tagNode = m_TagNodeMap[tag];
            TagNode parentNode = tagNode.Parent;
            UnlinkNode(tagNode.Parent, tagNode);

            for (int i = tagNode.Children.Count - 1; i >= 0; i--)
            {
                TagNode childNode = tagNode.Children[i];
                LinkNode(parentNode, childNode);
            }

            m_GameplayTagInfos.Remove(tagNode.GameplayTagEditInfo);
            m_RootNode.Update(string.Empty);
            Init();
        }
        public void RemoveTag(string tagName)
        {
            for (int i = GameplayTagInfos.Count - 1; i >= 0; i--)
            {
                string childTag = GameplayTagInfos[i].Name;
                if (childTag.StartTagIs(tagName))
                {
                    GameplayTagInfos.RemoveAt(i);
                    m_NameTagInfoMap.Remove(childTag);
                }
            }
            Init();
        }
        public void ChangeTag(string oldTag, string newTag, string newShortTag)
        {
            TagNode oldTagNode = m_TagNodeMap[oldTag];
            if (m_TagNodeMap.TryGetValue(newTag, out TagNode newTagNode))
            {
                GameplayTagInfo oldGameplayTagEditInfo = m_NameTagInfoMap[oldTag];
                GameplayTagInfos.Remove(oldGameplayTagEditInfo);

                UnlinkNode(oldTagNode.Parent, oldTagNode);
                for (int i = oldTagNode.Children.Count - 1; i >= 0; i--)
                {
                    TagNode childNode = oldTagNode.Children[i];
                    LinkNode(newTagNode, childNode);
                }
            }
            else
            {
                oldTagNode.ShortName = newShortTag;
            }

            m_RootNode.Update(string.Empty);
            Init();
        }
        public void MoveTag(string movingTag, string targetParentTag)
        {
            TagNode oldTagNode = m_TagNodeMap[movingTag];
            TagNode targetParentTagNode = m_TagNodeMap[targetParentTag];
            LinkNode(targetParentTagNode, oldTagNode);
            m_RootNode.Update(string.Empty);
            Init();
        }
        public void MoveToRoot(string movingTag)
        {
            TagNode oldTagNode = m_TagNodeMap[movingTag];
            LinkNode(m_RootNode, oldTagNode);
            m_RootNode.Update(string.Empty);
            Init();
        }
        public void SetExpandedState(string name, bool state)
        {
            if (m_NameTagInfoMap.ContainsKey(name))
            {
                m_NameTagInfoMap[name].Expanded = state;
            }
        }
        public void SetMultiState(string name, bool state)
        {
            if (m_NameTagInfoMap.ContainsKey(name))
            {
                m_NameTagInfoMap[name].Multi = state;
            }
        }
        public List<GameplayTagInfo> GetChildTagInfos(string parentTag, bool includeSelf)
        {
            List<GameplayTagInfo> childTags = new List<GameplayTagInfo>();
            foreach (var gameplayTagEditInfo in m_GameplayTagInfos)
            {
                if (gameplayTagEditInfo.Name.StartTagIs(parentTag) && (gameplayTagEditInfo.Name != parentTag || includeSelf))
                    childTags.Add(gameplayTagEditInfo);
            }
            return childTags;
        }
        public void ChangeReference()
        {
            OnReferenceChanged?.Invoke();
        }

        TagNode AddNode(string tag, GameplayTagInfo gameplayTagEditInfo)
        {
            TagNode tagNode = new TagNode(tag, gameplayTagEditInfo);
            string parentTag = GameplayTagUtility.GetParentTag(tag);
            if (string.IsNullOrEmpty(parentTag))
                LinkNode(m_RootNode, tagNode);
            else
                LinkNode(m_TagNodeMap[parentTag], tagNode);
            m_TagNodeMap.Add(tag, tagNode);
            return tagNode;
        }
        void LinkNode(TagNode parentNode, TagNode childNode)
        {
            UnlinkNode(childNode.Parent, childNode);
            if (parentNode.Children.Find(i => i.ShortName == childNode.ShortName) is TagNode tagNode)
            {
                GameplayTagInfos.Remove(childNode.GameplayTagEditInfo);
                for (int i = childNode.Children.Count - 1; i >= 0; i--)
                {
                    TagNode child = childNode.Children[i];
                    LinkNode(tagNode, child);
                }
            }
            else
            {
                parentNode.Children.Add(childNode);
                childNode.Parent = parentNode;
            }
        }
        void UnlinkNode(TagNode parentNode, TagNode childNode)
        {
            parentNode?.Children.Remove(childNode);
            childNode.Parent = null;
        }

        class TagNode
        {
            public string ParentName;
            public string ShortName;
            public TagNode Parent;
            public List<TagNode> Children = new List<TagNode>();
            public GameplayTagInfo GameplayTagEditInfo;
            public string Name
            {
                get
                {
                    if (Parent == null || string.IsNullOrEmpty(Parent.Name))
                        return ShortName;
                    else
                        return ParentName + '.' + ShortName;
                }
            }

            public TagNode(string name, GameplayTagInfo gameplayTagEditInfo)
            {
                var splits = name.Split('.');
                ShortName = splits[splits.Length - 1];
                ParentName = GameplayTagUtility.GetParentTag(ShortName);
                GameplayTagEditInfo = gameplayTagEditInfo;
            }
            public void Update(string parentName)
            {
                ParentName = parentName;
                if (GameplayTagEditInfo != null)
                    GameplayTagEditInfo.Name = Name;
                Children.ForEach(x => x.Update(Name));
            }
        }
#endif
    }

    [Serializable]
    public class GameplayTagInfo
    {
        public string Name;
        public string Guid;

#if UNITY_EDITOR
        public bool Expanded;
        public bool Multi;
        public List<string> Reference;

        public GameplayTagInfo(string name, bool expanded, bool multi)
        {
            Name = name;
            Guid = System.Guid.NewGuid().ToString();
            Expanded = expanded;
            Multi = multi;
            Reference = new List<string>();
        }
    }
#endif
}