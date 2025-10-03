using System;

namespace Sklep
{
    class Program
    {
        class Klient
        {
            private string email;
            private string haslo;

            public Klient(string email, string haslo)
            {
                this.email = email;
                this.haslo = haslo;
            }


        }

        class Lista_klientow
        {
            private List<Klient> klienci;

            public Lista_klientow()
            {
                klienci = new List<Klient>();
            }
        }

        static void Main(string[] args)
        {

            //
            //costam@gmail.com; maslo; chleb
            //inny@gmail.com; suszarka; dzem; 


        }
    }
}