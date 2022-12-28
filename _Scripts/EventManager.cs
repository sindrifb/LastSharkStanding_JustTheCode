using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using System;

public class EventManager : MonoBehaviour
{
    private Dictionary<EventCodes, UnityEvent> eventDictionary;
    private static EventManager EM;

    public static EventManager Instance
    {
        get
        {
            if (!EM)
            {
                EM = FindObjectOfType(typeof(EventManager)) as EventManager;

                if (!EM)
                {
                    //Debug.LogError("There needs to be one active EventManger script on a GameObject in your scene.");
                }
                else
                {
                    EM.Initialize();
                }
            }

            return EM;
        }
    }

    internal static void StartListening(EventCodes sceneRestart)
    {
        throw new NotImplementedException();
    }

    void Initialize()
    {
        if (eventDictionary == null)
        {
            eventDictionary = new Dictionary<EventCodes, UnityEvent>();
        }
    }

    public static void StartListening(EventCodes pCode, UnityAction listener)
    {
        UnityEvent thisEvent = null;
        if (Instance.eventDictionary.TryGetValue(pCode, out thisEvent))
        {
            thisEvent.AddListener(listener);
        }
        else
        {
            thisEvent = new UnityEvent();
            thisEvent.AddListener(listener);
            Instance.eventDictionary.Add(pCode, thisEvent);
        }
    }

    public static void StopListening(EventCodes pCode, UnityAction listener)
    {
        if (EM == null) return;
        UnityEvent thisEvent = null;
        if (Instance.eventDictionary.TryGetValue(pCode, out thisEvent))
        {
            thisEvent.RemoveListener(listener);
        }
    }

    public static void TriggerEvent(EventCodes pCode)
    {
        UnityEvent thisEvent = null;
        if (Instance.eventDictionary.TryGetValue(pCode, out thisEvent))
        {
            thisEvent?.Invoke();
        }
    }

    public enum EventCodes
    {
        NoEvent,
        RoundStart,
        RoundEnd,
        GameStart,
        GameEnd,
        PlayersCleared
    }
}