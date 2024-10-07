using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Core.Views
{
    public class TileDrag : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public event Action<Vector3> OnTileMoved;

        public void OnBeginDrag(PointerEventData eventData)
        {
            Debug.Log("OnBeginDrag");
        }
        
        public void OnDrag(PointerEventData eventData)
        {
            transform.position = Camera.main.ScreenToWorldPoint(eventData.position);
            transform.position = new Vector3(transform.position.x, transform.position.y, 0);
            OnTileMoved?.Invoke(transform.position);
        }
        
        public void OnEndDrag(PointerEventData eventData)
        {
            Debug.Log("OnEndDrag");
        }
    }
}