using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace FlatEarth
{
    public class EventManager : MonoBehaviour
    { 
        public class EventMessage
        {
            public readonly int id;
            public readonly Node node;

        public EventMessage(Node node, int id)
        {
            this.node = node; // Node reference
            this.id = id; // Sender Id
        }
    }
    [System.Serializable]
    public class Message : UnityEvent<EventMessage>
    {
    };
    
    private Dictionary<string, Message> events;

    private static EventManager _eventManager;

    private static EventManager Instance
    {
        get
        {
            if (_eventManager)
            {
                return _eventManager;
            }

            _eventManager = FindObjectOfType(typeof(EventManager)) as EventManager;

            if (!_eventManager)
            {
                Debug.LogError("There needs to be one active EventManager script on a GameObject in your scene.");
            }
            else
            {
                if (_eventManager != null)
                {
                    _eventManager.Init();
                }
            }

            return _eventManager;
        }
    }

    private void Init()
    {
        if (events == null)
        {
            events = new Dictionary<string, Message>();
        }
    }

    public static void StartListening(string eventName, UnityAction<EventMessage> listener)
    {
        if (Instance.events.TryGetValue(eventName, out var thisEvent))
        {
            thisEvent.AddListener(listener);
        }
        else
        {
            thisEvent = new Message();
            thisEvent.AddListener(listener);
            Instance.events.Add(eventName, thisEvent);
        }
    }
    
    
    public static void StopListening(string eventName, UnityAction<EventMessage> listener)
    {
        if (_eventManager == null)
        {
            return;
        }

        if (Instance.events.TryGetValue(eventName, out var thisEvent))
        {
            thisEvent.RemoveListener(listener);
        }
    }

    public static void TriggerEvent(string eventName, EventMessage eventMessage)
    {
        if (Instance.events.TryGetValue(eventName, out var thisEvent))
        {
           // Debug.Log("Event triggered:" + eventName);
            thisEvent.Invoke(eventMessage);
        }
    }
}
}
