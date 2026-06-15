using dynamicscroll;
using UnityEngine;

namespace UIs.Chat
{
    public class ChatDynamicScrollRect: DynamicScrollRect
    {
        
        protected override Vector2 CalculateOffset(Vector2 delta)
        {   
            var mViewBounds = new Bounds(viewRect.rect.center, viewRect.rect.size);
            return InternalCalculateOffset(ref mViewBounds, ref delta);
        }

        
        internal override Vector2 InternalCalculateOffset(ref Bounds viewBounds, ref Vector2 delta)
        {
            var offset = Vector2.zero;
            if (!needElasticReturn)
                return offset;
        
            var max = new Vector2((content.anchoredPosition.x - clampedPosition.x) + content.rect.width / 2, content.anchoredPosition.y + content.rect.height - viewBounds.max.y);
            var min = new Vector2(content.anchoredPosition.x - content.rect.width / 2, (content.anchoredPosition.y) - viewBounds.max.y);
        
            if (horizontal)
            {
                min.x += delta.x;
                max.x += delta.x;
                if (min.x > viewBounds.min.x)
                    offset.x = viewBounds.min.x - min.x;
                else if (max.x < viewBounds.max.x)
                    offset.x = viewBounds.max.x - max.x;
            }
        
            if (vertical)
            {
                min.y += delta.y;
                max.y += delta.y;
        
                if (max.y < viewBounds.max.y)
                    offset.y = viewBounds.max.y - max.y;
                else if (min.y > viewBounds.min.y)
                    offset.y = viewBounds.min.y - min.y;
            }
        
            return offset;
        }
    }
}