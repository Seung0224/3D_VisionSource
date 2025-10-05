using Sunny.UI;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using _3D_VisionSource.Viewer;

namespace _3D_VisionSource
{
    public partial class MainForm : UIForm
    {
        #region Nested Types
        private sealed class FusionContext
        {
            public string IntensityPath { get; private set; }
            public string ZMapPath { get; private set; }
            public bool IsReady => !string.IsNullOrEmpty(IntensityPath) && !string.IsNullOrEmpty(ZMapPath);
            public void SetPair(string intensity, string zmap) { IntensityPath = intensity; ZMapPath = zmap; }
            public void SetIntensityOnly(string intensity) => IntensityPath = intensity;
            public void SetZMapOnly(string zmap) => ZMapPath = zmap;
            public void Clear() { IntensityPath = null; ZMapPath = null; }
        }

        private static class PathResolver
        {
            public enum FileKind { Unknown, Intensity, ZMap }

            private const string IntensityFolderName = "Intensity";
            private const string ZMapFolderName = "ZMap";
            private static readonly Regex RxIntensity = new Regex(@"^Intensity_(\d+)(\.\w+)$", RegexOptions.IgnoreCase);
            private static readonly Regex RxZMap = new Regex(@"^ZMap_(\d+)(\.\w+)$", RegexOptions.IgnoreCase);

            public static FileKind DetermineKind(string path)
            {
                var name = Path.GetFileName(path) ?? "";
                if (RxIntensity.IsMatch(name)) return FileKind.Intensity;
                if (RxZMap.IsMatch(name)) return FileKind.ZMap;

                var leaf = Path.GetFileName(Path.GetDirectoryName(path) ?? "");
                if (leaf.Equals(IntensityFolderName, StringComparison.OrdinalIgnoreCase)) return FileKind.Intensity;
                if (leaf.Equals(ZMapFolderName, StringComparison.OrdinalIgnoreCase)) return FileKind.ZMap;
                return FileKind.Unknown;
            }

            public static string GetSiblingFolder(string filePath, bool toZMap)
            {
                var dir = Path.GetDirectoryName(filePath);
                if (dir == null) return null;
                var parent = Path.GetDirectoryName(dir);
                var leaf = Path.GetFileName(dir);
                if (parent == null || leaf == null) return null;
                if (toZMap && !leaf.Equals(ZMapFolderName, StringComparison.OrdinalIgnoreCase)) return Path.Combine(parent, ZMapFolderName);
                if (!toZMap && !leaf.Equals(IntensityFolderName, StringComparison.OrdinalIgnoreCase)) return Path.Combine(parent, IntensityFolderName);
                return dir;
            }

            public static string MapCounterpartFileName(string selectedFileName, bool toZMap)
            {
                if (toZMap)
                {
                    var m = RxIntensity.Match(selectedFileName);
                    if (m.Success) return $"ZMap_{m.Groups[1].Value}{m.Groups[2].Value}";
                }
                else
                {
                    var m = RxZMap.Match(selectedFileName);
                    if (m.Success) return $"Intensity_{m.Groups[1].Value}{m.Groups[2].Value}";
                }
                return selectedFileName;
            }

            public static string TryFindCounterpart(string selectedPath, bool toZMap)
            {
                var selectedName = Path.GetFileName(selectedPath);
                var siblingDir = GetSiblingFolder(selectedPath, toZMap);
                if (siblingDir == null || !Directory.Exists(siblingDir)) return null;

                var selectedFull = Path.GetFullPath(selectedPath);

                var mapped = MapCounterpartFileName(selectedName, toZMap);
                var candidate = Path.Combine(siblingDir, mapped);
                if (File.Exists(candidate) && !Path.GetFullPath(candidate).Equals(selectedFull, StringComparison.OrdinalIgnoreCase))
                    return candidate;

                var rx = toZMap ? RxIntensity : RxZMap;
                var m = rx.Match(selectedName);
                if (m.Success)
                {
                    var num = m.Groups[1].Value;
                    var prefix = toZMap ? "ZMap_" : "Intensity_";
                    var pattern = $"{prefix}{num}.*";
                    var found = Directory.EnumerateFiles(siblingDir, pattern).FirstOrDefault(p =>
                        File.Exists(p) && !Path.GetFullPath(p).Equals(selectedFull, StringComparison.OrdinalIgnoreCase));
                    if (found != null) return found;
                }

                candidate = Path.Combine(siblingDir, selectedName);
                if (File.Exists(candidate) && !Path.GetFullPath(candidate).Equals(selectedFull, StringComparison.OrdinalIgnoreCase))
                    return candidate;

                return null;
            }
        }
        #endregion

        #region Fields
        private readonly FusionContext _ctx = new FusionContext();
        private Viewer3DControl _viewer;
        #endregion

        #region Init
        public MainForm()
        {
            InitializeComponent();
            InitializeMainUI();
            Initialize3DViwerUI();
        }

        private void InitializeMainUI()
        {
            Style = UIStyle.Blue;
            Text = "3D Vision Source";
            titleForeColor = Color.Black;
            ShowIcon = false;
            WindowState = FormWindowState.Maximized;
            TitleFont = new Font("Segoe UI", 12f, FontStyle.Regular, GraphicsUnit.Point);
        }

        private void Initialize3DViwerUI()
        {
            _viewer = new Viewer3DControl();
            ViewerHost.Child = _viewer;
        }
        #endregion

        #region Helpers
        private void UpdatePathPanels()
        {
            PNL_INTENSITY_PATH.Text = string.IsNullOrEmpty(_ctx.IntensityPath) ? "(not set)" : _ctx.IntensityPath;
            PNL_ZMAP_PATH.Text = string.IsNullOrEmpty(_ctx.ZMapPath) ? "(not set)" : _ctx.ZMapPath;
        }

        private static string PickImageFile(IWin32Window owner, string title, string initialDir = null)
        {
            using (var ofd = new OpenFileDialog())
            {
                ofd.Title = title;
                ofd.Filter = "Image Files|*.png;*.jpg;*.jpeg;*.bmp;*.tif;*.tiff|All Files|*.*";
                ofd.CheckFileExists = true;
                if (!string.IsNullOrEmpty(initialDir) && Directory.Exists(initialDir))
                    ofd.InitialDirectory = initialDir;
                return (ofd.ShowDialog(owner) == DialogResult.OK) ? ofd.FileName : null;
            }
        }

        private void TryPickAndResolve(bool pickIntensityFirst)
        {
            var startDir = pickIntensityFirst
                ? (!string.IsNullOrEmpty(_ctx.IntensityPath) ? Path.GetDirectoryName(_ctx.IntensityPath) : null)
                : (!string.IsNullOrEmpty(_ctx.ZMapPath) ? Path.GetDirectoryName(_ctx.ZMapPath) : null);

            var chosen = PickImageFile(this, pickIntensityFirst ? "Select Intensity Image" : "Select ZMap Image", startDir);
            if (string.IsNullOrEmpty(chosen)) return;

            var kind = PathResolver.DetermineKind(chosen);
            if (kind == PathResolver.FileKind.Intensity)
            {
                _ctx.SetIntensityOnly(chosen);
                var mate = PathResolver.TryFindCounterpart(chosen, toZMap: true);
                if (mate != null) _ctx.SetPair(chosen, mate);
            }
            else if (kind == PathResolver.FileKind.ZMap)
            {
                _ctx.SetZMapOnly(chosen);
                var mate = PathResolver.TryFindCounterpart(chosen, toZMap: false);
                if (mate != null) _ctx.SetPair(mate, chosen);
            }
            else
            {
                if (pickIntensityFirst)
                {
                    _ctx.SetIntensityOnly(chosen);
                    var mate = PathResolver.TryFindCounterpart(chosen, toZMap: true);
                    if (mate != null) _ctx.SetPair(chosen, mate);
                }
                else
                {
                    _ctx.SetZMapOnly(chosen);
                    var mate = PathResolver.TryFindCounterpart(chosen, toZMap: false);
                    if (mate != null) _ctx.SetPair(mate, chosen);
                }
            }

            UpdatePathPanels();
        }
        #endregion

        #region Button Handlers
        private void BTN_IMAGE_OPEN_Click(object sender, EventArgs e) => TryPickAndResolve(pickIntensityFirst: true);
        private void BTN_PICK_INTENSITY_Click(object sender, EventArgs e) => TryPickAndResolve(pickIntensityFirst: true);
        private void BTN_PICK_ZMAP_Click(object sender, EventArgs e) => TryPickAndResolve(pickIntensityFirst: false);

        private void BTN_IMAGE_FUSION_Click(object sender, EventArgs e)
        {
            try
            {
                if (!_ctx.IsReady)
                {
                    UIMessageTip.ShowWarning("먼저 Intensity/ZMap 이미지를 모두 로드하세요.");
                    return;
                }

                var fp = new FusionParams
                {
                    Sx = 0.05f,
                    Sy = 0.05f,
                    ZScale = 0.001f,
                    ZOffset = 0f,
                    InvalidZ = 0,
                    InvalidZ16 = 0,
                    CenterOrigin = true
                };

                var sw = Stopwatch.StartNew();
                var result = FusionEngine.BuildPointCloudFromFiles(_ctx.IntensityPath, _ctx.ZMapPath, fp);
                sw.Stop();

                _viewer.LoadPoints(result.Points, result.Colors, pointSize: 2.0);
                UIMessageTip.ShowOk($"Fusion 성공");
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
                UIMessageBox.ShowError($"Fusion 실패\n{ex.Message}");
            }
        }
        #endregion

        #region Form Events
      
        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            _ctx.Clear();
            SafeDisposeViewer();

            try { ViewerHost?.Dispose(); } catch { /* ignore */ }
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            try
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
            }
            catch { /* ignore */ }

            try { Environment.Exit(0); } catch { /* ignore */ }
        }

        private void SafeDisposeViewer()
        {
            try
            {
                if (_viewer is IDisposable d)
                    d.Dispose();

                // (3) 참조 제거
                _viewer = null;
            }
            catch { /* ignore */ }
        }
        #endregion
    }
}
