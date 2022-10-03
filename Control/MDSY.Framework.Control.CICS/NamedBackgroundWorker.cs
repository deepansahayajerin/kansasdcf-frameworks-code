using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.ComponentModel;

namespace MDSY.Framework.Control.CICS
{
    public class NamedBackgroundWorker : BackgroundWorker
    {
        public NamedBackgroundWorker(string name)
        {
            Name = name;
        }

        public string Name { get; private set; }

        protected override void OnDoWork(DoWorkEventArgs e)
        {
                Thread.CurrentThread.Name = Name;

            base.OnDoWork(e);
        }
    }
}
