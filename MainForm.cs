using _3D_VisionSource.Viewer;
using Cyotek.Windows.Forms;
using OpenCvSharp;
using Sunny.UI;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace _3D_VisionSource
{
    public partial class MainForm : UIForm
    {
        #region Fields
        // Viwer 및 Image 관련 Fields
        private Viewer3DControl _viewer = null;
        private Image _intensityImg = null, _zmapImg = null;
        private Mat _intensityMat = null, _zmapMat = null;   // 처리용 Mat
        private string _intensityPath = null, _zMapPath = null;
        private float[,] _zRawCache = null;

        // ImageBox 공용 컨텍스트 메뉴
        private readonly UIContextMenuStrip _imageMenu = new UIContextMenuStrip();
        private ToolStripMenuItem _miFit = new ToolStripMenuItem(), _miSave = new ToolStripMenuItem();

        private RoiOverlayForImageBox _roi;
        #endregion

        /// 폼 및 UI 초기화(생성자)
        public MainForm()
        {
            InitializeComponent();
            InitializeMainUI();
            Initialize3DViewerUI();
            InitializeImageBoxContextMenu();
            InitializeROI();
            InitializeLogger();
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

        private void InitializeROI()
        {
            _roi = new RoiOverlayForImageBox(IntensityImageBox);
        }

        private void InitializeLogger()
        {
            FusionEngine.LogSink = new UiListBoxLogger(LB_3D_VISION_LOG, capacity: 2000);
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
                if (_intensityMat == null || _intensityMat.Empty() || _zmapMat == null || _zmapMat.Empty())
                {
                    UIMessageTip.ShowWarning("Mat이 올바르게 로드되지 않았습니다.");
                    return;
                }

                // Z 변환: 캐시가 없으면 포맷에 맞춰 1회 변환
                var zRaw = _zRawCache;
                if (zRaw == null)
                {
                    if (_zmapMat.Type() == MatType.CV_16UC1)
                        zRaw = FusionEngine.LoadZ16(_zmapMat);
                    else
                        throw new NotSupportedException($"지원하지 않는 Z 포맷: {_zmapMat.Type()}");

                    _zRawCache = zRaw; // 캐시
                }

                var roiRect = _roi.GetRoiImageRect();

                var p = new InspectionParams
                {
                    Sx = UP_Sx.Text.ToFloat(),
                    Sy = UP_Sy.Text.ToFloat(),
                    ZScale = UP_Zscale.Text.ToFloat(),
                    ZOffset = UP_Zoff.Text.ToFloat(),
                    MinAreaMm2 = UP_Spec.Text.ToDouble(),
                    OverlayAlpha = UP_Overlay.Text.ToDouble(),
                    Centinal = UP_Centinal.Active,
                };

                // Inspect: Mat 기반 오버로드 사용 (경로/Bitmap 재-리드 없음)
                var res = FusionEngine.Inspect(_intensityMat, zRaw, p, roiRectImg: roiRect);

                // 검사 결과 테이블
                InspectionResultsTable.Bind(GV_3D_VISION_LOG, InspectionResultsTable.ToRows(res));

                // 포인트 클라우드
                _viewer.LoadPoints(res.Points, res.Colors, 2.0);

                // 2D 오버레이
                if (res.Overlay2D != null)
                {
                    TWODImageBox.Image = res.Overlay2D;
                    TWODImageBox.ZoomToFit();
                }

                // 3D 오버레이: 채움 메쉬 우선, 없으면 라인 루프
                var meshes = FusionEngine.Make3DFilledMeshes(res, zRaw, 2, 1.5);
                if (meshes != null && meshes.Length > 0)
                    _viewer.OverlayFillMeshes(meshes, System.Windows.Media.Colors.Red, 0.35f);
                else
                {
                    var loops = FusionEngine.Make3DContourLoops(res, zRaw, 2);
                    if (loops != null && loops.Count > 0)
                        _viewer.OverlayLineLoops(loops.ToArray(), System.Windows.Media.Colors.Red, 2.0f);
                }
            }
            catch (Exception ex)
            {
                UIMessageBox.ShowError("Fusion 실패\n" + ex.Message);
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

            OverlayDisplayClear();
            RefreshUI();

            try
            {
                // 처리용 Mat 로드 (Unchanged로 원본 그대로)
                _intensityMat?.Dispose();
                _zmapMat?.Dispose();
                _intensityMat = string.IsNullOrEmpty(_intensityPath) ? null : Cv2.ImRead(_intensityPath, ImreadModes.Unchanged);
                _zmapMat = string.IsNullOrEmpty(_zMapPath) ? null : Cv2.ImRead(_zMapPath, ImreadModes.Unchanged);

                // Z 캐시 미리 생성(선택): 포맷에 맞춰 1회 변환
                _zRawCache = null;
                if (_zmapMat != null && !_zmapMat.Empty())
                {
                    if (_zmapMat.Type() == MatType.CV_16UC1)
                        _zRawCache = FusionEngine.LoadZ16(_zmapMat);
                    else
                        UIMessageTip.ShowWarning($"지원하지 않는 Z 포맷: {_zmapMat.Type()}");
                }
            }
            catch (Exception ex)
            {
                UIMessageTip.ShowWarning("Mat 로드 실패: " + ex.Message);
            }
        }
        /// 패널 텍스트 및 2D 프리뷰 동시 갱신
        private void RefreshUI()
        {
            PNL_INTENSITY_PATH.Text = string.IsNullOrEmpty(_intensityPath) ? "(not set)" : _intensityPath;
            PNL_ZMAP_PATH.Text = string.IsNullOrEmpty(_zMapPath) ? "(not set)" : _zMapPath;

            SetImage(IntensityImageBox, ref _intensityImg, _intensityPath);
            SetImage(ZMapImageBox, ref _zmapImg, _zMapPath);
        }

        private void BTN_SET_ROI_Click(object sender, EventArgs e)
        {
            _roi.ConfirmRoi();
        }

        private void BTN_SHOW_ROI_Click(object sender, EventArgs e)
        {
            _roi.BtnShowRoi();
        }

        private void BTN_3D_VISION_LOG_CLEAR_Click(object sender, EventArgs e)
        {
            LB_3D_VISION_LOG.Items.Clear();
        }

        private void BTN_3D_VISION_LOG_DATA_CLEAR_Click(object sender, EventArgs e)
        {
            // 데이터소스가 BindingList라면
            if (GV_3D_VISION_LOG.DataSource is System.ComponentModel.IBindingList bl)
                bl.Clear();                          // 컬럼 유지 + 행만 삭제
            else
            {
                // 데이터소스가 없거나 Rows를 직접 쓰는 경우
                GV_3D_VISION_LOG.DataSource = null;   // 바인딩 해제
                GV_3D_VISION_LOG.Rows.Clear();        // 행 전체 삭제(컬럼은 유지)
            }
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

        private void UP_Sx_Click(object sender, EventArgs e)
        {
            InlineNumberEdit.Start(host: UP_Sx, initText: UP_Sx?.Text, onCommit: (val) => { UP_Sx.Text = val; });
        }

        private void UP_Sy_Click(object sender, EventArgs e)
        {
            InlineNumberEdit.Start(host: UP_Sy, initText: UP_Sy?.Text, onCommit: (val) => { UP_Sy.Text = val; });
        }

        private void UP_Zscale_Click(object sender, EventArgs e)
        {
            InlineNumberEdit.Start(host: UP_Zscale, initText: UP_Zscale?.Text, onCommit: (val) => { UP_Zscale.Text = val; });
        }
        private void UP_Zoff_Click(object sender, EventArgs e)
        {
            InlineNumberEdit.Start(host: UP_Zoff, initText: UP_Zoff?.Text, onCommit: (val) => { UP_Zoff.Text = val; });
        }

        private void UP_Spec_Click(object sender, EventArgs e)
        {
            InlineNumberEdit.Start(host: UP_Spec, initText: UP_Spec?.Text, onCommit: (val) => { UP_Spec.Text = val; });
        }

        private void UP_Overlay_Click(object sender, EventArgs e)
        {
            InlineNumberEdit.Start(host: UP_Overlay, initText: UP_Overlay?.Text, onCommit: (val) => { UP_Overlay.Text = val; });
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

            try { _intensityMat?.Dispose(); _intensityMat = null; } catch { }
            try { _zmapMat?.Dispose(); _zmapMat = null; } catch { }

            try { IntensityImageBox.Image = null; } catch { }
            try { ZMapImageBox.Image = null; } catch { }

            try { if (_viewer is IDisposable) ((IDisposable)_viewer).Dispose(); _viewer = null; } catch { }
            try { ViewerHost.Child = null; } catch { }
            try { ViewerHost.Dispose(); } catch { }
        }

        private void OverlayDisplayClear()
        {
            // 2D 오버레이 정리 (Bitmap 핸들 해제)
            if (TWODImageBox.Image is Bitmap bmp)
            {
                try { bmp.Dispose(); } catch { /* ignore */ }
            }
            TWODImageBox.Image = null;

            // 3D 뷰 정리 (Geometry/Material/IDisposable 해제 후 Clear)
            var items = _viewer.Viewport.Items.ToArray(); // 컬렉션 복사 후 순회
            foreach (var el in items)
            {
                if (el is HelixToolkit.Wpf.SharpDX.MeshGeometryModel3D mg)
                {
                    mg.Geometry = null;
                    mg.Material = null;
                }
                else if (el is HelixToolkit.Wpf.SharpDX.LineGeometryModel3D lg)
                {
                    lg.Geometry = null;
                    // lg.Color = Colors.Transparent; // 필요시
                }
                else if (el is HelixToolkit.Wpf.SharpDX.PointGeometryModel3D pg)
                {
                    pg.Geometry = null;
                }
                (el as IDisposable)?.Dispose(); // 가능하면 자원 해제
            }
            _viewer.Viewport.Items.Clear();
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
