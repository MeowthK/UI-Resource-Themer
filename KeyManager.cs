using System;
using System.Collections.Generic;
using System.Windows;

namespace UI_Resource_Themer
{
    public class KeyManager
    {
        private HashSet<string> keyCombinations = new HashSet<string>();
        public event EventHandler<KeyCombinationEvent> KeyProcessed;

        public KeyManager(UIElement element)
        {
            element.PreviewKeyDown += (obj, ev) => keyCombinations.Add(ev.Key + "");

            element.PreviewKeyUp += (obj, ev) =>
            {
                keyCombinations.Add(ev.Key + "");

                var keyStr = string.Empty;
                foreach (var key in keyCombinations)
                    keyStr += key + "+";

                KeyCombinationEvent evt = new KeyCombinationEvent { KeyCombinations = keyStr.Remove(keyStr.Length - 1) };

                EventHandler<KeyCombinationEvent> handler = KeyProcessed;
                handler?.Invoke(this, evt);

                keyCombinations.Clear();
            };
        }

        public static bool CheckKeys(string combination, string qualifier)
        {
            string[] combinationS = combination.Split('+');
            string[] qualifierS = qualifier.Split('+');

            if (combinationS.Length != qualifierS.Length)
                return false;

            Array.Sort(combinationS);
            Array.Sort(qualifierS);

            for (int i = 0; i < combinationS.Length; i++)
            {
                if (combinationS[i] != qualifierS[i])
                    return false;
            }

            return true;
        }
    }

    public class KeyCombinationEvent : EventArgs
    {
        public string KeyCombinations { get; set; }
    }
}
