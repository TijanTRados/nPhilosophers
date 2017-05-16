using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PP_DEMO
{
    enum Stanje
    {
        prljavo,
        cisto
    };

    class Vilica
    {
        bool imam;
        Stanje stanje = Stanje.prljavo;

        public Vilica(bool imam)
        {
            this.imam = imam;
        }

        public bool Prljava
        {
            get
            {
                if (stanje == Stanje.prljavo) return true;
                else return false;
            }
        }

        public bool Cista
        {
            get
            {
                if (stanje == Stanje.cisto) return true;
                else return false;
            }
        }

        public bool Imam
        {
            get { return imam; }
            set { imam = value; }
        }

        public bool Trebam
        {
            get
            {
                if (!imam) return true;
                else return false;
            }
        }

        public void Predaj()
        {
            imam = false;
        }

        public void Uzmi()
        {
            imam = true;
        }

        public void Ocisti()
        {
            stanje = Stanje.cisto;
        }

        public void Zaprljaj()
        {
            stanje = Stanje.prljavo;
        }
    }
}
