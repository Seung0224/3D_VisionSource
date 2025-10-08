using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Cyotek.Windows.Forms; // NuGet: Cyotek.Windows.Forms.ImageBox

namespace _3D_VisionSource
{
    // C# 7.3 / WinForms / Cyotek ImageBox 전용 ROI 오버레이 매니저 (단일 파일)
    // 사용법:
    //   1) 이 파일을 프로젝트에 추가
    //   2) Form 생성자 등에서: _roi = new RoiOverlayForImageBox(IntensityImageBox);
    //   3) btn_show_roi 클릭 핸들러에서: _roi.BtnShowRoi();
    //   4) ROI 가져오기(이미지 좌표): RectangleF? r = _roi.GetRoiImageRect();
    //   5) ROI 설정(이미지 좌표): _roi.SetRoiImageRect(new RectangleF(x,y,w,h));
    //
    // 주의: Cyotek ImageBox의 Zoom/스크롤/팬을 사용해도 ROI는 이미지 좌표 기준으로 정확히 유지됩니다.

    // C# 7.3 / WinForms / Cyotek ImageBox 전용 ROI 오버레이 매니저
    // - 줌/스크롤에도 ROI 흔들림 없음 (ImageBox ViewPort + AutoScrollPosition + ZoomFactor 기반 변환)
    // - ROI 드래그 중에는 ImageBox의 왼쪽버튼 팬을 일시 억제(PanMode Middle) 후 MouseUp에 복원
    using System;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Windows.Forms;
    using Cyotek.Windows.Forms;

    public sealed class RoiOverlayForImageBox : IDisposable
    {
        private readonly ImageBox _box;

        // ROI (이미지 좌표)
        private RectangleF? _roiImg;

        // 마우스 편집 상태
        private bool _draggingBody;
        private bool _draggingHandle;
        private int _activeHandle = -1; // -1=none, 0..7
        private Point _lastClient;      // 마지막 마우스 위치(클라이언트 좌표)
        private const float HandleScreenSize = 8f; // 화면좌표 기준 핸들 크기(px)

        // ==== 팬 억제 상태 ====
        private ImageBoxPanMode _savedPanMode = ImageBoxPanMode.Both;
        private bool _panSuppressed;

        // 스타일
        public Color Stroke { get; set; } = Color.DeepSkyBlue;
        public Color Fill { get; set; } = Color.FromArgb(64, Color.DeepSkyBlue);
        public float StrokeWidth { get; set; } = 2f;

        public RoiOverlayForImageBox(ImageBox imageBox)
        {
            if (imageBox == null) throw new ArgumentNullException(nameof(imageBox));
            _box = imageBox;

            _box.Paint += OnBoxPaint;
            _box.MouseDown += OnBoxMouseDown;
            _box.MouseMove += OnBoxMouseMove;
            _box.MouseUp += OnBoxMouseUp;
            _box.MouseLeave += OnBoxMouseLeave;

            _box.ZoomChanged += (s, e) => _box.Invalidate();
            _box.Scroll += (s, e) => _box.Invalidate();
            _box.SizeChanged += (s, e) => _box.Invalidate();
        }

        public void Dispose()
        {
            _box.Paint -= OnBoxPaint;
            _box.MouseDown -= OnBoxMouseDown;
            _box.MouseMove -= OnBoxMouseMove;
            _box.MouseUp -= OnBoxMouseUp;
            _box.MouseLeave -= OnBoxMouseLeave;
            RestorePan();
        }

        /// <summary>btn_show_roi에서 호출: ROI 없으면 중앙 30% 생성, 있으면 선택만</summary>
        public void BtnShowRoi()
        {
            if (_box.Image == null) { System.Media.SystemSounds.Beep.Play(); return; }

            if (_roiImg.HasValue)
            {
                _box.Focus();
                _box.Invalidate();
                return;
            }

            var w = (float)_box.Image.Width * 0.3f;
            var h = (float)_box.Image.Height * 0.3f;
            var x = ((float)_box.Image.Width - w) * 0.5f;
            var y = ((float)_box.Image.Height - h) * 0.5f;
            _roiImg = Normalize(new RectangleF(x, y, w, h));
            _box.Invalidate();
        }

        public RectangleF? GetRoiImageRect() => _roiImg;

        public void SetRoiImageRect(RectangleF rectImg)
        {
            if (_box.Image == null) return;
            _roiImg = Normalize(rectImg);
            _box.Invalidate();
        }

        public void ClearRoi()
        {
            _roiImg = null;
            _box.Invalidate();
        }

        // ===== 드로잉 =====
        private void OnBoxPaint(object sender, PaintEventArgs e)
        {
            if (_box.Image == null || !_roiImg.HasValue)
                return;

            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            var rc = ImageRectToClient(_roiImg.Value);

            using (var br = new SolidBrush(Fill))
                g.FillRectangle(br, rc);
            using (var pen = new Pen(Stroke, StrokeWidth))
                g.DrawRectangle(pen, rc.X, rc.Y, rc.Width, rc.Height);

            var handles = GetHandleRects(rc);
            using (var brh = new SolidBrush(Color.White))
            using (var penh = new Pen(Color.Black, 1f))
            {
                for (int i = 0; i < handles.Length; i++)
                {
                    var hr = handles[i];
                    g.FillRectangle(brh, hr);
                    g.DrawRectangle(penh, hr.X, hr.Y, hr.Width, hr.Height);
                }
            }
        }

        // ===== 마우스 =====
        private void OnBoxMouseDown(object sender, MouseEventArgs e)
        {
            if (_box.Image == null) return;
            _box.Focus();

            _lastClient = e.Location;

            if (_roiImg.HasValue && e.Button == MouseButtons.Left)
            {
                var rcClient = ImageRectToClient(_roiImg.Value);
                var handleIndex = HitTestHandle(rcClient, e.Location);
                if (handleIndex >= 0)
                {
                    _activeHandle = handleIndex;
                    _draggingHandle = true;
                    SuppressPan(); // 핸들 리사이즈 중에는 팬 금지
                    return;
                }

                if (rcClient.Contains(e.Location))
                {
                    _draggingBody = true;
                    SuppressPan(); // ROI 본체 이동 중에도 팬 금지
                    return;
                }
            }
        }

        private void OnBoxMouseMove(object sender, MouseEventArgs e)
        {
            if (_box.Image == null) return;

            var cur = e.Location;

            if (_draggingHandle && _roiImg.HasValue)
            {
                var imgPrev = ClientPointToImage(_lastClient);
                var imgCur = ClientPointToImage(cur);

                var r = _roiImg.Value;
                float dx = imgCur.X - imgPrev.X;
                float dy = imgCur.Y - imgPrev.Y;

                ResizeByHandle(_activeHandle, ref r, dx, dy);
                _roiImg = Normalize(r);

                _lastClient = cur;
                _box.Invalidate();
                return;
            }

            if (_draggingBody && _roiImg.HasValue)
            {
                var imgPrev = ClientPointToImage(_lastClient);
                var imgCur = ClientPointToImage(cur);
                float dx = imgCur.X - imgPrev.X;
                float dy = imgCur.Y - imgPrev.Y;

                var r = _roiImg.Value;
                r.X += dx; r.Y += dy;
                _roiImg = r;

                _lastClient = cur;
                _box.Invalidate();
                return;
            }

            UpdateCursorByHit(e.Location);
        }

        private void OnBoxMouseUp(object sender, MouseEventArgs e)
        {
            _draggingHandle = false;
            _draggingBody = false;
            _activeHandle = -1;
            RestorePan(); // 드래그 종료 → 팬 복원
            _box.Cursor = Cursors.Default;
        }

        private void OnBoxMouseLeave(object sender, EventArgs e)
        {
            if (!_draggingBody && !_draggingHandle)
            {
                _box.Cursor = Cursors.Default;
                RestorePan(); // 안전 복원
            }
        }

        // ===== 좌표 변환 (ImageBox 공식 준수)
        private RectangleF ImageRectToClient(RectangleF imgRect)
        {
            var vp = _box.GetImageViewPort();
            float z = (float)_box.ZoomFactor;
            float ox = vp.Left + _box.Padding.Left + _box.AutoScrollPosition.X;
            float oy = vp.Top + _box.Padding.Top + _box.AutoScrollPosition.Y;

            float x = ox + imgRect.X * z;
            float y = oy + imgRect.Y * z;
            float w = imgRect.Width * z;
            float h = imgRect.Height * z;
            return new RectangleF(x, y, w, h);
        }

        private PointF ClientPointToImage(Point clientPoint)
        {
            var vp = _box.GetImageViewPort();
            float z = (float)_box.ZoomFactor;
            float ox = vp.Left + _box.Padding.Left + _box.AutoScrollPosition.X;
            float oy = vp.Top + _box.Padding.Top + _box.AutoScrollPosition.Y;

            float ix = (clientPoint.X - ox) / z;
            float iy = (clientPoint.Y - oy) / z;
            return new PointF(ix, iy);
        }

        // ===== 팬 억제/복원 =====
        private void SuppressPan()
        {
            if (_panSuppressed) return;
            _savedPanMode = _box.PanMode;                 // 현재 설정 저장
            _box.PanMode = ImageBoxPanMode.Middle;        // 왼쪽버튼 팬 비활성(중버튼만 허용)
            _panSuppressed = true;
        }

        private void RestorePan()
        {
            if (!_panSuppressed) return;
            _box.PanMode = _savedPanMode;                 // 원래 설정 복구
            _panSuppressed = false;
        }

        // ===== 유틸 =====
        private static RectangleF Normalize(RectangleF r)
        {
            float x = r.X, y = r.Y, w = r.Width, h = r.Height;
            if (w < 0) { x += w; w = -w; }
            if (h < 0) { y += h; h = -h; }
            return new RectangleF(x, y, w, h);
        }

        private RectangleF[] GetHandleRects(RectangleF rcClient)
        {
            float s = HandleScreenSize;
            float hs = s / 2f;

            var cx = rcClient.X + rcClient.Width / 2f;
            var cy = rcClient.Y + rcClient.Height / 2f;

            return new[]
            {
            new RectangleF(rcClient.Left - hs,      rcClient.Top - hs,       s, s), // 0 좌상
            new RectangleF(cx - hs,                 rcClient.Top - hs,       s, s), // 1 상
            new RectangleF(rcClient.Right - hs,     rcClient.Top - hs,       s, s), // 2 우상
            new RectangleF(rcClient.Right - hs,     cy - hs,                 s, s), // 3 우
            new RectangleF(rcClient.Right - hs,     rcClient.Bottom - hs,    s, s), // 4 우하
            new RectangleF(cx - hs,                 rcClient.Bottom - hs,    s, s), // 5 하
            new RectangleF(rcClient.Left - hs,      rcClient.Bottom - hs,    s, s), // 6 좌하
            new RectangleF(rcClient.Left - hs,      cy - hs,                 s, s), // 7 좌
        };
        }

        private int HitTestHandle(RectangleF rcClient, Point ptClient)
        {
            var handles = GetHandleRects(rcClient);
            for (int i = 0; i < handles.Length; i++)
                if (handles[i].Contains(ptClient))
                    return i;
            return -1;
        }

        private void UpdateCursorByHit(Point ptClient)
        {
            if (!_roiImg.HasValue)
            {
                _box.Cursor = Cursors.Default;
                return;
            }

            var rc = ImageRectToClient(_roiImg.Value);
            int h = HitTestHandle(rc, ptClient);
            if (h >= 0)
            {
                switch (h)
                {
                    case 0: case 4: _box.Cursor = Cursors.SizeNWSE; break;
                    case 2: case 6: _box.Cursor = Cursors.SizeNESW; break;
                    case 1: case 5: _box.Cursor = Cursors.SizeNS; break;
                    case 3: case 7: _box.Cursor = Cursors.SizeWE; break;
                }
                return;
            }

            _box.Cursor = rc.Contains(ptClient) ? Cursors.SizeAll : Cursors.Default;
        }

        private static void ResizeByHandle(int handleIndex, ref RectangleF r, float dx, float dy)
        {
            switch (handleIndex)
            {
                case 0: r.X += dx; r.Y += dy; r.Width -= dx; r.Height -= dy; break; // 좌상
                case 1: r.Y += dy; r.Height -= dy; break;                          // 상
                case 2: r.Y += dy; r.Width += dx; r.Height -= dy; break;           // 우상
                case 3: r.Width += dx; break;                                      // 우
                case 4: r.Width += dx; r.Height += dy; break;                      // 우하
                case 5: r.Height += dy; break;                                     // 하
                case 6: r.X += dx; r.Width -= dx; r.Height += dy; break;           // 좌하
                case 7: r.X += dx; r.Width -= dx; break;                           // 좌
            }
        }
    }
}
