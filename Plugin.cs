using System;
using UnityEngine;
using TMPro;
using System.Collections;
using Utilla;
using BepInEx;
using UnityEngine.Networking;

namespace BibleMod
{
    [BepInPlugin(PluginInfo.GUID, PluginInfo.Name, PluginInfo.Version)]
    public class BibleVerses : BaseUnityPlugin
    {
        public const string GUID = "grzybeksigma.BibleMod";
        public const string Name = "Bible Verses";
        public const string Version = "1.0.0";

        TMP_Text motdText;
        string currentVerse = "PSALM 77:11";
        string currentText = "I WILL REMEMBER THE DEEDS OF THE LORD; YES, I WILL REMEMBER YOUR WONDERS OF OLD";
        bool motdReady;

        void Start()
        {
            Utilla.Events.GameInitialized += GameStart;
            Utilla.Events.RoomJoined += RoomJoin;
            StartCoroutine(FindMOTD());
        }

        IEnumerator FindMOTD()
        {
            while (!motdReady)
            {
                var environment = GameObject.Find("Environment Objects");
                if (environment == null) yield break;

                var localObjects = environment.transform.Find("LocalObjects_Prefab");
                if (localObjects == null) yield break;

                var treeRoom = localObjects.Find("TreeRoom");
                if (treeRoom == null) yield break;

                var motdTransform = treeRoom.Find("motdtext");
                if (motdTransform == null) yield break;

                motdText = motdTransform.GetComponent<TMP_Text>();
                if (motdText != null)
                {
                    motdReady = true;
                    motdText.alignment = TextAlignmentOptions.Center;
                    UpdateText();
                }

                if (!motdReady) yield return new WaitForSeconds(1f);
            }
        }

        void OnEnable() => StartCoroutine(GetVerse());

        void GameStart(object sender, EventArgs e) => StartCoroutine(GetVerse());
        void RoomJoin(object sender, EventArgs e) => StartCoroutine(GetVerse());

        IEnumerator GetVerse()
        {
            using (var web = UnityWebRequest.Get("https://bible-api.com/?random=verse"))
            {
                web.timeout = 5;
                yield return web.SendWebRequest();

                if (web.result == UnityWebRequest.Result.Success)
                {
                    var data = JsonUtility.FromJson<BibleData>(web.downloadHandler.text);
                    currentVerse = data.reference.ToUpper();
                    currentText = data.text.ToUpper();
                }

                UpdateText();
            }
        }

        void UpdateText()
        {
            if (motdReady && motdText != null)
            {
                motdText.text = $"<color=purple>{currentVerse}\n{currentText}</color>";
            }
        }

        [System.Serializable]
        class BibleData
        {
            public string reference;
            public string text;
        }

        public static class PluginInfo
        {
            public const string GUID = "grzybeksigma.BibleMod";
            public const string Name = "Bible Verses";
            public const string Version = "1.0.0";
        }
    }
}