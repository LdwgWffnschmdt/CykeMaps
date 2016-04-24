using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;

namespace CykeMaps.UI
{
    public class SnapPointsPanel : StackPanel, IScrollSnapPointsInfo
    {
        public List<float> SnapPoints { get; set; }

        bool IScrollSnapPointsInfo.AreHorizontalSnapPointsRegular
        {
            get { return false; }
        }

        bool IScrollSnapPointsInfo.AreVerticalSnapPointsRegular
        {
            get { return false; }
        }

        IReadOnlyList<float> IScrollSnapPointsInfo.GetIrregularSnapPoints(Orientation orientation, SnapPointsAlignment alignment)
        {
            if (SnapPoints == null) return new List<float>();
            return SnapPoints;
        }
    }
}
