using System;
using System.Collections.Generic;
using UnityEngine;

namespace Taco.Gameplay
{
    public static class GameplayTagUtility
    {
        public static bool StartTagIs(this string tag,string targetTag)
        {
            if (tag == targetTag)
                return true;
            else if(tag.StartsWith(targetTag))
            {
                var str = tag.Substring(0, Mathf.Min(tag.Length, targetTag.Length + 1));
                return str[str.Length - 1] == '.';
            }
            else
                return false;
        }
        public static bool EndTagIs(this string tag, string targetTag)
        {
            if (tag == targetTag)
                return true;
            else if (tag.EndsWith(targetTag))
            {
                var str = tag.Substring(0, Mathf.Max(0, tag.Length - targetTag.Length));
                return str[str.Length - 1] == '.';
            }
            else
                return false;
        }


        /// <summary>
        /// Get parent tag
        /// </summary>
        /// <param name="childTag"></param>
        /// <returns></returns>
        public static string GetParentTag(string childTag)
        {
            var splitStrings = childTag.Split('.');
            if (splitStrings.Length == 1)
                return string.Empty;
            else
            {
                string parentTag =  childTag.Substring(0, childTag.Length - splitStrings[splitStrings.Length - 1].Length - 1);
                return parentTag;
            }
        }

        /// <summary>
        /// Get parent tags (exclude self)
        /// </summary>
        /// <param name="childTag"></param>
        /// <returns></returns>
        public static string[] GetParentTags(string childTag)
        {
            var splitStrings = childTag.Split('.');
            string[] parents = new string[splitStrings.Length - 1];

            for (var i = splitStrings.Length - 1; i > 0; i--)
            {
                childTag = childTag.Substring(0, childTag.Length - splitStrings[i].Length - 1);
                parents[i - 1] = childTag;
            }
            return parents;
        }

        /// <summary>
        /// Get children tags (exclude self)
        /// </summary>
        /// <param name="parentTag">parentTag</param>
        /// <param name="tags">probablyChildTags</param>
        /// <returns></returns>
        public static string[] GetChildrenTags(string parentTag, List<string> tags)
        {
            string[] children = new string[tags.Count];
            int index = 0;
            foreach (var tag in tags)
            {
                if (tag.StartTagIs(parentTag) && tag != parentTag)
                {
                    children[index] = tag;
                    index++;
                }
            }
            Array.Resize(ref children, index);
            return children;
        }

        /// <summary>
        /// Get middle tags (exclude self)
        /// </summary>
        /// <param name="parentTag"></param>
        /// <param name="childTag"></param>
        /// <returns></returns>
        public static string[] GetMiddleTags(string parentTag, string childTag)
        {
            var splitParentStrings = parentTag.Split(".");
            var splitChildrenStrings = childTag.Split('.');

            int length = splitChildrenStrings.Length - splitParentStrings.Length - 1;
            string[] middleTags = new string[length];

            for (var i = splitChildrenStrings.Length - 1; i > splitParentStrings.Length; i--)
            {
                childTag = childTag.Substring(0, childTag.Length - splitChildrenStrings[i].Length - 1);
                middleTags[i - splitParentStrings.Length - 1] = childTag;
            }

            return middleTags;
        }
    }
}