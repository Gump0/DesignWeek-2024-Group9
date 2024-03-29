using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace team09
{
    public class EventManager : MicrogameEvents
    {
        //Should the events be randomized or done in order
        //If true: one event will be selected randomly when the prior event ends (can only run one event at a time)
        //If false: events will begin at their set start time and end when their duration is complete
        public bool randomized;

        public int testEvent;

        //List of bullet events
        public List<BulletEvent> events;

        //Time at which the manager is initialized
        private double startTime = -1;
        //The currently playing event
        //Only used in randomized mode
        private BulletEvent currentEvent;

        // Start is called before the first frame update
        void Start()
        {
            //Initialize bullet events
            foreach (BulletEvent e in events)
            {
                e.Initialize();
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (startTime <= -0.1 || events.Count <= 0) return;

            double activeTime = Time.timeAsDouble - startTime;

            if (randomized)
            {
                //If there is no currently playing event, or the duration of the current event has completed, choose a new random event
                if (currentEvent == null || activeTime > currentEvent.startTime + currentEvent.duration)
                {
                    currentEvent = events[UnityEngine.Random.Range(0, events.Count)];
                    //currentEvent = events[testEvent];
                    currentEvent.startTime = (float)activeTime;
                }

                //Run the current event
                currentEvent.run();
            }
            else
            {
                foreach (BulletEvent e in events)
                {
                    //If the event has already completed, ignore it
                    if (activeTime > e.startTime + e.duration) continue;
                    //If the event has started, run it
                    if (activeTime > e.startTime)
                    {
                        e.run();
                    }
                }
            }
        }

        public void Activate()
        {
            startTime = Time.timeAsDouble;
        }

        public void Deactivate()
        {
            startTime = -1;
        }
    }

    //Custom editor made following this tutorial: https://www.youtube.com/watch?v=RImM7XYdeAc
    //Considering the scope of the project, just pretend this is a magic black box and don't touch it
    //I don't wanna go through and comment this mess
#if UNITY_EDITOR
    [CustomEditor(typeof(EventManager)), CanEditMultipleObjects]
    public class EventManagerEditor : Editor
    {
        private bool listToggle;
        private List<bool> elementToggles = new List<bool>();
        private List<bool> spawnPointsToggles = new List<bool>();

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EventManager manager = (EventManager)target;

            EditorGUILayout.BeginHorizontal();
            listToggle = EditorGUILayout.Foldout(listToggle, "Events", true);
            int size = Mathf.Max(0, EditorGUILayout.IntField(manager.events.Count));
            EditorGUILayout.EndHorizontal();

            if (listToggle)
            {
                while (size > manager.events.Count)
                {
                    manager.events.Add(null);
                }
                while (size < manager.events.Count)
                {
                    manager.events.RemoveAt(manager.events.Count - 1);
                }

                while (manager.events.Count > elementToggles.Count)
                {
                    elementToggles.Add(false);
                    spawnPointsToggles.Add(false);
                }
                while (manager.events.Count < elementToggles.Count)
                {
                    elementToggles.RemoveAt(elementToggles.Count - 1);
                    spawnPointsToggles.RemoveAt(spawnPointsToggles.Count - 1);
                }

                for (int i = 0; i < manager.events.Count; i++)
                {
                    elementToggles[i] = EditorGUILayout.Foldout(elementToggles[i], "Event " + i, true);

                    if (elementToggles[i])
                    {
                        bool transformsToggle = spawnPointsToggles[i];
                        DrawPattern(manager.events[i], ref transformsToggle);
                        spawnPointsToggles[i] = transformsToggle;

                        DrawBehaviour(manager.events[i]);

                        EditorGUILayout.LabelField("Timing");

                        if (!manager.randomized)
                        {
                            manager.events[i].startTime = EditorGUILayout.FloatField("Start", manager.events[i].startTime);
                        }
                        manager.events[i].duration = EditorGUILayout.FloatField("Duration", manager.events[i].duration);

                        EditorGUILayout.Space();
                    }
                }
            }
        }

        private static void DrawPattern(BulletEvent bulletEvent, ref bool transfomsToggle)
        {
            EditorGUILayout.LabelField("Pattern");

            bulletEvent.patternType = (PatternType)EditorGUILayout.EnumPopup("Bullet Pattern Type", bulletEvent.patternType);

            if(bulletEvent.behaviourType == BehaviourType.RandomSpawnPoint || bulletEvent.behaviourType == BehaviourType.RandomSpawnPointAimed)
            {
                EditorGUILayout.BeginHorizontal();
                transfomsToggle = EditorGUILayout.Foldout(transfomsToggle, "Spawn Points", true);
                int size = Mathf.Max(0, EditorGUILayout.IntField(bulletEvent.spawnPoints.Count));
                EditorGUILayout.EndHorizontal();

                if (transfomsToggle)
                {
                    while(size > bulletEvent.spawnPoints.Count)
                    {
                        bulletEvent.spawnPoints.Add(null);
                    }
                    while(size < bulletEvent.spawnPoints.Count)
                    {
                        bulletEvent.spawnPoints.RemoveAt(bulletEvent.spawnPoints.Count - 1);
                    }

                    for(int i = 0; i < bulletEvent.spawnPoints.Count; i++)
                    {
                        bulletEvent.spawnPoints[i] = (Transform)EditorGUILayout.ObjectField("Spawn Point " + i, bulletEvent.spawnPoints[i], typeof(Transform), true);
                    }
                }
            }
            else
            {
                bulletEvent.spawnPoint = (Transform)EditorGUILayout.ObjectField("Spawn Point", bulletEvent.spawnPoint, typeof(Transform), true);
            }

            bulletEvent.bulletSpeed = EditorGUILayout.FloatField("Bullet Speed", bulletEvent.bulletSpeed);
            bulletEvent.interval = EditorGUILayout.FloatField("Interval", bulletEvent.interval);
            bulletEvent.bulletPrefab = (GameObject)EditorGUILayout.ObjectField("Bullet Type", bulletEvent.bulletPrefab, typeof(GameObject), true);

            switch (bulletEvent.patternType)
            {
                case PatternType.Ring:
                    bulletEvent.extraArgInt = EditorGUILayout.IntField("Ring Density", bulletEvent.extraArgInt);
                    break;

                case PatternType.RingWithGap:
                    bulletEvent.extraArgInt = EditorGUILayout.IntField("Ring Density", bulletEvent.extraArgInt);
                    bulletEvent.extraArgFloat1 = EditorGUILayout.FloatField("Gap Size", bulletEvent.extraArgFloat1);
                    break;

                case PatternType.Line:
                    bulletEvent.extraArgInt = EditorGUILayout.IntField("Line Density", bulletEvent.extraArgInt);
                    bulletEvent.extraArgFloat1 = EditorGUILayout.FloatField("Line Length", bulletEvent.extraArgFloat1);
                    break;

                case PatternType.LineWithGap:
                    bulletEvent.extraArgInt = EditorGUILayout.IntField("Line Density", bulletEvent.extraArgInt);
                    bulletEvent.extraArgFloat1 = EditorGUILayout.FloatField("Line Length", bulletEvent.extraArgFloat1);
                    bulletEvent.extraArgFloat2 = EditorGUILayout.FloatField("Gap Position", bulletEvent.extraArgFloat2);
                    bulletEvent.extraArgFloat3 = EditorGUILayout.FloatField("Gap Size", bulletEvent.extraArgFloat3);
                    break;

                case PatternType.BentLine:
                    bulletEvent.extraArgInt = EditorGUILayout.IntField("Line Density", bulletEvent.extraArgInt);
                    bulletEvent.extraArgFloat1 = EditorGUILayout.FloatField("Line Length", bulletEvent.extraArgFloat1);
                    bulletEvent.extraArgFloat2 = EditorGUILayout.FloatField("Bend Angle", bulletEvent.extraArgFloat2);
                    break;
            }

            EditorGUILayout.Space();
        }

        private static void DrawBehaviour(BulletEvent bulletEvent)
        {
            EditorGUILayout.LabelField("Behaviour");

            bulletEvent.behaviourType = (BehaviourType)EditorGUILayout.EnumPopup("Bullet Behaviour Type", bulletEvent.behaviourType);

            switch (bulletEvent.behaviourType)
            {
                case BehaviourType.None:
                    bulletEvent.direction = EditorGUILayout.FloatField("Direction", bulletEvent.direction);
                    break;

                case BehaviourType.Aimed:
                    bulletEvent.target = (Transform)EditorGUILayout.ObjectField("Target", bulletEvent.target, typeof(Transform), true);
                    break;

                case BehaviourType.AimedPosition:
                    bulletEvent.direction = EditorGUILayout.FloatField("Direction", bulletEvent.direction);
                    bulletEvent.target = (Transform)EditorGUILayout.ObjectField("Target", bulletEvent.target, typeof(Transform), true);
                    break;

                case BehaviourType.Spiral:
                    bulletEvent.direction = EditorGUILayout.FloatField("Starting Direction", bulletEvent.direction);
                    bulletEvent.intervalAngle = EditorGUILayout.FloatField("Delta Angle", bulletEvent.intervalAngle);
                    break;

                case BehaviourType.MoveLine:
                    bulletEvent.direction = EditorGUILayout.FloatField("Direction", bulletEvent.direction);
                    bulletEvent.intervalDistance = EditorGUILayout.FloatField("Delta Position", bulletEvent.intervalDistance);
                    break;

                case BehaviourType.RandomRotation:
                    bulletEvent.direction = EditorGUILayout.FloatField("Direction", bulletEvent.direction);
                    bulletEvent.variance = EditorGUILayout.FloatField("Angle Variance", bulletEvent.variance);
                    break;

                case BehaviourType.RandomPosition:
                    bulletEvent.direction = EditorGUILayout.FloatField("Direction", bulletEvent.direction);
                    bulletEvent.variance = EditorGUILayout.FloatField("Position Variance", bulletEvent.variance);
                    break;

                case BehaviourType.RandomSpawnPoint:
                    bulletEvent.direction = EditorGUILayout.FloatField("Direction", bulletEvent.direction);
                    break;

                case BehaviourType.RandomSpawnPointAimed:
                    bulletEvent.target = (Transform)EditorGUILayout.ObjectField("Target", bulletEvent.target, typeof(Transform), true);
                    break;
            }

            EditorGUILayout.Space();
        }
    }
#endif
}
