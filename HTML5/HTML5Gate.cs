using System;

namespace EntryEngine.HTML5
{
    public class HTML5Gate
    {
        private static string _path;
        public static string PATH
        {
            get { return _path; }
            set
            {
                if (value != null && value.Length > 0 && !value.EndsWith("/"))
                    value += "/";
                _path = value;
            }
        }
        public static int OVER_HEAT = 0;

        EntryJS entry;
        int overheat;
        static int timer;
        public EntryJS Entry
        {
            get { return entry; }
        }
        public void Run(Action<Entry> onInitialized)
        {
            entry = new EntryJS();
            entry.Initialize();
            if (onInitialized != null)
                onInitialized(entry);
            Update();
        }
        static float updateTime;
        static float drawTime;
        static int fps;
        static int startTime;
        static int startTime2;
        FONT font;
        static int fps2;
        static float updateTime2;
        static float drawTime2;
        static float testTime;
        public static float TestTime;
        public static void Exit()
        {
            window.clearTimeout(timer);
        }
        private void InternalUpdate()
        {
            entry.Update();
        }
        private void InternalDraw()
        {
            entry.Draw();
        }
        protected void Update()
        {
            if (startTime2 == 0)
                startTime2 = new Date().getTime();
            var time = new Date().getTime();
            entry.Update();
            var time2 = new Date().getTime();
            updateTime += time2 - time;
            entry.Draw();
            var time3 = new Date().getTime();
            drawTime += time3 - time2;
            startTime += time3 - startTime2;
            startTime2 = time3;
            fps++;
            if (font == null)
                font = FONT.Default;
            Entry.GRAPHICS.Begin();
            Entry.GRAPHICS.Draw(font, string.Format("FPS:{0} UT:{1} DT:{2} TestTIME:{3}", fps2, (int)(updateTime2 * 100) / 100.0, (int)(drawTime2 * 100) / 100.0, (int)(testTime * 100) / 100.0), VECTOR2.Zero, COLOR.CornflowerBlue);
            Entry.GRAPHICS.End();
            if (startTime >= 1000)
            {
                fps2 = fps;
                updateTime2 = updateTime / fps;
                drawTime2 = drawTime / fps;
                testTime = TestTime / fps;
                //_LOG.Debug("FPS: {0} UT:{1} DT:{2}", fps, (int)updateTime, (int)drawTime);
                updateTime = 0;
                drawTime = 0;
                fps = 0;
                TestTime = 0;
                startTime -= 1000;
            }
            
            var elapsed = entry.GameTime.CurrentElapsed;
            if (elapsed < entry.IPlatform.FrameRate)
            {
                //window.clearTimeout(timer);
                timer = window.setTimeout(Update, (entry.IPlatform.FrameRate - elapsed).TotalMilliseconds);
            }
            else
            {
                if (overheat == OVER_HEAT)
                {
                    //_LOG.Debug("overheat: {0}", elapsed.TotalMilliseconds);
                    //window.clearTimeout(timer);
                    double wait = elapsed.TotalMilliseconds;
                    double frameRate = entry.IPlatform.FrameRate.TotalMilliseconds;
                    while (wait > frameRate)
                        wait -= frameRate;
                    timer = window.setTimeout(Update, wait);
                    overheat = 0;
                    return;
                }
                overheat++;
                Update();
            }
        }
        [ANonOptimize]public static void LogFps()
        {
            console.log(string.Format("FPS:{0} UT:{1} DT:{2} TestTIME:{3}", fps2, (int)(updateTime2 * 100) / 100.0, (int)(drawTime2 * 100) / 100.0, (int)(testTime * 100) / 100.0));
        }
    }
}
