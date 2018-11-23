using System;
using System.Collections;
using System.Text;
using System.Xml;
using UnityEngine;
using UnityEngine.Networking;

namespace f3
{

    public class AutoUpdate
    {
        public static void DoUpdateCheck(Cockpit activeCockpit, float fDelay)
        {
            AutoUpdateBehavior up = activeCockpit.RootGameObject.AddComponent<AutoUpdateBehavior>();
            up.parent = activeCockpit;
            up.CheckForUpdatesAndShowMessage(fDelay);
        }
    }


    public class AutoUpdateBehavior : MonoBehaviour
    {
        public Cockpit parent;

        public void CheckForUpdatesAndShowMessage(float fDelay)
        {
            StartCoroutine(DoAsyncVersionCheck(fDelay));
        }


        IEnumerator DoAsyncVersionCheck(float fDelay)
        {
            bool bShowUpdateMessage = false;

            yield return new WaitForSeconds(fDelay);

            UnityWebRequest webRequest = UnityWebRequest.Get("http://www.gradientspace.com/s/simplex_version.txt");
            yield return webRequest.Send();

            try { 
                if (webRequest.isNetworkError) {
                    DebugUtil.Warning("AutoUpdate : webRequest failed : " + webRequest.error);
                } else {
                    byte[] data = webRequest.downloadHandler.data;
                    string xml = Encoding.UTF8.GetString(data);
                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(xml);

                    bool bOK = false;
                    XmlNode n1 = doc.SelectSingleNode("SimplexVersion");
                    if (n1 != null) {
                        XmlNode n2 = n1.SelectSingleNode("Current");
                        if (n2 != null) {
                            string version = n2.InnerText;
                            int nVersion;
                            if (int.TryParse(version, out nVersion)) {
                                bOK = true;
                                if (nVersion > SimplexConfig.CurrentVersion)
                                    bShowUpdateMessage = true;
                            }
                        }
                    }

                    if (!bOK)
                        DebugUtil.Warning("AutoUpdate : error reading/parsing downloaded data");
                }
            } catch ( Exception e ) {
                DebugUtil.Warning("AutoUpdate : exception trying to hande webreqeuest response : " + e.Message);
            }


            if ( bShowUpdateMessage ) {
                HUDUtil.ShowToastPopupMessage("A new version of Simplex is available! You should update!", parent);
            }

            UnityEngine.Component.Destroy(parent.RootGameObject.GetComponent<AutoUpdateBehavior>());
        }

    }
}
