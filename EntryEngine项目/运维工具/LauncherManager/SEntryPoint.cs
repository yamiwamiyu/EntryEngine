using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EntryEngine.UI;
using EntryEngine;
using EntryEngine.Serialize;

namespace LauncherManager
{
    public class SEntryPoint : UIScene
    {
        protected override IEnumerable<ICoroutine> Loading()
        {
            _LOG._Logger = new LoggerFile(new LoggerConsole());
            _SAVE.Load();
            //_LANGUAGE.Load(_IO.ReadText("Content\\Language.csv"), "");
            //_TABLE.Load("Content\\");

            //Entry.ShowMainScene<S登陆菜单>();
            Entry.ShowMainScene<S登陆菜单>();

            Entry.SetCoroutine(Run());

            this.State = EState.Release;
            return base.Loading();
        }

        IEnumerable<ICoroutine> Run()
        {
            while (true)
            {
                if (Maintainer.Maintainers != null)
                {
                    foreach (var maintainer in Maintainer.Maintainers.ToArray())
                    {
                        try
                        {
                            maintainer.Update();
                        }
                        catch (Exception ex)
                        {
                            _LOG.Error("Maintainer update error! msg={0} stack={1}", ex.Message, ex.StackTrace);
                        }
                    }
                }
                yield return null;
            }
        }
    }
}
