using System;
using Sunny.UI;
using System.IO;
using System.Linq;
using System.Drawing;
using System.Diagnostics;
using System.Windows.Forms;
using Cyotek.Windows.Forms;
using System.Drawing.Imaging;
using _3D_VisionSource.Viewer;
using System.Text.RegularExpressions;

namespace _3D_VisionSource
{
    public partial class MainForm : UIForm
    {
        #region Fields
        // Viwer 및 Image 관련 Fields
        private Viewer3DControl _viewer = null;
        private Image _intensityImg = null, _zmapImg = null;
        private string _intensityPath = null, _zMapPath = null;

        // ImageBox 공용 컨텍스트 메뉴
        private readonly UIContextMenuStrip _imageMenu = new UIContextMenuStrip();
        private ToolStripMenuItem _miFit = new ToolStripMenuItem(), _miSave = new ToolStripMenuItem();
        #endregion

        /// 폼 및 UI 초기화(생성자)
        public MainForm()
        {
            InitializeComponent();
            InitializeMainUI();
            Initialize3DViewerUI();
            InitializeImageBoxContextMenu();
        }

        #region Initialize
        /// Sunny.UI 스타일 및 기본 창 속성
        private void InitializeMainUI()
        {
            Style = UIStyle.Blue;
            Text = "3D Vision Source";
            titleForeColor = Color.Black;
            ShowIcon = false;
            WindowState = FormWindowState.Maximized;
            TitleFont = new Font("Segoe UI", 12f, FontStyle.Regular, GraphicsUnit.Point);
        }
        /// WPF 3D 뷰어(ElementHost) 초기화
        private void Initialize3DViewerUI()
        {
            _viewer = new Viewer3DControl();
            ViewerHost.Child = _viewer;
        }
        /// ImageBox 공용 컨텍스트 메뉴(우클릭) 초기화
        private void InitializeImageBoxContextMenu()
        {
            _miFit = new ToolStripMenuItem("Image Fit");
            _miSave = new ToolStripMenuItem("Image Save");

            _miFit.Font = new Font("Segoe UI", 9f, FontStyle.Regular, GraphicsUnit.Point);
            _miSave.Font = new Font("Segoe UI", 9f, FontStyle.Regular, GraphicsUnit.Point);

            _miFit.Click += delegate
            {
                var box = _imageMenu.SourceControl as ImageBox;
                if (box != null && box.Image != null) box.ZoomToFit();
            };

            _miSave.Click += delegate
            {
                var box = _imageMenu.SourceControl as ImageBox;
                SaveImageFromBox(box);
            };

            _imageMenu.Items.AddRange(new ToolStripItem[] { _miFit, _miSave });

            IntensityImageBox.ContextMenuStrip = _imageMenu;
            ZMapImageBox.ContextMenuStrip = _imageMenu;
            TWODImageBox.ContextMenuStrip = _imageMenu;
        }
        #endregion

        #region Buttons
        /// 이미지 오픈(유형 무관 선택 + 자동 매칭)
        private void BTN_IMAGE_OPEN_Click(object sender, EventArgs e) { SelectAndResolve(true); }
        // Fusion 수행: 포인트클라우드 생성, 홀검출, 2D/3D 오버레이 표시
        private void BTN_IMAGE_FUSION_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(_intensityPath) || string.IsNullOrEmpty(_zMapPath))
                {
                    UIMessageTip.ShowWarning("먼저 Intensity/ZMap 이미지를 모두 로드하세요.");
                    return;
                }

                var res = FusionEngine.Inspect(_intensityPath, _zMapPath, null, true);
                _viewer.LoadPoints(res.Points, res.Colors, 2.0);

                if (res.Overlay2D != null)
                {
                    TWODImageBox.Image = res.Overlay2D;
                    TWODImageBox.ZoomToFit();
                }

                bool is16;
                var zRaw = FusionEngine.LoadZRawFromFile(_zMapPath, out is16);

                // 3D 오버레이: 채움 메쉬 우선, 없으면 라인 루프
                var meshes = FusionEngine.Make3DFilledMeshes(res, zRaw, is16, 2, 1.5);
                if (meshes != null && meshes.Length > 0)
                {
                    _viewer.OverlayFillMeshes(meshes, System.Windows.Media.Colors.Red, 0.35f);
                }
                else
                {
                    var loops = FusionEngine.Make3DContourLoops(res, zRaw, is16, 2);
                    if (loops != null && loops.Count > 0)
                        _viewer.OverlayLineLoops(loops.ToArray(), System.Windows.Media.Colors.Red, 2.0f);
                }

                var compCount = (res.CompLabels != null) ? res.CompLabels.Count : 0;
                var totalArea = (res.CompAreaMm2 != null) ? res.CompAreaMm2.Sum() : 0.0;
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
            }
        }

        /// Intensity 전용 선택
        private void BTN_PICK_INTENSITY_Click(object sender, EventArgs e) { SelectAndResolve(true); }
        /// ZMap 전용 선택
        private void BTN_PICK_ZMAP_Click(object sender, EventArgs e) { SelectAndResolve(false); }
        #endregion

        #region Form Events
        /// 종료 직전 자원 해제
        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            _intensityPath = null;
            _zMapPath = null;
            DisposeResources();
        }
        /// 종료 후 정리(디버그 보조)
        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            try { GC.Collect(); GC.WaitForPendingFinalizers(); GC.Collect(); } catch { }
            try { Environment.Exit(0); } catch { }
        }
        #endregion

        #region Helpers (Condensed)
        /// 파일 선택→타입 판별→짝 매칭→UI 갱신
        private void SelectAndResolve(bool pickIntensityFirst)
        {
            string initialDir = pickIntensityFirst
                ? (!string.IsNullOrEmpty(_intensityPath) ? Path.GetDirectoryName(_intensityPath) : null)
                : (!string.IsNullOrEmpty(_zMapPath) ? Path.GetDirectoryName(_zMapPath) : null);

            string title = pickIntensityFirst ? "Select Intensity Image" : "Select ZMap Image";
            string chosen = null;

            using (var ofd = new OpenFileDialog())
            {
                ofd.Title = title;
                ofd.Filter = "Image Files|*.png;*.jpg;*.jpeg;*.bmp;*.tif;*.tiff|All Files|*.*";
                ofd.CheckFileExists = true;
                if (!string.IsNullOrEmpty(initialDir) && Directory.Exists(initialDir))
                    ofd.InitialDirectory = initialDir;

                if (ofd.ShowDialog(this) == DialogResult.OK) chosen = ofd.FileName;
            }

            if (string.IsNullOrEmpty(chosen)) return;

            var kind = PathResolver.DetermineKind(chosen);
            if (kind == PathResolver.FileKind.Intensity)
            {
                _intensityPath = chosen;
                var mate = PathResolver.TryFindCounterpart(chosen, true);
                if (mate != null) { _intensityPath = chosen; _zMapPath = mate; }
            }
            else if (kind == PathResolver.FileKind.ZMap)
            {
                _zMapPath = chosen;
                var mate = PathResolver.TryFindCounterpart(chosen, false);
                if (mate != null) { _intensityPath = mate; _zMapPath = chosen; }
            }
            else
            {
                if (pickIntensityFirst)
                {
                    _intensityPath = chosen;
                    var mate = PathResolver.TryFindCounterpart(chosen, true);
                    if (mate != null) { _intensityPath = chosen; _zMapPath = mate; }
                }
                else
                {
                    _zMapPath = chosen;
                    var mate = PathResolver.TryFindCounterpart(chosen, false);
                    if (mate != null) { _intensityPath = mate; _zMapPath = chosen; }
                }
            }

            RefreshUI();
        }
        /// 패널 텍스트 및 2D 프리뷰 동시 갱신
        private void RefreshUI()
        {
            PNL_INTENSITY_PATH.Text = string.IsNullOrEmpty(_intensityPath) ? "(not set)" : _intensityPath;
            PNL_ZMAP_PATH.Text = string.IsNullOrEmpty(_zMapPath) ? "(not set)" : _zMapPath;

            SetImage(IntensityImageBox, ref _intensityImg, _intensityPath);
            SetImage(ZMapImageBox, ref _zmapImg, _zMapPath);
        }
        /// ImageBox에 파일 경로를 로드/표시(잠금 없이)
        private static void SetImage(ImageBox box, ref Image old, string path)
        {
            try
            {
                if (old != null) { old.Dispose(); old = null; }
                if (string.IsNullOrEmpty(path) || !File.Exists(path)) { box.Image = null; return; }

                using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (var img = Image.FromStream(fs, false, false))
                {
                    old = new Bitmap(img);
                }

                box.Image = old;
                box.ZoomToFit();
            }
            catch
            {
                if (old != null) { old.Dispose(); old = null; }
                box.Image = null;
            }
        }
        /// 현재 ImageBox 이미지를 저장
        private void SaveImageFromBox(ImageBox box)
        {
            if (box == null || box.Image == null)
            {
                UIMessageTip.ShowWarning("저장할 이미지가 없습니다.");
                return;
            }

            using (var sfd = new SaveFileDialog())
            {
                sfd.Title = "이미지 저장";
                sfd.Filter = "PNG (*.png)|*.png|JPEG (*.jpg;*.jpeg)|*.jpg;*.jpeg|BMP (*.bmp)|*.bmp|TIFF (*.tif;*.tiff)|*.tif;*.tiff|All Files|*.*";
                sfd.AddExtension = true;
                sfd.FileName = "Image_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".png";

                if (sfd.ShowDialog(this) != DialogResult.OK) return;

                try
                {
                    var ext = (Path.GetExtension(sfd.FileName) ?? "").ToLowerInvariant();
                    ImageFormat fmt;
                    switch (ext)
                    {
                        case ".jpg":
                        case ".jpeg":
                            fmt = ImageFormat.Jpeg; break;
                        case ".bmp":
                            fmt = ImageFormat.Bmp; break;
                        case ".tif":
                        case ".tiff":
                            fmt = ImageFormat.Tiff; break;
                        case ".png":
                        default:
                            fmt = ImageFormat.Png; break;
                    }

                    using (var clone = new Bitmap(box.Image))
                    {
                        clone.Save(sfd.FileName, fmt);
                    }
                    UIMessageTip.ShowOk("이미지를 저장했습니다.");
                }
                catch (Exception ex)
                {
                    UIMessageBox.ShowError("저장 실패\n" + ex.Message);
                }
            }
        }
        /// 2D/3D/Host 자원 일괄 해제
        private void DisposeResources()
        {
            try { if (_intensityImg != null) { _intensityImg.Dispose(); _intensityImg = null; } } catch { }
            try { if (_zmapImg != null) { _zmapImg.Dispose(); _zmapImg = null; } } catch { }
            try { IntensityImageBox.Image = null; } catch { }
            try { ZMapImageBox.Image = null; } catch { }

            try { if (_viewer is IDisposable) ((IDisposable)_viewer).Dispose(); _viewer = null; } catch { }
            try { ViewerHost.Child = null; } catch { }
            try { ViewerHost.Dispose(); } catch { }
        }
        #endregion

        #region 이미지 파일 경로 해석기
        /// 파일명/폴더명 규칙에 따른 Intensity/ZMap 판별 및 짝 매칭
        internal static class PathResolver
        {
            public enum FileKind { Unknown, Intensity, ZMap }

            private const string IntensityFolderName = "Intensity";
            private const string ZMapFolderName = "ZMap";
            private static readonly Regex RxIntensity = new Regex(@"^Intensity_(\d+)(\.\w+)$", RegexOptions.IgnoreCase);
            private static readonly Regex RxZMap = new Regex(@"^ZMap_(\d+)(\.\w+)$", RegexOptions.IgnoreCase);

            // 파일 경로로 Intensity/ZMap 타입 판별
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
            // 선택 파일의 형제 폴더 경로(Intensity↔ZMap) 계산
            public static string GetSiblingFolder(string filePath, bool toZMap)
            {
                var dir = Path.GetDirectoryName(filePath);
                if (dir == null) return null;

                var parent = Path.GetDirectoryName(dir);
                var leaf = Path.GetFileName(dir);
                if (parent == null || leaf == null) return null;

                if (toZMap && !leaf.Equals(ZMapFolderName, StringComparison.OrdinalIgnoreCase))
                    return Path.Combine(parent, ZMapFolderName);

                if (!toZMap && !leaf.Equals(IntensityFolderName, StringComparison.OrdinalIgnoreCase))
                    return Path.Combine(parent, IntensityFolderName);

                return dir;
            }
            // Intensity_# ↔ ZMap_# 규칙으로 대응 파일명 생성
            public static string MapCounterpartFileName(string selectedFileName, bool toZMap)
            {
                if (toZMap)
                {
                    var m = RxIntensity.Match(selectedFileName);
                    if (m.Success) return "ZMap_" + m.Groups[1].Value + m.Groups[2].Value;
                }
                else
                {
                    var m = RxZMap.Match(selectedFileName);
                    if (m.Success) return "Intensity_" + m.Groups[1].Value + m.Groups[2].Value;
                }
                return selectedFileName;
            }
            // 선택 파일의 짝 파일 경로 탐색(확장자 유연, 자기 자신 제외)
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
                    var pattern = prefix + num + ".*";
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
    }
}
