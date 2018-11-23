using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using UnityEngine;
using g3;

namespace f3
{
    public delegate void FolderChangeEventHandler(IconFilesystemPanel newFolder);
    public delegate void FileClickedEventHandler(string sFilename);


    public class IconFilesystemPanel
    {
        public string FolderPath { get; set; }
        public FileSystemTreeSource Source { get; set; }
        public float IconSize { get; set; }
        public float IconPadding { get; set; }

        Cockpit activeCockpit;
        float activeHUDRadius;

        HUDCollection IconCollection { get; set; }
        float fScroll = 0;
        float fCurMaxScroll = 999;

        public event FolderChangeEventHandler OnFolderChanged;
        public event FileClickedEventHandler OnFileClicked;
        public event FileClickedEventHandler OnFileDoubleClicked;

        public IconFilesystemPanel()
        {
            FolderPath = "C:\\";
            Source = new FileSystemTreeSource();
            IconSize = 0.08f;
            IconPadding = 0.008f;
        }

        public void Initialize(Cockpit cockpit, float fCockpitRadius)
        {
            activeCockpit = cockpit;
            activeCockpit.PositionMode = Cockpit.MovementMode.Static;
            activeCockpit.GrabFocus = true;
            activeHUDRadius = fCockpitRadius;

            string[] files, folders;
            try {
                files = Source.GetFiles(FolderPath);
                folders = Source.GetFolders(FolderPath);
            } catch (Exception e) {
                Debug.Log("[CurrentFolderList.Initialize] exception! " + e.Message);
                return;
            }

            float fMinHorz = -45.0f, fMaxHorz = 45.0f;
            float fStartVert = 15.0f;
            float fTop = HUDUtil.GetSphereFrame(fCockpitRadius, 0.0f, fStartVert).Origin.y;

            int folderi = 0, filei = 0;

            Mesh folderMesh = Resources.Load<Mesh>("icon_meshes/folder_v1");
            Color folderColor = ColorUtil.make(241, 213, 146);
            Color inaccessibleFolderColor = ColorUtil.make(100, 100, 100);
            Mesh fileMesh = Resources.Load<Mesh>("icon_meshes/file_v1");
            Color fileColor = ColorUtil.make(250, 250, 250);

            // [TODO] something wrong here, icons are loading backwards...??
            Quaternion meshRotation =
                Quaternion.AngleAxis(270.0f, Vector3.right) *
                Quaternion.AngleAxis(180.0f + 25.0f, Vector3.forward);
            float meshScale = IconSize * 0.9f;

            HUDCylinder hudSurf = new HUDCylinder() { Radius = fCockpitRadius, VerticalCoordIsAngle = false };
            float fStepH = VRUtil.HorizontalStepAngle(fCockpitRadius, 0, IconSize + IconPadding);
            int nStepsHorz = (int)((fMaxHorz - fMinHorz) / fStepH);
            fMinHorz = -(nStepsHorz * fStepH * 0.5f);
            fMaxHorz = (nStepsHorz * fStepH * 0.5f);
            float fStepV = IconSize + IconPadding;


            IconCollection = new HUDCollection();
            IconCollection.Create();

            bool bDone = false;
            int yi = 0;
            while (!bDone) {
                float fCurV = fTop - yi * fStepV;
                yi++;

                for (int xi = 0; xi < nStepsHorz && bDone == false; ++xi) {
                    float fCurH = fMinHorz + ((float)xi + 0.5f) * fStepH;

                    string name = "x";
                    fMesh useMesh = null;
                    Color useColor = Color.white;
                    bool bAccessible = true;
                    bool bIsFile = false;
                    if (folderi < folders.Length) {
                        name = folders[folderi++];
                        useMesh = new fMesh(UnityEngine.Object.Instantiate<Mesh>(folderMesh));
                        useColor = folderColor;
                        if (Source.FilterInaccessibleFolders == false
                            && FileSystemUtils.CanAccessFolder(Path.Combine(FolderPath, name)) == false) {
                            bAccessible = false;
                            useColor = inaccessibleFolderColor;
                        }
                    } else if (filei < files.Length) {
                        name = files[filei++];
                        useMesh = new fMesh(UnityEngine.Object.Instantiate<Mesh>(fileMesh));
                        useColor = fileColor;
                        bIsFile = true;
                    } else {
                        bDone = true;
                        break;
                    }
                    //useColor.a = 0.999f;        // [RMS] can use this to force into alpha pass

                    string displayName = name;
                    if (displayName.Length > 12)
                        displayName = name.Substring(0, 12) + "...";

                    //float TextScale = 0.01f, ShiftX = -IconSize * 0.5f;
                    float TextScale = 0.005f, ShiftX = -IconSize * 0.5f;

                    HUDButton iconButton = HUDBuilder.CreateMeshClickButton(
                        useMesh, useColor, meshScale, meshRotation,
                        hudSurf, fCurH, fCurV,
                        new TextLabelGenerator() { Text = displayName, Scale = TextScale, Translate = new Vector3(ShiftX, 0.0f, 0.0f) });
                    iconButton.Name = name;
                    //cockpit.AddUIElement(iconButton, true);
                    IconCollection.AddChild(iconButton);

                    if (bIsFile) {
                        iconButton.OnClicked += (o, e) => {
                            if (this.OnFileClicked != null)
                                this.OnFileClicked(name);
                        };
                    }

                    if (bAccessible)
                        iconButton.OnDoubleClicked += IconButton_DoubleClick;
                }
            }

            cockpit.AddUIElement(IconCollection, true);
            fCurMaxScroll = Mathf.Max(0, (yi - 4) * fStepV);
        }

        private void IconButton_DoubleClick(object sender, InputEvent e)
        {
            HUDButton button = (HUDButton)sender;
            string sName = button.Name;
            string sNewPath = Path.Combine(FolderPath, sName);
            sNewPath = Path.GetFullPath(sNewPath);
            if (Directory.Exists(sNewPath)) {
                Dismiss();

                // create new folder source
                IconFilesystemPanel list = new IconFilesystemPanel() {
                    IconSize = this.IconSize,
                    FolderPath = sNewPath,
                    Source = this.Source
                };
                list.Initialize(activeCockpit, activeHUDRadius);

                // notify listeners that we have changed folder
                if (OnFolderChanged != null)
                    OnFolderChanged(list);

            } else if (File.Exists(sNewPath)) {
                OnFileDoubleClicked(sNewPath);
            }
        }


        public void Dismiss()
        {
            List<SceneUIElement> vIcons = new List<SceneUIElement>(IconCollection.Children);
            activeCockpit.RemoveUIElement(IconCollection, false);
            IconCollection.RemoveAllChildren();
            GameObject.Destroy(IconCollection.RootGameObject);

            foreach (HUDButton button in vIcons) {
                FolderIconRemover remove = button.RootGameObject.AddComponent<FolderIconRemover>();
                remove.Begin(button);
            }

        }



        public void Scroll(float fDelta)
        {
            IconCollection.RootGameObject.Translate(-fScroll * Vector3.up, true);
            fScroll += fDelta;
            fScroll = Mathf.Clamp(fScroll, 0.0f, fCurMaxScroll);
            IconCollection.RootGameObject.Translate(fScroll * Vector3.up, true);
        }


    }




    class FolderIconRemover : MonoBehaviour
    {
        HUDButton button;
        public void Begin(HUDButton button)
        {
            this.button = button;
            StartCoroutine(Animate(button));
        }

        IEnumerator Animate(HUDButton button)
        {
            yield return null;
            FlyAwayAnimator anim = button.RootGameObject.AddComponent<FlyAwayAnimator>();
            anim.Distance = 2.0f;
            anim.Direction = -Vector3.forward;
            anim.CompleteCallback = Destroy;
            anim.Begin(button, 1.5f);
        }

        void Destroy()
        {
            button.ClearGameObjects(true);
            GameObject.Destroy(button.RootGameObject);
        }
    }



}
