using System;
using Sunny.UI;

namespace _3D_VisionSource
{
    public sealed class UiListBoxLogger : FusionEngine.IFusionLogger
    {
        private readonly UIListBox _lb;
        private readonly int _capacity;
        private readonly object _gate = new object();

        public UiListBoxLogger(UIListBox listBox, int capacity = 1000)
        {
            _lb = listBox ?? throw new ArgumentNullException(nameof(listBox));
            _capacity = Math.Max(100, capacity);
        }

        public void Log(string message)
        {
            if (_lb.IsDisposed) return;

            void append()
            {
                // 시간 prefix
                string line = $"[{DateTime.Now:HH:mm:ss.fff}] {message}";
                lock (_gate)
                {
                    _lb.Items.Add(line);
                    // 용량 관리
                    while (_lb.Items.Count > _capacity)
                        _lb.Items.RemoveAt(0);
                    // 스크롤 맨 아래
                    _lb.SelectedIndex = _lb.Items.Count - 1;
                }
            }

            if (_lb.InvokeRequired) _lb.BeginInvoke((Action)append);
            else append();
        }
    }
}
