using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;

namespace DS4Windows
{
    using InterceptionContext = IntPtr;
    using InterceptionDevice = Int32;
    using InterceptionPrecedence = Int32;
    using InterceptionFilter = UInt16;

    public class Interception
    {
        public Interception()
        {
            m_mutex = new Mutex();

            m_stroke = new InterceptionStroke();
            m_context = InterceptionLayer.CreateContext();

            Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.High;

            InterceptionLayer.SetFilter(m_context, InterceptionLayer.IsKeyboard, (ushort)(InterceptionFilterKeyState.DOWN | InterceptionFilterKeyState.UP));
            InterceptionLayer.SetFilter(m_context, InterceptionLayer.IsMouse, (ushort)InterceptionFilterMouseState.ALL);

            m_mouseDevice = InterceptionLayer.Wait(m_context);
            while (InterceptionLayer.IsMouse(m_mouseDevice) == 0)
                m_mouseDevice = InterceptionLayer.Wait(m_context);


            m_keyboardDevice = InterceptionLayer.Wait(m_context);
            while (InterceptionLayer.IsKeyboard(m_keyboardDevice) == 0)
                m_keyboardDevice = InterceptionLayer.Wait(m_context);

            m_thread = new Thread(new ThreadStart(Update));
            m_thread.Start();
        }

        public void MouseClick(InterceptionMouseState state)
        {
            var stroke = new InterceptionStroke();

            stroke.Mouse.state = state;

            if (m_mutex.WaitOne())
            {
                InterceptionLayer.Send(m_context, m_mouseDevice, ref stroke, 1);
                m_mutex.ReleaseMutex();
            }
        }

        public void MouseWheel(short vertical)
        {
            var stroke = new InterceptionStroke();
            stroke.Mouse.state = InterceptionMouseState.WHEEL;
            stroke.Mouse.rolling = vertical;
            stroke.Mouse.flags = InterceptionMouseFlag.MOVE_RELATIVE;

            if (m_mutex.WaitOne())
            {
                InterceptionLayer.Send(m_context, m_mouseDevice, ref stroke, 1);
                m_mutex.ReleaseMutex();
            }
        }

        public void MouseHWheel(short horizontal)
        {
            var stroke = new InterceptionStroke();
            stroke.Mouse.state = InterceptionMouseState.HWHEEL;
            stroke.Mouse.rolling = horizontal;
            stroke.Mouse.flags = InterceptionMouseFlag.MOVE_RELATIVE;

            if (m_mutex.WaitOne())
            {
                InterceptionLayer.Send(m_context, m_mouseDevice, ref stroke, 1);
                m_mutex.ReleaseMutex();
            }
        }

        public void KeyDown(ushort key)
        {
            var stroke = new InterceptionStroke();
            stroke.Key.state = InterceptionKeyState.DOWN;
            stroke.Key.code = key;

            if (m_mutex.WaitOne())
            {
                InterceptionLayer.Send(m_context, m_keyboardDevice, ref stroke, 1);
                m_mutex.ReleaseMutex();
            }
        }

        public void KeyUp(ushort key)
        {
            var stroke = new InterceptionStroke();
            stroke.Key.state = InterceptionKeyState.UP;
            stroke.Key.code = key;

            if (m_mutex.WaitOne())
            {
                InterceptionLayer.Send(m_context, m_keyboardDevice, ref stroke, 1);
                m_mutex.ReleaseMutex();
            }
        }

        public void MoveMouseAbsolute(int x, int y)
        {
            var stroke = new InterceptionStroke();
            stroke.Mouse.x = x;
            stroke.Mouse.y = y;
            stroke.Mouse.flags = InterceptionMouseFlag.MOVE_RELATIVE;

            if (m_mutex.WaitOne())
            {
                InterceptionLayer.Send(m_context, m_mouseDevice, ref stroke, 1);
                m_mutex.ReleaseMutex();
            }
        }

        private void Update()
        {
            InterceptionDevice anyDevice = 0;
            while (true)
            {
                if (InterceptionLayer.Receive(m_context, anyDevice = InterceptionLayer.Wait(m_context), ref m_stroke, 1) <= 0)
                    break;

                if (InterceptionLayer.IsMouse(anyDevice) != 0)
                {
                    if (m_mutex.WaitOne())
                    {
                        InterceptionLayer.Send(m_context, anyDevice, ref m_stroke, 1);
                        m_mutex.ReleaseMutex();
                    }
                }

                if (InterceptionLayer.IsKeyboard(anyDevice) != 0)
                {
                    if (m_mutex.WaitOne())
                    {
                        InterceptionLayer.Send(m_context, anyDevice, ref m_stroke, 1);
                        m_mutex.ReleaseMutex();
                    }
                }
            }
        }

        ~Interception()
        {
            InterceptionLayer.DestroyContext(m_context);
        }

        private Mutex m_mutex;
        private Thread m_thread;

        private InterceptionDevice m_mouseDevice;
        private InterceptionDevice m_keyboardDevice;
        private InterceptionStroke m_stroke;
        private InterceptionContext m_context;
    }

}
