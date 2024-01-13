using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Taco.Gameplay.Editor
{
    public class GameplayTagEditData : ScriptableObject
    {
        [SerializeField]
        List<GameplayTagEditInfo> m_GameplayTagEditInfos = new List<GameplayTagEditInfo>();   
        public List<GameplayTagEditInfo> GameplayTagEditInfos => m_GameplayTagEditInfos;

        Dictionary<string, GameplayTagEditInfo> m_GameplayTagEditInfoMap = new Dictionary<string, GameplayTagEditInfo>();
        
        TagNode m_RootNode;
        Dictionary<string, TagNode> m_TagNodeMap = new Dictionary<string, TagNode>();

        public Action OnValueChanged;

        public GameplayTagEditInfo this[string tag]
        {
            get
            {
                if (m_GameplayTagEditInfoMap.TryGetValue(tag,out GameplayTagEditInfo gameplayTagEditInfo))
                    return gameplayTagEditInfo;
                else
                    return null;
            }
        }

        public void Init()
        {
            m_GameplayTagEditInfos = m_GameplayTagEditInfos.OrderBy(i => i.Name).ToList();
            m_GameplayTagEditInfoMap.Clear();

            m_RootNode = new TagNode(string.Empty, null);
            m_TagNodeMap.Clear();
            m_TagNodeMap.Add(string.Empty, m_RootNode);

            foreach (var gameplayTagEditInfo in m_GameplayTagEditInfos)
            {
                string tag = gameplayTagEditInfo.Name;
                m_GameplayTagEditInfoMap.Add(tag, gameplayTagEditInfo);
                AddNode(tag, gameplayTagEditInfo);
            }

            OnValueChanged?.Invoke();
        }
        public void SetExpandedState(string name, bool state)
        {
            if (m_GameplayTagEditInfoMap.ContainsKey(name))
            {
                m_GameplayTagEditInfoMap[name].Expanded = state;
            }
        }
        public void SetMultiState(string name, bool state)
        {
            if (m_GameplayTagEditInfoMap.ContainsKey(name))
            {
                m_GameplayTagEditInfoMap[name].Multi = state;
            }
        }

        public bool Contains(string tag)
        {
            return m_GameplayTagEditInfoMap.ContainsKey(tag);
        }

        public void AddTag(string tag)//A.A.A
        {
            var splitStrings = tag.Split('.');
            string parentTag = tag;//A.A.A
            for (int i = splitStrings.Length - 1; i >= 0; i--)
            {
                if (!Contains(parentTag))
                {
                    GameplayTagEditInfo gameplayTagEditInfo = new GameplayTagEditInfo(parentTag, true, false);
                    GameplayTagEditInfos.Add(gameplayTagEditInfo);
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

            m_GameplayTagEditInfos.Remove(tagNode.GameplayTagEditInfo);
            m_RootNode.Update(string.Empty);
            Init();
        }
        public void RemoveTag(string tagName)
        {
            for (int i = GameplayTagEditInfos.Count - 1; i >= 0; i--)
            {
                string childTag = GameplayTagEditInfos[i].Name;
                if (childTag.StartTagIs(tagName))
                {
                    GameplayTagEditInfos.RemoveAt(i);
                    m_GameplayTagEditInfoMap.Remove(childTag);
                }
            }
            Init();
        }
        public void ChangeTag(string oldTag, string newTag,string newShortTag)
        {
            TagNode oldTagNode = m_TagNodeMap[oldTag];
            if (m_TagNodeMap.TryGetValue(newTag, out TagNode newTagNode))
            {
                GameplayTagEditInfo oldGameplayTagEditInfo = m_GameplayTagEditInfoMap[oldTag];
                GameplayTagEditInfos.Remove(oldGameplayTagEditInfo);
                
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
        public void MoveTag(string movingTag,string targetParentTag)
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

        public List<GameplayTagEditInfo> GetChildTagInfos(string parentTag, bool includeSelf)
        {
            List<GameplayTagEditInfo> childTags = new List<GameplayTagEditInfo>();
            foreach (var gameplayTagEditInfo in m_GameplayTagEditInfos)
            {
                if (gameplayTagEditInfo.Name.StartTagIs(parentTag) && (gameplayTagEditInfo.Name != parentTag || includeSelf))
                    childTags.Add(gameplayTagEditInfo);
            }
            return childTags;
        }
        
        TagNode AddNode(string tag, GameplayTagEditInfo gameplayTagEditInfo)
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
            if (parentNode.Children.Find(i=>i.ShortName == childNode.ShortName) is TagNode tagNode)
            {
                GameplayTagEditInfos.Remove(childNode.GameplayTagEditInfo);
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
            public GameplayTagEditInfo GameplayTagEditInfo;
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

            public TagNode(string name, GameplayTagEditInfo gameplayTagEditInfo)
            {
                var splits = name.Split('.');
                ShortName = splits[splits.Length - 1];
                ParentName = GameplayTagUtility.GetParentTag(ShortName);
                GameplayTagEditInfo = gameplayTagEditInfo;
            }
            public void Update(string parentName)
            {
                ParentName = parentName;
                if(GameplayTagEditInfo != null)
                    GameplayTagEditInfo.Name = Name;
                Children.ForEach(x => x.Update(Name));
            }
        }
    }

    [Serializable]
    public class GameplayTagEditInfo
    {
        public string Name;
        public bool Expanded;
        public bool Multi;

        public GameplayTagEditInfo(string name, bool expanded, bool multi)
        {
            Name = name;
            Expanded = expanded;
            Multi = multi;
        }
    }
}