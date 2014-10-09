using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game
{
    class Felhasznalok
    {
        string id;

        public string ID
        {
            get { return id; }
        }
        string nev;

        public string Nev
        {
            get { return nev; }
        }
        public Felhasznalok(string id, string nev)
        {
            this.id = id;
            this.nev = nev;
        }
    }
}
