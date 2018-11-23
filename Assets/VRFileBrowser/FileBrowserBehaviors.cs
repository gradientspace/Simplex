using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using g3;


namespace f3
{
    public class FolderScrollBehavior : StandardInputBehavior
    {
        FolderListAdapter adapter;

        public FolderScrollBehavior(FolderListAdapter adapter)
        {
            this.adapter = adapter;
        }

        public override InputDevice SupportedDevices {
            get { return InputDevice.Mouse | InputDevice.Gamepad | InputDevice.AnySpatialDevice; }
        }


        // this is weird...

        public override CaptureRequest WantsCapture(InputState input)
        {
            if (input.fMouseWheel != 0 || input.vRightStickDelta2D[1] != 0)
                return CaptureRequest.Begin(this);
            return CaptureRequest.Ignore;
        }

        public override Capture BeginCapture(InputState input, CaptureSide eSide)
        {
            float fDelta = 0.0f;
            if (input.fMouseWheel != 0)
                fDelta = -input.fMouseWheel * 0.02f;
            else if (input.vLeftStickDelta2D[1] != 0)
                fDelta = input.vLeftStickDelta2D[1] * 0.015f;
            else if (input.vRightStickDelta2D[1] != 0)
                fDelta = input.vRightStickDelta2D[1] * 0.015f;
            adapter.FolderList.Scroll(fDelta);

            return Capture.Begin(this);
        }

        public override Capture UpdateCapture(InputState input, CaptureData data)
        {
            return Capture.End;
        }

        public override Capture ForceEndCapture(InputState input, CaptureData data) {
            return Capture.End;
        }
    }





    public class FileBrowserTextFieldKeyHandler : IShortcutKeyHandler
    {
        FContext controller;
        HUDTextEntry entryField;
        float deleteTime;

        public Action OnReturn = null;

        public FileBrowserTextFieldKeyHandler(FContext c, HUDTextEntry entry)
        {
            controller = c;
            entryField = entry;
        }
        public bool HandleShortcuts()
        {
            if (Input.GetKeyUp(KeyCode.Escape)) {
                controller.PopCockpit(true);
                return true;

            } else if ( Input.GetKeyUp(KeyCode.Return) ) {
                if (OnReturn != null)
                    OnReturn();
                return true;

            } else if (Input.GetKeyDown(KeyCode.Backspace)) {
                deleteTime = FPlatform.RealTime() + 0.5f;
                if (entryField.Text.Length > 0)
                    entryField.Text = entryField.Text.Substring(0, entryField.Text.Length - 1);
                return true;

            } else if (Input.GetKey(KeyCode.Backspace)) {
                if (entryField.Text.Length > 0 && FPlatform.RealTime() - deleteTime > 0.05f) {
                    entryField.Text = entryField.Text.Substring(0, entryField.Text.Length - 1);
                    deleteTime = FPlatform.RealTime();
                }
                return true;

            } else if (Input.GetKeyDown(KeyCode.V) && Input.GetKey(KeyCode.LeftControl)) {
                //entryField.Text = "https://www.dropbox.com/s/momif47x1erb2fp/test_bunny.obj?raw=1";
                return true;

            } else if (Input.anyKeyDown) {
                if (Input.inputString.Length > 0 && FileSystemUtils.IsValidFilenameString(Input.inputString)) {
                    entryField.Text += Input.inputString;
                    return true;
                }
            }

            return false;
        }
    }

}
