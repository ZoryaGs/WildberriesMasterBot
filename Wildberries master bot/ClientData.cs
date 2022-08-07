using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wildberries_master_bot
{
    internal class ClientData
    {
        public int loadedPage = 0;
        public string apiKey = "";

        public ClientData(int loadedPage)
        {
            this.loadedPage = loadedPage;
        }
    }
}
