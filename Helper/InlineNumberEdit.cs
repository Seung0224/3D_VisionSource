// 파일: InlineNumberEdit.cs
using Sunny.UI;
using System;
using System.Globalization;
using System.Linq;
using System.Windows.Documents;
using System.Windows.Forms;

namespace _3D_VisionSource
{
    /// <summary>
    /// 패널 위에 인라인 숫자 입력 박스를 잠깐 띄웠다가,
    /// Enter(또는 포커스 아웃) 시 최종 문자열을 콜백(out 개념)으로 돌려주는 유틸.
    /// </summary>
    public static class InlineNumberEdit
    {
        /// <summary>
        /// 숫자 인라인 편집 시작.
        /// - host: 입력 박스를 띄울 UIPanel (또는 UIHeader, UIPanel 파생)
        /// - initText: 초기 표시 문자열(없으면 host 안에 보이는 첫 텍스트/라벨/텍스트박스 값 탐색)
        /// - decimals: 고정 소수 자릿수(예: 3 -> 0.123)
        /// - min/max: 허용 범위(초과/미만 시 clamp)
        /// - onCommit: Enter/포커스아웃으로 확정 시 콜백(string)
        /// - onCancel: Esc로 취소 시 콜백(optional)
        /// </summary>
        public static void Start(Control host, string initText, Action<string> onCommit, int decimals = 4, double min = 0.0000, double max = 100.0000, Action onCancel = null)
        {
            if (host == null || host.IsDisposed) return;

            // 이미 떠있는 에디터가 있으면 무시
            foreach (Control c in host.Controls)
            {
                if (c.Tag is string tag && tag == "__INLINE_NUMBER_EDITOR__")
                    return;
            }

            // 초기 텍스트 비어있다면 패널 내부에서 유추
            if (string.IsNullOrWhiteSpace(initText))
                initText = FindInitialText(host) ?? "";

            // Sunny.UI UITextBox 사용 (일반 TextBox도 OK)
            var tb = new UITextBox
            {
                Tag = "__INLINE_NUMBER_EDITOR__",
                Dock = DockStyle.Fill,
                Text = initText,
                TextAlignment = System.Drawing.ContentAlignment.MiddleCenter,
                Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0))),
                Cursor = Cursors.IBeam,
                ImeMode = ImeMode.Off, // 한글 IME 차단
                MinimumSize = new System.Drawing.Size(10, 10),
                Padding = new System.Windows.Forms.Padding(6, 3, 6, 3),
            };

            // Sunny.UI 스타일 살짝 맞추기(원하는 대로 조정 가능)
            tb.RectColor = host is UIPanel up ? up.RectColor : System.Drawing.Color.FromArgb(220, 230, 250);
            tb.FillColor = System.Drawing.Color.LightGray;
            tb.Radius = 3;

            // 편집키 필터링: 숫자/.-, 백스페이스, 좌우/Del/홈/엔드 허용
            tb.KeyPress += (s, e) =>
            {
                if (char.IsControl(e.KeyChar))
                    return;

                if (char.IsDigit(e.KeyChar))
                    return;

                if (e.KeyChar == '.')
                {
                    // 소수점은 1개만
                    if (tb.Text.Contains('.')) e.Handled = true;
                    return;
                }

                if (e.KeyChar == '-')
                {
                    // 음수는 맨 앞에서만
                    if (tb.SelectionStart != 0 || tb.Text.Contains('-'))
                        e.Handled = true;
                    return;
                }

                // 그 외 차단
                e.Handled = true;
            };

            // Enter/ESC 처리
            tb.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    CommitAndClose();
                    e.Handled = true;
                }
                else if (e.KeyCode == Keys.Escape)
                {
                    CancelAndClose();
                    e.Handled = true;
                }
            };

            // 포커스 아웃 시 자동 커밋(원치 않으면 주석)
            tb.Leave += (s, e) =>
            {
                if (!tb.IsDisposed) CommitAndClose();
            };

            // 호스트에 추가 & 포커스
            host.Controls.Add(tb);
            tb.BringToFront();
            tb.Focus();
            tb.SelectAll();

            // 내부 로컬 함수들
            void CommitAndClose()
            {
                if (tb.IsDisposed) return;
                var ok = TryNormalize(tb.Text, decimals, min, max, out string finalStr);
                if (!ok)
                {
                    // 비정상 입력이면 min으로 정규화
                    finalStr = FormatFixed(min, decimals);
                }

                try { onCommit?.Invoke(finalStr); }
                catch { /* swallow */ }

                SafeRemove(tb);
            }

            void CancelAndClose()
            {
                try { onCancel?.Invoke(); } catch { }
                SafeRemove(tb);
            }
        }

        /// <summary>
        /// host 내부에서 첫 텍스트 후보를 찾아 초기값으로 사용 (Label/TextBox/UIButton 등)
        /// </summary>
        private static string FindInitialText(Control host)
        {
            // 우선 순위: UITextBox -> TextBoxBase -> UILabel/UIButton -> 기타 Control.Text
            var tb = host.Controls.OfType<TextBoxBase>().FirstOrDefault();
            if (tb != null && !string.IsNullOrWhiteSpace(tb.Text)) return tb.Text;

            var uiTb = host.Controls.OfType<UITextBox>().FirstOrDefault();
            if (uiTb != null && !string.IsNullOrWhiteSpace(uiTb.Text)) return uiTb.Text;

            var lbl = host.Controls.OfType<Label>().FirstOrDefault();
            if (lbl != null && !string.IsNullOrWhiteSpace(lbl.Text)) return lbl.Text;

            var uiLbl = host.Controls.OfType<UILabel>().FirstOrDefault();
            if (uiLbl != null && !string.IsNullOrWhiteSpace(uiLbl.Text)) return uiLbl.Text;

            var btn = host.Controls.OfType<Button>().FirstOrDefault();
            if (btn != null && !string.IsNullOrWhiteSpace(btn.Text)) return btn.Text;

            var uiBtn = host.Controls.OfType<UIButton>().FirstOrDefault();
            if (uiBtn != null && !string.IsNullOrWhiteSpace(uiBtn.Text)) return uiBtn.Text;

            // 마지막으로 host.Text
            if (!string.IsNullOrWhiteSpace(host.Text)) return host.Text;

            return null;
        }

        private static bool TryNormalize(string src, int decimals, double min, double max, out string normalized)
        {
            normalized = null;
            if (src == null) return false;

            // 공백/콤마 제거, 현재 문화/Invariant 모두 시도
            var s = src.Trim().Replace(",", "");
            if (!double.TryParse(s, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out double v))
            {
                if (!double.TryParse(s, out v))
                    return false;
            }

            if (double.IsNaN(v) || double.IsInfinity(v)) return false;

            // 범위 클램프
            if (v < min) v = min;
            if (v > max) v = max;

            normalized = FormatFixed(v, decimals);
            return true;
        }

        private static string FormatFixed(double value, int decimals)
        {
            if (decimals <= 0) return Math.Round(value).ToString(CultureInfo.InvariantCulture);
            var fmt = "F" + decimals.ToString();
            return value.ToString(fmt, CultureInfo.InvariantCulture);
        }

        private static void SafeRemove(Control c)
        {
            try
            {
                var parent = c.Parent;
                if (parent != null)
                {
                    parent.Controls.Remove(c);
                    c.Dispose();
                }
            }
            catch { /* ignore */ }
        }
    }
}
