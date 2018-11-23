using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using UnityEngine;
using UnityEngine.Networking;
using System.Threading;
using g3;

namespace f3
{

    public static class FileBrowserMessageCodes
    {
        public static readonly string MESHES_TOO_LARGE = "MESHES_TOO_LARGE";
    }


 


    public class FolderListAdapter
    {
        public IconFilesystemPanel FolderList;

        public Action<IconFilesystemPanel> OnListChanged;

        public Action<string> OnFileClickedAction;
        public Action<string> OnFileDoubleClickedAction;

        public FolderListAdapter()
        {
        }

        public void OnFolderChanged(IconFilesystemPanel newFolder)
        {
            if (FolderList != null) {
                FolderList.OnFolderChanged -= OnFolderChanged;
                if (OnFileClickedAction != null)
                    FolderList.OnFileClicked -= OnFileClickedAction.Invoke;
                if (OnFileDoubleClickedAction != null)
                    FolderList.OnFileDoubleClicked -= OnFileDoubleClickedAction.Invoke;
            }

            FolderList = newFolder;

            FolderList.OnFolderChanged += OnFolderChanged;
            if (OnFileClickedAction != null)
                FolderList.OnFileClicked += OnFileClickedAction.Invoke;
            if (OnFileDoubleClickedAction != null)
                FolderList.OnFileDoubleClicked += OnFileDoubleClickedAction.Invoke;

            if (OnListChanged != null)
                OnListChanged(newFolder);
        }
    }






    public class FileOpenCockpit : ICockpitInitializer
    {
        // configuration
        public string InitialPath { get; set; }
        public string Tooltip = "Enter a filename";
        public string[] ValidExtensions { get; set; }

        // must set thse before Initialize() call
        public float HUDRadius = 1.0f;
        public float IconSize = 0.2f;

        // these are set internally
        public IconFilesystemPanel ListView { get; set; }
        public Cockpit ActiveCockpit { get; set; }

        HUDTextEntry entryField;
        HUDLabel tooltipText;

        public FileOpenCockpit()
        {
            InitialPath = "C:\\";
        }


        // [RMS] you can either listen for this event, 
        //   or you can overload LoadFile in a subclass. 
        //   Either way, don't forget to dismiss the cockpit!
        public event FileClickedEventHandler OnFileClicked;
        virtual public void LoadFile(string sNewPath)
        {
            if ( OnFileClicked != null )
                OnFileClicked(sNewPath);
        }



        public void Initialize(Cockpit cockpit)
        {
            ActiveCockpit = cockpit;
            cockpit.Name = "fileOpenCockpit";

            float fHUDRadius = this.HUDRadius;
            cockpit.DefaultCursorDepth = fHUDRadius;

            ListView = new IconFilesystemPanel() {
                IconSize = this.IconSize,
                FolderPath = InitialPath
            };
            if (ValidExtensions != null) {
                foreach (string s in ValidExtensions)
                    ListView.Source.ValidExtensions.Add(s);
            }
            ListView.Source.FilterInaccessibleFolders = true;
            ListView.Initialize(cockpit, fHUDRadius);

            add_text_entry_field(cockpit);

            // setup interactions
            cockpit.AddKeyHandler(new FileBrowserTextFieldKeyHandler(cockpit.Context, entryField) {
                OnReturn = () => { LoadFile(Path.GetFullPath(Path.Combine(ListView.FolderPath, entryField.Text))); }
            });
            cockpit.InputBehaviors.Add(new VRMouseUIBehavior(cockpit.Context) { Priority = 0 });
            cockpit.InputBehaviors.Add(new VRGamepadUIBehavior(cockpit.Context) { Priority = 0 });
            cockpit.InputBehaviors.Add(new VRSpatialDeviceUIBehavior(cockpit.Context) { Priority = 0 });

            // IconFileSystemPanel objects basically replace themselves in the scene when
            //  the user changes directories. So, we need a way to transfer event handlers
            //  between these objects. This is what FolderListAdapter does.
            FolderListAdapter adapter = new FolderListAdapter() {
                OnFileClickedAction = (s) => { entryField.Text = s; },
                OnFileDoubleClickedAction = (s) => LoadFile(s),
                OnListChanged = (l) => { ListView = l; }
            };
            adapter.OnFolderChanged(ListView);
            cockpit.InputBehaviors.Add(new FolderScrollBehavior(adapter));


            //cockpit.OverrideBehaviors.Add(new ScreenCaptureBehavior() { Priority = 0,
            //    ScreenshotPath = Environment.GetEnvironmentVariable("homepath") + "\\DropBox\\ScreenShots\\" });
            cockpit.OverrideBehaviors.Add(new FBEncoderCaptureBehavior() { Priority = 0,
                ScreenshotPath = Environment.GetEnvironmentVariable("homepath") + "\\DropBox\\ScreenShots\\" });
        }


        public void add_text_entry_field(Cockpit cockpit)
        {
            float fUseRadius = HUDRadius * 0.85f;
            float fAngle = -35.0f;

            entryField = new HUDTextEntry();
            entryField.Text = "";
            entryField.Width = 1.0f;
            entryField.Height = 0.08f;
            entryField.TextHeight = 0.06f;
            entryField.Create();
            entryField.Name = "selectFileName";
            HUDUtil.PlaceInSphere(entryField, fUseRadius, 0.0f, fAngle);
            cockpit.AddUIElement(entryField, true);

            tooltipText = new HUDLabel();
            tooltipText.Shape = new HUDShape(HUDShapeType.Rectangle, 1.0f, 0.04f);
            tooltipText.Text = Tooltip;
            tooltipText.TextHeight = 0.03f;
            tooltipText.BackgroundColor = Colorf.TransparentBlack;
            tooltipText.TextColor = Colorf.Silver;
            tooltipText.Create();
            tooltipText.Name = "tooltip";
            HUDUtil.PlaceInSphere(tooltipText, fUseRadius, 0.0f, fAngle);
            UnityUtil.TranslateInFrame(tooltipText.RootGameObject, 0.0f, -entryField.Height, 0, CoordSpace.WorldCoords);
            cockpit.AddUIElement(tooltipText, true);


            HUDButton loadButton = HUDBuilder.CreateRectIconClickButton(
                 0.2f, 0.1f, fUseRadius, 0.0f, fAngle,
                  "icons/load_v1");
            UnityUtil.TranslateInFrame(loadButton.RootGameObject, 0.2f, -0.1f, 0, CoordSpace.WorldCoords);
            loadButton.Name = "load";
            cockpit.AddUIElement(loadButton, true);
            loadButton.OnClicked += (s, e) => { LoadFromEntryText(); };

            HUDButton cancelButton = HUDBuilder.CreateRectIconClickButton(
                0.2f, 0.1f, fUseRadius, 0.0f, fAngle,
                 "icons/cancel_v1");
            UnityUtil.TranslateInFrame(cancelButton.RootGameObject, 0.4f, -0.1f, 0, CoordSpace.WorldCoords);
            cancelButton.Name = "cancel";
            cockpit.AddUIElement(cancelButton, true);
            cancelButton.OnClicked += (s, e) => {
                cockpit.Context.PopCockpit(true);
            };
        }


        virtual public void LoadFromEntryText()
        {
            if (FileSystemUtils.IsFullFilesystemPath(entryField.Text))
                LoadFile(entryField.Text);
            else if (FileSystemUtils.IsWebURL(entryField.Text))
                LoadURL(entryField.Text);
            else
                LoadFile(Path.Combine(ListView.FolderPath, entryField.Text));
        }



        // Not ready for production yet - this will hang for a long time, throw exceptions, etc, etc
        virtual public void LoadURL(string sURL)
        {
            string sFileURL = sURL.Substring(0, sURL.IndexOf('?'));
            string sExtension = Path.GetExtension(sFileURL);

            UnityWebRequest webRequest = UnityWebRequest.Get(sURL);
            AsyncOperation o = webRequest.Send();
            while (!o.isDone)
                ;
            byte[] data = webRequest.downloadHandler.data;

            string sTempFile = FileSystemUtils.GetTempFilePathWithExtension(sExtension);
            File.WriteAllBytes(sTempFile, data);

            LoadFile(sTempFile);

            File.Delete(sTempFile);
        }



    }




    public class FileImportCockpit : FileOpenCockpit
    {
        public FileImportCockpit()
        {
            ValidExtensions = new string[] { ".obj" };
        }

        public override void LoadFile(string sNewPath)
        {
            // save this path because it is super-annoying otherwise...
            SceneGraphConfig.LastFileOpenPath = ListView.FolderPath;
            if (IsMeshFile(sNewPath)) {
                FContext controller = ActiveCockpit.Context;

                // read mesh file
                SceneMeshImporter import = new SceneMeshImporter();
                bool bOK = import.ReadFile(sNewPath);
                if (bOK == false && import.LastReadResult.code != IOCode.Ok) {
                    Debug.Log("[Import] import failed: " + import.LastReadResult.message);
                    HUDUtil.ShowCenteredPopupMessage("Import Failed!", "Sorry, could not import " + sNewPath, ActiveCockpit);
                    return;
                }
                import.AppendReadMeshesToScene(controller.Scene, true);
                // save undo/redo checkpoint
                controller.Scene.History.PushInteractionCheckpoint();

                // once we have opened file, done with cockpit
                controller.PopCockpit(true);

                // emit this message after we pop cockpit
                if (import.SomeMeshesTooLargeForUnityWarning) {
                    Debug.Log("[Import] some meshes too large! ignored!");
                    MessageStream.Get.AddMessage(new Message() {
                        Type = Message.Types.Warning, Code = FileBrowserMessageCodes.MESHES_TOO_LARGE
                    });
                }
            }
        }


        public bool IsMeshFile(string sPath)
        {
            if (sPath.EndsWith(".obj", StringComparison.CurrentCultureIgnoreCase))
                return true;
            return false;
        }

    }




    public class FileLoadSceneCockpit : FileOpenCockpit
    {
        public FileLoadSceneCockpit()
        {
            ValidExtensions = new string[] { ".xml" };
        }

        public override void LoadFile(string sNewPath)
        {
            // save this path because it is super-annoying otherwise...
            SceneGraphConfig.LastFileOpenPath = ListView.FolderPath;
            FContext controller = ActiveCockpit.Context;

            // read xml 
            try {
                DebugUtil.Log(1, "Loading scene from path " + sNewPath);

                XmlDocument doc = new XmlDocument();
                doc.Load(sNewPath);
                XMLInputStream stream = new XMLInputStream() { xml = doc };
                SceneSerializer serializer = new SceneSerializer();
                serializer.TargetFilePath = sNewPath;
                serializer.SOFactory = new SOFactory();
                serializer.Restore(stream, ActiveCockpit.Scene);

                // once we have opened file, done with cockpit
                controller.PopCockpit(true);

            } catch ( Exception e){
                Debug.Log("[LoadScene] read failed: " + e.Message);
                HUDUtil.ShowCenteredPopupMessage("Load Failed!", "Sorry could not load scene " + sNewPath, ActiveCockpit);
            }

        }


    }





    public class FileSaveCockpit : ICockpitInitializer
    {
        public string InitialPath { get; set; }
        public string Tooltip = "Enter a filename";


        public string[] ValidExtensions { get; set; }

        // must set thse before Initialize() call
        public float HUDRadius = 1.0f;
        public float IconSize = 0.2f;

        // these are set internally
        public IconFilesystemPanel ListView { get; set; }
        public Cockpit ActiveCockpit { get; set; }

        HUDTextEntry entryField;
        HUDLabel tooltipText;

        public FileSaveCockpit()
        {
            InitialPath = "C:\\";
        }


        // [RMS] you can either listen for this event, 
        //   or you can overload LoadFile in a subclass. 
        //   Either way, don't forget to dismiss the cockpit!
        public event FileClickedEventHandler OnFileClicked;
        virtual public void SaveFile(string sNewPath)
        {
            if (OnFileClicked != null)
                OnFileClicked(sNewPath);
        }

        virtual public string GetDefaultFileName()
        {
            return "exmaple.obj";
        }


        public void Initialize(Cockpit cockpit)
        {
            ActiveCockpit = cockpit;
            cockpit.Name = "fileSaveCockpit";

            float fHUDRadius = this.HUDRadius;
            cockpit.DefaultCursorDepth = fHUDRadius;

            ListView = new IconFilesystemPanel() {
                IconSize = this.IconSize,
                FolderPath = InitialPath
            };
            if (ValidExtensions != null) {
                foreach (string s in ValidExtensions)
                    ListView.Source.ValidExtensions.Add(s);
            }
            ListView.Source.FilterInaccessibleFolders = true;
            ListView.Initialize(cockpit, fHUDRadius);

            add_text_entry_field(cockpit);

            // setup interactions
            cockpit.AddKeyHandler(new FileBrowserTextFieldKeyHandler(cockpit.Context, entryField) {
                OnReturn = () => { SaveFile(Path.GetFullPath(Path.Combine(ListView.FolderPath, entryField.Text))); }
            });
            cockpit.InputBehaviors.Add(new VRMouseUIBehavior(cockpit.Context) { Priority = 0 });
            cockpit.InputBehaviors.Add(new VRGamepadUIBehavior(cockpit.Context) { Priority = 0 });
            cockpit.InputBehaviors.Add(new VRSpatialDeviceUIBehavior(cockpit.Context) { Priority = 0 });

            FolderListAdapter adapter = new FolderListAdapter() {
                OnFileClickedAction = (s) => { entryField.Text = s; },
                OnFileDoubleClickedAction = (s) => SaveFile(s),
                OnListChanged = (l) => { ListView = l; }
            };
            adapter.OnFolderChanged(ListView);
            cockpit.InputBehaviors.Add(new FolderScrollBehavior(adapter));

            cockpit.OverrideBehaviors.Add(new ScreenCaptureBehavior() { Priority = 0,
                ScreenshotPath = Environment.GetEnvironmentVariable("homepath") + "\\DropBox\\ScreenShots\\" });
        }


        public void add_text_entry_field(Cockpit cockpit)
        {
            float fUseRadius = HUDRadius * 0.85f;
            float fAngle = -35.0f;

            entryField = new HUDTextEntry();
            entryField.Text = GetDefaultFileName();
            entryField.Width = 1.0f;
            entryField.Height = 0.08f;
            entryField.TextHeight = 0.06f;
            entryField.Create();
            entryField.Name = "selectFileName";
            HUDUtil.PlaceInSphere(entryField, fUseRadius, 0.0f, fAngle);
            cockpit.AddUIElement(entryField, true);

            tooltipText = new HUDLabel();
            tooltipText.Shape = new HUDShape(HUDShapeType.Rectangle, 1.0f, 0.04f);
            tooltipText.Text = Tooltip;
            tooltipText.TextHeight = 0.03f;
            tooltipText.BackgroundColor = Colorf.TransparentBlack;
            tooltipText.TextColor = Colorf.Silver;
            tooltipText.Create();
            tooltipText.Name = "tooltip";
            HUDUtil.PlaceInSphere(tooltipText, fUseRadius, 0.0f, fAngle);
            UnityUtil.TranslateInFrame(tooltipText.RootGameObject, 0.0f, -entryField.Height, 0, CoordSpace.WorldCoords);
            cockpit.AddUIElement(tooltipText, true);


            HUDButton saveButton = HUDBuilder.CreateRectIconClickButton(
                 0.2f, 0.1f, fUseRadius, 0.0f, fAngle,
                  "icons/save_v1");
            UnityUtil.TranslateInFrame(saveButton.RootGameObject, 0.2f, -0.1f, 0, CoordSpace.WorldCoords);
            saveButton.Name = "save";
            cockpit.AddUIElement(saveButton, true);
            saveButton.OnClicked += (s, e) => { SaveFromEntryText(); };

            HUDButton cancelButton = HUDBuilder.CreateRectIconClickButton(
                0.2f, 0.1f, fUseRadius, 0.0f, fAngle,
                 "icons/cancel_v1");
            UnityUtil.TranslateInFrame(cancelButton.RootGameObject, 0.4f, -0.1f, 0, CoordSpace.WorldCoords);
            cancelButton.Name = "cancel";
            cockpit.AddUIElement(cancelButton, true);
            cancelButton.OnClicked += (s, e) => {
                cockpit.Context.PopCockpit(true);
            };
        }



        virtual public void SaveFromEntryText()
        {
            if (FileSystemUtils.IsFullFilesystemPath(entryField.Text))
                SaveFile(entryField.Text);
            else
                SaveFile(Path.Combine(ListView.FolderPath, entryField.Text));
        }

    }




    public class FileExportCockpit : FileSaveCockpit
    {
        public FileExportCockpit()
        {
            ValidExtensions = new string[] { ".obj" };
        }

        public override string GetDefaultFileName()
        {
            return "Simplex_" + DateTime.Now.ToString("MMMdd_hhmm") + DateTime.Now.ToString("tt").ToLower() + ".obj";
        }

        public override void SaveFile(string sNewPath)
        {
            DebugUtil.Log(1, "Exporting scene to path " + sNewPath);

            SceneGraphConfig.LastFileOpenPath = ListView.FolderPath;

            string sSavePath = Path.Combine(ListView.FolderPath, sNewPath);
            SceneMeshExporter export = new SceneMeshExporter();
            export.WriteInBackgroundThreads = true;
            export.BackgroundWriteCompleteF = (exp, status) => {
                if (status.Ok) {
                    ActiveCockpit.Context.RegisterNextFrameAction(() => {
                        ActiveCockpit.Context.PopCockpit(true);
                    });
                } else {
                    ActiveCockpit.Context.RegisterNextFrameAction(() => {
                        Debug.Log("[ExportScene] save at " + sNewPath + " failed: " + export.LastErrorMessage);
                        HUDUtil.ShowCenteredPopupMessage("Export Failed", "Sorry, could not export to path " + sNewPath, ActiveCockpit);
                    });
                }
            };
            export.Export(ActiveCockpit.Context.Scene, sSavePath);
        }
    }




    public class JanusVRExportCockpit : FileSaveCockpit
    {
        public JanusVRExportCockpit()
        {
            ValidExtensions = new string[] { ".html" };
            Tooltip = "Enter a new folder name";
        }

        public override string GetDefaultFileName()
        {
            return "janusFolder";
        }

        public override void SaveFile(string sNewPath)
        {
            DebugUtil.Log(1, "Exporting scene to path " + sNewPath);
            SceneGraphConfig.LastFileOpenPath = ListView.FolderPath;

            string sTargetFolder = "";
            bool use_auto = sNewPath.EndsWith(GetDefaultFileName());
            if ( use_auto == false )
                sTargetFolder = Path.GetFileName(sNewPath);

            string sSavePath = Path.Combine(ListView.FolderPath, sNewPath);
            JanusVRExporter export = new JanusVRExporter() { GlobalTranslation = -20 * Vector3f.AxisZ };
            export.WriteInBackgroundThreads = true;
            export.BackgroundWriteCompleteF = (exp, status) => {
                if (status.Ok) {
                    ActiveCockpit.Context.RegisterNextFrameAction(() => {
                        FContext context = ActiveCockpit.Context;
                        ActiveCockpit.Context.PopCockpit(true);
                        //HUDUtil.ShowCenteredPopupMessage("Done!", "Exported to subfolder " + export.ExportPath, context.ActiveCockpit);
                        HUDUtil.ShowToastPopupMessage("Exported JanusVR Room to subfolder " + export.ExportPath, context.ActiveCockpit, 1.25f, 0.8f);
                    });
                } else {
                    ActiveCockpit.Context.RegisterNextFrameAction(() => {
                        Debug.Log("[JanusExportScene] save at " + sNewPath + " failed: " + export.LastErrorMessage);
                        HUDUtil.ShowCenteredPopupMessage("Export Failed", "Sorry, could not export to path " + sNewPath, ActiveCockpit);
                    });
                }
            };
            export.Export(ActiveCockpit.Context.Scene, ListView.FolderPath, sTargetFolder);
        }
    }





    public class FileSaveSceneCockpit : FileSaveCockpit
    {
        public FileSaveSceneCockpit()
        {
            ValidExtensions = new string[] { ".xml" };
        }

        public override string GetDefaultFileName()
        {
            return "Simplex_" + DateTime.Now.ToString("MMMdd_hhmm") + DateTime.Now.ToString("tt").ToLower() + ".xml";
        }

        public override void SaveFile(string sNewPath)
        {
            try {
                DebugUtil.Log(1, "Saving scene to path " + sNewPath);

                SceneGraphConfig.LastFileOpenPath = ListView.FolderPath;

                if (!sNewPath.EndsWith(".xml", StringComparison.InvariantCultureIgnoreCase))
                    sNewPath += ".xml";

                XMLOutputStream stream = new XMLOutputStream( XmlTextWriter.Create(sNewPath,
                 new XmlWriterSettings() { Indent = true, NewLineOnAttributes = true }) );
                stream.Writer.WriteStartDocument();
                SceneSerializer serializer = new SceneSerializer();
                serializer.TargetFilePath = sNewPath;
                serializer.Store(stream, ActiveCockpit.Scene);
                stream.Writer.WriteEndDocument();

                stream.Writer.Close();

                ActiveCockpit.Context.PopCockpit(true);
            } catch ( Exception e ){
                Debug.Log("[SaveScene] save of " + sNewPath + " failed: " + e.Message);
                HUDUtil.ShowCenteredPopupMessage("Save Failed", "Sorry, could not save to path " + sNewPath, ActiveCockpit);
            }
}
    }

}