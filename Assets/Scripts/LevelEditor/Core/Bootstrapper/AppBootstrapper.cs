using System.Globalization;
using System.Threading;
using UnityEngine;

namespace TimeLine.LevelEditor.Core.BootBootstrapper
{
    public class AppBootstrapper : MonoBehaviour
    {
        private void Awake()
        {
            CultureInfo culture = CultureInfo.InvariantCulture;
            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;
            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;
        }
    }
}