using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace TimeLine
{
    public class testcursor : MonoBehaviour
    {
        // Структура, которую ожидает Windows
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public class OpenFileName {
            public int structSize = 0;
            public IntPtr dlgOwner = IntPtr.Zero;
            public IntPtr instance = IntPtr.Zero;
            public string filter = null;
            public string customFilter = null;
            public int maxCustFilter = 0;
            public int filterIndex = 0;
            public string file = null;
            public int maxFile = 0;
            public string fileTitle = null;
            public int maxFileTitle = 0;
            public string initialDir = null;
            public string title = null;
            public int flags = 0;
            public short fileOffset = 0;
            public short fileExtension = 0;
            public string defExt = null;
            public IntPtr custData = IntPtr.Zero;
            public IntPtr hook = IntPtr.Zero;
            public string templateName = null;
            public IntPtr reservedPtr = IntPtr.Zero;
            public int reservedInt = 0;
            public int flagsEx = 0;
        }

        public class Win32FilePicker {
            [DllImport("comdlg32.dll", SetLastError = true, CharSet = CharSet.Auto)]
            public static extern bool GetOpenFileName([In, Out] OpenFileName ofn);

            public static string ShowDialog() {
                OpenFileName ofn = new OpenFileName();
                ofn.structSize = Marshal.SizeOf(ofn);
        
                // Фильтры пишутся через нулевой символ \0
                ofn.filter = "All Files\0*.*\0Text Files\0*.txt\0";
        
                ofn.file = new string(new char[256]);
                ofn.maxFile = ofn.file.Length;
        
                ofn.fileTitle = new string(new char[64]);
                ofn.maxFileTitle = ofn.fileTitle.Length;
        
                ofn.initialDir = UnityEngine.Application.dataPath; // Папка по умолчанию
                ofn.title = "Выберите файл, милорд";
                ofn.flags = 0x00080000 | 0x00001000 | 0x00000800 | 0x00000200; // Explorer-style, File must exist

                if (GetOpenFileName(ofn)) {
                    return ofn.file; // Возвращаем путь к выбранному файлу
                }
                return null;
            }
        }

        // private void Start()
        // {
        //     Debug.Log(Win32FilePicker.ShowDialog());
        // }
    }
}
