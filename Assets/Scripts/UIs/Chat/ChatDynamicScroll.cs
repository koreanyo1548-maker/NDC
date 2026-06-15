using System.Collections.Generic;
using UnityEngine;
using dynamicscroll;
using UnityEngine.UI;

namespace UIs.Chat
{
    public class ChatDynamicScroll<T, T1> : DynamicScroll<T, T1> 
        where T : class
            where T1 : DynamicScrollObject<T>
    {
        public override void ChangeList(IList<T> infoList, int startIndex = -1, bool resetContentPosition = false)
        {
         	if (startIndex == -1)
            {
                var highest = GetHighest();
                if (highest == null) startIndex = 0;
                else startIndex = highest.CurrentIndex;
            }
 
            ScrollRect.StopMovement();
            ScrollRect.content.anchoredPosition = Vector2.zero;
 
            var objs = objectPool.GetAllWithState(true);
            objs.ForEach(x => objectPool.Release(x));
             
            this.infoList = infoList;
 
            CreateList(startIndex);
            var height = 0f;
            var count = ScrollRect.content.childCount;
            for (var idx = 0; idx < count; ++idx)
            {
                height += ScrollRect.content.GetChild(idx).GetComponent<RectTransform>().rect.size.y;
            }
            ScrollRect.content.SetSizeWithCurrentAnchors((RectTransform.Axis)1, height);
        }
        
        protected override void DisableGridComponents()
        {
            if (mVerticalLayoutGroup != null)
                mVerticalLayoutGroup.enabled = false;

            if (mHorizontalLayoutGroup != null)
                mHorizontalLayoutGroup.enabled = false;

            if (mContentSizeFitter != null)
                mContentSizeFitter.enabled = false;

            if (mGridLayoutGroup != null)
                mGridLayoutGroup.enabled = false;
        }      

    }
}